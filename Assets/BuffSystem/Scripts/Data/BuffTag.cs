using System;
using UnityEngine;

namespace BuffSystem.Data
{
    /// <summary>
    /// Buff标签 - 使用哈希优化性能
    /// 替代string标签，提供O(1)的哈希比较性能
    /// </summary>
    [Serializable]
    public struct BuffTag : IEquatable<BuffTag>
    {
        [SerializeField]
        [Tooltip("标签名称")]
        private string tagName;

        private int tagHash;
        private bool hashInitialized;

        /// <summary>
        /// 标签名称
        /// </summary>
        public string TagName
        {
            get => tagName;
            set
            {
                tagName = value;
                hashInitialized = false;
            }
        }

        /// <summary>
        /// 标签哈希值（延迟计算，缓存）
        /// </summary>
        public int TagHash
        {
            get
            {
                if (!hashInitialized && tagName != null)
                {
                    tagHash = BuffTagManager.GetTagHash(tagName);
                    hashInitialized = true;
                }
                return tagHash;
            }
        }

        /// <summary>
        /// 创建BuffTag
        /// </summary>
        public BuffTag(string name)
        {
            tagName = name;
            tagHash = 0;
            hashInitialized = false;
        }

        /// <summary>
        /// 检查是否与另一个标签相等（基于哈希）
        /// </summary>
        public bool Equals(BuffTag other)
        {
            // 优先比较哈希值（O(1)）
            if (TagHash != other.TagHash)
                return false;

            // 哈希冲突时，比较字符串
            return string.Equals(tagName, other.tagName);
        }

        /// <summary>
        /// 检查是否与字符串标签相等
        /// </summary>
        public bool Equals(string otherTagName)
        {
            if (otherTagName == null)
                return tagName == null;

            return TagHash == BuffTagManager.GetTagHash(otherTagName) &&
                   string.Equals(tagName, otherTagName);
        }

        public override bool Equals(object obj)
        {
            return obj is BuffTag other && Equals(other);
        }

        public override int GetHashCode()
        {
            return TagHash;
        }

        public override string ToString()
        {
            return tagName ?? string.Empty;
        }

        #region 运算符重载

        public static bool operator ==(BuffTag left, BuffTag right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BuffTag left, BuffTag right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// 从string隐式转换
        /// </summary>
        public static implicit operator BuffTag(string name)
        {
            return new BuffTag(name);
        }

        /// <summary>
        /// 隐式转换为string
        /// </summary>
        public static implicit operator string(BuffTag tag)
        {
            return tag.tagName;
        }

        #endregion
    }
}
