using System;
using System.Collections.Generic;
using BuffSystem.Core;
using BuffSystem.Utils;

namespace BuffSystem.Events
{
    /// <summary>
    /// Buff事件参数对象池 - 减少GC Alloc
    /// </summary>
    public static class BuffEventArgsPool
    {
        // 各种事件参数的对象池
        private static readonly ObjectPool<PooledBuffAddedEventArgs> AddedArgsPool = new(
            createFunc: () => new PooledBuffAddedEventArgs(),
            actionOnGet: null,
            actionOnRelease: ResetArgs,
            defaultCapacity: 32,
            maxSize: 128
        );

        private static readonly ObjectPool<PooledBuffRemovedEventArgs> RemovedArgsPool = new(
            createFunc: () => new PooledBuffRemovedEventArgs(),
            actionOnGet: null,
            actionOnRelease: ResetArgs,
            defaultCapacity: 32,
            maxSize: 128
        );

        private static readonly ObjectPool<PooledBuffStackChangedEventArgs> StackChangedArgsPool = new(
            createFunc: () => new PooledBuffStackChangedEventArgs(),
            actionOnGet: null,
            actionOnRelease: ResetArgs,
            defaultCapacity: 32,
            maxSize: 128
        );

        private static readonly ObjectPool<PooledBuffRefreshedEventArgs> RefreshedArgsPool = new(
            createFunc: () => new PooledBuffRefreshedEventArgs(),
            actionOnGet: null,
            actionOnRelease: ResetArgs,
            defaultCapacity: 16,
            maxSize: 64
        );

        private static readonly ObjectPool<PooledBuffExpiredEventArgs> ExpiredArgsPool = new(
            createFunc: () => new PooledBuffExpiredEventArgs(),
            actionOnGet: null,
            actionOnRelease: ResetArgs,
            defaultCapacity: 16,
            maxSize: 64
        );

        private static readonly ObjectPool<PooledBuffClearedEventArgs> ClearedArgsPool = new(
            createFunc: () => new PooledBuffClearedEventArgs(),
            actionOnGet: null,
            actionOnRelease: ResetArgs,
            defaultCapacity: 8,
            maxSize: 32
        );

        /// <summary>
        /// 获取BuffAddedEventArgs
        /// </summary>
        public static BuffAddedEventArgs GetAddedArgs(IBuff buff)
        {
            var args = AddedArgsPool.Get();
            args.Buff = buff;
            return args;
        }

        /// <summary>
        /// 回收BuffAddedEventArgs
        /// </summary>
        public static void ReleaseAddedArgs(BuffAddedEventArgs args)
        {
            if (args is PooledBuffAddedEventArgs pooledArgs)
            {
                AddedArgsPool.Release(pooledArgs);
            }
        }

        /// <summary>
        /// 获取BuffRemovedEventArgs
        /// </summary>
        public static BuffRemovedEventArgs GetRemovedArgs(IBuff buff)
        {
            var args = RemovedArgsPool.Get();
            args.Buff = buff;
            return args;
        }

        /// <summary>
        /// 回收BuffRemovedEventArgs
        /// </summary>
        public static void ReleaseRemovedArgs(BuffRemovedEventArgs args)
        {
            if (args is PooledBuffRemovedEventArgs pooledArgs)
            {
                RemovedArgsPool.Release(pooledArgs);
            }
        }

        /// <summary>
        /// 获取BuffStackChangedEventArgs
        /// </summary>
        public static BuffStackChangedEventArgs GetStackChangedArgs(IBuff buff, int oldStack, int newStack)
        {
            var args = StackChangedArgsPool.Get();
            args.Buff = buff;
            args.OldStack = oldStack;
            args.NewStack = newStack;
            return args;
        }

        /// <summary>
        /// 回收BuffStackChangedEventArgs
        /// </summary>
        public static void ReleaseStackChangedArgs(BuffStackChangedEventArgs args)
        {
            if (args is PooledBuffStackChangedEventArgs pooledArgs)
            {
                StackChangedArgsPool.Release(pooledArgs);
            }
        }

