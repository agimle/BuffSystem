using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Data;
using BuffSystem.Events;
using BuffSystem.Strategy;

namespace BuffSystem.Runtime
{
    /// <summary>
    /// Buff实体 - 运行时的Buff实例
    /// 使用对象池复用
    /// </summary>
    /// <remarks>
    /// 🔒 稳定API: v6.0后保证向后兼容
    /// 版本历史: v1.0-v6.0 逐步完善
    /// 修改策略: 只允许bug修复，不允许破坏性变更
    /// </remarks>
    public class BuffEntity : IBuff
    {
        private static int globalInstanceId;
        
        // 基础信息
        private int instanceId;
        private IBuffData data;
        private IBuffOwner owner;
        private object source;
        private int sourceId;
        
        // 运行时数据
        private int currentStack;
        private float duration;
        private float removeIntervalTimer;
        private bool isMarkedForRemoval;
        
        // 逻辑实例
        private IBuffLogic logic;
        
        // 移除策略缓存
        private static readonly Dictionary<BuffRemoveMode, IRemoveStrategy> removeStrategies = new()
        {
            [BuffRemoveMode.Remove] = new DirectRemoveStrategy(),
            [BuffRemoveMode.Reduce] = new ReduceStackStrategy()
        };
        
        #region IBuff Implementation
        
        public int InstanceId => instanceId;
        public int DataId => data?.Id ?? -1;
        public string Name => data?.Name ?? "Unknown";
        public int CurrentStack => currentStack;
        public int MaxStack => data?.MaxStack ?? 1;
        public float Duration => duration;
        public float TotalDuration => data?.Duration ?? 0f;
        public float RemainingTime => IsPermanent ? float.MaxValue : Mathf.Max(0, TotalDuration - duration);
        public bool IsPermanent => data?.IsPermanent ?? false;
        public bool IsMarkedForRemoval => isMarkedForRemoval;
        public bool IsActive => !isMarkedForRemoval && currentStack > 0;
        public object Source => source;
        public IBuffOwner Owner => owner;
        public IBuffData Data => data;
        
        public T GetSource<T>() where T : class
        {
            return source as T;
        }
        
        #endregion
        
        /// <summary>
        /// 构造函数（对象池使用）
        /// </summary>
        public BuffEntity()
        {
            instanceId = ++globalInstanceId;
        }
        
        /// <summary>
        /// 重置Buff（对象池回收后重新初始化）
        /// </summary>
        public void Reset(IBuffData newData, IBuffOwner newOwner, object newSource)
        {
            data = newData ?? throw new ArgumentNullException(nameof(newData));
            owner = newOwner ?? throw new ArgumentNullException(nameof(newOwner));
            source = newSource;

            // 计算SourceId（使用RuntimeHelpers.GetHashCode确保稳定性）
            sourceId = source != null ? RuntimeHelpers.GetHashCode(source) : 0;

            currentStack = 0;
            duration = 0f;
            removeIntervalTimer = 0f;
            isMarkedForRemoval = false;

            // 创建逻辑实例
            logic = data.CreateLogic();
            logic?.Initialize(this);

            // 初始层数
            AddStack(data.AddStackCount);

            // 触发开始事件
            if (logic is IBuffStart startLogic)
            {
                startLogic.OnStart();
            }
        }
        
        /// <summary>
        /// 清理Buff（归还对象池前调用）
        /// </summary>
        internal void Cleanup()
        {
            // 触发结束事件
            if (logic is IBuffEnd endLogic)
            {
                endLogic.OnEnd();
            }
            
            logic?.Dispose();
            logic = null;
            data = null;
            owner = null;
            source = null;
        }
        
        /// <summary>
        /// 增加层数
        /// </summary>
        public void AddStack(int amount)
        {
            if (amount <= 0) return;
            
            int oldStack = currentStack;
            currentStack = Mathf.Min(currentStack + amount, MaxStack);
            
            if (currentStack != oldStack)
            {
                // 触发层数变化事件
                if (logic is IBuffStackChange stackChangeLogic)
                {
                    stackChangeLogic.OnStackChanged(oldStack, currentStack);
                }
                
                // 触发全局事件
                BuffEventSystem.TriggerStackChanged(this, oldStack, currentStack);
                
                // 触发持有者本地事件
                owner?.LocalEvents?.TriggerStackChanged(this, oldStack, currentStack);
            }
        }
        
