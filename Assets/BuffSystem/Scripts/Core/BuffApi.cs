using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Data;
using BuffSystem.Modifiers;
using BuffSystem.Utils;

namespace BuffSystem.Core
{
    /// <summary>
    /// Buff系统对外API
    /// 提供简洁的Buff操作接口
    /// </summary>
    /// <remarks>
    /// 🔒 稳定API: v6.0后保证向后兼容
    /// 版本历史: v1.0-v6.0 逐步完善
    /// 修改策略: 只允许bug修复，不允许破坏性变更
    /// </remarks>
    [StableApi("6.0", VersionHistory = "v1.0-v6.0 逐步完善")]
    public static class BuffApi
    {
        #region Initialization
        
        private static bool isInitialized;

        /// <summary>
        /// 初始化Buff系统
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized) return;
            
            BuffDatabase.Instance.Initialize();
            isInitialized = true;
            
            Debug.Log("[BuffApi] Buff系统初始化完成");
        }
        
        /// <summary>
        /// 重新加载Buff数据
        /// </summary>
        public static void ReloadData()
        {
            BuffDatabase.Instance.Reload();
            Debug.Log("[BuffApi] Buff数据重新加载完成");
        }

        #endregion

        #region Add Buff

        /// <summary>
        /// 添加Buff（通过ID）
        /// </summary>
        /// <param name="buffId">Buff配置ID</param>
        /// <param name="target">目标持有者</param>
        /// <param name="source">Buff来源（可选）</param>
        /// <returns>创建的Buff实例，失败返回null</returns>
        public static IBuff AddBuff(int buffId, IBuffOwner target, object source = null)
        {
            EnsureInitialized();

            if (target == null)
            {
                Debug.LogError("[BuffApi] 添加Buff失败：目标为空");
                return null;
            }

            var data = BuffDatabase.Instance.GetBuffData(buffId);
            if (data == null)
            {
                Debug.LogError($"[BuffApi] 添加Buff失败：未找到ID为 {buffId} 的Buff配置");
                return null;
            }

            return target.BuffContainer.AddBuff(data, source);
        }

        /// <summary>
        /// 添加Buff（通过名称）
        /// </summary>
        /// <param name="buffName">Buff名称</param>
        /// <param name="target">目标持有者</param>
        /// <param name="source">Buff来源（可选）</param>
        /// <returns>创建的Buff实例，失败返回null</returns>
        public static IBuff AddBuff(string buffName, IBuffOwner target, object source = null)
        {
            EnsureInitialized();

            if (target == null)
            {
                Debug.LogError("[BuffApi] 添加Buff失败：目标为空");
                return null;
            }

            var data = BuffDatabase.Instance.GetBuffData(buffName);
            if (data == null)
            {
                Debug.LogError($"[BuffApi] 添加Buff失败：未找到名称为 '{buffName}' 的Buff配置");
                return null;
            }

            return target.BuffContainer.AddBuff(data, source);
        }

        /// <summary>
        /// 添加Buff（通过数据）
        /// </summary>
        /// <param name="data">Buff数据</param>
        /// <param name="target">目标持有者</param>
        /// <param name="source">Buff来源（可选）</param>
        /// <returns>创建的Buff实例，失败返回null</returns>
        public static IBuff AddBuff(IBuffData data, IBuffOwner target, object source = null)
        {
            EnsureInitialized();

            if (data == null)
            {
                Debug.LogError("[BuffApi] 添加Buff失败：数据为空");
                return null;
            }

            if (target == null)
            {
                Debug.LogError("[BuffApi] 添加Buff失败：目标为空");
                return null;
            }

            return target.BuffContainer.AddBuff(data, source);
        }

        #region Add Buff with Modifiers

        /// <summary>
        /// 添加Buff（通过ID，带修饰器）
        /// </summary>
        /// <param name="buffId">Buff配置ID</param>
        /// <param name="target">目标持有者</param>
        /// <param name="modifiers">修饰器列表</param>
        /// <param name="source">Buff来源（可选）</param>
        /// <returns>创建的Buff实例，失败返回null</returns>
        public static IBuff AddBuff(int buffId, IBuffOwner target, IEnumerable<IBuffModifier> modifiers, object source = null)
        {
            EnsureInitialized();

            if (target == null)
            {
                Debug.LogError("[BuffApi] 添加Buff失败：目标为空");
                return null;
            }

            var data = BuffDatabase.Instance.GetBuffData(buffId);
            if (data == null)
            {
                Debug.LogError($"[BuffApi] 添加Buff失败：未找到ID为 {buffId} 的Buff配置");
                return null;
            }

            return target.BuffContainer.AddBuff(data, source, modifiers);
        }

        /// <summary>
        /// 添加Buff（通过名称，带修饰器）
        /// </summary>
        /// <param name="buffName">Buff名称</param>
        /// <param name="target">目标持有者</param>
        /// <param name="modifiers">修饰器列表</param>
        /// <param name="source">Buff来源（可选）</param>
        /// <returns>创建的Buff实例，失败返回null</returns>
        public static IBuff AddBuff(string buffName, IBuffOwner target, IEnumerable<IBuffModifier> modifiers, object source = null)
        {
            EnsureInitialized();

            if (target == null)
            {
                Debug.LogError("[BuffApi] 添加Buff失败：目标为空");
                return null;
            }

            var data = BuffDatabase.Instance.GetBuffData(buffName);
            if (data == null)
            {
                Debug.LogError($"[BuffApi] 添加Buff失败：未找到名称为 '{buffName}' 的Buff配置");
                return null;
            }

            return target.BuffContainer.AddBuff(data, source, modifiers);
        }

        /// <summary>
        /// 添加Buff（通过数据，带修饰器）
        /// </summary>
        /// <param name="data">Buff数据</param>
        /// <param name="target">目标持有者</param>
        /// <param name="modifiers">修饰器列表</param>
        /// <param name="source">Buff来源（可选）</param>
        /// <returns>创建的Buff实例，失败返回null</returns>
        public static IBuff AddBuff(IBuffData data, IBuffOwner target, IEnumerable<IBuffModifier> modifiers, object source = null)
        {
            EnsureInitialized();

            if (data == null)
            {
                Debug.LogError("[BuffApi] 添加Buff失败：数据为空");
                return null;
            }

            if (target == null)
            {
                Debug.LogError("[BuffApi] 添加Buff失败：目标为空");
                return null;
            }

            return target.BuffContainer.AddBuff(data, source, modifiers);
        }

        #endregion

        /// <summary>
        /// 尝试添加Buff（通过ID）
        /// </summary>
        /// <returns>是否成功添加</returns>
        public static bool TryAddBuff(int buffId, IBuffOwner target, out IBuff buff, object source = null)
        {
            buff = AddBuff(buffId, target, source);
            return buff != null;
        }

        /// <summary>
        /// 尝试添加Buff（通过名称）
        /// </summary>
        /// <returns>是否成功添加</returns>
        public static bool TryAddBuff(string buffName, IBuffOwner target, out IBuff buff, object source = null)
        {
            buff = AddBuff(buffName, target, source);
            return buff != null;
        }

        #endregion

        #region Remove Buff

        /// <summary>
        /// 移除Buff实例
        /// </summary>
        public static void RemoveBuff(IBuff buff)
        {
            buff?.Owner?.BuffContainer?.RemoveBuff(buff);
        }

        /// <summary>
        /// 移除指定ID的所有Buff
        /// </summary>
        public static void RemoveBuff(int buffId, IBuffOwner target)
        {
            target?.BuffContainer?.RemoveBuff(buffId);
        }

        /// <summary>
        /// 移除指定名称的所有Buff
        /// </summary>
        public static void RemoveBuff(string buffName, IBuffOwner target)
        {
            if (target?.BuffContainer == null) return;

            int buffId = BuffDatabase.Instance.GetBuffId(buffName);
            if (buffId >= 0)
            {
                target.BuffContainer.RemoveBuff(buffId);
            }
        }

        /// <summary>
        /// 移除指定来源的所有Buff
        /// </summary>
        public static void RemoveBuffBySource(object source, IBuffOwner target)
        {
            target?.BuffContainer?.RemoveBuffBySource(source);
        }

        /// <summary>
        /// 清空所有Buff
        /// </summary>
        public static void ClearBuffs(IBuffOwner target)
        {
            target?.BuffContainer?.ClearAllBuffs();
        }

        #endregion

        #region Query

        /// <summary>
        /// 是否拥有指定ID的Buff
        /// </summary>
        public static bool HasBuff(int buffId, IBuffOwner target)
        {
            return target?.BuffContainer != null && target.BuffContainer.HasBuff(buffId);
        }

        /// <summary>
        /// 是否拥有指定名称的Buff
        /// </summary>
        public static bool HasBuff(string buffName, IBuffOwner target)
        {
            if (target?.BuffContainer == null) return false;

            int buffId = BuffDatabase.Instance.GetBuffId(buffName);
            return buffId >= 0 && target.BuffContainer.HasBuff(buffId);
        }

        /// <summary>
        /// 是否拥有指定来源的Buff
        /// </summary>
        public static bool HasBuff(int buffId, object source, IBuffOwner target)
        {
            return target?.BuffContainer != null && target.BuffContainer.HasBuff(buffId, source);
        }
        
        /// <summary>
        /// 获取Buff
        /// </summary>
        public static IBuff GetBuff(int buffId, IBuffOwner target, object source = null)
        {
            return target?.BuffContainer?.GetBuff(buffId, source);
        }
        
        /// <summary>
        /// 获取Buff
        /// </summary>
        public static IBuff GetBuff(string buffName, IBuffOwner target, object source = null)
        {
            if (target?.BuffContainer == null) return null;
            
            int buffId = BuffDatabase.Instance.GetBuffId(buffName);
            if (buffId < 0) return null;
            
            return target.BuffContainer.GetBuff(buffId, source);
        }
        
        /// <summary>
        /// 获取所有指定ID的Buff
        /// </summary>
        public static IEnumerable<IBuff> GetBuffs(int buffId, IBuffOwner target)
        {
            return target?.BuffContainer != null ? target.BuffContainer.GetBuffs(buffId) : System.Array.Empty<IBuff>();
        }
        
        /// <summary>
        /// 获取所有指定名称的Buff
        /// </summary>
        public static IEnumerable<IBuff> GetBuffs(string buffName, IBuffOwner target)
        {
            if (target?.BuffContainer == null) return System.Array.Empty<IBuff>();
            
            int buffId = BuffDatabase.Instance.GetBuffId(buffName);
            if (buffId < 0) return System.Array.Empty<IBuff>();
            
            return target.BuffContainer.GetBuffs(buffId);
        }
        
        /// <summary>
        /// 获取所有Buff
        /// </summary>
        public static IReadOnlyCollection<IBuff> GetAllBuffs(IBuffOwner target)
        {
            return target?.BuffContainer != null ? target.BuffContainer.AllBuffs : System.Array.Empty<IBuff>();
        }
        
        /// <summary>
        /// 获取Buff数量
        /// </summary>
        public static int GetBuffCount(IBuffOwner target)
        {
            return target?.BuffContainer?.AllBuffs.Count ?? 0;
        }

        #endregion

        #region Tag Query

        /// <summary>
        /// 根据标签获取所有Buff（使用yield return，无GC Alloc）
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="target">目标持有者</param>
        /// <returns>拥有该标签的所有Buff</returns>
        public static IEnumerable<IBuff> GetBuffsByTag(string tag, IBuffOwner target)
        {
            return target.GetBuffsByTag(tag);
        }

        /// <summary>
        /// 根据标签获取所有Buff（非分配版本，适合高频调用）
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="target">目标持有者</param>
        /// <param name="result">结果列表（会被清空）</param>
        public static void GetBuffsByTagNonAlloc(string tag, IBuffOwner target, List<IBuff> result)
        {
            target?.BuffContainer?.AllBuffs.FilterByTagNonAlloc(tag, result);
        }

        /// <summary>
        /// 根据标签移除Buff
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="target">目标持有者</param>
        public static void RemoveBuffsByTag(string tag, IBuffOwner target)
        {
            if (target?.BuffContainer == null || string.IsNullOrEmpty(tag))
            {
                return;
            }

            var buffsToRemove = target.GetBuffsByTag(tag);
            foreach (var buff in buffsToRemove)
            {
                RemoveBuff(buff);
            }
        }

        /// <summary>
        /// 是否拥有指定标签的Buff
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="target">目标持有者</param>
        /// <returns>是否拥有</returns>
        public static bool HasBuffWithTag(string tag, IBuffOwner target)
        {
            return target.HasBuffWithTag(tag);
        }

        /// <summary>
        /// 获取指定标签的Buff数量
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="target">目标持有者</param>
        /// <returns>Buff数量</returns>
        public static int GetBuffCountByTag(string tag, IBuffOwner target)
        {
            return target.GetBuffCountByTag(tag);
        }

        #endregion

        #region Data Query
        
        /// <summary>
        /// 获取Buff数据（通过ID）
        /// </summary>
        public static IBuffData GetBuffData(int buffId)
        {
            EnsureInitialized();
            return BuffDatabase.Instance.GetBuffData(buffId);
        }
        
        /// <summary>
        /// 获取Buff数据（通过名称）
        /// </summary>
        public static IBuffData GetBuffData(string buffName)
        {
            EnsureInitialized();
            return BuffDatabase.Instance.GetBuffData(buffName);
        }

        /// <summary>
        /// 是否存在Buff数据
        /// </summary>
        public static bool HasBuffData(int buffId)
        {
            EnsureInitialized();
            return BuffDatabase.Instance.ContainsBuff(buffId);
        }
        
        /// <summary>
        /// 是否存在Buff数据
        /// </summary>
        public static bool HasBuffData(string buffName)
        {
            EnsureInitialized();
            return BuffDatabase.Instance.ContainsBuff(buffName);
        }
        
        /// <summary>
        /// 获取所有Buff数据
        /// </summary>
        public static IEnumerable<IBuffData> GetAllBuffData()
        {
            EnsureInitialized();
            return BuffDatabase.Instance.GetAllBuffData();
        }
        
        #endregion
        
        #region Utility
        
        /// <summary>
        /// 刷新Buff持续时间
        /// </summary>
        public static void RefreshBuff(IBuff buff)
        {
            buff?.RefreshDuration();
        }
        
        /// <summary>
        /// 增加Buff层数
        /// </summary>
        public static void AddStack(IBuff buff, int amount)
        {
            buff?.AddStack(amount);
        }
        
        /// <summary>
        /// 减少Buff层数
        /// </summary>
        public static void RemoveStack(IBuff buff, int amount)
        {
            buff?.RemoveStack(amount);
        }

        #endregion

        #region Private Methods
        
        private static void EnsureInitialized()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }
        
        #endregion
    }
}
