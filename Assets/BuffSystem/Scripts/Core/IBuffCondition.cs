using System;
using BuffSystem.Data;
using BuffSystem.Utils;

namespace BuffSystem.Core
{
    /// <summary>
    /// Buff条件接口
    /// 用于定义Buff的添加条件、维持条件等
    /// </summary>
    public interface IBuffCondition
    {
        /// <summary>
        /// 检查条件是否满足
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <param name="data">Buff数据</param>
        /// <returns>条件是否满足</returns>
        bool Check(IBuffOwner owner, IBuffData data);

        /// <summary>
        /// 条件描述（用于调试和显示）
        /// </summary>
        string Description { get; }
    }

    #region 内置条件实现
    
    /// <summary>
    /// 拥有Buff条件
    /// 检查持有者是否拥有指定的Buff
    /// </summary>
    [Serializable]
    public class HasBuffCondition : IBuffCondition
    {
        public int buffId;
        public bool requireActive = true;

        public bool Check(IBuffOwner owner, IBuffData data)
        {
            if (owner?.BuffContainer == null) return false;

            foreach (var buff in owner.BuffContainer.AllBuffs)
            {
                if (buff.DataId == buffId)
                {
                    return !requireActive || buff.IsActive;
                }
            }

            return false;
        }

        public string Description => $"拥有Buff [{buffId}]";
    }

    /// <summary>
    /// 不拥有Buff条件
    /// 检查持有者是否不拥有指定的Buff
    /// </summary>
    [Serializable]
    public class NotHasBuffCondition : IBuffCondition
    {
        public int buffId;

        public bool Check(IBuffOwner owner, IBuffData data)
        {
            if (owner?.BuffContainer == null) return true;

            foreach (var buff in owner.BuffContainer.AllBuffs)
            {
                if (buff.DataId == buffId)
                {
                    return false;
                }
            }

            return true;
        }

        public string Description => $"不拥有Buff [{buffId}]";
    }

    /// <summary>
    /// 组合条件 - 所有子条件都必须满足（AND）
    /// </summary>
    [Serializable]
    public class AllCondition : IBuffCondition
    {
        public System.Collections.Generic.List<IBuffCondition> conditions = new();

        public bool Check(IBuffOwner owner, IBuffData data)
        {
            return conditions.CheckAllConditions(owner, data);
        }

        public string Description
        {
            get
            {
                if (conditions.Count == 0) return "无条件";
                var descs = new System.Collections.Generic.List<string>();
                foreach (var c in conditions)
                {
                    if (c != null) descs.Add(c.Description);
                }
                return "满足所有: " + string.Join(" 且 ", descs);
            }
        }
    }

    /// <summary>
    /// 组合条件 - 任一子条件满足即可（OR）
    /// </summary>
    [Serializable]
    public class AnyCondition : IBuffCondition
    {
        public System.Collections.Generic.List<IBuffCondition> conditions = new();

        public bool Check(IBuffOwner owner, IBuffData data)
        {
            return conditions.CheckAnyCondition(owner, data);
        }

        public string Description
        {
            get
            {
                if (conditions.Count == 0) return "无条件";
                var descs = new System.Collections.Generic.List<string>();
                foreach (var c in conditions)
                {
                    if (c != null) descs.Add(c.Description);
                }
                return "满足任一: " + string.Join(" 或 ", descs);
            }
        }
    }

    #endregion

    #region 属性持有者接口

    /// <summary>
    /// 属性持有者接口
    /// 用于条件系统获取属性值
    /// </summary>
    public interface IAttributeOwner
    {
        /// <summary>
        /// 获取属性值
        /// </summary>
        float GetAttributeValue(string attributeName);

        /// <summary>
        /// 获取生命值百分比（0-1）
        /// </summary>
        float GetHealthPercent();
    }

    #endregion
}
