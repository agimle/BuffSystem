using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Data;
using BuffSystem.Runtime;

namespace BuffSystem.Advanced.Area
{
    /// <summary>
    /// Buff区域 - 持续性AOE光环效果
    /// 进入区域的单位自动获得Buff，离开则移除
    /// v4.0新增
    /// </summary>
    [AddComponentMenu("BuffSystem/Buff Area")]
    public class BuffArea : MonoBehaviour
    {
        [Header("Buff配置")]
        [SerializeField] private int buffId;
        [SerializeField] private float applyInterval = 1f;
        [Tooltip("Buff过期后是否自动重新应用")]
        [SerializeField] private bool reapplyOnExpire = true;

        [Header("区域设置")]
        [SerializeField] private float radius = 5f;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private bool use2DPhysics = false;

        [Header("行为设置")]
        [SerializeField] private bool removeOnExit = true;
        [SerializeField] private float exitGracePeriod = 0.5f;
        [Tooltip("是否在开始时立即应用一次")]
        [SerializeField] private bool applyOnStart = true;

        // 区域内条目
        private readonly Dictionary<IBuffOwner, BuffAreaEntry> entries = new();
        private float timer;
        private bool isInitialized;

        /// <summary>
        /// 区域内的条目数据
        /// </summary>
        private class BuffAreaEntry
        {
            public IBuffOwner Owner;
            public IBuff AppliedBuff;
            public float LastSeenTime;
            public bool IsInside;
        }

        #region Properties

        /// <summary>
        /// 当前区域内的单位数量
        /// </summary>
        public int OwnerCount => entries.Count;

        /// <summary>
        /// 区域半径
        /// </summary>
        public float Radius
        {
            get => radius;
            set => radius = value;
        }

        /// <summary>
        /// 目标Buff ID
        /// </summary>
        public int BuffId
        {
            get => buffId;
            set => buffId = value;
        }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // 注册到管理器
            BuffAreaManager.Instance?.RegisterArea(this);

            if (applyOnStart)
            {
                UpdateArea();
            }
            isInitialized = true;
        }

        private void OnEnable()
        {
            if (isInitialized)
            {
                BuffAreaManager.Instance?.RegisterArea(this);
            }
        }

        private void OnDisable()
        {
            BuffAreaManager.Instance?.UnregisterArea(this);
        }

        private void Update()
        {
            timer += Time.deltaTime;

            if (timer >= applyInterval)
            {
                timer -= applyInterval;
                UpdateArea();
            }

            CleanupExpiredEntries();
        }

        private void OnDestroy()
        {
            ClearAllEntries();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, radius);

