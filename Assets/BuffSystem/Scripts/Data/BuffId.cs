using System;
using BuffSystem.Core;
using UnityEngine;

namespace BuffSystem.Data
{
    /// <summary>
    /// BuffID - 支持int和string两种形式的ID
    /// 自动处理类型转换和哈希映射
    /// </summary>
    [Serializable]
    public struct BuffId : IEquatable<BuffId>
    {
        [SerializeField]
        [Tooltip("Buff数字ID")]
        private int id;

        [SerializeField]
        [Tooltip("Buff字符串ID（可选，优先级高于数字ID）")]
        private string stringId;

        /// <summary>
        /// 数字ID
        /// </summary>
        public int Id => id;

        /// <summary>
        /// 字符串ID
        /// </summary>
        public string StringId => stringId;

        /// <summary>
        /// 是否使用字符串ID
        /// </summary>
        public bool IsStringId => !string.IsNullOrEmpty(stringId);

        /// <summary>
        /// 创建数字ID
        /// </summary>
        public BuffId(int id)
        {
            this.id = id;
            this.stringId = null;
        }

        /// <summary>
        /// 创建字符串ID
        /// </summary>
        public BuffId(string stringId)
        {
            this.stringId = stringId;
            this.id = 0;
        }

        /// <summary>
        /// 获取最终用于查询的ID
        /// 如果是字符串ID，先查询映射表获取数字ID
        /// </summary>
        public int ResolveId()
        {
            if (IsStringId)
            {
                return BuffDatabase.Instance.GetBuffId(stringId);
            }
            return id;
        }

        /// <summary>
        /// 尝试解析ID
        /// </summary>
        /// <param name="resolvedId">解析后的数字ID</param>
        /// <returns>是否解析成功</returns>
        public bool TryResolveId(out int resolvedId)
        {
            if (IsStringId)
            {
                resolvedId = BuffDatabase.Instance.GetBuffId(stringId);
                return resolvedId >= 0;
            }
            resolvedId = id;
            return true;
        }

        /// <summary>
        /// 获取Buff数据
        /// </summary>
        public IBuffData GetBuffData()
        {
            if (IsStringId)
            {
                return BuffDatabase.Instance.GetBuffData(stringId);
            }
            return BuffDatabase.Instance.GetBuffData(id);
        }

        public bool Equals(BuffId other)
        {
            // 如果都是字符串ID，比较字符串
            if (IsStringId && other.IsStringId)
            {
                return string.Equals(stringId, other.stringId);
            }

            // 如果都是数字ID，比较数字
            if (!IsStringId && !other.IsStringId)
            {
                return id == other.id;
            }

            // 混合类型：都解析为数字ID后比较
            int thisId = ResolveId();
            int otherId = other.ResolveId();
            return thisId >= 0 && thisId == otherId;
        }

        public override bool Equals(object obj)
        {
            return obj is BuffId other && Equals(other);
        }

        public override int GetHashCode()
        {
            if (IsStringId)
            {
                return stringId.GetHashCode();
            }
            return id.GetHashCode();
        }

        public override string ToString()
        {
            if (IsStringId)
            {
                return $"{stringId}({ResolveId()})";
            }
            return id.ToString();
        }

        #region 运算符重载

        public static bool operator ==(BuffId left, BuffId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BuffId left, BuffId right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// 从int隐式转换
        /// </summary>
        public static implicit operator BuffId(int id)
        {
            return new BuffId(id);
        }

        /// <summary>
        /// 从string隐式转换
        /// </summary>
        public static implicit operator BuffId(string stringId)
        {
            return new BuffId(stringId);
        }

        /// <summary>
        /// 显式转换为int
        /// </summary>
        public static explicit operator int(BuffId buffId)
        {
            return buffId.ResolveId();
        }

        /// <summary>
        /// 显式转换为string
        /// </summary>
        public static explicit operator string(BuffId buffId)
        {
            return buffId.IsStringId ? buffId.stringId : buffId.id.ToString();
        }

        #endregion
    }
}
