// 命名空间兼容性别名 - v6.x到v7.0迁移支持
// 在Project Settings中定义BUFFSYSTEM_COMPATIBILITY_V6来使用旧命名空间

#if BUFFSYSTEM_COMPATIBILITY_V6

using System;
using System.Collections.Generic;

// Combo系统别名
namespace BuffSystem.Combo
{
    [Obsolete("使用 BuffSystem.Advanced.Combo.BuffComboManager 替代")]
    public class BuffComboManager : BuffSystem.Advanced.Combo.BuffComboManager { }
    
    [Obsolete("使用 BuffSystem.Advanced.Combo.BuffComboData 替代")]
    public class BuffComboData : BuffSystem.Advanced.Combo.BuffComboData { }
    
    [Obsolete("使用 BuffSystem.Advanced.Combo.BuffComboEventSystem 替代")]
    public class BuffComboEventSystem : BuffSystem.Advanced.Combo.BuffComboEventSystem { }
    
    [Obsolete("使用 BuffSystem.Advanced.Combo.ComboEffect 替代")]
    public class ComboEffect : BuffSystem.Advanced.Combo.ComboEffect { }
    
    [Obsolete("使用 BuffSystem.Advanced.Combo.ComboEffectType 替代")]
    public enum ComboEffectType
    {
        EnhanceDuration = BuffSystem.Advanced.Combo.ComboEffectType.EnhanceDuration,
        EnhanceStack = BuffSystem.Advanced.Combo.ComboEffectType.EnhanceStack,
        ReduceCooldown = BuffSystem.Advanced.Combo.ComboEffectType.ReduceCooldown,
        TriggerEvent = BuffSystem.Advanced.Combo.ComboEffectType.TriggerEvent,
        ModifyAttribute = BuffSystem.Advanced.Combo.ComboEffectType.ModifyAttribute,
        AddExtraBuff = BuffSystem.Advanced.Combo.ComboEffectType.AddExtraBuff,
        RemoveBuff = BuffSystem.Advanced.Combo.ComboEffectType.RemoveBuff,
        RefreshDuration = BuffSystem.Advanced.Combo.ComboEffectType.RefreshDuration,
        AddStack = BuffSystem.Advanced.Combo.ComboEffectType.AddStack,
        RemoveStack = BuffSystem.Advanced.Combo.ComboEffectType.RemoveStack
    }
    
    [Obsolete("使用 BuffSystem.Advanced.Combo.ComboTriggerMode 替代")]
    [Flags]
    public enum ComboTriggerMode
    {
        All = BuffSystem.Advanced.Combo.ComboTriggerMode.All,
        Any = BuffSystem.Advanced.Combo.ComboTriggerMode.Any,
        OnBuffAdd = BuffSystem.Advanced.Combo.ComboTriggerMode.OnBuffAdd,
        OnBuffRemove = BuffSystem.Advanced.Combo.ComboTriggerMode.OnBuffRemove,
        OnUpdate = BuffSystem.Advanced.Combo.ComboTriggerMode.OnUpdate,
        Default = BuffSystem.Advanced.Combo.ComboTriggerMode.Default
    }
    
    [Obsolete("使用 BuffSystem.Advanced.Combo.ComboTargetType 替代")]
    public enum ComboTargetType
    {
        SpecificBuff = BuffSystem.Advanced.Combo.ComboTargetType.SpecificBuff,
        AllComboBuffs = BuffSystem.Advanced.Combo.ComboTargetType.AllComboBuffs,
        Owner = BuffSystem.Advanced.Combo.ComboTargetType.Owner,
        NewestBuff = BuffSystem.Advanced.Combo.ComboTargetType.NewestBuff
    }
}

// Fusion系统别名
namespace BuffSystem.Fusion
{
    [Obsolete("使用 BuffSystem.Advanced.Fusion.FusionManager 替代")]
    public class FusionManager : BuffSystem.Advanced.Fusion.FusionManager { }
    
    [Obsolete("使用 BuffSystem.Advanced.Fusion.FusionEventSystem 替代")]
    public class FusionEventSystem : BuffSystem.Advanced.Fusion.FusionEventSystem { }
    
    [Obsolete("使用 BuffSystem.Advanced.Fusion.FusionRecipe 替代")]
    public class FusionRecipe : BuffSystem.Advanced.Fusion.FusionRecipe { }
    
    [Obsolete("使用 BuffSystem.Advanced.Fusion.Ingredient 替代")]
    public class Ingredient : BuffSystem.Advanced.Fusion.Ingredient { }
    
    [Obsolete("使用 BuffSystem.Advanced.Fusion.IFusionCondition 替代")]
    public interface IFusionCondition : BuffSystem.Advanced.Fusion.IFusionCondition { }
    
    [Obsolete("使用 BuffSystem.Advanced.Fusion.IBuffFusion 替代")]
    public interface IBuffFusion : BuffSystem.Advanced.Fusion.IBuffFusion { }
}

