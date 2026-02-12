using System;
using BuffSystem.Core;

namespace BuffSystem.Modifiers
{
    /// <summary>
    /// Buff修饰器接口
    /// 用于在添加Buff时动态修改Buff属性
    /// </summary>
    /// <remarks>
    /// 使用场景：
    /// - 技能等级影响Buff持续时间
    /// - 装备效果增强Buff层数
    /// - 天赋改变Buff效果
    /// </remarks>
    public interface IBuffModifier
    {
        /// <summary>
        /// 持续时间倍率（1.0为原始值）
        /// </summary>
        float DurationMultiplier { get; }
        
        /// <summary>
        /// 层数倍率（1.0为原始值）
        /// </summary>
        float StackMultiplier { get; }
        
        /// <summary>
        /// 是否可以修改目标Buff
        /// </summary>
        /// <param name="target">目标Buff</param>
        /// <returns>是否可以修改</returns>
        bool CanModify(IBuff target);
        
        /// <summary>
        /// 应用修饰器前的回调
        /// </summary>
        /// <param name="buff">目标Buff</param>
        void OnBeforeApply(IBuff buff);
        
        /// <summary>
        /// 应用修饰器后的回调
        /// </summary>
        /// <param name="buff">目标Buff</param>
        void OnAfterApply(IBuff buff);
    }
    
    /// <summary>
    /// Buff修饰器上下文
    /// 包含修饰器应用时的所有信息
    /// </summary>
    public class BuffModifierContext
    {
        /// <summary>
        /// 原始持续时间
        /// </summary>
        public float OriginalDuration { get; set; }
        
        /// <summary>
        /// 原始层数
        /// </summary>
        public int OriginalStack { get; set; }
        
        /// <summary>
        /// 修饰后的持续时间
        /// </summary>
        public float ModifiedDuration { get; set; }
        
        /// <summary>
        /// 修饰后的层数
        /// </summary>
        public int ModifiedStack { get; set; }
        
        /// <summary>
        /// Buff来源
        /// </summary>
        public object Source { get; set; }
        
        /// <summary>
        /// 目标持有者
        /// </summary>
        public IBuffOwner Target { get; set; }
        
        /// <summary>
        /// Buff数据
        /// </summary>
        public IBuffData Data { get; set; }
        
        /// <summary>
        /// 应用的修饰器列表
        /// </summary>
        public System.Collections.Generic.List<IBuffModifier> AppliedModifiers { get; } = new();
        
        /// <summary>
        /// 计算最终持续时间
        /// </summary>
        public float CalculateDuration()
        {
            float multiplier = 1f;
            foreach (var modifier in AppliedModifiers)
            {
                if (modifier.CanModify(null))
                {
                    multiplier *= modifier.DurationMultiplier;
                }
            }
            return OriginalDuration * multiplier;
        }
        
        /// <summary>
        /// 计算最终层数
        /// </summary>
        public int CalculateStack()
        {
            float multiplier = 1f;
            foreach (var modifier in AppliedModifiers)
            {
                if (modifier.CanModify(null))
                {
                    multiplier *= modifier.StackMultiplier;
                }
            }
            return (int)(OriginalStack * multiplier);
        }
    }
}
