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
        /// 创建Buff逻辑实例（深拷贝）
        /// </summary>
        public IBuffLogic CreateLogic()
        {
            if (buffLogicInstance == null)
            {
                return new EmptyBuffLogic();
            }
            
            // 通过序列化/反序列化创建深拷贝
            string json = JsonUtility.ToJson(buffLogicInstance);
            BuffLogicBase clone = (BuffLogicBase)System.Activator.CreateInstance(buffLogicInstance.GetType());
            JsonUtility.FromJsonOverwrite(json, clone);
            return clone;
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
