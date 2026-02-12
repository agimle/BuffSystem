using System;
using System.Collections.Generic;
using BuffSystem.Core;

namespace BuffSystem.Groups
{
    /// <summary>
    /// Buff组策略枚举
    /// 定义当组内Buff数量超过上限时的处理策略
    /// </summary>
    public enum BuffGroupPolicy
    {
        /// <summary>
        /// 替换最旧的Buff（最早添加的）
        /// </summary>
        ReplaceOldest,
        
        /// <summary>
        /// 替换最弱的Buff（层数最低的）
        /// </summary>
        ReplaceWeakest,
        
        /// <summary>
        /// 阻止新Buff添加
        /// </summary>
        BlockNew,
        
        /// <summary>
        /// 允许所有Buff（不限制）
        /// </summary>
        AllowAll
    }
    
    /// <summary>
    /// Buff组接口
    /// 用于将多个Buff归类到同一组，并应用组策略
    /// </summary>
    /// <remarks>
    /// 使用场景：
    /// - 限制同类型Buff同时存在的数量
    /// - 实现Buff栏位系统
    /// - 管理互斥Buff组
    /// </remarks>
    public interface IBuffGroup
    {
        /// <summary>
        /// 组唯一标识
        /// </summary>
        string GroupId { get; }
        
        /// <summary>
        /// 组内最大Buff数量
        /// </summary>
        int MaxGroupStack { get; }
        
        /// <summary>
        /// 组策略
        /// </summary>
        BuffGroupPolicy Policy { get; }
        
        /// <summary>
        /// 当前组内Buff列表
        /// </summary>
        IReadOnlyList<IBuff> GroupBuffs { get; }
        
        /// <summary>
        /// 添加Buff到组
        /// </summary>
        /// <param name="buff">要添加的Buff</param>
        /// <returns>是否成功添加</returns>
        bool AddToGroup(IBuff buff);
        
        /// <summary>
        /// 从组中移除Buff
        /// </summary>
        /// <param name="buff">要移除的Buff</param>
        void RemoveFromGroup(IBuff buff);
        
        /// <summary>
        /// 检查Buff是否在组内
        /// </summary>
        bool Contains(IBuff buff);
        
        /// <summary>
        /// 获取组内Buff数量
        /// </summary>
        int Count { get; }
        
        /// <summary>
        /// 清空组内所有Buff
        /// </summary>
        void Clear();
    }
    
    /// <summary>
    /// Buff组配置接口
    /// 用于配置Buff所属的组
    /// </summary>
    public interface IBuffGroupConfig
    {
        /// <summary>
        /// 所属组ID列表（一个Buff可以属于多个组）
        /// </summary>
        IReadOnlyList<string> GroupIds { get; }
        
        /// <summary>
        /// 添加组ID
        /// </summary>
        void AddGroupId(string groupId);
        
        /// <summary>
        /// 移除组ID
        /// </summary>
        void RemoveGroupId(string groupId);
        
        /// <summary>
        /// 是否属于指定组
        /// </summary>
        bool BelongsToGroup(string groupId);
    }
}
