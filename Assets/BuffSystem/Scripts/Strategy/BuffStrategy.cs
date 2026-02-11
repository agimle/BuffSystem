using BuffSystem.Core;
using BuffSystem.Runtime;

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
        
        /// <summary>
        /// 是否需要刷新持续时间
        /// </summary>
        /// <param name="data">Buff数据</param>
        /// <returns>是否需要刷新</returns>
        bool ShouldRefresh(IBuffData data);
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
        
        public bool ShouldRefresh(IBuffData data) => false; // 可叠加策略不刷新持续时间
    }
    
    /// <summary>
    /// 不可叠加策略 - 刷新持续时间
    /// </summary>
    public class NonStackableStrategy : IStackStrategy
    {
        public bool HandleStack(IBuff existingBuff, IBuffData newData)
        {
            // 叠层逻辑在HandleStack中处理，刷新逻辑由ShouldRefresh控制
            return false; // 不创建新Buff
        }
        
        public bool ShouldRefresh(IBuffData data) => data.CanRefresh;
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
        
        public bool ShouldRefresh(IBuffData data) => false; // 独立策略不刷新
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
        /// 处理Buff过期
        /// </summary>
        /// <param name="buff">Buff实例</param>
        /// <param name="deltaTime">时间增量</param>
        /// <param name="timer">计时器引用</param>
        /// <returns>是否已处理完成（需要移除）</returns>
        bool HandleExpiration(IBuff buff, float deltaTime, ref float timer);
    }
    
    /// <summary>
    /// 直接移除策略
    /// </summary>
    public class DirectRemoveStrategy : IRemoveStrategy
    {
        public bool HandleExpiration(IBuff buff, float deltaTime, ref float timer)
        {
            buff.MarkForRemoval();
            return true;
        }
    }
    
    /// <summary>
    /// 逐层移除策略
    /// </summary>
    public class ReduceStackStrategy : IRemoveStrategy
    {
        public bool HandleExpiration(IBuff buff, float deltaTime, ref float timer)
        {
            float interval = buff.Data.RemoveInterval;
            if (interval <= 0) interval = 0.1f; // 防止除零
            
            timer += deltaTime;
            
            while (timer >= interval && !buff.IsMarkedForRemoval)
            {
                timer -= interval;
                
                if (buff is BuffEntity entity)
                {
                    entity.RemoveStack(buff.Data.RemoveStackCount);
                }
                
                // 如果层数为0或已标记移除，返回true表示处理完成
                if (buff.CurrentStack <= 0 || buff.IsMarkedForRemoval)
                {
                    return true;
                }
            }
            
            return false;
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
        public static IRemoveStrategy CreateRemoveStrategy(BuffRemoveMode mode)
        {
            return mode switch
            {
                BuffRemoveMode.Remove => new DirectRemoveStrategy(),
                BuffRemoveMode.Reduce => new ReduceStackStrategy(),
                _ => new DirectRemoveStrategy()
            };
        }
    }
    
    #endregion
}
