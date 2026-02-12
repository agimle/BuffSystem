#if UNITY_NETCODE_GAMEOBJECTS
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Runtime;

namespace BuffSystem.Networking.Netcode
{
    /// <summary>
    /// Netcode for GameObjects网络同步组件
    /// 需要Netcode for GameObjects框架支持，定义UNITY_NETCODE_GAMEOBJECTS符号后启用
    /// </summary>
    [RequireComponent(typeof(BuffOwner))]
    public class BuffNetcodeSync : NetworkBehaviour
    {
        private BuffOwner buffOwner;
        
        // 网络变量 - 服务器权威
        private NetworkList<BuffSyncData> syncedBuffs;
        
        // 本地Buff缓存
        private readonly Dictionary<int, BuffSyncData> localBuffCache = new();
        
        // 等待服务器确认的本地添加
        private readonly HashSet<int> pendingAdds = new();

        private void Awake()
        {
            buffOwner = GetComponent<BuffOwner>();
            
            // 初始化NetworkList
            syncedBuffs = new NetworkList<BuffSyncData>(
                new List<BuffSyncData>(),
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server
            );
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsServer)
            {
                // 服务器订阅事件
                if (buffOwner != null)
                {
                    buffOwner.LocalEvents.OnBuffAdded += OnServerBuffAdded;
                    buffOwner.LocalEvents.OnBuffRemoved += OnServerBuffRemoved;
                    buffOwner.LocalEvents.OnStackChanged += OnServerBuffStackChanged;
                    buffOwner.LocalEvents.OnBuffRefreshed += OnServerBuffRefreshed;
                }
            }
            else
            {
                // 客户端订阅网络变量变更
                syncedBuffs.OnListChanged += OnSyncedBuffsChanged;
                
                // 初始同步
                foreach (var syncData in syncedBuffs)
                {
                    ApplySyncData(syncData);
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            if (IsServer)
            {
                if (buffOwner != null)
                {
                    buffOwner.LocalEvents.OnBuffAdded -= OnServerBuffAdded;
                    buffOwner.LocalEvents.OnBuffRemoved -= OnServerBuffRemoved;
                    buffOwner.LocalEvents.OnStackChanged -= OnServerBuffStackChanged;
                    buffOwner.LocalEvents.OnBuffRefreshed -= OnServerBuffRefreshed;
                }
            }
            else
            {
                syncedBuffs.OnListChanged -= OnSyncedBuffsChanged;
            }
        }

        #region Server Side

        private void OnServerBuffAdded(object sender, BuffAddedEventArgs e)
        {
            if (!IsServer || e.Buff == null) return;
            
            var syncData = new BuffSyncData(e.Buff);
            syncedBuffs.Add(syncData);
            
            if (BuffSystemConfig.Instance.EnableDebugLog)
            {
                Debug.Log($"[BuffNetcodeSync] Server: 添加Buff {e.Buff.Name} (ID: {e.Buff.InstanceId})");
            }
        }

        private void OnServerBuffRemoved(object sender, BuffRemovedEventArgs e)
        {
            if (!IsServer || e.Buff == null) return;
            
            for (int i = 0; i < syncedBuffs.Count; i++)
            {
                if (syncedBuffs[i].InstanceId == e.Buff.InstanceId)
                {
                    syncedBuffs.RemoveAt(i);
                    
                    if (BuffSystemConfig.Instance.EnableDebugLog)
                    {
                        Debug.Log($"[BuffNetcodeSync] Server: 移除Buff {e.Buff.Name} (ID: {e.Buff.InstanceId})");
                    }
                    break;
                }
            }
        }

        private void OnServerBuffStackChanged(object sender, BuffStackChangedEventArgs e)
        {
            if (!IsServer || e.Buff == null) return;
            
            for (int i = 0; i < syncedBuffs.Count; i++)
            {
                if (syncedBuffs[i].InstanceId == e.Buff.InstanceId)
                {
                    var updated = syncedBuffs[i];
                    updated.Stack = (byte)Mathf.Clamp(e.NewStack, 0, 255);
                    syncedBuffs[i] = updated;
                    
                    if (BuffSystemConfig.Instance.EnableDebugLog)
                    {
                        Debug.Log($"[BuffNetcodeSync] Server: Buff {e.Buff.Name} 层数变化 {e.OldStack} -> {e.NewStack}");
                    }
                    break;
                }
            }
        }

        private void OnServerBuffRefreshed(object sender, BuffRefreshedEventArgs e)
        {
            if (!IsServer || e.Buff == null) return;
            
            for (int i = 0; i < syncedBuffs.Count; i++)
            {
                if (syncedBuffs[i].InstanceId == e.Buff.InstanceId)
                {
                    var updated = syncedBuffs[i];
                    updated.Duration = CompressDuration(e.Buff.Duration);
                    syncedBuffs[i] = updated;
                    break;
                }
            }
        }

        #endregion

        #region Client Side

        private void OnSyncedBuffsChanged(NetworkListEvent<BuffSyncData> changeEvent)
        {
            if (IsServer) return; // 服务器已处理
            
            switch (changeEvent.Type)
            {
                case NetworkListEvent<BuffSyncData>.EventType.Add:
                    ApplySyncData(changeEvent.Value);
                    break;
                    
                case NetworkListEvent<BuffSyncData>.EventType.Remove:
                    RemoveBuffByInstanceId(changeEvent.Value.InstanceId);
                    break;
                    
                case NetworkListEvent<BuffSyncData>.EventType.Insert:
                    ApplySyncData(changeEvent.Value);
                    break;
                    
                case NetworkListEvent<BuffSyncData>.EventType.Value:
                    UpdateBuffFromSyncData(changeEvent.Value);
                    break;
                    
                case NetworkListEvent<BuffSyncData>.EventType.Clear:
                    ClearAllBuffs();
                    break;
            }
        }

        private void ApplySyncData(BuffSyncData syncData)
        {
            // 检查是否已存在
            if (localBuffCache.ContainsKey(syncData.InstanceId))
            {
                UpdateBuffFromSyncData(syncData);
                return;
            }
            
            var data = BuffDatabase.Instance?.GetBuffData(syncData.BuffId);
            if (data == null)
            {
                Debug.LogWarning($"[BuffNetcodeSync] 找不到Buff数据 ID: {syncData.BuffId}");
                return;
            }
            
            // 添加Buff
            var buff = buffOwner.BuffContainer.AddBuff(data, null);
            if (buff is BuffEntity entity)
            {
                // 恢复层数
                int targetStack = syncData.Stack;
                if (targetStack > 1 && buff.CurrentStack < targetStack)
                {
                    entity.AddStack(targetStack - buff.CurrentStack);
                }
                
                // 恢复持续时间
                float duration = DecompressDuration(syncData.Duration);
                entity.SetDuration(duration);
                
                // 缓存
                localBuffCache[syncData.InstanceId] = syncData;
                
                if (BuffSystemConfig.Instance.EnableDebugLog)
                {
                    Debug.Log($"[BuffNetcodeSync] Client: 同步添加Buff {data.Name} (层数: {targetStack})");
                }
            }
        }

        private void RemoveBuffByInstanceId(int instanceId)
        {
            // 从缓存中查找对应的Buff
            foreach (var buff in buffOwner.BuffContainer.AllBuffs)
            {
                if (localBuffCache.TryGetValue(instanceId, out var cachedData))
                {
                    if (buff.DataId == cachedData.BuffId)
                    {
                        buffOwner.BuffContainer.RemoveBuff(buff);
                        localBuffCache.Remove(instanceId);
                        
                        if (BuffSystemConfig.Instance.EnableDebugLog)
                        {
                            Debug.Log($"[BuffNetcodeSync] Client: 同步移除Buff {buff.Name}");
                        }
                        break;
                    }
                }
            }
        }

        private void UpdateBuffFromSyncData(BuffSyncData syncData)
        {
            if (!localBuffCache.TryGetValue(syncData.InstanceId, out var oldData))
                return;
            
            // 查找对应的Buff
            foreach (var buff in buffOwner.BuffContainer.AllBuffs)
            {
                if (buff.DataId != syncData.BuffId) continue;
                
                // 更新层数
                if (syncData.Stack != oldData.Stack)
                {
                    int diff = syncData.Stack - buff.CurrentStack;
                    if (diff > 0)
                    {
                        buff.AddStack(diff);
                    }
                    else if (diff < 0)
                    {
                        buff.RemoveStack(-diff);
                    }
                }
                
                // 更新缓存
                localBuffCache[syncData.InstanceId] = syncData;
                break;
            }
        }

        private void ClearAllBuffs()
        {
            buffOwner.BuffContainer.ClearAllBuffs();
            localBuffCache.Clear();
        }

        #endregion

        #region Server RPC (Client to Server)

        /// <summary>
        /// 客户端请求添加Buff（服务器权威）
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void RequestAddBuffServerRpc(int buffId, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            
            var data = BuffDatabase.Instance?.GetBuffData(buffId);
            if (data == null)
            {
                Debug.LogWarning($"[BuffNetcodeSync] Server: 找不到Buff数据 ID: {buffId}");
                return;
            }
            
            // 服务器添加Buff（会触发事件同步到所有客户端）
            buffOwner.BuffContainer.AddBuff(data, null);
            
            if (BuffSystemConfig.Instance.EnableDebugLog)
            {
                Debug.Log($"[BuffNetcodeSync] Server: 响应客户端请求添加Buff ID: {buffId}");
            }
        }

        /// <summary>
        /// 客户端请求移除Buff
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void RequestRemoveBuffServerRpc(int buffId, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            
            buffOwner.BuffContainer.RemoveBuff(buffId);
        }

        #endregion

        #region Utility

        /// <summary>
        /// 压缩持续时间（0.1秒精度，最大6553秒）
        /// </summary>
        private static ushort CompressDuration(float duration)
        {
            return (ushort)Mathf.Clamp(duration * 10f, 0, ushort.MaxValue);
        }

        /// <summary>
        /// 解压缩持续时间
        /// </summary>
        private static float DecompressDuration(ushort compressed)
        {
            return compressed / 10f;
        }

        #endregion
    }

    /// <summary>
    /// Buff同步数据结构（Netcode版本）
    /// 实现INetworkSerializable以优化序列化
    /// </summary>
    public struct BuffSyncData : INetworkSerializable
    {
        public int InstanceId;      // 4 bytes
        public ushort BuffId;       // 2 bytes
        public byte Stack;          // 1 byte
        public ushort Duration;     // 2 bytes
        // 总计: 9 bytes per buff

        public BuffSyncData(IBuff buff)
        {
            InstanceId = buff.InstanceId;
            BuffId = (ushort)Mathf.Clamp(buff.DataId, 0, ushort.MaxValue);
            Stack = (byte)Mathf.Clamp(buff.CurrentStack, 0, 255);
            Duration = CompressDuration(buff.Duration);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref InstanceId);
            serializer.SerializeValue(ref BuffId);
            serializer.SerializeValue(ref Stack);
            serializer.SerializeValue(ref Duration);
        }

        private static ushort CompressDuration(float duration)
        {
            return (ushort)Mathf.Clamp(duration * 10f, 0, ushort.MaxValue);
        }
    }
}
#endif
