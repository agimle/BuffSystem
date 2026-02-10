using System;
using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Events;
using BuffSystem.Utils;

namespace BuffSystem.Runtime
{
    /// <summary>
    /// Buff容器 - 管理持有者的所有Buff
    /// </summary>
    public class BuffContainer : IBuffContainer
    {
        // Buff存储
        private readonly Dictionary<int, BuffEntity> buffByInstanceId = new();
        private readonly Dictionary<int, List<BuffEntity>> buffsByDataId = new();
        private readonly Dictionary<object, List<BuffEntity>> buffsBySource = new();

        // 待移除队列
        private readonly Queue<int> removalQueue = new();

        // 对象池
        private readonly ObjectPool<BuffEntity> buffPool;

        // Buff列表缓存（避免GC）
        private readonly List<IBuff> buffCache = new();
        private IReadOnlyCollection<IBuff> allBuffsReadOnly;
        private bool isAllBuffsCacheValid;

        // 空集合缓存（避免GC）
        private static readonly List<IBuff> EmptyBuffList = new();
        private static readonly List<BuffEntity> EmptyBuffEntityList = new();

        // 所属持有者
        public IBuffOwner Owner { get; }

        /// <summary>
        /// 所有Buff（只读）
        /// </summary>
        public IReadOnlyCollection<IBuff> AllBuffs
        {
            get
            {
                if (!isAllBuffsCacheValid)
                {
                    buffCache.Clear();
                    foreach (var buff in buffByInstanceId.Values)
                    {
                        buffCache.Add(buff);
                    }
                    allBuffsReadOnly = buffCache.AsReadOnly();
                    isAllBuffsCacheValid = true;
                }
                return allBuffsReadOnly;
            }
        }

        /// <summary>
        /// 使AllBuffs缓存失效
        /// </summary>
        private void InvalidateAllBuffsCache()
        {
            isAllBuffsCacheValid = false;
        }
        
        /// <summary>
        /// 当前Buff数量
        /// </summary>
        public int Count => buffByInstanceId.Count;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public BuffContainer(IBuffOwner owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            
            var config = Data.BuffSystemConfig.Instance;
            buffPool = new ObjectPool<BuffEntity>(
                createFunc: CreateBuffEntity,
                actionOnGet: null,
                actionOnRelease: ReleaseBuffEntity,
                defaultCapacity: config.DefaultPoolCapacity,
                maxSize: config.MaxPoolSize
            );
        }
        
        #region Buff Management
        
        /// <summary>
        /// 添加Buff
        /// </summary>
        public IBuff AddBuff(IBuffData data, object source = null)
        {
            if (data == null)
            {
                Debug.LogError("[BuffContainer] 尝试添加空的Buff数据");
                return null;
            }
            
            // 处理唯一性
            if (data.IsUnique)
            {
                var existingBuff = GetUniqueBuff(data.Id);
                if (existingBuff != null)
                {
                    // 已存在，执行叠加或刷新逻辑
                    return HandleExistingBuff(existingBuff, data, source);
                }
            }
            
            // 创建新Buff
            return CreateNewBuff(data, source);
        }
        
        /// <summary>
        /// 获取唯一Buff（用于IsUnique的Buff）
        /// </summary>
        private BuffEntity GetUniqueBuff(int dataId)
        {
            if (buffsByDataId.TryGetValue(dataId, out var buffs) && buffs.Count > 0)
            {
                return buffs[0];
            }
            return null;
        }
        
        /// <summary>
        /// 处理已存在的Buff
        /// </summary>
        private IBuff HandleExistingBuff(BuffEntity existingBuff, IBuffData data, object source)
        {
            switch (data.StackMode)
            {
                case BuffStackMode.Stackable:
                    // 可叠加 - 增加层数
                    existingBuff.AddStack(data.AddStackCount);
                    break;
                    
                case BuffStackMode.None:
                    // 不可叠加 - 刷新持续时间
                    if (data.CanRefresh)
                    {
                        existingBuff.RefreshDuration();
                    }
                    break;
                    
                case BuffStackMode.Independent:
                    // 独立模式 - 创建新实例（忽略唯一性）
                    return CreateNewBuff(data, source);
            }
            
            return existingBuff;
        }
        
