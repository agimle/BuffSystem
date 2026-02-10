using System;
using UnityEngine;

namespace BuffSystem.Core
{
    /// <summary>
    /// Buff逻辑基类 - 所有Buff逻辑的抽象基类
    /// 支持序列化，可在Inspector中配置
    /// </summary>
    [Serializable]
    public abstract class BuffLogicBase : IBuffLogic, ICloneable, IBuffSerializable
    {
        [NonSerialized] private IBuff buff;

        /// <summary>
        /// 所属的Buff实例
        /// </summary>
        public IBuff Buff => buff;

        /// <summary>
        /// 所属Buff的持有者
        /// </summary>
        protected IBuffOwner Owner => buff?.Owner;

        /// <summary>
        /// Buff来源
        /// </summary>
        protected object Source => buff?.Source;

        /// <summary>
        /// 当前层数
        /// </summary>
        protected int CurrentStack => buff?.CurrentStack ?? 0;

        IBuff IBuffLogic.Buff { get => buff; set => buff = value; }

        /// <summary>
        /// 创建克隆实例
        /// </summary>
        public object Clone()
        {
            var clone = (BuffLogicBase)MemberwiseClone();
            clone.ResetInternalState();
            return clone;
        }

        /// <summary>
        /// 重置内部状态（子类可重写以重置运行时数据）
        /// </summary>
        protected virtual void ResetInternalState()
        {
            buff = null;
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Initialize(IBuff initBuff)
        {
            this.buff = initBuff ?? throw new ArgumentNullException(nameof(initBuff));
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
            buff = null;
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
            
            if (!(Owner is MonoBehaviour mono))
            {
                return false;
            }
            
            component = mono.GetComponent<T>();
            return component != null;
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

        #region IBuffSerializable Implementation

        /// <summary>
        /// 序列化为存档数据（子类可重写以保存自定义状态）
        /// </summary>
        public virtual void Serialize(BuffSaveData saveData)
        {
            // 基类不保存任何数据，子类可重写
        }

        /// <summary>
        /// 从存档数据反序列化（子类可重写以恢复自定义状态）
        /// </summary>
        public virtual void Deserialize(BuffSaveData saveData)
        {
            // 基类不恢复任何数据，子类可重写
        }

        #endregion
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