        /// <summary>
        /// 获取BuffRefreshedEventArgs
        /// </summary>
        public static BuffRefreshedEventArgs GetRefreshedArgs(IBuff buff)
        {
            var args = RefreshedArgsPool.Get();
            args.Buff = buff;
            return args;
        }

        /// <summary>
        /// 回收BuffRefreshedEventArgs
        /// </summary>
        public static void ReleaseRefreshedArgs(BuffRefreshedEventArgs args)
        {
            if (args is PooledBuffRefreshedEventArgs pooledArgs)
            {
                RefreshedArgsPool.Release(pooledArgs);
            }
        }

        /// <summary>
        /// 获取BuffExpiredEventArgs
        /// </summary>
        public static BuffExpiredEventArgs GetExpiredArgs(IBuff buff)
        {
            var args = ExpiredArgsPool.Get();
            args.Buff = buff;
            return args;
        }

        /// <summary>
        /// 回收BuffExpiredEventArgs
        /// </summary>
        public static void ReleaseExpiredArgs(BuffExpiredEventArgs args)
        {
            if (args is PooledBuffExpiredEventArgs pooledArgs)
            {
                ExpiredArgsPool.Release(pooledArgs);
            }
        }

        /// <summary>
        /// 获取BuffClearedEventArgs
        /// </summary>
        public static BuffClearedEventArgs GetClearedArgs(IBuffOwner owner)
        {
            var args = ClearedArgsPool.Get();
            args.Owner = owner;
            return args;
        }

        /// <summary>
        /// 回收BuffClearedEventArgs
        /// </summary>
        public static void ReleaseClearedArgs(BuffClearedEventArgs args)
        {
            if (args is PooledBuffClearedEventArgs pooledArgs)
            {
                ClearedArgsPool.Release(pooledArgs);
            }
        }

        /// <summary>
        /// 重置事件参数
        /// </summary>
        private static void ResetArgs(PooledBuffAddedEventArgs args)
        {
            if (args != null) args.Buff = null;
        }

        private static void ResetArgs(PooledBuffRemovedEventArgs args)
        {
            if (args != null) args.Buff = null;
        }

        private static void ResetArgs(PooledBuffStackChangedEventArgs args)
        {
            if (args != null)
            {
                args.Buff = null;
                args.OldStack = 0;
                args.NewStack = 0;
            }
        }

        private static void ResetArgs(PooledBuffRefreshedEventArgs args)
        {
            if (args != null) args.Buff = null;
        }

        private static void ResetArgs(PooledBuffExpiredEventArgs args)
        {
            if (args != null) args.Buff = null;
        }

        private static void ResetArgs(PooledBuffClearedEventArgs args)
        {
            if (args != null) args.Owner = null;
        }

        /// <summary>
        /// 清空所有对象池
        /// </summary>
        public static void ClearAllPools()
        {
            AddedArgsPool.Clear();
            RemovedArgsPool.Clear();
            StackChangedArgsPool.Clear();
            RefreshedArgsPool.Clear();
            ExpiredArgsPool.Clear();
            ClearedArgsPool.Clear();
        }

        /// <summary>
        /// 获取所有对象池的状态信息
        /// </summary>
        public static Dictionary<string, (int total, int active, int inactive)> GetPoolStatus()
        {
            return new Dictionary<string, (int, int, int)>
            {
                ["AddedArgs"] = (AddedArgsPool.CountAll, AddedArgsPool.CountActive, AddedArgsPool.CountInactive),
                ["RemovedArgs"] = (RemovedArgsPool.CountAll, RemovedArgsPool.CountActive, RemovedArgsPool.CountInactive),
                ["StackChangedArgs"] = (StackChangedArgsPool.CountAll, StackChangedArgsPool.CountActive, StackChangedArgsPool.CountInactive),
                ["RefreshedArgs"] = (RefreshedArgsPool.CountAll, RefreshedArgsPool.CountActive, RefreshedArgsPool.CountInactive),
                ["ExpiredArgs"] = (ExpiredArgsPool.CountAll, ExpiredArgsPool.CountActive, ExpiredArgsPool.CountInactive),
                ["ClearedArgs"] = (ClearedArgsPool.CountAll, ClearedArgsPool.CountActive, ClearedArgsPool.CountInactive)
            };
        }
    }