        /// <summary>
        /// 创建新Buff实例
        /// </summary>
        private IBuff CreateNewBuff(IBuffData data, object source)
        {
            // 从对象池获取
            BuffEntity buff = buffPool.Get();
            buff.Reset(data, Owner, source);
            
            // 存储
            buffByInstanceId[buff.InstanceId] = buff;
            
            if (!buffsByDataId.TryGetValue(data.Id, out var dataIdList))
            {
                dataIdList = new List<BuffEntity>();
                buffsByDataId[data.Id] = dataIdList;
            }
            dataIdList.Add(buff);
            
            if (source != null)
            {
                if (!buffsBySource.TryGetValue(source, out var sourceList))
                {
                    sourceList = new List<BuffEntity>();
                    buffsBySource[source] = sourceList;
                }
                sourceList.Add(buff);
            }
            
            // 触发获得事件
            if (buff.Data.CreateLogic() is IBuffAcquire acquireLogic)
            {
                acquireLogic.OnAcquire();
            }
            
            // 触发全局事件
            BuffEventSystem.TriggerBuffAdded(buff);
            Owner.OnBuffEvent(BuffEventType.Added, buff);

            // 使缓存失效
            InvalidateAllBuffsCache();

            if (Data.BuffSystemConfig.Instance.EnableDebugLog)
            {
                Debug.Log($"[BuffContainer] 添加Buff: {buff}");
            }

            return buff;
        }
        
        /// <summary>
        /// 移除Buff
        /// </summary>
        public void RemoveBuff(IBuff buff)
        {
            if (buff is BuffEntity entity)
            {
                entity.MarkForRemoval();
                removalQueue.Enqueue(entity.InstanceId);
            }
        }
        
        /// <summary>
        /// 根据ID移除Buff
        /// </summary>
        public void RemoveBuff(int dataId)
        {
            if (buffsByDataId.TryGetValue(dataId, out var buffs))
            {
                // 使用倒序遍历避免修改集合时的问题
                for (int i = buffs.Count - 1; i >= 0; i--)
                {
                    RemoveBuff(buffs[i]);
                }
            }
        }
        
        /// <summary>
        /// 根据来源移除Buff
        /// </summary>
        public void RemoveBuffBySource(object source)
        {
            if (source == null) return;

            if (buffsBySource.TryGetValue(source, out var buffs))
            {
                // 使用倒序遍历避免修改集合时的问题
                for (int i = buffs.Count - 1; i >= 0; i--)
                {
                    RemoveBuff(buffs[i]);
                }
            }
        }
        
        /// <summary>
        /// 清空所有Buff
        /// </summary>
        public void ClearAllBuffs()
        {
            // 将Values复制到临时列表避免修改集合时的问题
            var tempList = EmptyBuffEntityList;
            tempList.AddRange(buffByInstanceId.Values);

            for (int i = 0; i < tempList.Count; i++)
            {
                RemoveBuff(tempList[i]);
            }
            tempList.Clear();

            // 立即处理移除队列
            ProcessRemovalQueue();

            Owner.OnBuffEvent(BuffEventType.Cleared, null);
        }
        
        #endregion
        
        #region Query Methods
        
        /// <summary>
        /// 获取Buff
        /// </summary>
        public IBuff GetBuff(int dataId, object source = null)
        {
            if (buffsByDataId.TryGetValue(dataId, out var buffs))
            {
                if (source == null)
                {
                    // 替代FirstOrDefault()
                    return buffs.Count > 0 ? buffs[0] : null;
                }

                // 替代FirstOrDefault(predicate)
                for (int i = 0; i < buffs.Count; i++)
                {
                    if (buffs[i].Source == source)
                    {
                        return buffs[i];
                    }
                }
            }
            return null;
        }
        
        /// <summary>
        /// 获取所有指定ID的Buff
        /// </summary>
        public IEnumerable<IBuff> GetBuffs(int dataId)
        {
            if (buffsByDataId.TryGetValue(dataId, out var buffs))
            {
                return buffs;
            }
            return EmptyBuffList;
        }
        
