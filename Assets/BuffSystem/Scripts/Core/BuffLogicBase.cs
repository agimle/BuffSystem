using System;
using UnityEngine;

namespace BuffSystem.Core
{
    /// <summary>
    /// Buff逻辑基类 - 所有Buff逻辑的抽象基类
    /// 支持序列化，可在Inspector中配置
    /// </summary>
    [Serializable]
    public abstract class BuffLogicBase : IBuffLogic
    {
        [NonSerialized] private IBuff _buff;
        
        /// <summary>
        /// 所属的Buff实例
        /// </summary>
        public IBuff Buff => _buff;
        
        /// <summary>
        /// 所属Buff的持有者
        /// </summary>
        protected IBuffOwner Owner => _buff?.Owner;
        
        /// <summary>
        /// Buff来源
        /// </summary>
        protected object Source => _buff?.Source;
        
        /// <summary>
        /// 当前层数
        /// </summary>
        protected int CurrentStack => _buff?.CurrentStack ?? 0;
        
        /// <summary>
        /// 剩余时间
        /// </summary>
        protected float RemainingTime => _buff?.RemainingTime ?? 0f;

        IBuff IBuffLogic.Buff { get => _buff; set => _buff = value; }
        
        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Initialize(IBuff buff)
        {
            _buff = buff ?? throw new ArgumentNullException(nameof(buff));
            OnInitialized();
        }
        
        /// <summary>
        /// 初始化完成时调用（子类可重写）
        /// </summary>
        protected virtual void OnInitialized() { }
        
        /// <summary>
        /// 销毁时调用
        /// </summary>
        public virtual void Dispose()
        {
            OnDisposed();
            _buff = null;
        }
        
        /// <summary>
        /// 销毁时调用（子类可重写）
        /// </summary>
        protected virtual void OnDisposed() { }
        
        /// <summary>
        /// 获取来源并转换为指定类型
        /// </summary>
        protected T GetSource<T>() where T : class
        {
            return Source as T;
        }
        
        /// <summary>
        /// 尝试获取来源
        /// </summary>
        protected bool TryGetSource<T>(out T source) where T : class
        {
            source = Source as T;
            return source != null;
        }
        
        /// <summary>
        /// 尝试获取持有者身上的组件（如果是MonoBehaviour）
        /// </summary>
        protected bool TryGetOwnerComponent<T>(out T component) where T : class
        {
            component = null;
            
            if (Owner is MonoBehaviour mono)
            {
                component = mono.GetComponent<T>();
                return component != null;
            }
            
            return false;
        }
        
        /// <summary>
        /// 发送事件给持有者
        /// 用于与外部系统（如属性系统）通信
        /// </summary>
        protected void SendEvent(string eventName, object data = null)
        {
            if (Owner is IBuffEventReceiver receiver)
            {
                receiver.OnBuffEvent(Buff, eventName, data);
            }
        }
    }
    
    /// <summary>
    /// Buff事件接收接口
    /// 持有者可以实现此接口来接收Buff事件
    /// </summary>
    public interface IBuffEventReceiver
    {
        /// <summary>
        /// 当Buff发送事件时调用
        /// </summary>
        void OnBuffEvent(IBuff buff, string eventName, object data);
    }
    
    /// <summary>
    /// 空Buff逻辑 - 默认实现
    /// </summary>
    [Serializable]
    public class EmptyBuffLogic : BuffLogicBase
    {
    }
}
