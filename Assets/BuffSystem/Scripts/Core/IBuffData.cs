using System.Collections.Generic;
using BuffSystem.Data;

namespace BuffSystem.Core
{
    /// <summary>
    /// Buff数据接口 - 配置数据的抽象
    /// </summary>
    /// <remarks>
    /// 🔒 稳定API: v6.0后保证向后兼容
    /// 版本历史: v1.0-v6.0 逐步完善
    /// 修改策略: 只允许bug修复，不允许破坏性变更
    /// </remarks>
    public interface IBuffData
    {
        /// <summary>
        /// Buff唯一ID
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Buff名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Buff描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Buff效果类型
        /// </summary>
        BuffEffectType EffectType { get; }
        
        /// <summary>
        /// 是否唯一（同类型只能存在一个）
        /// </summary>
        bool IsUnique { get; }
        
        /// <summary>
        /// 叠加模式
        /// </summary>
        BuffStackMode StackMode { get; }
        
        /// <summary>
        /// 最大层数
        /// </summary>
        int MaxStack { get; }
        
        /// <summary>
        /// 每次添加的层数
        /// </summary>
        int AddStackCount { get; }
        
        /// <summary>
        /// 是否永久
        /// </summary>
        bool IsPermanent { get; }

        /// <summary>
        /// 持续时间
        /// </summary>
        float Duration { get; }
        
        /// <summary>
        /// 是否可刷新持续时间
        /// </summary>
        bool CanRefresh { get; }

        /// <summary>
        /// 移除模式
        /// </summary>
        BuffRemoveMode RemoveMode { get; }

        /// <summary>
        /// 每次移除的层数
        /// </summary>
        int RemoveStackCount { get; }

        /// <summary>
        /// 移除间隔
        /// </summary>
        float RemoveInterval { get; }
        
        /// <summary>
        /// 标签列表
        /// </summary>
        IReadOnlyList<string> Tags { get; }
        
        /// <summary>
        /// 是否拥有指定标签
        /// </summary>
        bool HasTag(string tag);
        
        /// <summary>
        /// 创建Buff逻辑实例
        /// </summary>
        IBuffLogic CreateLogic();
        
        /// <summary>
        /// 更新频率 - 用于分层更新优化CPU性能
        /// </summary>
        UpdateFrequency UpdateFrequency { get; }
    }
    
    /// <summary>
    /// Buff效果类型
    /// </summary>
    public enum BuffEffectType
    {
        Neutral = 0,
        Buff = 1,
        Debuff = 2,
        Special = 3
    }
    
    /// <summary>
    /// Buff叠加模式
    /// </summary>
    /// <remarks>
    /// 🔒 稳定API: v6.0后保证向后兼容
    /// 版本历史: v1.0-v6.0 逐步完善
    /// 修改策略: 只允许bug修复，不允许破坏性变更
    /// </remarks>
    public enum BuffStackMode
    {
        /// <summary>
        /// 不可叠加（新Buff会替换或忽略）
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 可叠加（层数增加）
        /// </summary>
        Stackable = 1,
        
        /// <summary>
        /// 独立（同ID可同时存在多个实例）
        /// </summary>
        Independent = 2
    }
    
    /// <summary>
    /// Buff移除模式
    /// </summary>
    /// <remarks>
    /// 🔒 稳定API: v6.0后保证向后兼容
    /// 版本历史: v1.0-v6.0 逐步完善
    /// 修改策略: 只允许bug修复，不允许破坏性变更
    /// </remarks>
    public enum BuffRemoveMode
    {
        /// <summary>
        /// 直接移除
        /// </summary>
        Remove = 0,
        
        /// <summary>
        /// 逐层移除
        /// </summary>
        Reduce = 1
    }
    
    /// <summary>
    /// 互斥优先级
    /// </summary>
    public enum MutexPriority
    {
        /// <summary>
        /// 阻止新Buff添加
        /// </summary>
        BlockNew = 0,
        
        /// <summary>
        /// 替换已有Buff
        /// </summary>
        ReplaceOthers = 1,
        
        /// <summary>
        /// 允许共存（仅标记关系）
        /// </summary>
        Coexist = 2
    }
}
