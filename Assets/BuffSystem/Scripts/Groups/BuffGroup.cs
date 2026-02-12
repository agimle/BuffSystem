using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Runtime;

namespace BuffSystem.Groups
{
    /// <summary>
    /// Buff组实现类
    /// 管理一组Buff并应用组策略
    /// </summary>
    [Serializable]
    public class BuffGroup : IBuffGroup
    {
        [SerializeField] private string groupId;
        [SerializeField] private int maxGroupStack = 5;
        [SerializeField] private BuffGroupPolicy policy = BuffGroupPolicy.ReplaceOldest;
        
        // 组内Buff列表（按添加顺序）
        private readonly List<IBuff> buffs = new();
        
        /// <summary>
        /// 组唯一标识
        /// </summary>
        public string GroupId => groupId;
        
        /// <summary>
        /// 组内最大Buff数量
        /// </summary>
        public int MaxGroupStack => maxGroupStack;
        
        /// <summary>
        /// 组策略
        /// </summary>
        public BuffGroupPolicy Policy => policy;
        
        /// <summary>
        /// 当前组内Buff列表（只读）
        /// </summary>
        public IReadOnlyList<IBuff> GroupBuffs => buffs;
        
        /// <summary>
        /// 当前组内Buff数量
        /// </summary>
        public int Count => buffs.Count;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public BuffGroup(string groupId, int maxGroupStack = 5, BuffGroupPolicy policy = BuffGroupPolicy.ReplaceOldest)
        {
            this.groupId = groupId ?? throw new ArgumentNullException(nameof(groupId));
            this.maxGroupStack = Mathf.Max(1, maxGroupStack);
            this.policy = policy;
        }
        
        /// <summary>
        /// 添加Buff到组
        /// 根据组策略处理超出限制的情况
        /// </summary>
        public bool AddToGroup(IBuff buff)
        {
            if (buff == null) return false;
            if (buffs.Contains(buff)) return true; // 已在组内
            
            // AllowAll策略：直接添加
            if (policy == BuffGroupPolicy.AllowAll)
            {
                buffs.Add(buff);
                return true;
            }
            
            // BlockNew策略：检查是否已满
            if (policy == BuffGroupPolicy.BlockNew && buffs.Count >= maxGroupStack)
            {
                Debug.Log($"[BuffGroup] 组 '{groupId}' 已满，阻止新Buff添加");
                return false;
            }
            
            // ReplaceOldest策略：移除最旧的
            if (policy == BuffGroupPolicy.ReplaceOldest && buffs.Count >= maxGroupStack)
            {
                RemoveOldestBuff();
            }
            
            // ReplaceWeakest策略：移除最弱的
            if (policy == BuffGroupPolicy.ReplaceWeakest && buffs.Count >= maxGroupStack)
            {
                RemoveWeakestBuff();
            }
            
            buffs.Add(buff);
            return true;
        }
        
        /// <summary>
        /// 从组中移除Buff
        /// </summary>
        public void RemoveFromGroup(IBuff buff)
        {
            if (buff == null) return;
            buffs.Remove(buff);
        }
        
        /// <summary>
        /// 检查Buff是否在组内
        /// </summary>
        public bool Contains(IBuff buff)
        {
            return buff != null && buffs.Contains(buff);
        }
        
        /// <summary>
        /// 清空组内所有Buff
        /// </summary>
        public void Clear()
        {
            buffs.Clear();
        }
        
        /// <summary>
        /// 移除最旧的Buff（最早添加的）
        /// </summary>
        private void RemoveOldestBuff()
        {
            if (buffs.Count == 0) return;
            
            var oldest = buffs[0];
            buffs.RemoveAt(0);
            
            // 标记Buff为待移除（由BuffContainer实际移除）
            if (oldest is BuffEntity entity)
            {
                entity.MarkForRemoval();
            }
            
            Debug.Log($"[BuffGroup] 组 '{groupId}' 移除最旧Buff: {oldest.Name}");
        }
        
        /// <summary>
        /// 移除最弱的Buff（层数最低的）
        /// </summary>
        private void RemoveWeakestBuff()
        {
            if (buffs.Count == 0) return;
            
            // 找到层数最低的Buff
            IBuff weakest = buffs[0];
            int minStack = weakest.CurrentStack;
            
            foreach (var buff in buffs)
            {
                if (buff.CurrentStack < minStack)
                {
                    minStack = buff.CurrentStack;
                    weakest = buff;
                }
            }
            
            buffs.Remove(weakest);
            
            // 标记Buff为待移除
            if (weakest is BuffEntity entity)
            {
                entity.MarkForRemoval();
            }
            
            Debug.Log($"[BuffGroup] 组 '{groupId}' 移除最弱Buff: {weakest.Name} (层数:{minStack})");
        }
        
        /// <summary>
        /// 获取组内指定ID的Buff
        /// </summary>
        public IBuff GetBuff(int dataId)
        {
            foreach (var buff in buffs)
            {
                if (buff.DataId == dataId)
                    return buff;
            }
            return null;
        }
        
        /// <summary>
        /// 获取组内所有指定ID的Buff
        /// </summary>
        public IEnumerable<IBuff> GetBuffs(int dataId)
        {
            return buffs.Where(b => b.DataId == dataId);
        }
        
        /// <summary>
        /// 检查组是否已满
        /// </summary>
        public bool IsFull => buffs.Count >= maxGroupStack;
        
        /// <summary>
        /// 检查是否还能添加Buff
        /// </summary>
        public bool CanAddBuff
        {
            get
            {
                if (policy == BuffGroupPolicy.AllowAll) return true;
                if (policy == BuffGroupPolicy.BlockNew) return !IsFull;
                return true; // ReplaceOldest 和 ReplaceWeakest 总是可以添加
            }
        }
    }
}
