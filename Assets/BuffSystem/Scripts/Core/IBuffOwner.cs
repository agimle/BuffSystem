using System.Collections.Generic;
using BuffSystem.Events;
using BuffSystem.Groups;
using BuffSystem.Modifiers;

namespace BuffSystem.Core
{
    /// <summary>
    /// Buff持有者接口 - 解耦MonoBehaviour依赖
    /// 任何需要持有Buff的对象都可以实现此接口
    /// </summary>
    /// <remarks>
    /// 🔒 稳定API: v6.0后保证向后兼容
    /// 版本历史: v1.0-v6.0 逐步完善
    /// 修改策略: 只允许bug修复，不允许破坏性变更
    /// </remarks>
    [StableApi("6.0", VersionHistory = "v1.0-v6.0 逐步完善")]
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
        /// 本地事件系统
        /// </summary>
        BuffLocalEventSystem LocalEvents { get; }
        
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
    /// <remarks>
    /// 🔒 稳定API: v6.0后保证向后兼容
    /// 版本历史: v1.0-v6.0 逐步完善
    /// 修改策略: 只允许bug修复，不允许破坏性变更
    /// </remarks>
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
        /// 添加Buff（带修饰器）
        /// </summary>
        IBuff AddBuff(IBuffData data, object source, IEnumerable<IBuffModifier> modifiers);
        
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
        
        #region Buff Groups
        
        /// <summary>
        /// 注册Buff组
        /// </summary>
        void RegisterBuffGroup(IBuffGroup group);
        
        /// <summary>
        /// 获取Buff组
        /// </summary>
        IBuffGroup GetBuffGroup(string groupId);
        
        /// <summary>
        /// 移除Buff组
        /// </summary>
        void RemoveBuffGroup(string groupId);
        
        /// <summary>
        /// 检查是否存在指定组
        /// </summary>
        bool HasBuffGroup(string groupId);
        
        /// <summary>
        /// 将Buff添加到组
        /// </summary>
        bool AddBuffToGroup(IBuff buff, string groupId);
        
        /// <summary>
        /// 从组中移除Buff
        /// </summary>
        void RemoveBuffFromGroup(IBuff buff, string groupId);
        
        /// <summary>
        /// 从所有组中移除Buff
        /// </summary>
        void RemoveBuffFromAllGroups(IBuff buff);
        
        /// <summary>
        /// 清空所有组
        /// </summary>
        void ClearAllGroups();
        
        #endregion
    }
}
