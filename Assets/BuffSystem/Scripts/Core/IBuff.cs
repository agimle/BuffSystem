namespace BuffSystem.Core
{
    /// <summary>
    /// Buff实例接口 - 运行时Buff实体的抽象
    /// </summary>
    public interface IBuff
    {
        /// <summary>
        /// Buff唯一标识符（实例ID）
        /// </summary>
        int InstanceId { get; }
        
        /// <summary>
        /// Buff数据ID（配置ID）
        /// </summary>
        int DataId { get; }
        
        /// <summary>
        /// Buff名称
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// 当前层数
        /// </summary>
        int CurrentStack { get; }
        
        /// <summary>
        /// 最大层数
        /// </summary>
        int MaxStack { get; }
        
        /// <summary>
        /// 当前持续时间
        /// </summary>
        float Duration { get; }
        
        /// <summary>
        /// 总持续时间
        /// </summary>
        float TotalDuration { get; }
        
        /// <summary>
        /// 剩余时间
        /// </summary>
        float RemainingTime { get; }
        
        /// <summary>
        /// 是否永久Buff
        /// </summary>
        bool IsPermanent { get; }
        
        /// <summary>
        /// 是否标记为移除
        /// </summary>
        bool IsMarkedForRemoval { get; }
        
        /// <summary>
        /// Buff来源（可以是技能、道具、角色、环境等）
        /// </summary>
        object Source { get; }
        
        /// <summary>
        /// 来源ID（用于快速比较，避免装箱）
        /// </summary>
        int SourceId { get; }
        
        /// <summary>
        /// 所属持有者
        /// </summary>
        IBuffOwner Owner { get; }
        
        /// <summary>
        /// Buff数据引用
        /// </summary>
        IBuffData Data { get; }
        
        /// <summary>
        /// 获取来源并转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <returns>转换后的对象，如果来源为null或类型不匹配则返回null</returns>
        T GetSource<T>() where T : class;
        
        /// <summary>
        /// 尝试获取来源并转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="source">转换后的对象</param>
        /// <returns>是否成功转换</returns>
        bool TryGetSource<T>(out T source) where T : class;
        
        /// <summary>
        /// 增加层数
        /// </summary>
        void AddStack(int amount);
        
        /// <summary>
        /// 减少层数
        /// </summary>
        void RemoveStack(int amount);
        
        /// <summary>
        /// 刷新持续时间
        /// </summary>
        void RefreshDuration();
        
        /// <summary>
        /// 标记为移除
        /// </summary>
        void MarkForRemoval();
        
        /// <summary>
        /// 重置Buff（用于对象池）
        /// </summary>
        void Reset(IBuffData data, IBuffOwner owner, object source);
    }
    
    /// <summary>
    /// 泛型Buff接口 - 提供类型安全的Source访问
    /// </summary>
    public interface IBuff<out TSource> : IBuff where TSource : class
    {
        /// <summary>
        /// 类型安全的Buff来源
        /// </summary>
        new TSource Source { get; }
    }
}
