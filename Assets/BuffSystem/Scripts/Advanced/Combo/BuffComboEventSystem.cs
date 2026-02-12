using System;
using BuffSystem.Core;

namespace BuffSystem.Advanced.Combo
{
    /// <summary>
    /// Buff Combo事件系统
    /// 管理所有Combo相关的事件
    /// </summary>
    public static class BuffComboEventSystem
    {
        #region Events

        /// <summary>
        /// Combo激活事件
        /// </summary>
        public static event Action<BuffComboData, IBuffOwner> OnComboActivated;

        /// <summary>
        /// Combo停用事件
        /// </summary>
        public static event Action<BuffComboData, IBuffOwner> OnComboDeactivated;

        /// <summary>
        /// 增强持续时间事件
        /// </summary>
        public static event Action<IBuff, float> OnEnhanceDuration;

        /// <summary>
        /// 增强层数事件
        /// </summary>
        public static event Action<IBuff, float> OnEnhanceStack;

        /// <summary>
        /// 减少冷却事件
        /// </summary>
        public static event Action<IBuffOwner, int, float> OnReduceCooldown;

        /// <summary>
        /// 修改属性事件
        /// </summary>
        public static event Action<IBuffOwner, string, float> OnModifyAttribute;

        /// <summary>
        /// 自定义事件
        /// </summary>
        public static event Action<string, BuffComboData, IBuffOwner, float> OnCustomEvent;

        #endregion

        #region Trigger Methods

        /// <summary>
        /// 触发Combo激活事件
        /// </summary>
        public static void TriggerComboActivated(BuffComboData combo, IBuffOwner owner)
        {
            OnComboActivated?.Invoke(combo, owner);
        }

        /// <summary>
        /// 触发Combo停用事件
        /// </summary>
        public static void TriggerComboDeactivated(BuffComboData combo, IBuffOwner owner)
        {
            OnComboDeactivated?.Invoke(combo, owner);
        }

        /// <summary>
        /// 触发增强持续时间事件
        /// </summary>
        public static void TriggerEnhanceDuration(IBuff buff, float multiplier)
        {
            OnEnhanceDuration?.Invoke(buff, multiplier);
        }

        /// <summary>
        /// 触发增强层数事件
        /// </summary>
        public static void TriggerEnhanceStack(IBuff buff, float multiplier)
        {
            OnEnhanceStack?.Invoke(buff, multiplier);
        }

        /// <summary>
        /// 触发减少冷却事件
        /// </summary>
        public static void TriggerReduceCooldown(IBuffOwner owner, int buffId, float multiplier)
        {
            OnReduceCooldown?.Invoke(owner, buffId, multiplier);
        }

        /// <summary>
        /// 触发修改属性事件
        /// </summary>
        public static void TriggerModifyAttribute(IBuffOwner owner, string attributeName, float value)
        {
            OnModifyAttribute?.Invoke(owner, attributeName, value);
        }

        /// <summary>
        /// 触发自定义事件
        /// </summary>
        public static void TriggerCustomEvent(string eventName, BuffComboData combo, IBuffOwner owner, float value)
        {
            OnCustomEvent?.Invoke(eventName, combo, owner, value);
        }

        #endregion

        #region Clear Events

        /// <summary>
        /// 清除所有事件监听
        /// </summary>
        public static void ClearAllEvents()
        {
            OnComboActivated = null;
            OnComboDeactivated = null;
            OnEnhanceDuration = null;
            OnEnhanceStack = null;
            OnReduceCooldown = null;
            OnModifyAttribute = null;
            OnCustomEvent = null;
        }

        #endregion
    }
}
