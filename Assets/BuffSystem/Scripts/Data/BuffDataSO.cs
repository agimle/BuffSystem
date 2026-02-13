using System;
using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Groups;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BuffSystem.Data
{
    /// <summary>
    /// 组策略类型 - 定义当组满时的处理方式
    /// </summary>
    public enum GroupStrategyType
    {
        /// <summary>添加到组（默认）</summary>
        AddToGroup,

        /// <summary>替换组内最旧的Buff</summary>
        ReplaceOldest,

        /// <summary>替换组内最弱的Buff</summary>
        ReplaceWeakest,

        /// <summary>组满时阻止添加</summary>
        BlockIfFull
    }

    /// <summary>
    /// Buff组配置项
    /// </summary>
    [Serializable]
    public class BuffGroupConfigEntry
    {
        [Tooltip("组ID")] public string groupId;

        [Tooltip("组内最大Buff数量（0表示使用组的全局配置）")] public int maxStack;

        [Tooltip("组策略")] public GroupStrategyType strategy = GroupStrategyType.AddToGroup;
    }

    /// <summary>
    /// Buff数据配置 - ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewBuffData", menuName = "BuffSystem/Buff Data", order = 1)]
    public class BuffDataSO : ScriptableObject, IBuffData
    {
        [Header("基础信息")] [SerializeField] private int id;
        [SerializeField] private string buffName = "New Buff";
        [SerializeField, TextArea(2, 4)] private string description = "";
        [SerializeField] private BuffEffectType effectType = BuffEffectType.Neutral;

        [Header("叠加设置")] [SerializeField] private bool isUnique = true;
        [SerializeField] private BuffStackMode stackMode = BuffStackMode.Stackable;
        [SerializeField] private int maxStack = 1;
        [SerializeField] private int addStackCount = 1;

        [Header("持续时间")] [SerializeField] private bool isPermanent = false;
        [SerializeField] private float duration = 5f;
        [SerializeField] private bool canRefresh = true;

        [Header("移除设置")] [SerializeField] private BuffRemoveMode removeMode = BuffRemoveMode.Remove;
        [SerializeField] private int removeStackCount = 1;
        [SerializeField] private float removeInterval = 0f;

        [Header("条件设置")] [SerializeReference, SubclassSelector]
        private List<IBuffCondition> addConditions = new();

        [SerializeReference, SubclassSelector] private List<IBuffCondition> maintainConditions = new();

        [Header("关系设置")] [SerializeField] private List<int> mutexBuffIds = new(); // 互斥Buff IDs
        [SerializeField] private List<int> dependBuffIds = new(); // 依赖Buff IDs
        [SerializeField] private MutexPriority mutexPriority = MutexPriority.ReplaceOthers;

        [Header("组配置")] [Tooltip("所属Buff组配置列表")] [SerializeField]
        private List<BuffGroupConfigEntry> groupConfigs = new();

        [Header("标签")] [SerializeField] private List<BuffTag> tags = new();

        [Header("性能设置")] [Tooltip("更新频率 - 用于分层更新优化CPU性能")] [SerializeField]
        private UpdateFrequency updateFrequency = UpdateFrequency.Every33ms;

        [Header("逻辑脚本")]
#if UNITY_EDITOR
        [SerializeField]
        private MonoScript buffLogicScript;
#endif
        [SerializeReference, SubclassSelector] private BuffLogicBase buffLogicInstance;

        #region IBuffData Implementation

        public int Id => id;
        public string Name => buffName;
        public string Description => description;
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

        /// <summary>
        /// 添加条件列表
        /// </summary>
        public IReadOnlyList<IBuffCondition> AddConditions => addConditions;

        /// <summary>
        /// 维持条件列表
        /// </summary>
        public IReadOnlyList<IBuffCondition> MaintainConditions => maintainConditions;

        /// <summary>
        /// 互斥Buff ID列表
        /// </summary>
        public IReadOnlyList<int> MutexBuffIds => mutexBuffIds;

        /// <summary>
        /// 依赖Buff ID列表
        /// </summary>
        public IReadOnlyList<int> DependBuffIds => dependBuffIds;

        /// <summary>
        /// 互斥优先级
        /// </summary>
        public MutexPriority MutexPriority => mutexPriority;

        /// <summary>
        /// 组配置列表
        /// </summary>
        public IReadOnlyList<BuffGroupConfigEntry> GroupConfigs => groupConfigs;

        /// <summary>
        /// 是否属于指定组
        /// </summary>
        public bool BelongsToGroup(string groupId)
        {
            if (string.IsNullOrEmpty(groupId) || groupConfigs.Count == 0)
                return false;

            foreach (var config in groupConfigs)
            {
                if (config.groupId == groupId)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 获取指定组的配置
        /// </summary>
        public BuffGroupConfigEntry GetGroupConfig(string groupId)
        {
            if (string.IsNullOrEmpty(groupId) || groupConfigs.Count == 0)
                return null;

            foreach (var config in groupConfigs)
            {
                if (config.groupId == groupId)
                    return config;
            }

            return null;
        }

        /// <summary>
        /// 标签列表（返回BuffTag列表）
        /// </summary>
        public IReadOnlyList<BuffTag> BuffTags => tags;

        /// <summary>
        /// 标签列表（字符串形式，兼容旧接口）
        /// </summary>
        public IReadOnlyList<string> Tags
        {
            get
            {
                // 实时转换，避免缓存不一致问题
                if (tags.Count == 0)
                    return System.Array.Empty<string>();

                var result = new List<string>(tags.Count);
                foreach (var tag in tags)
                {
                    result.Add(tag.TagName);
                }

                return result;
            }
        }

        /// <summary>
        /// 是否拥有指定标签（使用哈希优化）
        /// </summary>
        public bool HasTag(string tag)
        {
            if (string.IsNullOrEmpty(tag) || tags.Count == 0)
                return false;

            int hash = BuffTagManager.GetTagHash(tag);
            foreach (var buffTag in tags)
            {
                if (buffTag.TagHash == hash)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 是否拥有指定标签（BuffTag版本）
        /// </summary>
        public bool HasTag(BuffTag tag)
        {
            if (tags.Count == 0)
                return false;

            int hash = tag.TagHash;
            foreach (var buffTag in tags)
            {
                if (buffTag.TagHash == hash)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 更新频率 - 用于分层更新优化CPU性能
        /// </summary>
        public UpdateFrequency UpdateFrequency => updateFrequency;

        /// <summary>
        /// 创建Buff逻辑实例（深拷贝）
        /// </summary>
        public IBuffLogic CreateLogic()
        {
            if (buffLogicInstance == null)
            {
                return new EmptyBuffLogic();
            }

            // 使用原型模式进行克隆（性能优于Json序列化）
            return (IBuffLogic)buffLogicInstance.Clone();
        }

        #endregion

        #region 模板系统支持

#if UNITY_EDITOR

        [Header("模板继承")]
        [Tooltip("基础模板")]
        [SerializeField] private BuffTemplate baseTemplate;

        [Tooltip("是否继承模板值")]
        [SerializeField] private bool inheritFromTemplate = true;

        /// <summary>
        /// 基础模板
        /// </summary>
        public BuffTemplate BaseTemplate => baseTemplate;

        /// <summary>
        /// 是否继承模板
        /// </summary>
        public bool InheritFromTemplate => inheritFromTemplate;

        /// <summary>
        /// 应用模板（如果配置了模板且允许继承）
        /// </summary>
        public void ApplyTemplate()
        {
            if (baseTemplate != null && inheritFromTemplate)
            {
                baseTemplate.ApplyTemplate(this, false);
            }
        }

#endif

        #endregion

        #region 编辑器方法（模板系统使用）

#if UNITY_EDITOR

        /// <summary>
        /// 设置ID（模板系统使用）
        /// </summary>
        public void SetId(int value) => id = value;

        /// <summary>
        /// 设置名称（模板系统使用）
        /// </summary>
        public void SetName(string value) => buffName = value;

        /// <summary>
        /// 设置效果类型（模板系统使用）
        /// </summary>
        public void SetEffectType(BuffEffectType value) => effectType = value;

        /// <summary>
        /// 设置是否唯一（模板系统使用）
        /// </summary>
        public void SetIsUnique(bool value) => isUnique = value;

        /// <summary>
        /// 设置叠加模式（模板系统使用）
        /// </summary>
        public void SetStackMode(BuffStackMode value) => stackMode = value;

        /// <summary>
        /// 设置最大层数（模板系统使用）
        /// </summary>
        public void SetMaxStack(int value) => maxStack = Mathf.Max(1, value);

        /// <summary>
        /// 设置添加层数（模板系统使用）
        /// </summary>
        public void SetAddStackCount(int value) => addStackCount = Mathf.Max(1, value);

        /// <summary>
        /// 设置是否永久（模板系统使用）
        /// </summary>
        public void SetIsPermanent(bool value) => isPermanent = value;

        /// <summary>
        /// 设置持续时间（模板系统使用）
        /// </summary>
        public void SetDuration(float value) => duration = Mathf.Max(0.1f, value);

        /// <summary>
        /// 设置是否可刷新（模板系统使用）
        /// </summary>
        public void SetCanRefresh(bool value) => canRefresh = value;

        /// <summary>
        /// 设置移除模式（模板系统使用）
        /// </summary>
        public void SetRemoveMode(BuffRemoveMode value) => removeMode = value;

        /// <summary>
        /// 设置移除层数（模板系统使用）
        /// </summary>
        public void SetRemoveStackCount(int value) => removeStackCount = Mathf.Max(1, value);

        /// <summary>
        /// 设置移除间隔（模板系统使用）
        /// </summary>
        public void SetRemoveInterval(float value) => removeInterval = Mathf.Max(0f, value);

        /// <summary>
        /// 设置互斥优先级（模板系统使用）
        /// </summary>
        public void SetMutexPriority(MutexPriority value) => mutexPriority = value;

        /// <summary>
        /// 添加组配置（模板系统使用）
        /// </summary>
        public void AddGroupConfig(BuffGroupConfigEntry config)
        {
            if (config != null && !string.IsNullOrEmpty(config.groupId))
            {
                groupConfigs.Add(config);
            }
        }

        /// <summary>
        /// 添加标签（模板系统使用）
        /// </summary>
        public void AddTag(BuffTag tag)
        {
            if (!tags.Contains(tag))
            {
                tags.Add(tag);
            }
        }

        /// <summary>
        /// 设置更新频率（模板系统使用）
        /// </summary>
        public void SetUpdateFrequency(UpdateFrequency value) => updateFrequency = value;

#endif

        #endregion

        #region 编辑器方法

#if UNITY_EDITOR

        private void OnValidate()
        {
            // 确保ID不为0
            if (id == 0)
            {
                // 生成基于名称的哈希ID
                id = Mathf.Abs(buffName.GetHashCode());
            }

            // 确保数值合法
            maxStack = Mathf.Max(1, maxStack);
            addStackCount = Mathf.Max(1, addStackCount);
            removeStackCount = Mathf.Max(1, removeStackCount);
            duration = Mathf.Max(0.1f, duration);
            removeInterval = Mathf.Max(0f, removeInterval);

            // 应用模板（仅在编辑器中且配置了模板时）
            if (baseTemplate != null && inheritFromTemplate)
            {
                ApplyTemplate();
            }
        }

#endif

        #endregion
    }
}
