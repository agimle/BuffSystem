#if MIRROR
using System;
using System.Collections.Generic;
using Mirror;
using BuffSystem.Core;
using BuffSystem.Runtime;
using UnityEngine;

namespace BuffSystem.Networking.Mirror
{
    /// <summary>
    /// Mirror网络同步组件
    /// 需要Mirror框架支持，定义MIRROR符号后启用
    /// </summary>
    [RequireComponent(typeof(BuffOwner))]
    public class BuffMirrorSync : NetworkBehaviour
    {
        private BuffOwner buffOwner;
        
        // 同步列表 - 服务器权威
        private readonly SyncList<BuffSyncData> syncedBuffs = new();
        
        // 本地Buff缓存（用于对比变更）
        private readonly Dictionary<int, BuffSyncData> localBuffCache = new();
        
        // 等待服务器确认的本地添加（预测）
        private readonly HashSet<int> pendingAdds = new();
        
        private void Awake()
        {
            buffOwner = GetComponent<BuffOwner>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            // 订阅Buff事件
            if (buffOwner != null)
            {
                buffOwner.LocalEvents.OnBuffAdded += OnServerBuffAdded;
                buffOwner.LocalEvents.OnBuffRemoved += OnServerBuffRemoved;
                buffOwner.LocalEvents.OnStackChanged += OnServerBuffStackChanged;
                buffOwner.LocalEvents.OnBuffRefreshed += OnServerBuffRefreshed;
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            
            if (buffOwner != null)
            {
                buffOwner.LocalEvents.OnBuffAdded -= OnServerBuffAdded;
                buffOwner.LocalEvents.OnBuffRemoved -= OnServerBuffRemoved;
                buffOwner.LocalEvents.OnStackChanged -= OnServerBuffStackChanged;
                buffOwner.LocalEvents.OnBuffRefreshed -= OnServerBuffRefreshed;
            }
        }

        #region Server Side

        [Server]
        private void OnServerBuffAdded(object sender, BuffAddedEventArgs e)
        {
            if (e.Buff == null) return;
            
            var syncData = new BuffSyncData(e.Buff);
            syncedBuffs.Add(syncData);
            
            if (BuffSystemConfig.Instance.EnableDebugLog)
            {
                Debug.Log($"[BuffMirrorSync] Server: 添加Buff {e.Buff.Name} (ID: {e.Buff.InstanceId})");
            }
        }

        [Server]
        private void OnServerBuffRemoved(object sender, BuffRemovedEventArgs e)
        {
            if (e.Buff == null) return;
            
            for (int i = 0; i < syncedBuffs.Count; i++)
            {
                if (syncedBuffs[i].InstanceId == e.Buff.InstanceId)
                {
                    syncedBuffs.RemoveAt(i);
                    
                    if (BuffSystemConfig.Instance.EnableDebugLog)
                    {
                        Debug.Log($"[BuffMirrorSync] Server: 移除Buff {e.Buff.Name} (ID: {e.Buff.InstanceId})");
                    }
                    break;
                }
            }
        }

        [Server]
        private void OnServerBuffStackChanged(object sender, BuffStackChangedEventArgs e)
        {
            if (e.Buff == null) return;
            
            for (int i = 0; i < syncedBuffs.Count; i++)
            {
                if (syncedBuffs[i].InstanceId == e.Buff.InstanceId)
                {
                    var updated = syncedBuffs[i];
                    updated.Stack = (byte)Mathf.Clamp(e.NewStack, 0, 255);
                    syncedBuffs[i] = updated;
                    
                    if (BuffSystemConfig.Instance.EnableDebugLog)
                    {
                        Debug.Log($"[BuffMirrorSync] Server: Buff {e.Buff.Name} 层数变化 {e.OldStack} -> {e.NewStack}");
                    }
                    break;
                }
            }
        }

        [Server]
        private void OnServerBuffRefreshed(object sender, BuffRefreshedEventArgs e)
        {
            if (e.Buff == null) return;
            
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

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            syncedBuffs.Callback += OnSyncedBuffsChanged;
            
            // 初始同步
            if (!isServer)
            {
                foreach (var syncData in syncedBuffs)
                {
                    ApplySyncData(syncData);
                }
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            syncedBuffs.Callback -= OnSyncedBuffsChanged;
        }

        private void OnSyncedBuffsChanged(SyncList<BuffSyncData>.Operation op, int index, BuffSyncData item)
        {
            if (isServer) return; // 服务器已处理
            
            switch (op)
            {
                case SyncList<BuffSyncData>.Operation.OP_ADD:
                    ApplySyncData(item);
                    break;
                case SyncList<BuffSyncData>.Operation.OP_REMOVE:
                    RemoveBuffByInstanceId(item.InstanceId);
                    break;
                case SyncList<BuffSyncData>.Operation.OP_SET:
                    UpdateBuffFromSyncData(item);
                    break;
                case SyncList<BuffSyncData>.Operation.OP_CLEAR:
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
                Debug.LogWarning($"[BuffMirrorSync] 找不到Buff数据 ID: {syncData.BuffId}");
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
                    Debug.Log($"[BuffMirrorSync] Client: 同步添加Buff {data.Name} (层数: {targetStack})");
                }
            }
        }

        private void RemoveBuffByInstanceId(int instanceId)
        {
            // 从缓存中查找对应的Buff
            foreach (var buff in buffOwner.BuffContainer.AllBuffs)
            {
                // 注意：客户端创建的Buff会有不同的InstanceId
                // 这里需要通过其他方式匹配，比如BuffId和Source的组合
                if (localBuffCache.TryGetValue(instanceId, out var cachedData))
                {
                    if (buff.DataId == cachedData.BuffId)
                    {
                        buffOwner.BuffContainer.RemoveBuff(buff);
                        localBuffCache.Remove(instanceId);
                        
                        if (BuffSystemConfig.Instance.EnableDebugLog)
                        {
                            Debug.Log($"[BuffMirrorSync] Client: 同步移除Buff {buff.Name}");
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

        #region Client Prediction (Optional)

        /// <summary>
        /// 客户端预测添加Buff（需要服务器确认）
        /// </summary>
        [Client]
        public void PredictAddBuff(int buffId)
        {
            if (isServer) return;
            
            var data = BuffDatabase.Instance?.GetBuffData(buffId);
            if (data == null) return;
            
            // 生成临时InstanceId（负数表示预测）
            int tempId = -UnityEngine.Random.Range(1, int.MaxValue);
            pendingAdds.Add(tempId);
            
            // 添加Buff（会被服务器状态覆盖）
            buffOwner.BuffContainer.AddBuff(data, null);
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
    /// Buff同步数据结构（优化网络传输大小）
    /// </summary>
    [System.Serializable]
    public struct BuffSyncData
    {
        public int InstanceId;      // 4 bytes - Buff实例唯一ID
        public ushort BuffId;       // 2 bytes - Buff配置ID
        public byte Stack;          // 1 byte  - 当前层数 (0-255)
        public ushort Duration;     // 2 bytes - 持续时间（压缩）
        // 总计: 9 bytes per buff

        public BuffSyncData(IBuff buff)
        {
            InstanceId = buff.InstanceId;
            BuffId = (ushort)Mathf.Clamp(buff.DataId, 0, ushort.MaxValue);
            Stack = (byte)Mathf.Clamp(buff.CurrentStack, 0, 255);
            Duration = CompressDuration(buff.Duration);
        }

        private static ushort CompressDuration(float duration)
        {
            return (ushort)Mathf.Clamp(duration * 10f, 0, ushort.MaxValue);
        }
    }
}
#endif
