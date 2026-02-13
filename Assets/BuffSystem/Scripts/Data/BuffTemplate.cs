

using System;
using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;

namespace BuffSystem.Data
{
    /// <summary>
    /// Buff模板 - 可继承的基础配置
    /// 用于快速创建相似Buff，避免重复配置
    /// 仅在编辑器中使用
    /// </summary>
    [CreateAssetMenu(fileName = "NewBuffTemplate", menuName = "BuffSystem/Buff Template", order = 0)]
    public class BuffTemplate : ScriptableObject
    {
        [Header("基础模板")] [Tooltip("效果类型")] [SerializeField]
        private BuffEffectType effectType = BuffEffectType.Neutral;

        [Tooltip("是否唯一")] [SerializeField] private bool isUnique = true;

        [Tooltip("叠加模式")] [SerializeField] private BuffStackMode stackMode = BuffStackMode.Stackable;

        [Tooltip("最大层数")] [SerializeField] private int maxStack = 1;

        [Tooltip("添加层数")] [SerializeField] private int addStackCount = 1;

        [Header("持续时间")] [Tooltip("是否永久")] [SerializeField]
        private bool isPermanent = false;

        [Tooltip("持续时间（秒）")] [SerializeField] private float duration = 5f;

        [Tooltip("是否可刷新")] [SerializeField] private bool canRefresh = true;

        [Header("移除设置")] [Tooltip("移除模式")] [SerializeField]
        private BuffRemoveMode removeMode = BuffRemoveMode.Remove;

        [Tooltip("移除层数")] [SerializeField] private int removeStackCount = 1;

        [Tooltip("移除间隔")] [SerializeField] private float removeInterval = 0f;

        [Header("关系设置")] [Tooltip("互斥优先级")] [SerializeField]
        private MutexPriority mutexPriority = MutexPriority.ReplaceOthers;

        [Header("组配置")] [Tooltip("默认组配置")] [SerializeField]
        private List<BuffGroupConfigEntry> defaultGroupConfigs = new();

        [Header("标签")] [Tooltip("默认标签")] [SerializeField]
        private List<BuffTag> defaultTags = new();

        [Header("性能设置")] [Tooltip("更新频率")] [SerializeField]
        private UpdateFrequency updateFrequency = UpdateFrequency.Every33ms;

        #region 属性访问器

        public BuffEffectType EffectType => effectType;
        public bool IsUnique => isUnique;
        public BuffStackMode StackMode => stackMode;
        public int MaxStack => maxStack;
        public int AddStackCount => addStackCount;
        public bool IsPermanent => isPermanent;
        public float Duration => duration;
        public bool CanRefresh => canRefresh;
        public BuffRemoveMode RemoveMode => removeMode;
        public int RemoveStackCount => removeStackCount;
        public float RemoveInterval => removeInterval;
        public MutexPriority MutexPriority => mutexPriority;
        public IReadOnlyList<BuffGroupConfigEntry> DefaultGroupConfigs => defaultGroupConfigs;
        public IReadOnlyList<BuffTag> DefaultTags => defaultTags;
        public UpdateFrequency UpdateFrequency => updateFrequency;

        #endregion

#if UNITY_EDITOR
        /// <summary>
        /// 应用模板到BuffDataSO
        /// 只应用目标未设置的字段（使用默认值时）
        /// </summary>
        /// <param name="target">目标Buff数据</param>
        /// <param name="forceApply">是否强制应用所有字段</param>
        
        public void ApplyTemplate(BuffDataSO target, bool forceApply = false)
        {
            if (target == null) return;

            // 基础信息
            if (forceApply || target.EffectType == BuffEffectType.Neutral)
                target.SetEffectType(effectType);

            if (forceApply || target.IsUnique == true)
                target.SetIsUnique(isUnique);

            if (forceApply || target.StackMode == BuffStackMode.Stackable)
                target.SetStackMode(stackMode);

            if (forceApply || target.MaxStack == 1)
                target.SetMaxStack(maxStack);

            if (forceApply || target.AddStackCount == 1)
                target.SetAddStackCount(addStackCount);

            // 持续时间
            if (forceApply || target.IsPermanent == false)
                target.SetIsPermanent(isPermanent);

            if (forceApply || Mathf.Abs(target.Duration - 5f) < 0.001f)
                target.SetDuration(duration);

            if (forceApply || target.CanRefresh == true)
                target.SetCanRefresh(canRefresh);

            // 移除设置
            if (forceApply || target.RemoveMode == BuffRemoveMode.Remove)
                target.SetRemoveMode(removeMode);

            if (forceApply || target.RemoveStackCount == 1)
                target.SetRemoveStackCount(removeStackCount);

            if (forceApply || Mathf.Abs(target.RemoveInterval - 0f) < 0.001f)
                target.SetRemoveInterval(removeInterval);

            // 关系设置
            if (forceApply || target.MutexPriority == MutexPriority.ReplaceOthers)
                target.SetMutexPriority(mutexPriority);

            // 组配置
            if (forceApply || target.GroupConfigs.Count == 0)
            {
                foreach (var config in defaultGroupConfigs)
                {
                    target.AddGroupConfig(config);
                }
            }

            // 标签
            if (forceApply || target.BuffTags.Count == 0)
            {
                foreach (var tag in defaultTags)
                {
                    target.AddTag(tag);
                }
            }

            // 性能设置
            if (forceApply || target.UpdateFrequency == UpdateFrequency.Every33ms)
                target.SetUpdateFrequency(updateFrequency);
        }

        /// <summary>
        /// 创建基于此模板的新BuffData
        /// </summary>
        public BuffDataSO CreateBuffData(string name, int id)
        {
            var buffData = ScriptableObject.CreateInstance<BuffDataSO>();
            buffData.name = name;
            buffData.SetId(id);
            buffData.SetName(name);

            ApplyTemplate(buffData, true);

            return buffData;
        }
#endif
    }
}


