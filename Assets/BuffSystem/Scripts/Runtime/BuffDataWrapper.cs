using BuffSystem.Core;
using BuffSystem.Data;
using UnityEngine;

namespace BuffSystem.Runtime
{
    /// <summary>
    /// BuffData包装器 - 为BuffDataStruct提供IBuff接口
    /// 这是结构体与接口之间的桥梁，实现零GC访问
    /// </summary>
    public class BuffDataWrapper : IBuff
    {
        private readonly BuffContainerOptimized container;
        private readonly int index;
        
        public BuffDataWrapper(BuffContainerOptimized container, int index)
        {
            this.container = container;
            this.index = index;
        }
        
        /// <summary>
        /// 获取结构体引用 - 直接访问数组元素
        /// </summary>
        private ref BuffDataStruct BuffStruct => ref container.GetDataRef(index);
        
        #region IBuff Implementation
        
        public int InstanceId => BuffStruct.InstanceId;
        
        public int DataId => BuffStruct.DataId;
        
        public string Name
        {
            get
            {
                var buffData = BuffDatabase.Instance.GetBuffData(DataId);
                return buffData?.Name ?? "Unknown";
            }
        }
        
        public string Description
        {
            get
            {
                var buffData = BuffDatabase.Instance.GetBuffData(DataId);
                return buffData?.Description ?? "";
            }
        }
        
        public int CurrentStack => BuffStruct.CurrentStack;
        
        public int MaxStack => BuffStruct.MaxStack;
        
        public float Duration => BuffStruct.Duration;
        
        public float TotalDuration => BuffStruct.TotalDuration;
        
        public float RemainingTime => BuffStruct.RemainingTime;
        
        public bool IsPermanent => BuffStruct.IsPermanent;
        
        public bool IsActive => BuffStruct.IsActive;
        
        public bool IsMarkedForRemoval => BuffStruct.IsMarkedForRemoval;
        
        public object Source => null; // 结构体中只存储SourceId，需要额外映射才能获取Source对象
        
        public int SourceId => BuffStruct.SourceId;
        
        public IBuffOwner Owner => container.Owner;
        
        public IBuffData Data => BuffDatabase.Instance.GetBuffData(DataId);
        
        public void AddStack(int amount)
        {
            var data = BuffStruct;
            data.CurrentStack = (short)Mathf.Min(data.CurrentStack + amount, data.MaxStack);
            container.GetDataRef(index) = data;
        }
        
        public void RemoveStack(int amount)
        {
            var data = BuffStruct;
            data.CurrentStack = (short)Mathf.Max(data.CurrentStack - amount, 0);
            if (data.CurrentStack <= 0)
            {
                data.MarkForRemoval();
            }
            container.GetDataRef(index) = data;
        }
        
        public void RefreshDuration()
        {
            var data = BuffStruct;
            data.Duration = 0f;
            container.GetDataRef(index) = data;
        }
        
        public void MarkForRemoval()
        {
            var data = BuffStruct;
            data.MarkForRemoval();
            container.GetDataRef(index) = data;
        }
        
        public bool IsImmuneTo(int buffId)
        {
            return Owner.IsImmuneTo(buffId);
        }
        
        public bool IsImmuneToTag(string tag)
        {
            return Owner.IsImmuneToTag(tag);
        }
        
        public T GetSource<T>() where T : class
        {
            // 优化容器只存储SourceId，无法直接获取Source对象
            // 如果需要此功能，需要额外维护SourceId到Source对象的映射
            return null;
        }
        
        public bool TryGetSource<T>(out T source) where T : class
        {
            source = null;
            return false;
        }
        
        public void Reset(IBuffData data, IBuffOwner owner, object source)
        {
            // 优化容器不支持Reset，因为结构体存储在数组中
            // 需要通过Remove+Add来实现类似功能
            Debug.LogWarning("[BuffDataWrapper] Reset不支持在优化容器中直接调用，请使用RemoveBuff+AddBuff替代");
        }
        
        #endregion
        
        #region Object Overrides
        
        public override bool Equals(object obj)
        {
            if (obj is BuffDataWrapper other)
            {
                return InstanceId == other.InstanceId;
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            return InstanceId;
        }
        
        public override string ToString()
        {
            return $"Buff[{InstanceId}]: {Name} (Stack: {CurrentStack}/{MaxStack})";
        }
        
        #endregion
    }
}
