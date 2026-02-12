namespace BuffSystem.Core
{
    /// <summary>
    /// Buff事件类型
    /// </summary>
    /// <remarks>
    /// 🔒 稳定API: v6.0后保证向后兼容
    /// 版本历史: v1.0-v6.0 逐步完善
    /// 修改策略: 只允许bug修复，不允许破坏性变更
    /// </remarks>
    public enum BuffEventType
    {
        /// <summary>
        /// Buff被添加
        /// </summary>
        Added = 0,
        
        /// <summary>
        /// Buff被移除
        /// </summary>
        Removed = 1,
        
        /// <summary>
        /// Buff层数变化
        /// </summary>
        StackChanged = 2,
        
        /// <summary>
        /// Buff持续时间刷新
        /// </summary>
        Refreshed = 3,
        
        /// <summary>
        /// Buff持续时间结束
        /// </summary>
        Expired = 4,
        
        /// <summary>
        /// Buff被清空
        /// </summary>
        Cleared = 5
    }
}
