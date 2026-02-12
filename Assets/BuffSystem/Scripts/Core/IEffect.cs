namespace BuffSystem.Core
{
    /// <summary>
    /// 效果接口 - 定义Buff效果的基本方法
    /// </summary>
    /// <remarks>
    /// 🔒 稳定API: v6.0后保证向后兼容
    /// 版本历史: v1.0-v6.0 逐步完善
    /// 修改策略: 只允许bug修复，不允许破坏性变更
    /// </remarks>
    public interface IEffect
    {
        /// <summary>
        /// 执行效果
        /// </summary>
        void Execute(IBuff buff);
        
        /// <summary>
        /// 取消效果
        /// </summary>
        void Cancel(IBuff buff);
    }
}
