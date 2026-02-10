namespace BuffSystem.Data
{
    /// <summary>
    /// Buff更新模式
    /// </summary>
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
