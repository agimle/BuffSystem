using System;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Events;

namespace BuffSystem.Runtime
{
    /// <summary>
    /// Buff实体 - 运行时的Buff实例
    /// 使用对象池复用
    /// </summary>
    public class BuffEntity : IBuff
    {
        private static int _globalInstanceId;
        
        // 基础信息
        private int _instanceId;
        private IBuffData _data;
        private IBuffOwner _owner;
        private object _source;
        
        // 运行时数据
        private int _currentStack;
        private float _duration;
        private float _removeIntervalTimer;
        private bool _isMarkedForRemoval;
        
        // 逻辑实例
        private IBuffLogic _logic;
        
        #region IBuff Implementation
        
        public int InstanceId => _instanceId;
        public int DataId => _data?.Id ?? -1;
        public string Name => _data?.Name ?? "Unknown";
        public int CurrentStack => _currentStack;
        public int MaxStack => _data?.MaxStack ?? 1;
        public float Duration => _duration;
        public float TotalDuration => _data?.Duration ?? 0f;
        public float RemainingTime => IsPermanent ? float.MaxValue : Mathf.Max(0, TotalDuration - _duration);
        public bool IsPermanent => _data?.IsPermanent ?? false;
        public bool IsMarkedForRemoval => _isMarkedForRemoval;
        public object Source => _source;
        public IBuffOwner Owner => _owner;
        public IBuffData Data => _data;
        
        public T GetSource<T>() where T : class
        {
            return _source as T;
        }
        
        #endregion
        
        /// <summary>
        /// 构造函数（对象池使用）
        /// </summary>
        public BuffEntity()
        {
            _instanceId = ++_globalInstanceId;
        }
        
        /// <summary>
        /// 重置Buff（对象池回收后重新初始化）
        /// </summary>
        public void Reset(IBuffData data, IBuffOwner owner, object source)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _source = source;
            
            _currentStack = 0;
            _duration = 0f;
            _removeIntervalTimer = 0f;
            _isMarkedForRemoval = false;
            
            // 创建逻辑实例
            _logic = data.CreateLogic();
            _logic?.Initialize(this);
            
            // 初始层数
            AddStack(data.AddStackCount);
            
            // 触发开始事件
            if (_logic is IBuffStart startLogic)
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
            if (_logic is IBuffEnd endLogic)
            {
                endLogic.OnEnd();
            }
            
            _logic?.Dispose();
            _logic = null;
            _data = null;
            _owner = null;
            _source = null;
        }
        
        /// <summary>
        /// 增加层数
        /// </summary>
        public void AddStack(int amount)
        {
            if (amount <= 0) return;
            
            int oldStack = _currentStack;
            _currentStack = Mathf.Min(_currentStack + amount, MaxStack);
            
            if (_currentStack != oldStack)
            {
                // 触发层数变化事件
                if (_logic is IBuffStackChange stackChangeLogic)
                {
                    stackChangeLogic.OnStackChanged(oldStack, _currentStack);
                }
                
                BuffEventSystem.TriggerStackChanged(this, oldStack, _currentStack);
            }
        }
        
        /// <summary>
        /// 减少层数
        /// </summary>
        public void RemoveStack(int amount)
        {
            if (amount <= 0) return;
            
            int oldStack = _currentStack;
            _currentStack = Mathf.Max(_currentStack - amount, 0);
            
            if (_currentStack != oldStack)
            {
                // 触发消层事件
                if (_logic is IBuffReduce reduceLogic)
                {
                    reduceLogic.OnReduce();
                }
                
                // 触发层数变化事件
                if (_logic is IBuffStackChange stackChangeLogic)
                {
                    stackChangeLogic.OnStackChanged(oldStack, _currentStack);
                }
                
                BuffEventSystem.TriggerStackChanged(this, oldStack, _currentStack);
            }
            
            // 层数为0时标记移除
            if (_currentStack <= 0)
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
            
            float oldDuration = _duration;
            _duration = 0f;
            _removeIntervalTimer = 0f;
            
            // 触发刷新事件
            if (_logic is IBuffRefresh refreshLogic)
            {
                refreshLogic.OnRefresh();
            }
            
            // 触发持续时间变化事件
            if (_logic is IBuffDurationChange durationChangeLogic)
            {
                durationChangeLogic.OnDurationChanged(oldDuration, _duration);
            }
            
            BuffEventSystem.TriggerRefreshed(this);
        }
        
        /// <summary>
        /// 标记为移除
        /// </summary>
        public void MarkForRemoval()
        {
            if (_isMarkedForRemoval) return;
            
            _isMarkedForRemoval = true;
            
            // 触发移除事件
            if (_logic is IBuffRemove removeLogic)
            {
                removeLogic.OnRemove();
            }
        }
        
        /// <summary>
        /// 每帧更新
        /// </summary>
        internal void Update(float deltaTime)
        {
            if (_isMarkedForRemoval) return;
            if (_data == null) return;
            
            // 逻辑更新
            if (_logic is IBuffLogicUpdate logicUpdate)
            {
                logicUpdate.OnLogicUpdate(deltaTime);
            }
            
            // 持续时间更新
            if (!IsPermanent)
            {
                UpdateDuration(deltaTime);
            }
            
            // 表现更新
            if (_logic is IBuffVisualUpdate visualUpdate)
            {
                visualUpdate.OnVisualUpdate(deltaTime);
            }
        }
        
        /// <summary>
        /// 更新持续时间
        /// </summary>
        private void UpdateDuration(float deltaTime)
        {
            _duration += deltaTime;
            
            if (_duration >= TotalDuration)
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
            if (_data.RemoveMode == BuffRemoveMode.Remove)
            {
                // 直接移除
                MarkForRemoval();
            }
            else
            {
                // 逐层移除
                _removeIntervalTimer += deltaTime;
                float interval = _data.RemoveInterval;
                
                if (interval <= 0) interval = 0.1f; // 防止除零
                
                while (_removeIntervalTimer >= interval && !_isMarkedForRemoval)
                {
                    _removeIntervalTimer -= interval;
                    RemoveStack(_data.RemoveStackCount);
                }
            }
        }
        
        /// <summary>
        /// 是否可以刷新
        /// </summary>
        private bool CanRefresh => _data?.CanRefresh ?? false;

        public int SourceId => _source?.GetHashCode() ?? 0;

        public override string ToString()
        {
            return $"Buff[{InstanceId}] {Name} (Stack: {CurrentStack}/{MaxStack}, Time: {RemainingTime:F1}s)";
        }

        public bool TryGetSource<T>(out T source) where T : class
        {
            source = _source as T;
            return source != null;
        }
    }
}
