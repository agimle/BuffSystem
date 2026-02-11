using System;
using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;

namespace BuffSystem.Snapshot
{
    /// <summary>
    /// Buff快照 - 捕获特定时刻的属性状态
    /// 用于实现DOTA2风格的快照机制，Buff效果基于快照时的属性计算
    /// v4.0新增
    /// </summary>
    [Serializable]
    public class BuffSnapshot
    {
        /// <summary>
        /// 快照时间戳
        /// </summary>
        public float Timestamp { get; set; }

        /// <summary>
        /// 快照属性值（不绑定特定数值修改器，使用通用键值对）
        /// </summary>
        public Dictionary<string, float> Attributes { get; set; }

        /// <summary>
        /// 快照来源的Buff实例ID
        /// </summary>
        public int SourceBuffInstanceId { get; set; }

        /// <summary>
        /// 快照来源的Buff数据ID
        /// </summary>
        public int SourceBuffDataId { get; set; }

        /// <summary>
        /// 创建空快照
        /// </summary>
        public BuffSnapshot()
        {
            Attributes = new Dictionary<string, float>();
        }

        /// <summary>
        /// 创建快照（从Buff创建）
        /// </summary>
        /// <param name="buff">来源Buff</param>
        /// <param name="attributes">要捕获的属性</param>
        public BuffSnapshot(IBuff buff, Dictionary<string, float> attributes)
        {
            Timestamp = Time.time;
            SourceBuffInstanceId = buff?.InstanceId ?? 0;
            SourceBuffDataId = buff?.DataId ?? 0;
            Attributes = attributes ?? new Dictionary<string, float>();
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="key">属性键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>属性值</returns>
        public float GetAttribute(string key, float defaultValue = 0f)
        {
            return Attributes.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="key">属性键</param>
        /// <param name="value">属性值</param>
        public void SetAttribute(string key, float value)
        {
            Attributes[key] = value;
        }

        /// <summary>
        /// 检查是否包含指定属性
        /// </summary>
        /// <param name="key">属性键</param>
        /// <returns>是否包含</returns>
        public bool HasAttribute(string key)
        {
            return Attributes.ContainsKey(key);
        }

        /// <summary>
        /// 获取快照经过的时间
        /// </summary>
        /// <returns>经过的秒数</returns>
        public float GetElapsedTime()
        {
            return Time.time - Timestamp;
        }

        /// <summary>
        /// 克隆快照
        /// </summary>
        /// <returns>快照副本</returns>
        public BuffSnapshot Clone()
        {
            var clone = new BuffSnapshot
            {
                Timestamp = Timestamp,
                SourceBuffInstanceId = SourceBuffInstanceId,
                SourceBuffDataId = SourceBuffDataId,
                Attributes = new Dictionary<string, float>(Attributes)
            };
            return clone;
        }
    }

    /// <summary>
    /// 快照捕获器接口 - 定义如何捕获属性快照
    /// </summary>
    public interface ISnapshotCapturer
    {
        /// <summary>
        /// 捕获快照
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <param name="buff">来源Buff</param>
        /// <returns>快照数据</returns>
        BuffSnapshot Capture(IBuffOwner owner, IBuff buff);
    }

    /// <summary>
    /// 默认快照捕获器
    /// </summary>
    public class DefaultSnapshotCapturer : ISnapshotCapturer
    {
        /// <summary>
        /// 要捕获的属性键列表
        /// </summary>
        public List<string> AttributeKeys { get; set; } = new List<string>();

        /// <summary>
        /// 捕获回调 - 用于自定义属性获取逻辑
        /// </summary>
        public Func<IBuffOwner, string, float> GetAttributeCallback { get; set; }

        public BuffSnapshot Capture(IBuffOwner owner, IBuff buff)
        {
            var snapshot = new BuffSnapshot
            {
                Timestamp = Time.time,
                SourceBuffInstanceId = buff?.InstanceId ?? 0,
                SourceBuffDataId = buff?.DataId ?? 0,
                Attributes = new Dictionary<string, float>()
            };

            // 捕获指定属性
            foreach (var key in AttributeKeys)
            {
                float value = GetAttributeCallback?.Invoke(owner, key) ?? 0f;
                snapshot.Attributes[key] = value;
            }

            return snapshot;
        }
    }

    /// <summary>
    /// 使用快照的Effect接口
    /// </summary>
    public interface ISnapshotBasedEffect
    {
        /// <summary>
        /// 快照数据
        /// </summary>
        BuffSnapshot Snapshot { get; set; }

        /// <summary>
        /// 是否使用快照
        /// </summary>
        bool UseSnapshot { get; set; }

        /// <summary>
        /// 捕获快照
        /// </summary>
        void CaptureSnapshot();
    }
}
