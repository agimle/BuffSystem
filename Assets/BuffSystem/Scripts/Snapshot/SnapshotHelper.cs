using System;
using System.Collections.Generic;
using BuffSystem.Core;

namespace BuffSystem.Snapshot
{
    /// <summary>
    /// 快照辅助类 - 提供便捷的快照操作方法
    /// v4.0新增
    /// </summary>
    public static class SnapshotHelper
    {
        /// <summary>
        /// 创建并缓存快照
        /// </summary>
        /// <param name="buff">Buff</param>
        /// <param name="attributes">属性字典</param>
        /// <returns>快照</returns>
        public static BuffSnapshot CreateSnapshot(this IBuff buff, Dictionary<string, float> attributes)
        {
            if (buff == null) return null;
            return BuffSnapshotManager.Instance?.CreateSnapshot(buff.Owner, buff, attributes);
        }

        /// <summary>
        /// 使用捕获器创建快照
        /// </summary>
        /// <param name="buff">Buff</param>
        /// <param name="capturer">捕获器</param>
        /// <returns>快照</returns>
        public static BuffSnapshot CreateSnapshot(this IBuff buff, ISnapshotCapturer capturer)
        {
            if (buff == null) return null;
            return BuffSnapshotManager.Instance?.CreateSnapshot(buff.Owner, buff, capturer);
        }

        /// <summary>
        /// 获取Buff的快照
        /// </summary>
        /// <param name="buff">Buff</param>
        /// <returns>快照</returns>
        public static BuffSnapshot GetSnapshot(this IBuff buff)
        {
            if (buff == null) return null;
            return BuffSnapshotManager.Instance?.GetSnapshot(buff);
        }

        /// <summary>
        /// 检查Buff是否有快照
        /// </summary>
        /// <param name="buff">Buff</param>
        /// <returns>是否有快照</returns>
        public static bool HasSnapshot(this IBuff buff)
        {
            if (buff == null) return false;
            return BuffSnapshotManager.Instance?.HasSnapshot(buff.InstanceId) ?? false;
        }

        /// <summary>
        /// 移除Buff的快照
        /// </summary>
        /// <param name="buff">Buff</param>
        public static void RemoveSnapshot(this IBuff buff)
        {
            if (buff == null) return;
            BuffSnapshotManager.Instance?.RemoveSnapshot(buff);
        }

        /// <summary>
        /// 创建默认捕获器
        /// </summary>
        /// <param name="attributeKeys">属性键列表</param>
        /// <param name="getAttribute">获取属性的回调</param>
        /// <returns>捕获器</returns>
        public static ISnapshotCapturer CreateCapturer(
            List<string> attributeKeys,
            Func<IBuffOwner, string, float> getAttribute)
        {
            return new DefaultSnapshotCapturer
            {
                AttributeKeys = attributeKeys ?? new List<string>(),
                GetAttributeCallback = getAttribute
            };
        }

        /// <summary>
        /// 创建简单捕获器（单个属性）
        /// </summary>
        /// <param name="attributeKey">属性键</param>
        /// <param name="getAttribute">获取属性的回调</param>
        /// <returns>捕获器</returns>
        public static ISnapshotCapturer CreateSimpleCapturer(
            string attributeKey,
            Func<IBuffOwner, float> getAttribute)
        {
            return new DefaultSnapshotCapturer
            {
                AttributeKeys = new List<string> { attributeKey },
                GetAttributeCallback = (owner, key) => getAttribute?.Invoke(owner) ?? 0f
            };
        }
    }
}
