namespace BuffSystem.Core
{
    /// <summary>
    /// Buff数据接口 - 配置数据的抽象
    /// </summary>
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
        /// 创建Buff逻辑实例
        /// </summary>
        IBuffLogic CreateLogic();
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
}
