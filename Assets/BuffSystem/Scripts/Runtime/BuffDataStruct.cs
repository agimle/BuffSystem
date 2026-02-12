using System;
using UnityEngine;

namespace BuffSystem.Runtime
{
    /// <summary>
    /// Buff数据结构体 - 极致内存优化
    /// 总大小: 32 bytes，可放入一个缓存行
    /// </summary>
    public struct BuffDataStruct : IEquatable<BuffDataStruct>
    {
        // 标识 (8 bytes)
        public int InstanceId;      // 4 bytes
        public int DataId;          // 4 bytes
        
        // 层数 (4 bytes)
        public short CurrentStack;  // 2 bytes
        public short MaxStack;      // 2 bytes
        
        // 时间 (8 bytes)
        public float Duration;      // 4 bytes
        public float TotalDuration; // 4 bytes
        
        // 关联ID (8 bytes)
        public int OwnerId;         // 4 bytes
        public int SourceId;        // 4 bytes
        
        // 状态标志 (4 bytes)
        public BuffFlags Flags;     // 4 bytes
        
        // 总计: 32 bytes
        
        // 属性访问器
        public bool IsActive => (Flags & BuffFlags.IsActive) != 0;
        public bool IsPermanent => (Flags & BuffFlags.IsPermanent) != 0;
        public bool IsMarkedForRemoval => (Flags & BuffFlags.IsMarkedForRemoval) != 0;
        public bool CanRefresh => (Flags & BuffFlags.CanRefresh) != 0;
        public bool IsUnique => (Flags & BuffFlags.IsUnique) != 0;
        public float RemainingTime => IsPermanent ? float.MaxValue : Mathf.Max(0, TotalDuration - Duration);
        
        public void MarkForRemoval()
        {
            Flags |= BuffFlags.IsMarkedForRemoval;
        }
        
        public void SetActive(bool active)
        {
            if (active)
                Flags |= BuffFlags.IsActive;
            else
                Flags &= ~BuffFlags.IsActive;
        }
        
        public bool Equals(BuffDataStruct other)
        {
            return InstanceId == other.InstanceId;
        }
        
        public override int GetHashCode()
        {
            return InstanceId;
        }
        
        public override bool Equals(object obj)
        {
            return obj is BuffDataStruct other && Equals(other);
        }
        
        public static bool operator ==(BuffDataStruct left, BuffDataStruct right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(BuffDataStruct left, BuffDataStruct right)
        {
            return !left.Equals(right);
        }
    }
    
    /// <summary>
    /// Buff状态标志
    /// </summary>
    [Flags]
    public enum BuffFlags : uint
    {
        None = 0,
        IsActive = 1 << 0,
        IsPermanent = 1 << 1,
        IsMarkedForRemoval = 1 << 2,
        CanRefresh = 1 << 3,
        IsUnique = 1 << 4,
        UseVisualUpdate = 1 << 5,
    }
}