            // 绘制区域内的单位
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                foreach (var entry in entries.Values)
                {
                    if (entry.Owner is MonoBehaviour mono && mono != null)
                    {
                        Gizmos.DrawLine(transform.position, mono.transform.position);
                    }
                }
            }
        }

        #endregion

        #region Area Management

        /// <summary>
        /// 立即更新区域
        /// </summary>
        public void UpdateArea()
        {
            if (use2DPhysics)
            {
                UpdateArea2D();
            }
            else
            {
                UpdateArea3D();
            }
        }

        /// <summary>
        /// 3D物理检测
        /// </summary>
        private void UpdateArea3D()
        {
            var colliders = Physics.OverlapSphere(transform.position, radius, targetLayers);
            var currentOwners = new HashSet<IBuffOwner>();

            foreach (var col in colliders)
            {
                if (col.TryGetComponent<IBuffOwner>(out var owner))
                {
                    currentOwners.Add(owner);
                    ProcessEntry(owner);
                }
            }

            // 标记不在范围内的条目
            foreach (var entry in entries.Values)
            {
                if (!currentOwners.Contains(entry.Owner))
                {
                    entry.IsInside = false;
                }
            }
        }

        /// <summary>
        /// 2D物理检测
        /// </summary>
        private void UpdateArea2D()
        {
            var colliders = Physics2D.OverlapCircleAll(transform.position, radius, targetLayers);
            var currentOwners = new HashSet<IBuffOwner>();

            foreach (var col in colliders)
            {
                if (col.TryGetComponent<IBuffOwner>(out var owner))
                {
                    currentOwners.Add(owner);
                    ProcessEntry(owner);
                }
            }

            // 标记不在范围内的条目
            foreach (var entry in entries.Values)
            {
                if (!currentOwners.Contains(entry.Owner))
                {
                    entry.IsInside = false;
                }
            }
        }

        /// <summary>
        /// 处理区域内条目
        /// </summary>
        private void ProcessEntry(IBuffOwner owner)
        {
            if (!entries.TryGetValue(owner, out var entry))
            {
                // 新进入的单位
                entry = new BuffAreaEntry
                {
                    Owner = owner,
                    IsInside = true,
                    LastSeenTime = Time.time
                };
                entries[owner] = entry;

                // 添加Buff
                ApplyBuffToEntry(entry);

                OnOwnerEnter(owner);
            }
            else
            {
                // 更新已存在的条目
                entry.IsInside = true;
                entry.LastSeenTime = Time.time;

                // 如果Buff已过期且需要重新应用
                if (reapplyOnExpire && (entry.AppliedBuff == null || entry.AppliedBuff.IsMarkedForRemoval))
                {
                    ApplyBuffToEntry(entry);
                }
            }
        }

        /// <summary>
        /// 为条目应用Buff
        /// </summary>
        private void ApplyBuffToEntry(BuffAreaEntry entry)
        {
            var buffData = BuffDatabase.Instance.GetBuffData(buffId);
            if (buffData != null)
            {
                entry.AppliedBuff = entry.Owner.BuffContainer?.AddBuff(buffData, this);
            }
        }

        /// <summary>
        /// 清理过期的条目
        /// </summary>
        private void CleanupExpiredEntries()
        {
            var expiredOwners = new List<IBuffOwner>();

            foreach (var kvp in entries)
            {
                if (!kvp.Value.IsInside && Time.time - kvp.Value.LastSeenTime > exitGracePeriod)
                {
                    expiredOwners.Add(kvp.Key);
                }
            }

            foreach (var owner in expiredOwners)
            {
                RemoveEntry(owner);
            }
        }

        /// <summary>
        /// 移除指定条目
        /// </summary>
        private void RemoveEntry(IBuffOwner owner)
        {
            if (entries.TryGetValue(owner, out var entry))
            {
                if (removeOnExit && entry.AppliedBuff != null)
                {
                    owner.BuffContainer?.RemoveBuff(entry.AppliedBuff);
                }

                OnOwnerExit(owner);
                entries.Remove(owner);
            }
        }

        /// <summary>
        /// 清空所有条目
        /// </summary>
        public void ClearAllEntries()
        {
            if (removeOnExit)
            {
                foreach (var entry in entries.Values)
                {
                    if (entry.AppliedBuff != null)
                    {
                        entry.Owner.BuffContainer?.RemoveBuff(entry.AppliedBuff);
                    }
                }
            }

            entries.Clear();
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// 当单位进入区域时调用（可重写）
        /// </summary>
        protected virtual void OnOwnerEnter(IBuffOwner owner) { }

        /// <summary>
        /// 当单位离开区域时调用（可重写）
        /// </summary>
        protected virtual void OnOwnerExit(IBuffOwner owner) { }

        #endregion

        #region Public Methods

        /// <summary>
        /// 检查指定单位是否在区域内
        /// </summary>
        public bool ContainsOwner(IBuffOwner owner)
        {
            return entries.ContainsKey(owner);
        }

        /// <summary>
        /// 获取区域内的所有单位
        /// </summary>
        public IEnumerable<IBuffOwner> GetAllOwners()
        {
            foreach (var entry in entries.Values)
            {
                yield return entry.Owner;
            }
        }

        /// <summary>
        /// 强制刷新区域
        /// </summary>
        public void ForceRefresh()
        {
            timer = applyInterval;
            UpdateArea();
        }

        /// <summary>
        /// 设置Buff ID并刷新
        /// </summary>
        public void SetBuffId(int newBuffId)
        {
            if (buffId == newBuffId) return;

            // 清除现有条目
            ClearAllEntries();

            buffId = newBuffId;

            // 立即更新
            if (isInitialized)
            {
                UpdateArea();
            }
        }

        #endregion
    }
}
