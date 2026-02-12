using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BuffSystem.Data
{
    /// <summary>
    /// Buff数据配置 - ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewBuffData", menuName = "BuffSystem/Buff Data", order = 1)]
    public class BuffDataSO : ScriptableObject, IBuffData
    {
        [Header("基础信息")]
        [SerializeField] private int id;
        [SerializeField] private string buffName = "New Buff";
        [SerializeField, TextArea(2, 4)] private string description = "";
        [SerializeField] private BuffEffectType effectType = BuffEffectType.Neutral;
        
        [Header("叠加设置")]
        [SerializeField] private bool isUnique = true;
        [SerializeField] private BuffStackMode stackMode = BuffStackMode.Stackable;
        [SerializeField] private int maxStack = 1;
        [SerializeField] private int addStackCount = 1;
        
        [Header("持续时间")]
        [SerializeField] private bool isPermanent = false;
        [SerializeField] private float duration = 5f;
        [SerializeField] private bool canRefresh = true;
        
        [Header("移除设置")]
        [SerializeField] private BuffRemoveMode removeMode = BuffRemoveMode.Remove;
        [SerializeField] private int removeStackCount = 1;
        [SerializeField] private float removeInterval = 0f;
        
        [Header("条件设置")]
        [SerializeReference, SubclassSelector]
        private List<IBuffCondition> addConditions = new();
        
        [SerializeReference, SubclassSelector]
        private List<IBuffCondition> maintainConditions = new();
        
        [Header("关系设置")]
        [SerializeField] private List<int> mutexBuffIds = new(); // 互斥Buff IDs
        [SerializeField] private List<int> dependBuffIds = new(); // 依赖Buff IDs
        [SerializeField] private MutexPriority mutexPriority = MutexPriority.ReplaceOthers;
        
        [Header("标签")]
        [SerializeField] private List<string> tags = new();
        
        [Header("性能设置")]
        [Tooltip("更新频率 - 用于分层更新优化CPU性能")]
        [SerializeField] private UpdateFrequency updateFrequency = UpdateFrequency.Every33ms;
        
        [Header("逻辑脚本")]
        #if UNITY_EDITOR
        [SerializeField] private MonoScript buffLogicScript;
        #endif
        [SerializeReference, SubclassSelector]
        private BuffLogicBase buffLogicInstance;
        
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
        /// 标签列表
        /// </summary>
        public IReadOnlyList<string> Tags => tags;
        
        /// <summary>
        /// 是否拥有指定标签
        /// </summary>
        public bool HasTag(string tag) => tags.Contains(tag);
        
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
        }
        
        #endif
    }
}
