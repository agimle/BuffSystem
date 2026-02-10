namespace BuffSystem.Core
{
    /// <summary>
    /// Buff逻辑基接口
    /// 所有Buff逻辑类都需要实现此接口
    /// </summary>
    public interface IBuffLogic
    {
        /// <summary>
        /// 所属的Buff实例
        /// </summary>
        IBuff Buff { get; set; }

        /// <summary>
        /// 初始化时调用
        /// </summary>
        void Initialize(IBuff buff);

        /// <summary>
        /// 销毁时调用
        /// </summary>
        void Dispose();
    }
    
    /// <summary>
    /// Buff生命周期接口 - 开始
    /// </summary>
    public interface IBuffStart : IBuffLogic
    {
        /// <summary>
        /// Buff逻辑开始（配置加载完成时）
        /// </summary>
        void OnStart();
    }
    
    /// <summary>
    /// Buff生命周期接口 - 获得
    /// </summary>
    public interface IBuffAcquire : IBuffLogic
    {
        /// <summary>
        /// Buff被添加到持有者时
        /// </summary>
        void OnAcquire();
    }
    
    /// <summary>
    /// Buff生命周期接口 - 逻辑更新
    /// </summary>
    public interface IBuffLogicUpdate : IBuffLogic
    {
        /// <summary>
        /// 每帧逻辑更新
        /// </summary>
        void OnLogicUpdate(float deltaTime);
    }
    
    /// <summary>
    /// Buff生命周期接口 - 表现更新
    /// </summary>
    public interface IBuffVisualUpdate : IBuffLogic
    {
        /// <summary>
        /// 每帧表现更新（可用于UI、特效等）
        /// </summary>
        void OnVisualUpdate(float deltaTime);
    }

    /// <summary>
    /// Buff生命周期接口 - 刷新
    /// </summary>
    public interface IBuffRefresh : IBuffLogic
    {
        /// <summary>
        /// Buff被刷新时（重新添加同类型Buff）
        /// </summary>
        void OnRefresh();
    }

    /// <summary>
    /// Buff生命周期接口 - 层数变化
    /// </summary>
    public interface IBuffStackChange : IBuffLogic
    {
        /// <summary>
        /// 层数变化时
        /// </summary>
        void OnStackChanged(int oldStack, int newStack);
    }

    /// <summary>
    /// Buff生命周期接口 - 消层
    /// </summary>
    public interface IBuffReduce : IBuffLogic
    {
        /// <summary>
        /// 层数减少时
        /// </summary>
        void OnReduce();
    }
    
    /// <summary>
    /// Buff生命周期接口 - 移除
    /// </summary>
    public interface IBuffRemove : IBuffLogic
    {
        /// <summary>
        /// Buff被标记移除时
        /// </summary>
        void OnRemove();
    }

    /// <summary>
    /// Buff生命周期接口 - 结束
    /// </summary>
    public interface IBuffEnd : IBuffLogic
    {
        /// <summary>
        /// Buff完全销毁时
        /// </summary>
        void OnEnd();
    }

    /// <summary>
    /// Buff生命周期接口 - 持续时间变化
    /// </summary>
    public interface IBuffDurationChange : IBuffLogic
    {
        /// <summary>
        /// 持续时间变化时
        /// </summary>
        void OnDurationChanged(float oldDuration, float newDuration);
    }
}
