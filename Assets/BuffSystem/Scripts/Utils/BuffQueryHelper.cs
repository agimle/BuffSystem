using System.Collections.Generic;
using BuffSystem.Core;

namespace BuffSystem.Utils
{
    /// <summary>
    /// Buff查询辅助工具类
    /// 提供Buff相关的扩展方法和查询工具
    /// </summary>
    public static class BuffQueryHelper
    {
        #region IBuffData Extensions

        /// <summary>
        /// 检查Buff数据是否拥有指定标签
        /// </summary>
        /// <param name="data">Buff数据</param>
        /// <param name="tag">标签</param>
        /// <returns>是否拥有该标签</returns>
        public static bool HasTag(this IBuffData data, string tag)
        {
            if (data == null || string.IsNullOrEmpty(tag) || data.Tags == null)
            {
                return false;
            }

            for (int i = 0; i < data.Tags.Count; i++)
            {
                if (data.Tags[i] == tag)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查Buff数据是否拥有任一指定标签
        /// </summary>
        /// <param name="data">Buff数据</param>
        /// <param name="tags">标签列表</param>
        /// <returns>是否拥有任一标签</returns>
        public static bool HasAnyTag(this IBuffData data, IEnumerable<string> tags)
        {
            if (data == null || tags == null)
            {
                return false;
            }

            foreach (var tag in tags)
            {
                if (data.HasTag(tag))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查Buff数据是否拥有所有指定标签
        /// </summary>
        /// <param name="data">Buff数据</param>
        /// <param name="tags">标签列表</param>
        /// <returns>是否拥有所有标签</returns>
        public static bool HasAllTags(this IBuffData data, IEnumerable<string> tags)
        {
            if (data == null || tags == null)
            {
                return false;
            }

            foreach (var tag in tags)
            {
                if (!data.HasTag(tag))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region IEnumerable<IBuff> Extensions

        /// <summary>
        /// 根据标签筛选Buff（使用yield return，无GC Alloc）
        /// </summary>
        /// <param name="buffs">Buff集合</param>
        /// <param name="tag">标签</param>
        /// <returns>拥有该标签的Buff</returns>
        public static IEnumerable<IBuff> FilterByTag(this IEnumerable<IBuff> buffs, string tag)
        {
            if (buffs == null || string.IsNullOrEmpty(tag))
            {
                yield break;
            }

            foreach (var buff in buffs)
            {
                if (buff?.Data != null && buff.Data.HasTag(tag))
                {
                    yield return buff;
                }
            }
        }

        /// <summary>
        /// 根据Buff ID筛选Buff
        /// </summary>
        /// <param name="buffs">Buff集合</param>
        /// <param name="buffId">Buff ID</param>
        /// <returns>指定ID的Buff</returns>
        public static IEnumerable<IBuff> FilterById(this IEnumerable<IBuff> buffs, int buffId)
        {
            if (buffs == null)
            {
                yield break;
            }

            foreach (var buff in buffs)
            {
                if (buff != null && buff.DataId == buffId)
                {
                    yield return buff;
                }
            }
        }

        /// <summary>
        /// 根据来源筛选Buff
        /// </summary>
        /// <param name="buffs">Buff集合</param>
        /// <param name="source">来源对象</param>
        /// <returns>指定来源的Buff</returns>
        public static IEnumerable<IBuff> FilterBySource(this IEnumerable<IBuff> buffs, object source)
        {
            if (buffs == null || source == null)
            {
                yield break;
            }

            foreach (var buff in buffs)
            {
                if (buff != null && buff.Source == source)
                {
                    yield return buff;
                }
            }
        }

        /// <summary>
        /// 根据标签筛选Buff（非分配版本，适合高频调用）
        /// </summary>
        /// <param name="buffs">Buff集合</param>
        /// <param name="tag">标签</param>
        /// <param name="result">结果列表（会被清空）</param>
        public static void FilterByTagNonAlloc(this IEnumerable<IBuff> buffs, string tag, List<IBuff> result)
        {
            result.Clear();
            if (buffs == null || string.IsNullOrEmpty(tag))
            {
                return;
            }

            foreach (var buff in buffs)
            {
                if (buff?.Data != null && buff.Data.HasTag(tag))
                {
                    result.Add(buff);
                }
            }
        }

        /// <summary>
        /// 获取第一个匹配标签的Buff
        /// </summary>
        /// <param name="buffs">Buff集合</param>
        /// <param name="tag">标签</param>
        /// <returns>第一个匹配的Buff，未找到返回null</returns>
        public static IBuff FirstByTag(this IEnumerable<IBuff> buffs, string tag)
        {
            if (buffs == null || string.IsNullOrEmpty(tag))
            {
                return null;
            }

            foreach (var buff in buffs)
            {
                if (buff?.Data != null && buff.Data.HasTag(tag))
                {
                    return buff;
                }
            }
            return null;
        }

        /// <summary>
        /// 检查是否包含指定标签的Buff
        /// </summary>
        /// <param name="buffs">Buff集合</param>
        /// <param name="tag">标签</param>
        /// <returns>是否包含</returns>
        public static bool ContainsTag(this IEnumerable<IBuff> buffs, string tag)
        {
            if (buffs == null || string.IsNullOrEmpty(tag))
            {
                return false;
            }

            foreach (var buff in buffs)
            {
                if (buff?.Data != null && buff.Data.HasTag(tag))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 统计指定标签的Buff数量
        /// </summary>
        /// <param name="buffs">Buff集合</param>
        /// <param name="tag">标签</param>
        /// <returns>Buff数量</returns>
        public static int CountByTag(this IEnumerable<IBuff> buffs, string tag)
        {
            if (buffs == null || string.IsNullOrEmpty(tag))
            {
                return 0;
            }

            int count = 0;
            foreach (var buff in buffs)
            {
                if (buff?.Data != null && buff.Data.HasTag(tag))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 获取所有Buff的总层数
        /// </summary>
        /// <param name="buffs">Buff集合</param>
        /// <returns>总层数</returns>
        public static int GetTotalStackCount(this IEnumerable<IBuff> buffs)
        {
            if (buffs == null)
            {
                return 0;
            }

            int total = 0;
            foreach (var buff in buffs)
            {
                if (buff != null)
                {
                    total += buff.CurrentStack;
                }
            }
            return total;
        }

        /// <summary>
        /// 获取指定标签Buff的总层数
        /// </summary>
        /// <param name="buffs">Buff集合</param>
        /// <param name="tag">标签</param>
        /// <returns>总层数</returns>
        public static int GetTotalStackCountByTag(this IEnumerable<IBuff> buffs, string tag)
        {
            if (buffs == null || string.IsNullOrEmpty(tag))
            {
                return 0;
            }

            int total = 0;
            foreach (var buff in buffs)
            {
                if (buff != null && buff.Data != null && buff.Data.HasTag(tag))
                {
                    total += buff.CurrentStack;
                }
            }
            return total;
        }

        #endregion

        #region IBuffOwner Extensions

        /// <summary>
        /// 获取持有者的所有Buff
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <returns>Buff集合</returns>
        public static IEnumerable<IBuff> GetAllBuffs(this IBuffOwner owner)
        {
            return owner?.BuffContainer?.AllBuffs ?? System.Array.Empty<IBuff>();
        }

        /// <summary>
        /// 检查持有者是否拥有指定标签的Buff
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <param name="tag">标签</param>
        /// <returns>是否拥有</returns>
        public static bool HasBuffWithTag(this IBuffOwner owner, string tag)
        {
            return owner?.BuffContainer?.AllBuffs.ContainsTag(tag) ?? false;
        }

        /// <summary>
        /// 获取持有者指定标签的Buff
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <param name="tag">标签</param>
        /// <returns>Buff集合</returns>
        public static IEnumerable<IBuff> GetBuffsByTag(this IBuffOwner owner, string tag)
        {
            return owner?.BuffContainer?.AllBuffs.FilterByTag(tag) ?? System.Array.Empty<IBuff>();
        }

        /// <summary>
        /// 获取持有者指定标签的Buff数量
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <param name="tag">标签</param>
        /// <returns>Buff数量</returns>
        public static int GetBuffCountByTag(this IBuffOwner owner, string tag)
        {
            return owner?.BuffContainer?.AllBuffs.CountByTag(tag) ?? 0;
        }

        #endregion

        #region Condition Helpers

        /// <summary>
        /// 检查所有条件是否满足（AND逻辑）
        /// </summary>
        /// <param name="conditions">条件列表</param>
        /// <param name="owner">Buff持有者</param>
        /// <param name="data">Buff数据</param>
        /// <returns>是否全部满足</returns>
        public static bool CheckAllConditions(this IEnumerable<IBuffCondition> conditions, IBuffOwner owner, IBuffData data)
        {
            if (conditions == null)
            {
                return true;
            }

            foreach (var condition in conditions)
            {
                if (condition != null && !condition.Check(owner, data))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 检查是否有任一条件满足（OR逻辑）
        /// </summary>
        /// <param name="conditions">条件列表</param>
        /// <param name="owner">Buff持有者</param>
        /// <param name="data">Buff数据</param>
        /// <returns>是否有任一满足</returns>
        public static bool CheckAnyCondition(this IEnumerable<IBuffCondition> conditions, IBuffOwner owner, IBuffData data)
        {
            if (conditions == null)
            {
                return true;
            }

            foreach (var condition in conditions)
            {
                if (condition != null && condition.Check(owner, data))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
