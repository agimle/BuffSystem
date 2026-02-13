using System.Collections.Generic;

namespace BuffSystem.Data
{
    /// <summary>
    /// Buff标签管理器 - 全局标签哈希缓存
    /// 提供线程安全的标签哈希缓存，避免重复计算
    /// </summary>
    public static class BuffTagManager
    {
        // 标签哈希缓存字典
        private static readonly Dictionary<string, int> TagHashCache = new();

        // 空标签的哈希值
        private static readonly int EmptyHash = string.Empty.GetHashCode();

        /// <summary>
        /// 获取标签的哈希值（带缓存）
        /// </summary>
        /// <param name="tagName">标签名称</param>
        /// <returns>哈希值</returns>
        public static int GetTagHash(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                return EmptyHash;
            }

            // 尝试从缓存获取
            if (!TagHashCache.TryGetValue(tagName, out var hash))
            {
                // 计算哈希并缓存
                hash = tagName.GetHashCode();
                TagHashCache[tagName] = hash;
            }

            return hash;
        }

        /// <summary>
        /// 预缓存多个标签的哈希值
        /// 适用于初始化时批量缓存常用标签
        /// </summary>
        /// <param name="tagNames">标签名称列表</param>
        public static void PreCacheTags(IEnumerable<string> tagNames)
        {
            if (tagNames == null) return;

            foreach (var tagName in tagNames)
            {
                if (!string.IsNullOrEmpty(tagName) && !TagHashCache.ContainsKey(tagName))
                {
                    TagHashCache[tagName] = tagName.GetHashCode();
                }
            }
        }

        /// <summary>
        /// 预缓存单个标签
        /// </summary>
        /// <param name="tagName">标签名称</param>
        public static void PreCacheTag(string tagName)
        {
            if (!string.IsNullOrEmpty(tagName) && !TagHashCache.ContainsKey(tagName))
            {
                TagHashCache[tagName] = tagName.GetHashCode();
            }
        }

        /// <summary>
        /// 检查标签是否已缓存
        /// </summary>
        /// <param name="tagName">标签名称</param>
        /// <returns>是否已缓存</returns>
        public static bool IsTagCached(string tagName)
        {
            return !string.IsNullOrEmpty(tagName) && TagHashCache.ContainsKey(tagName);
        }

        /// <summary>
        /// 从缓存中移除标签
        /// </summary>
        /// <param name="tagName">标签名称</param>
        public static void RemoveFromCache(string tagName)
        {
            if (!string.IsNullOrEmpty(tagName))
            {
                TagHashCache.Remove(tagName);
            }
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public static void ClearCache()
        {
            TagHashCache.Clear();
        }

        /// <summary>
        /// 获取缓存的标签数量
        /// </summary>
        public static int CachedTagCount => TagHashCache.Count;
    }
}
