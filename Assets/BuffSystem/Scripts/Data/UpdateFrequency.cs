namespace BuffSystem.Data
{
    /// <summary>
    /// Buff更新频率 - 用于分层更新优化CPU性能
    /// </summary>
    /// <remarks>
    /// 🔒 稳定API: v6.0后保证向后兼容
    /// 版本历史: v6.0 新增
    /// 修改策略: 只允许bug修复，不允许破坏性变更
    /// </remarks>
    public enum UpdateFrequency
    {
        /// <summary>
        /// 每帧更新 - 用于视觉效果和需要实时响应的Buff
        /// 更新频率: 60fps (约16.67ms)
        /// </summary>
        EveryFrame = 0,
        
        /// <summary>
        /// 30fps更新 - 用于高频逻辑Buff
        /// 更新频率: 30fps (约33.33ms)
        /// </summary>
        Every33ms = 1,
        
        /// <summary>
        /// 10fps更新 - 用于中频逻辑Buff
        /// 更新频率: 10fps (100ms)
        /// </summary>
        Every100ms = 2,
        
        /// <summary>
        /// 2fps更新 - 用于低频逻辑Buff
        /// 更新频率: 2fps (500ms)
        /// </summary>
        Every500ms = 3,
        
        /// <summary>
        /// 事件驱动 - 不自动更新，只在事件触发时更新
        /// 适用于: 被动Buff、触发式Buff
        /// </summary>
        OnEventOnly = 4
    }
}
