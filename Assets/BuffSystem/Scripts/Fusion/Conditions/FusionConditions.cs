using System;
using UnityEngine;
using BuffSystem.Core;

namespace BuffSystem.Fusion
{
    /// <summary>
    /// 最小层数条件 - 要求材料Buff达到指定层数
    /// </summary>
    [Serializable]
    public class MinStackCondition : IFusionCondition
    {
        [Tooltip("Buff ID")]
        public int buffId;
        
        [Tooltip("最小层数")]
        public int minStack = 1;
        
        public string Description => $"要求Buff {buffId} 达到 {minStack} 层";
        
        public bool Check(IBuffContainer container, FusionRecipe recipe)
        {
            var buff = container.GetBuff(buffId);
            return buff != null && buff.CurrentStack >= minStack;
        }
    }
    
    /// <summary>
    /// 最大层数条件 - 限制材料Buff层数上限
    /// </summary>
    [Serializable]
    public class MaxStackCondition : IFusionCondition
    {
        [Tooltip("Buff ID")]
        public int buffId;
        
        [Tooltip("最大层数")]
        public int maxStack = 999;
        
        public string Description => $"要求Buff {buffId} 不超过 {maxStack} 层";
        
        public bool Check(IBuffContainer container, FusionRecipe recipe)
        {
            var buff = container.GetBuff(buffId);
            return buff == null || buff.CurrentStack <= maxStack;
        }
    }
    
    /// <summary>
    /// 最小持续时间条件 - 要求Buff持续一定时间
    /// </summary>
    [Serializable]
    public class MinDurationCondition : IFusionCondition
    {
        [Tooltip("Buff ID")]
        public int buffId;
        
        [Tooltip("最小持续时间（秒）")]
        public float minDuration = 0f;
        
        public string Description => $"要求Buff {buffId} 持续至少 {minDuration} 秒";
        
        public bool Check(IBuffContainer container, FusionRecipe recipe)
        {
            var buff = container.GetBuff(buffId);
            if (buff == null) return false;
            if (buff.IsPermanent) return true;
            return buff.TotalDuration >= minDuration;
        }
    }
    
    /// <summary>
    /// Buff数量条件 - 要求容器中有指定数量的Buff
    /// </summary>
    [Serializable]
    public class BuffCountCondition : IFusionCondition
    {
        [Tooltip("最小Buff数量")]
        public int minCount = 0;
        
        [Tooltip("最大Buff数量")]
        public int maxCount = 999;
        
        public string Description => $"要求拥有 {minCount} 到 {maxCount} 个Buff";
        
        public bool Check(IBuffContainer container, FusionRecipe recipe)
        {
            int count = container.AllBuffs.Count;
            return count >= minCount && count <= maxCount;
        }
    }
    
    /// <summary>
    /// 自定义条件 - 通过委托实现
    /// </summary>
    public class CustomFusionCondition : IFusionCondition
    {
        private Func<IBuffContainer, FusionRecipe, bool> checkFunc;
        private string description;
        
        public string Description => description;
        
        public CustomFusionCondition(Func<IBuffContainer, FusionRecipe, bool> check, string desc)
        {
            checkFunc = check;
            description = desc;
        }
        
        public bool Check(IBuffContainer container, FusionRecipe recipe)
        {
            return checkFunc?.Invoke(container, recipe) ?? true;
        }
    }
}