        /// <summary>
        /// 减少层数
        /// </summary>
        public void RemoveStack(int amount)
        {
            if (amount <= 0) return;
            
            int oldStack = currentStack;
            currentStack = Mathf.Max(currentStack - amount, 0);
            
            if (currentStack != oldStack)
            {
                // 触发消层事件
                if (logic is IBuffReduce reduceLogic)
                {
                    reduceLogic.OnReduce();
                }
                
                // 触发层数变化事件
                if (logic is IBuffStackChange stackChangeLogic)
                {
                    stackChangeLogic.OnStackChanged(oldStack, currentStack);
                }
                
                // 触发全局事件
                BuffEventSystem.TriggerStackChanged(this, oldStack, currentStack);
                
                // 触发持有者本地事件
                owner?.LocalEvents?.TriggerStackChanged(this, oldStack, currentStack);
            }
            
            // 层数为0时标记移除
            if (currentStack <= 0)
            {
                MarkForRemoval();
            }
        }
        
        /// <summary>
        /// 刷新持续时间
        /// </summary>
        public void RefreshDuration()
        {
            if (!CanRefresh) return;
            
            float oldDuration = duration;
            duration = 0f;
            removeIntervalTimer = 0f;
            
            // 触发刷新事件
            if (logic is IBuffRefresh refreshLogic)
            {
                refreshLogic.OnRefresh();
            }
            
            // 触发持续时间变化事件
            if (logic is IBuffDurationChange durationChangeLogic)
            {
                durationChangeLogic.OnDurationChanged(oldDuration, duration);
            }
            
            // 触发全局事件
            BuffEventSystem.TriggerRefreshed(this);
            
            // 触发持有者本地事件
            owner?.LocalEvents?.TriggerRefreshed(this);
        }
        
        /// <summary>
        /// 标记为移除
        /// </summary>
        public void MarkForRemoval()
        {
            if (isMarkedForRemoval) return;
            
            isMarkedForRemoval = true;
            
            // 触发移除事件
            if (logic is IBuffRemove removeLogic)
            {
                removeLogic.OnRemove();
            }
        }
        
        /// <summary>
        /// 每帧更新
        /// </summary>
        internal void Update(float deltaTime)
        {
            if (isMarkedForRemoval) return;
            if (data == null) return;
            
            // 逻辑更新
            if (logic is IBuffLogicUpdate logicUpdate)
            {
                logicUpdate.OnLogicUpdate(deltaTime);
            }
            
            // 持续时间更新
            if (!IsPermanent)
            {
                UpdateDuration(deltaTime);
            }
            
            // 表现更新
            if (logic is IBuffVisualUpdate visualUpdate)
            {
                visualUpdate.OnVisualUpdate(deltaTime);
            }
        }
        
        /// <summary>
        /// 更新持续时间
        /// </summary>
        private void UpdateDuration(float deltaTime)
        {
            duration += deltaTime;
            
            if (duration >= TotalDuration)
            {
                // 处理消层或移除
                HandleExpiration(deltaTime);
            }
        }
        
        /// <summary>
        /// 处理Buff过期
        /// </summary>
        private void HandleExpiration(float deltaTime)
        {
            if (removeStrategies.TryGetValue(data.RemoveMode, out var strategy))
            {
                strategy.HandleExpiration(this, deltaTime, ref removeIntervalTimer);
            }
        }
        
        /// <summary>
        /// 是否可以刷新
        /// </summary>
        private bool CanRefresh => data?.CanRefresh ?? false;

        public int SourceId => sourceId;

        /// <summary>
        /// 设置持续时间（用于网络同步和存档恢复）
        /// </summary>
        /// <param name="newDuration">新的持续时间值</param>
        internal void SetDuration(float newDuration)
        {
            duration = Mathf.Max(0, newDuration);
            
            if (BuffSystemConfig.Instance.EnableDebugLog)
            {
                Debug.Log($"[BuffEntity] {Name} 持续时间设置为 {duration:F1}s");
            }
        }

        /// <summary>
        /// 从存档数据恢复Buff状态
        /// </summary>
        internal void RestoreState(BuffSaveData saveData)
        {
            if (saveData == null) return;

            // 恢复层数
            int targetStack = saveData.CurrentStack;
            if (targetStack > 0)
            {
                // 先重置层数
                currentStack = 0;
                // 再添加层数（触发事件）
                AddStack(targetStack);
            }

            // 恢复持续时间（已持续时间）
            duration = saveData.ElapsedDuration;

            if (BuffSystemConfig.Instance.EnableDebugLog)
            {
                Debug.Log($"[BuffEntity] 恢复状态: {Name}, 层数: {CurrentStack}, 持续时间: {duration:F1}s");
            }
        }

        public override string ToString()
        {
            return $"Buff[{InstanceId}] {Name} (Stack: {CurrentStack}/{MaxStack}, Time: {RemainingTime:F1}s)";
        }

        public bool TryGetSource<T>(out T sourceOut) where T : class
        {
            sourceOut = source as T;
            return sourceOut != null;
        }
    }
}
