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
    
    #region Event Queue Item
    
    /// <summary>
    /// 事件类型枚举
    /// </summary>
    internal enum EventQueueItemType
    {
        BuffAdded,
        BuffRemoved,
        StackChanged,
        Refreshed,
        Expired,
        Cleared
    }
    
    /// <summary>
    /// 事件队列项
    /// </summary>
    internal readonly struct EventQueueItem
    {
        public readonly EventQueueItemType Type;
        public readonly IBuff Buff;
        public readonly IBuffOwner Owner;
        public readonly int OldStack;
        public readonly int NewStack;
        
        public EventQueueItem(EventQueueItemType type, IBuff buff = null, IBuffOwner owner = null, int oldStack = 0, int newStack = 0)
        {
            Type = type;
            Buff = buff;
            Owner = owner;
            OldStack = oldStack;
            NewStack = newStack;
        }
    }
    
    #endregion
    
    #region Event System
    
    /// <summary>
    /// Buff全局事件系统
    /// 提供全局Buff事件监听（队列化实现，避免递归问题）
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
        
        #region Event Queue

        private static readonly Queue<EventQueueItem> eventQueue = new();
        private static bool isProcessingQueue = false;
        private static int maxEventsPerFrame = 1000; // 防止无限循环保护

        /// <summary>
        /// 当前队列中的事件数量
        /// </summary>
        public static int PendingEventCount => eventQueue.Count;

        /// <summary>
        /// 是否正在处理队列
        /// </summary>
        public static bool IsProcessingQueue => isProcessingQueue;

        #endregion

        #region Generic Event System (For TriggerEventEffect)

        // 通用事件回调存储
        private static readonly Dictionary<string, Delegate> genericEventHandlers = new();

        /// <summary>
        /// 订阅通用事件
        /// </summary>
        /// <typeparam name="T">事件数据类型</typeparam>
        /// <param name="eventName">事件名称</param>
        /// <param name="callback">回调函数</param>
        public static void On<T>(string eventName, Action<T> callback)
        {
            if (string.IsNullOrEmpty(eventName)) return;
            if (callback == null) return;

            if (!genericEventHandlers.TryGetValue(eventName, out var handlers))
            {
                handlers = null;
            }

            genericEventHandlers[eventName] = (handlers as Action<T>) + callback;
        }

        /// <summary>
        /// 取消订阅通用事件
        /// </summary>
        /// <typeparam name="T">事件数据类型</typeparam>
        /// <param name="eventName">事件名称</param>
        /// <param name="callback">回调函数</param>
        public static void Off<T>(string eventName, Action<T> callback)
        {
            if (string.IsNullOrEmpty(eventName)) return;
            if (callback == null) return;

            if (genericEventHandlers.TryGetValue(eventName, out var handlers))
            {
                var updatedHandlers = (handlers as Action<T>) - callback;
                if (updatedHandlers == null)
                {
                    genericEventHandlers.Remove(eventName);
                }
                else
                {
                    genericEventHandlers[eventName] = updatedHandlers;
                }
            }
        }

        /// <summary>
        /// 触发通用事件
        /// </summary>
        /// <typeparam name="T">事件数据类型</typeparam>
        /// <param name="eventName">事件名称</param>
        /// <param name="data">事件数据</param>
        public static void Trigger<T>(string eventName, T data)
        {
            if (string.IsNullOrEmpty(eventName)) return;

            if (genericEventHandlers.TryGetValue(eventName, out var handlers))
            {
                (handlers as Action<T>)?.Invoke(data);
            }
        }

        /// <summary>
        /// 清除所有通用事件订阅
        /// </summary>
        public static void ClearGenericEvents()
        {
            genericEventHandlers.Clear();
        }

        /// <summary>
        /// 获取已注册的通用事件名称列表
        /// </summary>
        public static IReadOnlyCollection<string> GetRegisteredEventNames()
        {
            return genericEventHandlers.Keys;
        }

        #endregion
        
        #region Trigger Methods (Enqueue)
        
        internal static void TriggerBuffAdded(IBuff buff)
        {
            eventQueue.Enqueue(new EventQueueItem(EventQueueItemType.BuffAdded, buff));
            ProcessQueue();
        }
        
        internal static void TriggerBuffRemoved(IBuff buff)
        {
            eventQueue.Enqueue(new EventQueueItem(EventQueueItemType.BuffRemoved, buff));
            ProcessQueue();
        }
        
        internal static void TriggerStackChanged(IBuff buff, int oldStack, int newStack)
        {
            eventQueue.Enqueue(new EventQueueItem(EventQueueItemType.StackChanged, buff, null, oldStack, newStack));
            ProcessQueue();
        }
        
        internal static void TriggerRefreshed(IBuff buff)
        {
            eventQueue.Enqueue(new EventQueueItem(EventQueueItemType.Refreshed, buff));
            ProcessQueue();
        }
        
        internal static void TriggerExpired(IBuff buff)
        {
            eventQueue.Enqueue(new EventQueueItem(EventQueueItemType.Expired, buff));
            ProcessQueue();
        }
        
        internal static void TriggerCleared(IBuffOwner owner)
        {
            eventQueue.Enqueue(new EventQueueItem(EventQueueItemType.Cleared, null, owner));
            ProcessQueue();
        }
        
        #endregion
        
        #region Queue Processing
        
        /// <summary>
        /// 处理事件队列
        /// </summary>
        private static void ProcessQueue()
        {
            if (isProcessingQueue) return;
            
            isProcessingQueue = true;
            int processedCount = 0;
            
            while (eventQueue.Count > 0 && processedCount < maxEventsPerFrame)
            {
                var item = eventQueue.Dequeue();
                DispatchEvent(item);
                processedCount++;
            }
            
            isProcessingQueue = false;
            
            // 如果还有事件（达到帧上限），下一帧继续处理
            if (eventQueue.Count > 0)
            {
                UnityEngine.Debug.LogWarning($"[BuffEventSystem] 事件队列超过每帧处理上限({maxEventsPerFrame})，剩余{eventQueue.Count}个事件将在后续处理");
            }
        }
        
        /// <summary>
        /// 分发事件到对应的处理器
        /// </summary>
        private static void DispatchEvent(EventQueueItem item)
        {
            switch (item.Type)
            {
                case EventQueueItemType.BuffAdded:
                    if (item.Buff != null)
                        OnBuffAdded?.Invoke(null, new BuffAddedEventArgs(item.Buff));
                    break;
                    
                case EventQueueItemType.BuffRemoved:
                    if (item.Buff != null)
                        OnBuffRemoved?.Invoke(null, new BuffRemovedEventArgs(item.Buff));
                    break;
                    
                case EventQueueItemType.StackChanged:
                    if (item.Buff != null)
                        OnStackChanged?.Invoke(null, new BuffStackChangedEventArgs(item.Buff, item.OldStack, item.NewStack));
                    break;
                    
                case EventQueueItemType.Refreshed:
                    if (item.Buff != null)
                        OnBuffRefreshed?.Invoke(null, new BuffRefreshedEventArgs(item.Buff));
                    break;
                    
                case EventQueueItemType.Expired:
                    if (item.Buff != null)
                        OnBuffExpired?.Invoke(null, new BuffExpiredEventArgs(item.Buff));
                    break;
                    
                case EventQueueItemType.Cleared:
                    if (item.Owner != null)
                        OnBuffCleared?.Invoke(null, new BuffClearedEventArgs(item.Owner));
                    break;
            }
        }
        
        /// <summary>
        /// 强制处理所有待处理的事件（立即执行）
        /// </summary>
        public static void Flush()
        {
            if (isProcessingQueue) return;
            
            isProcessingQueue = true;
            while (eventQueue.Count > 0)
            {
                var item = eventQueue.Dequeue();
                DispatchEvent(item);
            }
            isProcessingQueue = false;
        }
        
        /// <summary>
        /// 清空事件队列（不触发事件）
        /// </summary>
        public static void ClearQueue()
        {
            eventQueue.Clear();
        }
        
        #endregion
    }
    
    #endregion
    
    #region Local Event System
    
    /// <summary>
    /// Buff持有者本地事件系统
    /// 每个持有者独立的Buff事件（队列化实现）
    /// </summary>
    public class BuffLocalEventSystem
    {
        private readonly IBuffOwner owner;
        private readonly Queue<EventQueueItem> localEventQueue = new();
        private bool isProcessingLocalQueue = false;
        private int maxEventsPerFrame = 100;
        
        public event EventHandler<BuffAddedEventArgs> OnBuffAdded;
        public event EventHandler<BuffRemovedEventArgs> OnBuffRemoved;
        public event EventHandler<BuffStackChangedEventArgs> OnStackChanged;
        public event EventHandler<BuffRefreshedEventArgs> OnBuffRefreshed;
        public event EventHandler<BuffExpiredEventArgs> OnBuffExpired;
        public event EventHandler OnBuffCleared;
        
        /// <summary>
        /// 当前本地队列中的事件数量
        /// </summary>
        public int PendingEventCount => localEventQueue.Count;
        
        public BuffLocalEventSystem(IBuffOwner owner)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
        
        internal void TriggerBuffAdded(IBuff buff)
        {
            localEventQueue.Enqueue(new EventQueueItem(EventQueueItemType.BuffAdded, buff));
            ProcessLocalQueue();
        }
        
        internal void TriggerBuffRemoved(IBuff buff)
        {
            localEventQueue.Enqueue(new EventQueueItem(EventQueueItemType.BuffRemoved, buff));
            ProcessLocalQueue();
        }
        
        internal void TriggerStackChanged(IBuff buff, int oldStack, int newStack)
        {
            localEventQueue.Enqueue(new EventQueueItem(EventQueueItemType.StackChanged, buff, null, oldStack, newStack));
            ProcessLocalQueue();
        }
        
        internal void TriggerRefreshed(IBuff buff)
        {
            localEventQueue.Enqueue(new EventQueueItem(EventQueueItemType.Refreshed, buff));
            ProcessLocalQueue();
        }
        
        internal void TriggerExpired(IBuff buff)
        {
            localEventQueue.Enqueue(new EventQueueItem(EventQueueItemType.Expired, buff));
            ProcessLocalQueue();
        }
        
        internal void TriggerCleared()
        {
            localEventQueue.Enqueue(new EventQueueItem(EventQueueItemType.Cleared, null, owner));
            ProcessLocalQueue();
        }
        
        private void ProcessLocalQueue()
        {
            if (isProcessingLocalQueue) return;
            
            isProcessingLocalQueue = true;
            int processedCount = 0;
            
            while (localEventQueue.Count > 0 && processedCount < maxEventsPerFrame)
            {
                var item = localEventQueue.Dequeue();
                DispatchLocalEvent(item);
                processedCount++;
            }
            
            isProcessingLocalQueue = false;
        }
        
        private void DispatchLocalEvent(EventQueueItem item)
        {
            switch (item.Type)
            {
                case EventQueueItemType.BuffAdded:
                    if (item.Buff != null)
                        OnBuffAdded?.Invoke(owner, new BuffAddedEventArgs(item.Buff));
                    break;
                    
                case EventQueueItemType.BuffRemoved:
                    if (item.Buff != null)
                        OnBuffRemoved?.Invoke(owner, new BuffRemovedEventArgs(item.Buff));
                    break;
                    
                case EventQueueItemType.StackChanged:
                    if (item.Buff != null)
                        OnStackChanged?.Invoke(owner, new BuffStackChangedEventArgs(item.Buff, item.OldStack, item.NewStack));
                    break;
                    
                case EventQueueItemType.Refreshed:
                    if (item.Buff != null)
                        OnBuffRefreshed?.Invoke(owner, new BuffRefreshedEventArgs(item.Buff));
                    break;
                    
                case EventQueueItemType.Expired:
                    if (item.Buff != null)
                        OnBuffExpired?.Invoke(owner, new BuffExpiredEventArgs(item.Buff));
                    break;
                    
                case EventQueueItemType.Cleared:
                    OnBuffCleared?.Invoke(owner, EventArgs.Empty);
                    break;
            }
        }
        
        /// <summary>
        /// 强制处理所有待处理的本地事件
        /// </summary>
        public void Flush()
        {
            if (isProcessingLocalQueue) return;
            
            isProcessingLocalQueue = true;
            while (localEventQueue.Count > 0)
            {
                var item = localEventQueue.Dequeue();
                DispatchLocalEvent(item);
            }
            isProcessingLocalQueue = false;
        }
        
        /// <summary>
        /// 清空本地事件队列
        /// </summary>
        public void ClearQueue()
        {
            localEventQueue.Clear();
        }
    }
    
    #endregion
}
