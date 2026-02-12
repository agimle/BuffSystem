using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Data;

namespace BuffSystem.Runtime
{
    /// <summary>
    /// 自动频率分配器 - 根据Buff特性自动分配最佳更新频率
    /// </summary>
    /// <remarks>
    /// 🔒 稳定API: v6.0后保证向后兼容
    /// 版本历史: v6.0 新增
    /// 修改策略: 只允许bug修复，不允许破坏性变更
    /// 
    /// 分配策略:
    /// - EveryFrame: 视觉Buff、实时响应Buff
    /// - Every33ms: 高频逻辑Buff、战斗Buff
    /// - Every100ms: 中频逻辑Buff、持续效果Buff
    /// - Every500ms: 低频逻辑Buff、长期Buff
    /// - OnEventOnly: 被动Buff、触发式Buff
    /// </remarks>
    public static class FrequencyAssigner
    {
        // 缓存BuffData到频率的映射，避免重复计算
        private static readonly Dictionary<int, UpdateFrequency> dataIdToFrequencyCache = new();

        /// <summary>
        /// 为Buff自动分配最佳更新频率
        /// </summary>
        /// <param name="buff">Buff实例</param>
        /// <returns>推荐的更新频率</returns>
        public static UpdateFrequency AssignFrequency(IBuff buff)
        {
            if (buff?.Data == null)
                return UpdateFrequency.EveryFrame;

            return AssignFrequency(buff.Data);
        }

        /// <summary>
        /// 为Buff数据自动分配最佳更新频率
        /// </summary>
        /// <param name="data">Buff数据</param>
        /// <returns>推荐的更新频率</returns>
        public static UpdateFrequency AssignFrequency(IBuffData data)
        {
            if (data == null)
                return UpdateFrequency.EveryFrame;

            // 检查缓存
            if (dataIdToFrequencyCache.TryGetValue(data.Id, out var cachedFrequency))
            {
                return cachedFrequency;
            }

            // 计算最佳频率
            var frequency = CalculateOptimalFrequency(data);

            // 缓存结果
            dataIdToFrequencyCache[data.Id] = frequency;

            return frequency;
        }

        /// <summary>
        /// 批量为多个Buff分配频率
        /// </summary>
        /// <param name="buffs">Buff列表</param>
        /// <returns>Buff到频率的映射</returns>
        public static Dictionary<IBuff, UpdateFrequency> AssignFrequenciesBatch(IEnumerable<IBuff> buffs)
        {
            var result = new Dictionary<IBuff, UpdateFrequency>();

            foreach (var buff in buffs)
            {
                if (buff != null)
                {
                    result[buff] = AssignFrequency(buff);
                }
            }

            return result;
        }

        /// <summary>
        /// 清除频率缓存
        /// </summary>
        public static void ClearCache()
        {
            dataIdToFrequencyCache.Clear();

            if (BuffSystemConfig.Instance.EnableDebugLog)
            {
                Debug.Log("[FrequencyAssigner] 频率缓存已清除");
            }
        }

