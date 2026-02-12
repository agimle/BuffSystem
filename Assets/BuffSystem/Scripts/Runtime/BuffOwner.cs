using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Data;
using BuffSystem.Events;
using BuffSystem.Combo;

namespace BuffSystem.Runtime
{
    /// <summary>
    /// Buff持有者组件 - MonoBehaviour适配器
    /// 挂载到需要持有Buff的GameObject上
    /// </summary>
    /// <remarks>
    /// 🔒 稳定API: v6.0后保证向后兼容
    /// 版本历史: v1.0-v6.0 逐步完善
    /// 修改策略: 只允许bug修复，不允许破坏性变更
    /// </remarks>
    [AddComponentMenu("BuffSystem/Buff Owner")]
    [DisallowMultipleComponent]
    public class BuffOwner : MonoBehaviour, IBuffOwner
    {
        [Header("设置")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool updateInFixedUpdate = false;
        [SerializeField] private bool showDebugInfo = false;

        [Header("免疫设置 (v4.0)")]
        [SerializeField] private List<int> immuneBuffIds = new();
        [SerializeField] private List<string> immuneTags = new();
        [SerializeField] private bool removeExistingOnImmunity = true;

        // 静态列表 - 管理所有BuffOwner实例
        private static readonly List<BuffOwner> allOwners = new();

        // 内部组件
        private BuffContainer buffContainer;
        private BuffLocalEventSystem localEvents;
        
        // v4.0: 免疫系统的HashSet缓存（提高查询性能）
        private HashSet<int> immuneBuffIdSet;
        private HashSet<string> immuneTagSet;
        
        #region Properties
        
        /// <summary>
        /// Buff容器
        /// </summary>
        public IBuffContainer BuffContainer => buffContainer;
        
        /// <summary>
        /// 本地事件系统
        /// </summary>
        public BuffLocalEventSystem LocalEvents => localEvents;
        
        /// <summary>
        /// 当前Buff数量
        /// </summary>
        public int BuffCount => buffContainer?.Count ?? 0;
        
        #endregion
        
        #region IBuffOwner Implementation
        
        public int OwnerId => GetInstanceID();
        
        public string OwnerName => gameObject.name;
        
        public void OnBuffEvent(BuffEventType eventType, IBuff buff)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[BuffOwner] {gameObject.name} - 事件: {eventType}, Buff: {buff?.Name}");
            }
            
            // 触发本地事件
            switch (eventType)
            {
                case BuffEventType.Added:
                    localEvents?.TriggerBuffAdded(buff);
                    break;
                case BuffEventType.Removed:
                    localEvents?.TriggerBuffRemoved(buff);
                    break;
                case BuffEventType.StackChanged:
                    // 层数变化在Buff内部处理
                    break;
                case BuffEventType.Refreshed:
                    localEvents?.TriggerRefreshed(buff);
                    break;
                case BuffEventType.Expired:
                    localEvents?.TriggerExpired(buff);
                    break;
                case BuffEventType.Cleared:
                    localEvents?.TriggerCleared();
                    break;
            }
        }
        
        // v4.0: 免疫系统实现
        public bool IsImmuneTo(int buffId)
        {
            return immuneBuffIdSet?.Contains(buffId) ?? false;
        }
        
