using System;

namespace BuffSystem.Core
{
    /// <summary>
    /// 效果基类 - 所有效果的抽象基类
    /// 支持序列化，可在Inspector中配置
    /// </summary>
    [Serializable]
    public abstract class EffectBase : IEffect
    {
        /// <summary>
        /// 执行效果
        /// </summary>
        public abstract void Execute(IBuff buff);
        
        /// <summary>
        /// 取消效果
        /// </summary>
        public abstract void Cancel(IBuff buff);
        
        /// <summary>
        /// 尝试获取Buff持有者的组件（如果是MonoBehaviour）
        /// </summary>
        protected bool TryGetOwnerComponent<T>(IBuff buff, out T component) where T : class
        {
            component = null;
            
            if (buff?.Owner == null)
            {
                return false;
            }
            
            if (!(buff.Owner is UnityEngine.MonoBehaviour mono))
            {
                return false;
            }
            
            component = mono.GetComponent<T>();
            return component != null;
        }
        
        /// <summary>
        /// 发送事件给Buff持有者
        /// </summary>
        protected void SendEvent(IBuff buff, string eventName, object data = null)
        {
            if (buff?.Owner is IBuffEventReceiver receiver)
            {
                receiver.OnBuffEvent(buff, eventName, data);
            }
        }
    }
}