        /// <summary>
        /// 移除指定BuffData的缓存
        /// </summary>
        /// <param name="dataId">BuffData ID</param>
        public static void RemoveFromCache(int dataId)
        {
            dataIdToFrequencyCache.Remove(dataId);
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        public static string GetCacheStats()
        {
            return $"[FrequencyAssigner] 缓存条目数: {dataIdToFrequencyCache.Count}";
        }

        #region Private Methods

        /// <summary>
        /// 计算Buff的最佳更新频率
        /// </summary>
        private static UpdateFrequency CalculateOptimalFrequency(IBuffData data)
        {
            int score = 0;

            // 1. 基于持续时间的评分
            score += CalculateDurationScore(data);

            // 2. 基于标签的评分
            score += CalculateTagScore(data);

            // 3. 基于层数机制的评分
            score += CalculateStackScore(data);

            // 4. 基于效果的评分
            score += CalculateEffectScore(data);

            // 5. 基于刷新机制的评分
            score += CalculateRefreshScore(data);

            // 根据总分分配频率
            return ScoreToFrequency(score);
        }

        /// <summary>
        /// 基于持续时间计算评分
        /// </summary>
        private static int CalculateDurationScore(IBuffData data)
        {
            // 永久Buff或极长持续时间 -> 低频更新
            if (data.IsPermanent)
                return -10;

            if (data.Duration <= 0)
                return -5; // 无持续时间，可能是被动Buff

            if (data.Duration < 1f)
                return 10; // 极短Buff，需要高频更新

            if (data.Duration < 3f)
                return 5; // 短持续时间Buff

            if (data.Duration < 10f)
                return 0; // 中等持续时间

            if (data.Duration < 30f)
                return -3; // 较长持续时间

            return -5; // 长期Buff
        }

        /// <summary>
        /// 基于标签计算评分
        /// </summary>
        private static int CalculateTagScore(IBuffData data)
        {
            int score = 0;
            var tags = data.Tags;

            foreach (var tag in tags)
            {
                var lowerTag = tag.ToLowerInvariant();

                // 高频标签
                if (IsHighFrequencyTag(lowerTag))
                    score += 8;

                // 视觉相关标签
                if (IsVisualTag(lowerTag))
                    score += 10;

                // 被动/触发标签
                if (IsPassiveTag(lowerTag))
                    score -= 15;

                // 长期标签
                if (IsLongTermTag(lowerTag))
                    score -= 5;
            }

            return score;
        }

        /// <summary>
        /// 基于层数机制计算评分
        /// </summary>
        private static int CalculateStackScore(IBuffData data)
        {
            // 可堆叠Buff通常需要更频繁的更新
            if (data.MaxStack > 1)
            {
                // 检查叠加模式
                if (data.StackMode == BuffStackMode.Stackable)
                    return 3;

                if (data.StackMode == BuffStackMode.Independent)
                    return 5;
            }

            return 0;
        }

        /// <summary>
        /// 基于效果计算评分
        /// </summary>
        private static int CalculateEffectScore(IBuffData data)
        {
            int score = 0;

            // 这里可以通过反射或接口检查BuffLogic类型
            // 简化处理：基于Buff名称关键词判断
            var name = data.Name?.ToLowerInvariant() ?? "";

            // 视觉相关
            if (name.Contains("visual") || name.Contains("effect") || name.Contains("particle"))
                score += 10;

            // 伤害/治疗相关（需要及时响应）
            if (name.Contains("damage") || name.Contains("heal") || name.Contains("dot") || name.Contains("hot"))
                score += 8;

            // 控制效果（需要精确时机）
            if (name.Contains("stun") || name.Contains("freeze") || name.Contains("silence"))
                score += 7;

            // 被动效果
            if (name.Contains("passive") || name.Contains("aura") || name.Contains("permanent"))
                score -= 10;

            // 属性加成（通常变化较慢）
            if (name.Contains("buff") || name.Contains("stat") || name.Contains("attribute"))
                score -= 3;

            return score;
        }

        /// <summary>
        /// 基于刷新机制计算评分
        /// </summary>
        private static int CalculateRefreshScore(IBuffData data)
        {
            // 可刷新的Buff通常需要更频繁的更新
            if (data.CanRefresh)
                return 3;

            return 0;
        }

        /// <summary>
        /// 将评分转换为频率
        /// </summary>
        private static UpdateFrequency ScoreToFrequency(int score)
        {
            // 评分范围: -30 ~ +40
            // 高分 = 高频更新
            // 低分 = 低频更新

            if (score >= 15)
                return UpdateFrequency.EveryFrame;

            if (score >= 5)
                return UpdateFrequency.Every33ms;

            if (score >= -5)
                return UpdateFrequency.Every100ms;

            if (score >= -15)
                return UpdateFrequency.Every500ms;

            return UpdateFrequency.OnEventOnly;
        }

        #region Tag Classification

        private static bool IsHighFrequencyTag(string tag)
        {
            return tag.Contains("combat") ||
                   tag.Contains("battle") ||
                   tag.Contains("urgent") ||
                   tag.Contains("realtime") ||
                   tag.Contains("fast");
        }

        private static bool IsVisualTag(string tag)
        {
            return tag.Contains("visual") ||
                   tag.Contains("effect") ||
                   tag.Contains("particle") ||
                   tag.Contains("animation") ||
                   tag.Contains("vfx") ||
                   tag.Contains("ui");
        }

        private static bool IsPassiveTag(string tag)
        {
            return tag.Contains("passive") ||
                   tag.Contains("static") ||
                   tag.Contains("permanent") ||
                   tag.Contains("innate") ||
                   tag.Contains("trait");
        }

        private static bool IsLongTermTag(string tag)
        {
            return tag.Contains("longterm") ||
                   tag.Contains("persistent") ||
                   tag.Contains("duration") ||
                   tag.Contains("buff") ||
                   tag.Contains("debuff");
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// 频率分配策略配置
    /// </summary>
    [System.Serializable]
    public class FrequencyAssignmentConfig
    {
        [Tooltip("短持续时间阈值（秒）")]
        public float shortDurationThreshold = 3f;

        [Tooltip("中等持续时间阈值（秒）")]
        public float mediumDurationThreshold = 10f;

        [Tooltip("长持续时间阈值（秒）")]
        public float longDurationThreshold = 30f;

        [Tooltip("高频标签列表")]
        public List<string> highFrequencyTags = new()
        {
            "combat", "battle", "urgent", "realtime", "fast"
        };

        [Tooltip("视觉标签列表")]
        public List<string> visualTags = new()
        {
            "visual", "effect", "particle", "animation", "vfx", "ui"
        };

        [Tooltip("被动标签列表")]
        public List<string> passiveTags = new()
        {
            "passive", "static", "permanent", "innate", "trait"
        };

        [Tooltip("长期标签列表")]
        public List<string> longTermTags = new()
        {
            "longterm", "persistent", "duration", "buff", "debuff"
        };
    }
}
