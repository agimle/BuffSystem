using System;
using System.Collections.Generic;
using BuffSystem.Core;

namespace BuffSystem.Events
{
    #region Event Args
    
    /// <summary>
    /// Buff事件参数基类
    /// </summary>
    public class BuffEventArgs : EventArgs
    {
        /// <summary>
        /// 相关的Buff实例
        /// </summary>
        public IBuff Buff { get; }
        
        public BuffEventArgs(IBuff buff)
        {
            Buff = buff;
        }
    }
    
    /// <summary>
    /// Buff添加事件参数
    /// </summary>
    public class BuffAddedEventArgs : BuffEventArgs
    {
        public BuffAddedEventArgs(IBuff buff) : base(buff) { }
    }
    
    /// <summary>
    /// Buff移除事件参数
    /// </summary>
    public class BuffRemovedEventArgs : BuffEventArgs
    {
        public BuffRemovedEventArgs(IBuff buff) : base(buff) { }
    }
    
    /// <summary>
    /// Buff层数变化事件参数
    /// </summary>
    public class BuffStackChangedEventArgs : BuffEventArgs
    {
        public int OldStack { get; }
        public int NewStack { get; }
        
        public BuffStackChangedEventArgs(IBuff buff, int oldStack, int newStack) : base(buff)
        {
            OldStack = oldStack;
            NewStack = newStack;
        }
    }
    
    /// <summary>
    /// Buff刷新事件参数
    /// </summary>
    public class BuffRefreshedEventArgs : BuffEventArgs
    {
        public BuffRefreshedEventArgs(IBuff buff) : base(buff) { }
    }
    
    /// <summary>
    /// Buff过期事件参数
    /// </summary>
    public class BuffExpiredEventArgs : BuffEventArgs
    {
        public BuffExpiredEventArgs(IBuff buff) : base(buff) { }
    }
    
    /// <summary>
    /// Buff清空事件参数
    /// </summary>
    public class BuffClearedEventArgs : EventArgs
    {
        public IBuffOwner Owner { get; }
        
        public BuffClearedEventArgs(IBuffOwner owner)
        {
            Owner = owner;
        }
    }
    
    #endregion
    
    #region Event System
    
    /// <summary>
    /// Buff全局事件系统
    /// 提供全局Buff事件监听
    /// </summary>
    public static class BuffEventSystem
    {
        #region Events
        
        /// <summary>
        /// Buff被添加时触发
        /// </summary>
        public static event EventHandler<BuffAddedEventArgs> OnBuffAdded;
        
        /// <summary>
        /// Buff被移除时触发
        /// </summary>
        public static event EventHandler<BuffRemovedEventArgs> OnBuffRemoved;
        
        /// <summary>
        /// Buff层数变化时触发
        /// </summary>
        public static event EventHandler<BuffStackChangedEventArgs> OnStackChanged;
        
        /// <summary>
        /// Buff刷新时触发
        /// </summary>
        public static event EventHandler<BuffRefreshedEventArgs> OnBuffRefreshed;
        
        /// <summary>
        /// Buff过期时触发
        /// </summary>
        public static event EventHandler<BuffExpiredEventArgs> OnBuffExpired;
        
        /// <summary>
        /// Buff被清空时触发
        /// </summary>
        public static event EventHandler<BuffClearedEventArgs> OnBuffCleared;
        
        #endregion
        
        #region Trigger Methods
        
        internal static void TriggerBuffAdded(IBuff buff)
        {
            OnBuffAdded?.Invoke(null, new BuffAddedEventArgs(buff));
        }
        
        internal static void TriggerBuffRemoved(IBuff buff)
        {
            OnBuffRemoved?.Invoke(null, new BuffRemovedEventArgs(buff));
        }
        
        internal static void TriggerStackChanged(IBuff buff, int oldStack, int newStack)
        {
            OnStackChanged?.Invoke(null, new BuffStackChangedEventArgs(buff, oldStack, newStack));
        }
        
        internal static void TriggerRefreshed(IBuff buff)
        {
            OnBuffRefreshed?.Invoke(null, new BuffRefreshedEventArgs(buff));
        }
        
        internal static void TriggerExpired(IBuff buff)
        {
            OnBuffExpired?.Invoke(null, new BuffExpiredEventArgs(buff));
        }
        
        internal static void TriggerCleared(IBuffOwner owner)
        {
            OnBuffCleared?.Invoke(null, new BuffClearedEventArgs(owner));
        }
        
        #endregion
    }
    
    #endregion
    
    #region Local Event System
    
    /// <summary>
    /// Buff持有者本地事件系统
    /// 每个持有者独立的Buff事件
    /// </summary>
    public class BuffLocalEventSystem
    {
        private readonly IBuffOwner _owner;
        
        public event EventHandler<BuffAddedEventArgs> OnBuffAdded;
        public event EventHandler<BuffRemovedEventArgs> OnBuffRemoved;
        public event EventHandler<BuffStackChangedEventArgs> OnStackChanged;
        public event EventHandler<BuffRefreshedEventArgs> OnBuffRefreshed;
        public event EventHandler<BuffExpiredEventArgs> OnBuffExpired;
        public event EventHandler OnBuffCleared;
        
        public BuffLocalEventSystem(IBuffOwner owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
        
        internal void TriggerBuffAdded(IBuff buff)
        {
            OnBuffAdded?.Invoke(_owner, new BuffAddedEventArgs(buff));
        }
        
        internal void TriggerBuffRemoved(IBuff buff)
        {
            OnBuffRemoved?.Invoke(_owner, new BuffRemovedEventArgs(buff));
        }
        
        internal void TriggerStackChanged(IBuff buff, int oldStack, int newStack)
        {
            OnStackChanged?.Invoke(_owner, new BuffStackChangedEventArgs(buff, oldStack, newStack));
        }
        
        internal void TriggerRefreshed(IBuff buff)
        {
            OnBuffRefreshed?.Invoke(_owner, new BuffRefreshedEventArgs(buff));
        }
        
        internal void TriggerExpired(IBuff buff)
        {
            OnBuffExpired?.Invoke(_owner, new BuffExpiredEventArgs(buff));
        }
        
        internal void TriggerCleared()
        {
            OnBuffCleared?.Invoke(_owner, EventArgs.Empty);
        }
    }
    
    #endregion
}
