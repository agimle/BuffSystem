using System.Collections.Generic;

namespace BuffSystem.Core
{
    /// <summary>
    /// Buff持有者接口 - 解耦MonoBehaviour依赖
    /// 任何需要持有Buff的对象都可以实现此接口
    /// </summary>
    public interface IBuffOwner
    {
        /// <summary>
        /// 持有者唯一标识
        /// </summary>
        int OwnerId { get; }

        /// <summary>
        /// 持有者名称（用于调试）
        /// </summary>
        string OwnerName { get; }

        /// <summary>
        /// 获取Buff容器
        /// </summary>
        IBuffContainer BuffContainer { get; }
        
        /// <summary>
        /// 当Buff事件发生时调用
        /// </summary>
        void OnBuffEvent(BuffEventType eventType, IBuff buff);
        
        #region Immunity System (v4.0)
        
        /// <summary>
        /// 检查是否对指定Buff免疫
        /// </summary>
        /// <param name="buffId">Buff ID</param>
        /// <returns>是否免疫</returns>
        bool IsImmuneTo(int buffId);
        
        /// <summary>
        /// 检查是否对指定标签免疫
        /// </summary>
        /// <param name="tag">标签</param>
        /// <returns>是否免疫</returns>
        bool IsImmuneToTag(string tag);
        
        /// <summary>
        /// 获取免疫标签列表
        /// </summary>
        IReadOnlyList<string> ImmuneTags { get; }
        
        #endregion
    }
    
    /// <summary>
    /// Buff容器接口 - 管理Buff的添加、移除、查询
    /// </summary>
    public interface IBuffContainer
    {
        /// <summary>
        /// 持有者引用
        /// </summary>
        IBuffOwner Owner { get; }
        
        /// <summary>
        /// 当前所有Buff
        /// </summary>
        IReadOnlyCollection<IBuff> AllBuffs { get; }
        
        /// <summary>
        /// 添加Buff
        /// </summary>
        IBuff AddBuff(IBuffData data, object source = null);
        
        /// <summary>
        /// 移除Buff
        /// </summary>
        void RemoveBuff(IBuff buff);
        
        /// <summary>
        /// 根据ID移除Buff
        /// </summary>
        void RemoveBuff(int dataId);
        
        /// <summary>
        /// 根据来源移除Buff
        /// </summary>
        void RemoveBuffBySource(object source);

        /// <summary>
        /// 清空所有Buff
        /// </summary>
        void ClearAllBuffs();

        /// <summary>
        /// 获取Buff
        /// </summary>
        IBuff GetBuff(int dataId, object source = null);
        
        /// <summary>
        /// 获取所有指定ID的Buff
        /// </summary>
        IEnumerable<IBuff> GetBuffs(int dataId);

        /// <summary>
        /// 获取所有指定来源的Buff
        /// </summary>
        IEnumerable<IBuff> GetBuffsBySource(object source);
        
        /// <summary>
        /// 是否拥有指定Buff
        /// </summary>
        bool HasBuff(int dataId);

        /// <summary>
        /// 是否拥有指定来源的Buff
        /// </summary>
        bool HasBuff(int dataId, object source);

        /// <summary>
        /// 每帧更新
        /// </summary>
        void Update(float deltaTime);
    }
}
