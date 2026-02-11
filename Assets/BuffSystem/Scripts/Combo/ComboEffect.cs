using System;
using UnityEngine;

namespace BuffSystem.Combo
{
    /// <summary>
    /// Combo效果配置
    /// </summary>
    [Serializable]
    public class ComboEffect
    {
        [Header("效果配置")]
        [SerializeField] private ComboEffectType effectType = ComboEffectType.EnhanceDuration;

        [SerializeField] private ComboTargetType targetType = ComboTargetType.SpecificBuff;

        [SerializeField] private int targetBuffId;

        [Header("效果数值")]
        [SerializeField] private float value = 1f;

        [SerializeField] private bool usePercentage = true;

        [Header("可选配置")]
        [SerializeField] private string eventName = "";

        [SerializeField] private int extraBuffId;

        #region Properties

        /// <summary>
        /// 效果类型
        /// </summary>
        public ComboEffectType EffectType => effectType;

        /// <summary>
        /// 目标类型
        /// </summary>
        public ComboTargetType TargetType => targetType;

        /// <summary>
        /// 目标Buff ID（当TargetType为SpecificBuff时使用）
        /// </summary>
        public int TargetBuffId => targetBuffId;

        /// <summary>
        /// 效果数值
        /// </summary>
        public float Value => value;

        /// <summary>
        /// 是否使用百分比
        /// </summary>
        public bool UsePercentage => usePercentage;

        /// <summary>
        /// 事件名称（当EffectType为TriggerEvent时使用）
        /// </summary>
        public string EventName => eventName;

        /// <summary>
        /// 额外Buff ID（当EffectType为AddExtraBuff时使用）
        /// </summary>
        public int ExtraBuffId => extraBuffId;

        #endregion

        /// <summary>
        /// 创建增强持续时间的Effect
        /// </summary>
        public static ComboEffect EnhanceDuration(int targetBuffId, float multiplier)
        {
            return new ComboEffect
            {
                effectType = ComboEffectType.EnhanceDuration,
                targetType = ComboTargetType.SpecificBuff,
                targetBuffId = targetBuffId,
                value = multiplier,
                usePercentage = true
            };
        }

        /// <summary>
        /// 创建增强层数的Effect
        /// </summary>
        public static ComboEffect EnhanceStack(int targetBuffId, float multiplier)
        {
            return new ComboEffect
            {
                effectType = ComboEffectType.EnhanceStack,
                targetType = ComboTargetType.SpecificBuff,
                targetBuffId = targetBuffId,
                value = multiplier,
                usePercentage = true
            };
        }

        /// <summary>
        /// 创建触发事件的Effect
        /// </summary>
        public static ComboEffect TriggerEvent(string eventName, ComboTargetType target = ComboTargetType.Owner)
        {
            return new ComboEffect
            {
                effectType = ComboEffectType.TriggerEvent,
                targetType = target,
                eventName = eventName
            };
        }

        /// <summary>
        /// 创建添加额外Buff的Effect
        /// </summary>
        public static ComboEffect AddExtraBuff(int buffId)
        {
            return new ComboEffect
            {
                effectType = ComboEffectType.AddExtraBuff,
                extraBuffId = buffId
            };
        }

        /// <summary>
        /// 创建刷新持续时间的Effect
        /// </summary>
        public static ComboEffect RefreshDuration(int targetBuffId)
        {
            return new ComboEffect
            {
                effectType = ComboEffectType.RefreshDuration,
                targetType = ComboTargetType.SpecificBuff,
                targetBuffId = targetBuffId
            };
        }

        /// <summary>
        /// 创建增加层数的Effect
        /// </summary>
        public static ComboEffect AddStack(int targetBuffId, int amount)
        {
            return new ComboEffect
            {
                effectType = ComboEffectType.AddStack,
                targetType = ComboTargetType.SpecificBuff,
                targetBuffId = targetBuffId,
                value = amount,
                usePercentage = false
            };
        }
    }
}
