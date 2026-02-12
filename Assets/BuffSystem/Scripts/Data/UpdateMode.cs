namespace BuffSystem.Data
{
    /// <summary>
    /// Buff更新模式
    /// </summary>
    /// <remarks>
    /// 🔒 稳定API: v6.0后保证向后兼容
    /// 版本历史: v1.0-v6.0 逐步完善
    /// 修改策略: 只允许bug修复，不允许破坏性变更
    /// </remarks>
    public enum UpdateMode
    {
        /// <summary>
        /// 每帧更新（实时游戏）
        /// </summary>
        EveryFrame = 0,

        /// <summary>
        /// 按固定间隔更新
        /// </summary>
        Interval = 1,

        /// <summary>
        /// 手动更新（通过代码控制）
        /// </summary>
        Manual = 2,

        /// <summary>
        /// 回合制更新（只在回合切换时更新）
        /// </summary>
        TurnBased = 3
    }
}