        public bool IsImmuneToTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return false;
            return immuneTagSet?.Contains(tag) ?? false;
        }
        
        public IReadOnlyList<string> ImmuneTags => immuneTags;
        
        #endregion
        
        #region Unity Lifecycle

        private void Awake()
        {
            if (autoInitialize)
            {
                Initialize();
            }
        }

        private void OnEnable()
        {
            if (!allOwners.Contains(this))
            {
                allOwners.Add(this);
            }
        }

        private void OnDisable()
        {
            allOwners.Remove(this);
        }

        private void Update()
        {
            // 回合制模式下不自动更新
            if (BuffSystemUpdater.CurrentUpdateMode == UpdateMode.TurnBased) return;

            if (!updateInFixedUpdate && buffContainer != null)
            {
                buffContainer.Update(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            // 回合制模式下不自动更新
            if (BuffSystemUpdater.CurrentUpdateMode == UpdateMode.TurnBased) return;

            if (updateInFixedUpdate && buffContainer != null)
            {
                buffContainer.Update(Time.fixedDeltaTime);
            }
        }

        private void OnDestroy()
        {
            // 清理Combo状态
            if (BuffComboManager.Instance != null)
            {
                BuffComboManager.Instance.ClearOwnerCombos(this);
            }

            // 清理所有Buff
            buffContainer?.ClearAllBuffs();
            buffContainer = null;
            localEvents = null;
        }

        #endregion

        #region Static Update Methods

        /// <summary>
        /// 批量更新所有激活的BuffOwner
        /// </summary>
        internal static void UpdateAll(float deltaTime)
        {
            for (int i = allOwners.Count - 1; i >= 0; i--)
            {
                var owner = allOwners[i];
                if (owner == null)
                {
                    allOwners.RemoveAt(i);
                    continue;
                }

                if (owner.gameObject.activeInHierarchy && owner.buffContainer != null)
                {
                    owner.buffContainer.Update(deltaTime);
                }
            }
        }

        /// <summary>
        /// 分批更新BuffOwner
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        /// <param name="batchIndex">当前批次索引</param>
        /// <param name="totalBatches">总批次数量</param>
        internal static void UpdateBatch(float deltaTime, int batchIndex, int totalBatches)
        {
            if (totalBatches <= 1)
            {
                UpdateAll(deltaTime);
                return;
            }

            // 分批更新：从batchIndex开始，每次跳过totalBatches个
            for (int i = batchIndex; i < allOwners.Count; i += totalBatches)
            {
                var owner = allOwners[i];
                if (owner == null) continue;

                if (owner.gameObject.activeInHierarchy && owner.buffContainer != null)
                {
                    owner.buffContainer.Update(deltaTime);
                }
            }
        }

        /// <summary>
        /// 获取所有活跃持有者的数量
        /// </summary>
        internal static int ActiveOwnerCount => allOwners.Count;

        /// <summary>
        /// 获取所有Buff持有者（只读）
        /// </summary>
        public static IReadOnlyList<BuffOwner> AllOwners => allOwners;

        #endregion
        
        #region Initialization
        
        /// <summary>
        /// 初始化Buff持有者
        /// </summary>
        public void Initialize()
        {
            if (buffContainer != null) return;
            
            buffContainer = new BuffContainer(this);
            localEvents = new BuffLocalEventSystem(this);
            
            // v4.0: 初始化免疫系统
            InitializeImmunity();
            
            // 初始化Buff系统
            BuffApi.Initialize();
            
            // 预热对象池
            var config = Data.BuffSystemConfig.Instance;
            if (config.PrewarmOnInitialize && config.PrewarmCount > 0)
            {
                buffContainer.Prewarm(config.PrewarmCount);
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[BuffOwner] {gameObject.name} 初始化完成");
            }
        }
        
        /// <summary>
        /// v4.0: 初始化免疫系统
        /// </summary>
        private void InitializeImmunity()
        {
            immuneBuffIdSet = new HashSet<int>(immuneBuffIds);
            immuneTagSet = new HashSet<string>(immuneTags);
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 添加Buff（通过ID）
        /// </summary>
        public IBuff AddBuff(int buffId, object source = null)
        {
            return BuffApi.AddBuff(buffId, this, source);
        }
        
        /// <summary>
        /// 添加Buff（通过名称）
        /// </summary>
        public IBuff AddBuff(string buffName, object source = null)
        {
            return BuffApi.AddBuff(buffName, this, source);
        }
        
        /// <summary>
        /// 移除Buff
        /// </summary>
        public void RemoveBuff(IBuff buff)
        {
            BuffApi.RemoveBuff(buff);
        }
        
        /// <summary>
        /// 移除指定ID的Buff
        /// </summary>
        public void RemoveBuff(int buffId)
        {
            BuffApi.RemoveBuff(buffId, this);
        }
        
        /// <summary>
        /// 移除指定名称的Buff
        /// </summary>
        public void RemoveBuff(string buffName)
        {
            BuffApi.RemoveBuff(buffName, this);
        }
        
        /// <summary>
        /// 清空所有Buff
        /// </summary>
        public void ClearBuffs()
        {
            BuffApi.ClearBuffs(this);
        }
        
        /// <summary>
        /// 是否拥有指定Buff
        /// </summary>
        public bool HasBuff(int buffId)
        {
            return BuffApi.HasBuff(buffId, this);
        }
        
        /// <summary>
        /// 是否拥有指定Buff
        /// </summary>
        public bool HasBuff(string buffName)
        {
            return BuffApi.HasBuff(buffName, this);
        }
        
        /// <summary>
        /// 获取Buff
        /// </summary>
        public IBuff GetBuff(int buffId, object source = null)
        {
            return BuffApi.GetBuff(buffId, this, source);
        }
        
        /// <summary>
        /// 获取Buff
        /// </summary>
        public IBuff GetBuff(string buffName, object source = null)
        {
            return BuffApi.GetBuff(buffName, this, source);
        }

        #endregion

        #region Immunity Methods (v4.0)
        
        /// <summary>
        /// 添加对指定Buff的免疫
        /// </summary>
        /// <param name="buffId">Buff ID</param>
        public void AddImmunity(int buffId)
        {
            if (immuneBuffIdSet.Add(buffId))
            {
                immuneBuffIds.Add(buffId);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[BuffOwner] {gameObject.name} 添加对Buff {buffId} 的免疫");
                }
                
                // 移除已存在的该Buff
                if (removeExistingOnImmunity)
                {
                    RemoveBuff(buffId);
                }
            }
        }
        
        /// <summary>
        /// 移除对指定Buff的免疫
        /// </summary>
        /// <param name="buffId">Buff ID</param>
        public void RemoveImmunity(int buffId)
        {
            if (immuneBuffIdSet.Remove(buffId))
            {
                immuneBuffIds.Remove(buffId);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[BuffOwner] {gameObject.name} 移除对Buff {buffId} 的免疫");
                }
            }
        }
        
        /// <summary>
        /// 添加对指定标签的免疫
        /// </summary>
        /// <param name="tag">标签</param>
        public void AddImmunityTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;
            
            if (immuneTagSet.Add(tag))
            {
                immuneTags.Add(tag);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[BuffOwner] {gameObject.name} 添加对标签 {tag} 的免疫");
                }
                
                // 移除已存在的该标签的Buff
                if (removeExistingOnImmunity)
                {
                    RemoveBuffsByTag(tag);
                }
            }
        }
        
        /// <summary>
        /// 移除对指定标签的免疫
        /// </summary>
        /// <param name="tag">标签</param>
        public void RemoveImmunityTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;
            
            if (immuneTagSet.Remove(tag))
            {
                immuneTags.Remove(tag);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[BuffOwner] {gameObject.name} 移除对标签 {tag} 的免疫");
                }
            }
        }
        
        /// <summary>
        /// 清空所有免疫
        /// </summary>
        public void ClearImmunity()
        {
            immuneBuffIdSet.Clear();
            immuneBuffIds.Clear();
            immuneTagSet.Clear();
            immuneTags.Clear();
            
            if (showDebugInfo)
            {
                Debug.Log($"[BuffOwner] {gameObject.name} 清空所有免疫");
            }
        }
        
        /// <summary>
        /// 根据标签移除Buff
        /// </summary>
        /// <param name="tag">标签</param>
        private void RemoveBuffsByTag(string tag)
        {
            if (buffContainer == null) return;
            
            var buffsToRemove = new List<IBuff>();
            foreach (var buff in buffContainer.AllBuffs)
            {
                if (buff.Data.Tags.Contains(tag))
                {
                    buffsToRemove.Add(buff);
                }
            }
            
            foreach (var buff in buffsToRemove)
            {
                RemoveBuff(buff);
            }
        }
        
        #endregion

        #region Combo Methods

        /// <summary>
        /// 检查是否激活了指定Combo
        /// </summary>
        public bool IsComboActive(int comboId)
        {
            return BuffComboManager.Instance?.IsComboActive(comboId, this) ?? false;
        }

        /// <summary>
        /// 检查是否激活了指定Combo
        /// </summary>
        public bool IsComboActive(BuffComboData combo)
        {
            return combo != null && IsComboActive(combo.ComboId);
        }

        /// <summary>
        /// 获取所有激活的Combo
        /// </summary>
        public IEnumerable<BuffComboData> GetActiveCombos()
        {
            return BuffComboManager.Instance?.GetActiveCombos(this) ?? System.Array.Empty<BuffComboData>();
        }

        /// <summary>
        /// 获取Combo触发次数
        /// </summary>
        public int GetComboTriggerCount(int comboId)
        {
            return BuffComboManager.Instance?.GetTriggerCount(this, comboId) ?? 0;
        }

        /// <summary>
        /// 获取Combo触发次数
        /// </summary>
        public int GetComboTriggerCount(BuffComboData combo)
        {
            return combo != null ? GetComboTriggerCount(combo.ComboId) : 0;
        }

        #endregion

        #region Debug

        private static readonly Rect debugWindowRect = new(10, 10, 250, 300);

        private void OnGUI()
        {
            if (!showDebugInfo || buffContainer == null) return;

            GUILayout.BeginArea(debugWindowRect);
            GUILayout.BeginVertical("box");

            GUILayout.Label($"<b>{gameObject.name}</b>");
            GUILayout.Label($"Buff数量: {BuffCount}");

            if (BuffCount > 0)
            {
                GUILayout.Space(5);
                GUILayout.Label("<b>当前Buff:</b>");

                foreach (var buff in buffContainer.AllBuffs)
                {
                    string timeText = buff.IsPermanent ? "∞" : $"{buff.RemainingTime:F1}s";
                    GUILayout.Label($"  • {buff.Name} ({buff.CurrentStack}/{buff.MaxStack}) [{timeText}]");
                }
            }

            // 显示激活的Combo
            var activeCombos = GetActiveCombos();
            int comboCount = 0;
            foreach (var _ in activeCombos) comboCount++;

            if (comboCount > 0)
            {
                GUILayout.Space(5);
                GUILayout.Label("<b>激活Combo:</b>");

                foreach (var combo in GetActiveCombos())
                {
                    GUILayout.Label($"  • {combo.ComboName}");
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion
    }
}
