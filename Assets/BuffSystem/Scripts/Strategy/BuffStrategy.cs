using BuffSystem.Core;

namespace BuffSystem.Strategy
{
    #region 叠层策略
    
    /// <summary>
    /// 叠层策略接口
    /// </summary>
    public interface IStackStrategy
    {
        /// <summary>
        /// 处理叠层逻辑
        /// </summary>
        /// <param name="existingBuff">已存在的Buff</param>
        /// <param name="newData">新添加的Buff数据</param>
        /// <returns>是否创建新Buff</returns>
        bool HandleStack(IBuff existingBuff, IBuffData newData);
    }
    
    /// <summary>
    /// 可叠加策略 - 增加层数
    /// </summary>
    public class StackableStrategy : IStackStrategy
    {
        public bool HandleStack(IBuff existingBuff, IBuffData newData)
        {
            existingBuff.AddStack(newData.AddStackCount);
            return false; // 不创建新Buff
        }
    }
    
    /// <summary>
    /// 不可叠加策略 - 刷新持续时间
    /// </summary>
    public class NonStackableStrategy : IStackStrategy
    {
        public bool HandleStack(IBuff existingBuff, IBuffData newData)
        {
            if (newData.CanRefresh)
            {
                existingBuff.RefreshDuration();
            }
            return false; // 不创建新Buff
        }
    }
    
    /// <summary>
    /// 独立策略 - 允许同ID多个实例共存
    /// </summary>
    public class IndependentStrategy : IStackStrategy
    {
        public bool HandleStack(IBuff existingBuff, IBuffData newData)
        {
            return true; // 创建新Buff
        }
    }
    
    #endregion
    
    #region 刷新策略
    
    /// <summary>
    /// 刷新策略接口
    /// </summary>
    public interface IRefreshStrategy
    {
        /// <summary>
        /// 是否可以刷新
        /// </summary>
        bool CanRefresh(IBuffData data);
        
        /// <summary>
        /// 执行刷新
        /// </summary>
        void Refresh(IBuff buff);
    }
    
    /// <summary>
    /// 可刷新策略
    /// </summary>
    public class RefreshableStrategy : IRefreshStrategy
    {
        public bool CanRefresh(IBuffData data) => true;
        
        public void Refresh(IBuff buff)
        {
            buff.RefreshDuration();
        }
    }
    
    /// <summary>
    /// 不可刷新策略
    /// </summary>
    public class NonRefreshableStrategy : IRefreshStrategy
    {
        public bool CanRefresh(IBuffData data) => false;
        
        public void Refresh(IBuff buff)
        {
            // 不执行任何操作
        }
    }
    
    #endregion
    
    #region 移除策略
    
    /// <summary>
    /// 移除策略接口
    /// </summary>
    public interface IRemoveStrategy
    {
        /// <summary>
        /// 处理Buff过期/移除
        /// </summary>
        void HandleRemove(IBuff buff);
    }
    
    /// <summary>
    /// 直接移除策略
    /// </summary>
    public class DirectRemoveStrategy : IRemoveStrategy
    {
        public void HandleRemove(IBuff buff)
        {
            buff.MarkForRemoval();
        }
    }
    
    /// <summary>
    /// 逐层移除策略
    /// </summary>
    public class ReduceStackStrategy : IRemoveStrategy
    {
        private readonly int _reduceAmount;
        private readonly float _interval;

        public ReduceStackStrategy(int reduceAmount, float interval)
        {
            _reduceAmount = reduceAmount;
            _interval = interval;
        }

        public void HandleRemove(IBuff buff)
        {
            // 逐层移除策略的具体实现在 BuffEntity.HandleExpiration 中处理
            // 这里只负责标记移除，实际消层逻辑由 BuffEntity 根据 RemoveInterval 控制
            buff.MarkForRemoval();
        }
    }
    
    #endregion
    
    #region 策略工厂
    
    /// <summary>
    /// Buff策略工厂 - 创建各种策略实例
    /// </summary>
    public static class BuffStrategyFactory
    {
        /// <summary>
        /// 创建叠层策略
        /// </summary>
        public static IStackStrategy CreateStackStrategy(BuffStackMode mode)
        {
            return mode switch
            {
                BuffStackMode.Stackable => new StackableStrategy(),
                BuffStackMode.None => new NonStackableStrategy(),
                BuffStackMode.Independent => new IndependentStrategy(),
                _ => new NonStackableStrategy()
            };
        }
        
        /// <summary>
        /// 创建刷新策略
        /// </summary>
        public static IRefreshStrategy CreateRefreshStrategy(bool canRefresh)
        {
            return canRefresh ? new RefreshableStrategy() : new NonRefreshableStrategy();
        }
        
        /// <summary>
        /// 创建移除策略
        /// </summary>
        public static IRemoveStrategy CreateRemoveStrategy(BuffRemoveMode mode, int removeStack = 1, float interval = 0f)
        {
            return mode switch
            {
                BuffRemoveMode.Remove => new DirectRemoveStrategy(),
                BuffRemoveMode.Reduce => new ReduceStackStrategy(removeStack, interval),
                _ => new DirectRemoveStrategy()
            };
        }
    }
    
    #endregion
}