        /// <summary>
        /// 获取所有指定来源的Buff
        /// </summary>
        public IEnumerable<IBuff> GetBuffsBySource(object source)
        {
            if (source != null && buffsBySource.TryGetValue(source, out var buffs))
            {
                return buffs;
            }
            return EmptyBuffList;
        }
        
        /// <summary>
        /// 是否拥有指定Buff
        /// </summary>
        public bool HasBuff(int dataId)
        {
            return buffsByDataId.ContainsKey(dataId) && buffsByDataId[dataId].Count > 0;
        }
        
        /// <summary>
        /// 是否拥有指定来源的Buff
        /// </summary>
        public bool HasBuff(int dataId, object source)
        {
            if (buffsByDataId.TryGetValue(dataId, out var buffs))
            {
                // 替代Any(predicate)
                for (int i = 0; i < buffs.Count; i++)
                {
                    if (buffs[i].Source == source)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        #endregion
        
        #region Update
        
        /// <summary>
        /// 每帧更新
        /// </summary>
        public void Update(float deltaTime)
        {
            // 更新所有Buff
            foreach (var buff in buffByInstanceId.Values)
            {
                buff.Update(deltaTime);
                
                if (buff.IsMarkedForRemoval && !removalQueue.Contains(buff.InstanceId))
                {
                    removalQueue.Enqueue(buff.InstanceId);
                }
            }
            
            // 处理移除队列
            ProcessRemovalQueue();
        }
        
        /// <summary>
        /// 处理移除队列
        /// </summary>
        private void ProcessRemovalQueue()
        {
            while (removalQueue.Count > 0)
            {
                int instanceId = removalQueue.Dequeue();
                
                if (!buffByInstanceId.TryGetValue(instanceId, out var buff))
                {
                    continue;
                }
                
                // 从存储中移除
                buffByInstanceId.Remove(instanceId);
                
                if (buffsByDataId.TryGetValue(buff.DataId, out var dataIdList))
                {
                    dataIdList.Remove(buff);
                    if (dataIdList.Count == 0)
                    {
                        buffsByDataId.Remove(buff.DataId);
                    }
                }
                
                if (buff.Source != null && buffsBySource.TryGetValue(buff.Source, out var sourceList))
                {
                    sourceList.Remove(buff);
                    if (sourceList.Count == 0)
                    {
                        buffsBySource.Remove(buff.Source);
                    }
                }
                
                // 触发事件
                BuffEventSystem.TriggerBuffRemoved(buff);
                Owner.OnBuffEvent(BuffEventType.Removed, buff);
                
                // 清理并归还对象池
                buff.Cleanup();
                buffPool.Release(buff);

                if (Data.BuffSystemConfig.Instance.EnableDebugLog)
                {
                    Debug.Log($"[BuffContainer] 移除Buff: {buff.Name}");
                }
            }

            // 使缓存失效
            if (removalQueue.Count == 0)
            {
                InvalidateAllBuffsCache();
            }
        }
        
        #endregion
        
        #region Object Pool Callbacks
        
        private BuffEntity CreateBuffEntity()
        {
            return new BuffEntity();
        }
        
        private void ReleaseBuffEntity(BuffEntity buff)
        {
            // 清理工作已在Cleanup中完成
        }
        
        #endregion
        
        #region Prewarm
        
        /// <summary>
        /// 预热对象池，预先创建指定数量的对象
        /// </summary>
        /// <param name="count">预热数量</param>
        public void Prewarm(int count)
        {
            if (count <= 0) return;
            
            var tempList = new List<BuffEntity>(count);
            
            // 预先创建对象
            for (int i = 0; i < count; i++)
            {
                tempList.Add(buffPool.Get());
            }
            
            // 立即归还到池中
            foreach (var buff in tempList)
            {
                buffPool.Release(buff);
            }
            
            if (Data.BuffSystemConfig.Instance.EnableDebugLog)
            {
                Debug.Log($"[BuffContainer] 对象池预热完成，预分配 {count} 个对象，当前池大小: {buffPool.CountAll}");
            }
        }
        
        /// <summary>
        /// 获取对象池状态信息
        /// </summary>
        public (int total, int active, int inactive) GetPoolStatus()
        {
            return (buffPool.CountAll, buffPool.CountActive, buffPool.CountInactive);
        }
        
        #endregion
    }
}
