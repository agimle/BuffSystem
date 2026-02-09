namespace BuffSystem.Data
{
    /// <summary>
    /// Buff更新模式
    /// </summary>
    public enum UpdateMode
    {
        /// <summary>
        /// 每帧更新
        /// </summary>
        EveryFrame = 0,

        /// <summary>
        /// 按固定间隔更新
        /// </summary>
        Interval = 1,

        /// <summary>
        /// 手动更新
        /// </summary>
        Manual = 2
    }
}
