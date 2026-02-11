using System;

namespace BuffSystem.Combo
{
    /// <summary>
    /// Combo效果类型
    /// </summary>
    public enum ComboEffectType
    {
        /// <summary>
        /// 增强持续时间
        /// </summary>
        EnhanceDuration,

        /// <summary>
        /// 增强层数效果
        /// </summary>
        EnhanceStack,

        /// <summary>
        /// 减少冷却
        /// </summary>
        ReduceCooldown,

        /// <summary>
        /// 触发事件
        /// </summary>
        TriggerEvent,

        /// <summary>
        /// 修改属性
        /// </summary>
        ModifyAttribute,

        /// <summary>
        /// 添加额外Buff
        /// </summary>
        AddExtraBuff,

        /// <summary>
        /// 移除Buff
        /// </summary>
        RemoveBuff,

        /// <summary>
        /// 刷新持续时间
        /// </summary>
        RefreshDuration,

        /// <summary>
        /// 增加层数
        /// </summary>
        AddStack,

        /// <summary>
        /// 减少层数
        /// </summary>
        RemoveStack
    }

    /// <summary>
    /// Combo触发模式
    /// </summary>
    [Flags]
    public enum ComboTriggerMode
    {
        /// <summary>
        /// 当所有必需Buff都存在时触发
        /// </summary>
        All = 1,

        /// <summary>
        /// 当任一必需Buff存在时触发
        /// </summary>
        Any = 2,

        /// <summary>
        /// 当Buff添加时检查
        /// </summary>
        OnBuffAdd = 4,

        /// <summary>
        /// 当Buff移除时检查
        /// </summary>
        OnBuffRemove = 8,

        /// <summary>
        /// 每帧检查
        /// </summary>
        OnUpdate = 16,

        /// <summary>
        /// 默认：所有Buff添加时触发
        /// </summary>
        Default = All | OnBuffAdd
    }

    /// <summary>
    /// Combo效果目标类型
    /// </summary>
    public enum ComboTargetType
    {
        /// <summary>
        /// 特定Buff
        /// </summary>
        SpecificBuff,

        /// <summary>
        /// 所有参与Combo的Buff
        /// </summary>
        AllComboBuffs,

        /// <summary>
        /// 持有者本身
        /// </summary>
        Owner,

        /// <summary>
        /// 新添加的Buff
        /// </summary>
        NewestBuff
    }
}
