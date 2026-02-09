namespace BuffSystem.Core
{
    /// <summary>
    /// Buff事件类型
    /// </summary>
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
