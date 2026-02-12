using BuffSystem.Core;
using BuffSystem.Fluent;

namespace BuffSystem
{
    /// <summary>
    /// BuffBuilder扩展方法
    /// 为IBuffOwner提供便捷的Fluent API入口
    /// </summary>
    public static class BuffBuilderExtensions
    {
        /// <summary>
        /// 创建Buff构建器
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <returns>Buff构建器</returns>
        public static BuffBuilder CreateBuffBuilder(this IBuffOwner owner)
        {
            return new BuffBuilder(owner);
        }
        
        /// <summary>
        /// 创建Buff数据构建器
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <returns>Buff数据构建器</returns>
        public static BuffDataBuilder CreateBuffDataBuilder(this IBuffOwner owner)
        {
            return new BuffDataBuilder(owner);
        }
        
        /// <summary>
        /// 便捷方法：快速添加Buff
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <param name="buffId">Buff ID</param>
        /// <returns>Buff构建器</returns>
        public static BuffBuilder AddBuff(this IBuffOwner owner, int buffId)
        {
            return new BuffBuilder(owner).Add(buffId);
        }
        
        /// <summary>
        /// 便捷方法：快速添加Buff（带来源）
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <param name="buffId">Buff ID</param>
        /// <param name="source">Buff来源</param>
        /// <returns>创建的Buff实例</returns>
        public static IBuff AddBuff(this IBuffOwner owner, int buffId, object source)
        {
            return new BuffBuilder(owner).Add(buffId).WithSource(source).Execute();
        }
        
        /// <summary>
        /// 便捷方法：快速添加Buff（完整参数）
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <param name="buffId">Buff ID</param>
        /// <param name="source">Buff来源</param>
        /// <param name="stack">初始层数</param>
        /// <param name="duration">持续时间</param>
        /// <returns>创建的Buff实例</returns>
        public static IBuff AddBuff(this IBuffOwner owner, int buffId, object source, int stack, float duration)
        {
            return new BuffBuilder(owner)
                .Add(buffId)
                .WithSource(source)
                .WithStack(stack)
                .WithDuration(duration)
                .Execute();
        }
    }
}