    #region Pooled Event Args Classes

    /// <summary>
    /// 可池化的BuffAddedEventArgs
    /// </summary>
    public class PooledBuffAddedEventArgs : BuffAddedEventArgs
    {
        public PooledBuffAddedEventArgs() : base(null) { }

        public new IBuff Buff
        {
            get => base.Buff;
            internal set => SetBuff(value);
        }

        private void SetBuff(IBuff buff)
        {
            var field = typeof(BuffEventArgs).GetField("<Buff>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(this, buff);
        }
    }

    /// <summary>
    /// 可池化的BuffRemovedEventArgs
    /// </summary>
    public class PooledBuffRemovedEventArgs : BuffRemovedEventArgs
    {
        public PooledBuffRemovedEventArgs() : base(null) { }

        public new IBuff Buff
        {
            get => base.Buff;
            internal set => SetBuff(value);
        }

        private void SetBuff(IBuff buff)
        {
            var field = typeof(BuffEventArgs).GetField("<Buff>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(this, buff);
        }
    }

    /// <summary>
    /// 可池化的BuffStackChangedEventArgs
    /// </summary>
    public class PooledBuffStackChangedEventArgs : BuffStackChangedEventArgs
    {
        public PooledBuffStackChangedEventArgs() : base(null, 0, 0) { }

        public new IBuff Buff
        {
            get => base.Buff;
            internal set => SetBuff(value);
        }

        public new int OldStack
        {
            get => base.OldStack;
            internal set => SetOldStack(value);
        }

        public new int NewStack
        {
            get => base.NewStack;
            internal set => SetNewStack(value);
        }

        private void SetBuff(IBuff buff)
        {
            var field = typeof(BuffEventArgs).GetField("<Buff>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(this, buff);
        }

        private void SetOldStack(int value)
        {
            var field = typeof(BuffStackChangedEventArgs).GetField("<OldStack>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(this, value);
        }

        private void SetNewStack(int value)
        {
            var field = typeof(BuffStackChangedEventArgs).GetField("<NewStack>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(this, value);
        }
    }

    /// <summary>
    /// 可池化的BuffRefreshedEventArgs
    /// </summary>
    public class PooledBuffRefreshedEventArgs : BuffRefreshedEventArgs
    {
        public PooledBuffRefreshedEventArgs() : base(null) { }

        public new IBuff Buff
        {
            get => base.Buff;
            internal set => SetBuff(value);
        }

        private void SetBuff(IBuff buff)
        {
            var field = typeof(BuffEventArgs).GetField("<Buff>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(this, buff);
        }
    }

    /// <summary>
    /// 可池化的BuffExpiredEventArgs
    /// </summary>
    public class PooledBuffExpiredEventArgs : BuffExpiredEventArgs
    {
        public PooledBuffExpiredEventArgs() : base(null) { }

        public new IBuff Buff
        {
            get => base.Buff;
            internal set => SetBuff(value);
        }

        private void SetBuff(IBuff buff)
        {
            var field = typeof(BuffEventArgs).GetField("<Buff>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(this, buff);
        }
    }

    /// <summary>
    /// 可池化的BuffClearedEventArgs
    /// </summary>
    public class PooledBuffClearedEventArgs : BuffClearedEventArgs
    {
        public PooledBuffClearedEventArgs() : base(null) { }

        public new IBuffOwner Owner
        {
            get => base.Owner;
            internal set => SetOwner(value);
        }

        private void SetOwner(IBuffOwner owner)
        {
            var field = typeof(BuffClearedEventArgs).GetField("<Owner>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(this, owner);
        }
    }

    #endregion
}
