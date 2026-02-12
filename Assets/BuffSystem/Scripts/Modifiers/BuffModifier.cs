using System;
using UnityEngine;
using BuffSystem.Core;

namespace BuffSystem.Modifiers
{
    /// <summary>
    /// Buff修饰器基础类
    /// 可序列化的修饰器配置
    /// </summary>
    [Serializable]
    public class BuffModifier : IBuffModifier
    {
        [SerializeField] private float durationMultiplier = 1f;
        [SerializeField] private float stackMultiplier = 1f;
        [SerializeField] private int priority = 0;
        [SerializeField] private bool applyToAllBuffs = true;
        [SerializeField] private int[] targetBuffIds = Array.Empty<int>();
        
        /// <summary>
        /// 持续时间倍率
        /// </summary>
        public float DurationMultiplier => durationMultiplier;
        
        /// <summary>
        /// 层数倍率
        /// </summary>
        public float StackMultiplier => stackMultiplier;
        
        /// <summary>
        /// 优先级（数值越大越优先）
        /// </summary>
        public int Priority => priority;
        
        /// <summary>
        /// 是否应用于所有Buff
        /// </summary>
        public bool ApplyToAllBuffs => applyToAllBuffs;
        
        /// <summary>
        /// 目标Buff ID列表（当applyToAllBuffs为false时有效）
        /// </summary>
        public int[] TargetBuffIds => targetBuffIds;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public BuffModifier(float durationMultiplier = 1f, float stackMultiplier = 1f, int priority = 0)
        {
            this.durationMultiplier = durationMultiplier;
            this.stackMultiplier = stackMultiplier;
            this.priority = priority;
        }
        
        /// <summary>
        /// 检查是否可以修改目标Buff
        /// </summary>
        public virtual bool CanModify(IBuff target)
        {
            if (target == null) return true;
            if (applyToAllBuffs) return true;
            
            foreach (var buffId in targetBuffIds)
            {
                if (target.DataId == buffId)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// 应用修饰器前的回调
        /// </summary>
        public virtual void OnBeforeApply(IBuff buff) { }
        
        /// <summary>
        /// 应用修饰器后的回调
        /// </summary>
        public virtual void OnAfterApply(IBuff buff) { }
        
        /// <summary>
        /// 设置目标Buff ID列表
        /// </summary>
        public void SetTargetBuffIds(params int[] buffIds)
        {
            targetBuffIds = buffIds ?? Array.Empty<int>();
            applyToAllBuffs = targetBuffIds.Length == 0;
        }
    }
    
    /// <summary>
    /// 持续时间修饰器
    /// 专门用于修改Buff持续时间
    /// </summary>
    [Serializable]
    public class DurationModifier : BuffModifier
    {
        public DurationModifier(float multiplier) : base(multiplier, 1f, 0) { }
        
        /// <summary>
        /// 创建增加持续时间的修饰器
        /// </summary>
        public static DurationModifier Increase(float percentage)
        {
            return new DurationModifier(1f + percentage);
        }
        
        /// <summary>
        /// 创建减少持续时间的修饰器
        /// </summary>
        public static DurationModifier Decrease(float percentage)
        {
            return new DurationModifier(1f - percentage);
        }
        
        /// <summary>
        /// 创建固定倍率的修饰器
        /// </summary>
        public static DurationModifier Multiply(float multiplier)
        {
            return new DurationModifier(multiplier);
        }
    }
    
    /// <summary>
    /// 层数修饰器
    /// 专门用于修改Buff层数
    /// </summary>
    [Serializable]
    public class StackModifier : BuffModifier
    {
        public StackModifier(float multiplier) : base(1f, multiplier, 0) { }
        
        /// <summary>
        /// 创建增加层数的修饰器
        /// </summary>
        public static StackModifier Increase(float percentage)
        {
            return new StackModifier(1f + percentage);
        }
        
        /// <summary>
        /// 创建减少层数的修饰器
        /// </summary>
        public static StackModifier Decrease(float percentage)
        {
            return new StackModifier(1f - percentage);
        }
        
        /// <summary>
        /// 创建固定倍率的修饰器
        /// </summary>
        public static StackModifier Multiply(float multiplier)
        {
            return new StackModifier(multiplier);
        }
    }
    
    /// <summary>
    /// 组合修饰器
    /// 可以同时应用多个修饰器效果
    /// </summary>
    public class CompositeModifier : IBuffModifier
    {
        private readonly System.Collections.Generic.List<IBuffModifier> modifiers = new();
        
        /// <summary>
        /// 添加修饰器
        /// </summary>
        public void AddModifier(IBuffModifier modifier)
        {
            if (modifier != null)
                modifiers.Add(modifier);
        }
        
        /// <summary>
        /// 移除修饰器
        /// </summary>
        public void RemoveModifier(IBuffModifier modifier)
        {
            modifiers.Remove(modifier);
        }
        
        /// <summary>
        /// 清除所有修饰器
        /// </summary>
        public void ClearModifiers()
        {
            modifiers.Clear();
        }
        
        public float DurationMultiplier
        {
            get
            {
                float result = 1f;
                foreach (var modifier in modifiers)
                    result *= modifier.DurationMultiplier;
                return result;
            }
        }
        
        public float StackMultiplier
        {
            get
            {
                float result = 1f;
                foreach (var modifier in modifiers)
                    result *= modifier.StackMultiplier;
                return result;
            }
        }
        
        public bool CanModify(IBuff target)
        {
            foreach (var modifier in modifiers)
            {
                if (modifier.CanModify(target))
                    return true;
            }
            return modifiers.Count == 0;
        }
        
        public void OnBeforeApply(IBuff buff)
        {
            foreach (var modifier in modifiers)
                modifier.OnBeforeApply(buff);
        }
        
        public void OnAfterApply(IBuff buff)
        {
            foreach (var modifier in modifiers)
                modifier.OnAfterApply(buff);
        }
    }
}