// Transmission系统别名
namespace BuffSystem.Transmission
{
    [Obsolete("使用 BuffSystem.Advanced.Transmission.TransmissionManager 替代")]
    public class TransmissionManager : BuffSystem.Advanced.Transmission.TransmissionManager { }
    
    [Obsolete("使用 BuffSystem.Advanced.Transmission.TransmissionEventSystem 替代")]
    public class TransmissionEventSystem : BuffSystem.Advanced.Transmission.TransmissionEventSystem { }
    
    [Obsolete("使用 BuffSystem.Advanced.Transmission.IBuffTransmissible 替代")]
    public interface IBuffTransmissible : BuffSystem.Advanced.Transmission.IBuffTransmissible { }
    
    [Obsolete("使用 BuffSystem.Advanced.Transmission.TransmissionMode 替代")]
    public enum TransmissionMode
    {
        Contact = BuffSystem.Advanced.Transmission.TransmissionMode.Contact,
        Range = BuffSystem.Advanced.Transmission.TransmissionMode.Range,
        Chain = BuffSystem.Advanced.Transmission.TransmissionMode.Chain,
        Inheritance = BuffSystem.Advanced.Transmission.TransmissionMode.Inheritance
    }
    
    [Obsolete("使用 BuffSystem.Advanced.Transmission.ChainTransmissionEventData 替代")]
    public class ChainTransmissionEventData : BuffSystem.Advanced.Transmission.ChainTransmissionEventData { }
}

// Area系统别名
namespace BuffSystem.Area
{
    [Obsolete("使用 BuffSystem.Advanced.Area.BuffArea 替代")]
    public class BuffArea : BuffSystem.Advanced.Area.BuffArea { }
    
    [Obsolete("使用 BuffSystem.Advanced.Area.BuffAreaManager 替代")]
    public class BuffAreaManager : BuffSystem.Advanced.Area.BuffAreaManager { }
}

// Snapshot系统别名
namespace BuffSystem.Snapshot
{
    [Obsolete("使用 BuffSystem.Advanced.Snapshot.BuffSnapshot 替代")]
    public class BuffSnapshot : BuffSystem.Advanced.Snapshot.BuffSnapshot { }
    
    [Obsolete("使用 BuffSystem.Advanced.Snapshot.BuffSnapshotManager 替代")]
    public class BuffSnapshotManager : BuffSystem.Advanced.Snapshot.BuffSnapshotManager { }
    
    [Obsolete("使用 BuffSystem.Advanced.Snapshot.ISnapshotCapturer 替代")]
    public interface ISnapshotCapturer : BuffSystem.Advanced.Snapshot.ISnapshotCapturer { }
    
    [Obsolete("使用 BuffSystem.Advanced.Snapshot.DefaultSnapshotCapturer 替代")]
    public class DefaultSnapshotCapturer : BuffSystem.Advanced.Snapshot.DefaultSnapshotCapturer { }
    
    [Obsolete("使用 BuffSystem.Advanced.Snapshot.ISnapshotBasedEffect 替代")]
    public interface ISnapshotBasedEffect : BuffSystem.Advanced.Snapshot.ISnapshotBasedEffect { }
}

// SnapshotHelper 是静态类，需要特殊处理
namespace BuffSystem.Snapshot
{
    [Obsolete("使用 BuffSystem.Advanced.Snapshot.SnapshotHelper 替代")]
    public static class SnapshotHelper
    {
        public static BuffSnapshot CreateSnapshot(this IBuff buff, Dictionary<string, float> attributes)
            => BuffSystem.Advanced.Snapshot.SnapshotHelper.CreateSnapshot(buff, attributes);
        
        public static BuffSnapshot CreateSnapshot(this IBuff buff, BuffSystem.Advanced.Snapshot.ISnapshotCapturer capturer)
            => BuffSystem.Advanced.Snapshot.SnapshotHelper.CreateSnapshot(buff, capturer);
        
        public static BuffSnapshot GetSnapshot(this IBuff buff)
            => BuffSystem.Advanced.Snapshot.SnapshotHelper.GetSnapshot(buff);
        
        public static bool HasSnapshot(this IBuff buff)
            => BuffSystem.Advanced.Snapshot.SnapshotHelper.HasSnapshot(buff);
        
        public static void RemoveSnapshot(this IBuff buff)
            => BuffSystem.Advanced.Snapshot.SnapshotHelper.RemoveSnapshot(buff);
        
        public static BuffSystem.Advanced.Snapshot.ISnapshotCapturer CreateCapturer(
            List<string> attributeKeys,
            Func<BuffSystem.Core.IBuffOwner, string, float> getAttribute)
            => BuffSystem.Advanced.Snapshot.SnapshotHelper.CreateCapturer(attributeKeys, getAttribute);
        
        public static BuffSystem.Advanced.Snapshot.ISnapshotCapturer CreateSimpleCapturer(
            string attributeKey,
            Func<BuffSystem.Core.IBuffOwner, float> getAttribute)
            => BuffSystem.Advanced.Snapshot.SnapshotHelper.CreateSimpleCapturer(attributeKey, getAttribute);
    }
}

#endif // BUFFSYSTEM_COMPATIBILITY_V6
