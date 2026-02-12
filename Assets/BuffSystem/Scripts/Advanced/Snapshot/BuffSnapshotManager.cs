using System;
using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;

namespace BuffSystem.Advanced.Snapshot
{
    /// <summary>
    /// Buff快照管理器 - 管理快照的创建、缓存和查询
    /// v4.0新增
    /// </summary>
    public class BuffSnapshotManager : MonoBehaviour
    {
        private static BuffSnapshotManager instance;
        public static BuffSnapshotManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<BuffSnapshotManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("BuffSnapshotManager");
                        instance = go.AddComponent<BuffSnapshotManager>();
                    }
                }
                return instance;
            }
        }

        // 快照缓存 - 按Buff实例ID存储
        private readonly Dictionary<int, BuffSnapshot> snapshotCache = new();

        // 注册的快照捕获器
        private readonly Dictionary<int, ISnapshotCapturer> capturerRegistry = new();

        [Header("设置")]
        [Tooltip("最大缓存快照数量")]
        [SerializeField] private int maxCacheSize = 100;
        [Tooltip("是否自动清理过期快照")]
        [SerializeField] private bool autoCleanup = true;
        [Tooltip("快照过期时间（秒）")]
        [SerializeField] private float snapshotExpireTime = 300f;

        #region Unity Lifecycle

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (autoCleanup)
            {
                CleanupExpiredSnapshots();
            }
        }

        #endregion

        #region Snapshot Creation

        /// <summary>
        /// 创建快照
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <param name="buff">来源Buff</param>
        /// <param name="attributes">属性字典</param>
        /// <returns>创建的快照</returns>
        public BuffSnapshot CreateSnapshot(IBuffOwner owner, IBuff buff, Dictionary<string, float> attributes)
        {
            var snapshot = new BuffSnapshot(buff, attributes);
            CacheSnapshot(buff.InstanceId, snapshot);
            return snapshot;
        }

        /// <summary>
        /// 使用捕获器创建快照
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <param name="buff">来源Buff</param>
        /// <param name="capturer">捕获器</param>
        /// <returns>创建的快照</returns>
        public BuffSnapshot CreateSnapshot(IBuffOwner owner, IBuff buff, ISnapshotCapturer capturer)
        {
            if (capturer == null)
            {
                Debug.LogWarning("[BuffSnapshotManager] 捕获器为空");
                return null;
            }

            var snapshot = capturer.Capture(owner, buff);
            if (snapshot != null && buff != null)
            {
                CacheSnapshot(buff.InstanceId, snapshot);
            }
            return snapshot;
        }

        /// <summary>
        /// 创建空快照
        /// </summary>
        /// <param name="buff">来源Buff</param>
        /// <returns>创建的快照</returns>
        public BuffSnapshot CreateEmptySnapshot(IBuff buff)
        {
            var snapshot = new BuffSnapshot(buff, new Dictionary<string, float>());
            if (buff != null)
            {
                CacheSnapshot(buff.InstanceId, snapshot);
            }
            return snapshot;
        }

        #endregion

        #region Cache Management

        /// <summary>
        /// 缓存快照
        /// </summary>
        /// <param name="buffInstanceId">Buff实例ID</param>
        /// <param name="snapshot">快照</param>
        private void CacheSnapshot(int buffInstanceId, BuffSnapshot snapshot)
        {
            // 检查缓存大小
            if (snapshotCache.Count >= maxCacheSize)
            {
                RemoveOldestSnapshot();
            }

            snapshotCache[buffInstanceId] = snapshot;
        }

        /// <summary>
        /// 获取快照
        /// </summary>
        /// <param name="buffInstanceId">Buff实例ID</param>
        /// <returns>快照，如果不存在则返回null</returns>
        public BuffSnapshot GetSnapshot(int buffInstanceId)
        {
            return snapshotCache.TryGetValue(buffInstanceId, out var snapshot) ? snapshot : null;
        }

        /// <summary>
        /// 获取快照（从Buff）
        /// </summary>
        /// <param name="buff">Buff</param>
        /// <returns>快照，如果不存在则返回null</returns>
        public BuffSnapshot GetSnapshot(IBuff buff)
        {
            return buff != null ? GetSnapshot(buff.InstanceId) : null;
        }

        /// <summary>
        /// 移除快照
        /// </summary>
        /// <param name="buffInstanceId">Buff实例ID</param>
        public void RemoveSnapshot(int buffInstanceId)
        {
            snapshotCache.Remove(buffInstanceId);
        }

        /// <summary>
        /// 移除快照（从Buff）
        /// </summary>
        /// <param name="buff">Buff</param>
        public void RemoveSnapshot(IBuff buff)
        {
            if (buff != null)
            {
                RemoveSnapshot(buff.InstanceId);
            }
        }

        /// <summary>
        /// 清除所有快照
        /// </summary>
        public void ClearAllSnapshots()
        {
            snapshotCache.Clear();
        }

        /// <summary>
        /// 移除最旧的快照
        /// </summary>
        private void RemoveOldestSnapshot()
        {
            int oldestKey = 0;
            float oldestTime = float.MaxValue;

            foreach (var kvp in snapshotCache)
            {
                if (kvp.Value.Timestamp < oldestTime)
                {
                    oldestTime = kvp.Value.Timestamp;
                    oldestKey = kvp.Key;
                }
            }

            if (oldestKey != 0)
            {
                snapshotCache.Remove(oldestKey);
            }
        }

        /// <summary>
        /// 清理过期快照
        /// </summary>
        private void CleanupExpiredSnapshots()
        {
            var expiredKeys = new List<int>();
            float currentTime = Time.time;

            foreach (var kvp in snapshotCache)
            {
                if (currentTime - kvp.Value.Timestamp > snapshotExpireTime)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (var key in expiredKeys)
            {
                snapshotCache.Remove(key);
            }
        }

        #endregion

        #region Capturer Registry

        /// <summary>
        /// 注册捕获器
        /// </summary>
        /// <param name="buffDataId">Buff数据ID</param>
        /// <param name="capturer">捕获器</param>
        public void RegisterCapturer(int buffDataId, ISnapshotCapturer capturer)
        {
            capturerRegistry[buffDataId] = capturer;
        }

        /// <summary>
        /// 注销捕获器
        /// </summary>
        /// <param name="buffDataId">Buff数据ID</param>
        public void UnregisterCapturer(int buffDataId)
        {
            capturerRegistry.Remove(buffDataId);
        }

        /// <summary>
        /// 获取捕获器
        /// </summary>
        /// <param name="buffDataId">Buff数据ID</param>
        /// <returns>捕获器，如果不存在则返回null</returns>
        public ISnapshotCapturer GetCapturer(int buffDataId)
        {
            return capturerRegistry.TryGetValue(buffDataId, out var capturer) ? capturer : null;
        }

        /// <summary>
        /// 使用注册的捕获器创建快照
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <param name="buff">Buff</param>
        /// <returns>快照，如果没有注册捕获器则返回null</returns>
        public BuffSnapshot CreateSnapshotWithRegisteredCapturer(IBuffOwner owner, IBuff buff)
        {
            if (buff == null) return null;

            var capturer = GetCapturer(buff.DataId);
            if (capturer == null) return null;

            return CreateSnapshot(owner, buff, capturer);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 获取缓存的快照数量
        /// </summary>
        public int CacheCount => snapshotCache.Count;

        /// <summary>
        /// 检查是否存在快照
        /// </summary>
        /// <param name="buffInstanceId">Buff实例ID</param>
        /// <returns>是否存在</returns>
        public bool HasSnapshot(int buffInstanceId)
        {
            return snapshotCache.ContainsKey(buffInstanceId);
        }

        /// <summary>
        /// 获取所有快照
        /// </summary>
        public IEnumerable<BuffSnapshot> GetAllSnapshots()
        {
            return snapshotCache.Values;
        }

        #endregion
    }
}
