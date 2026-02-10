namespace BuffSystem.Core
{
    /// <summary>
    /// 效果接口 - 定义Buff效果的基本方法
    /// </summary>
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
