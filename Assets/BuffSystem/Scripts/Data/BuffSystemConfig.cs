using UnityEngine;

namespace BuffSystem.Data
{
    /// <summary>
    /// Buff系统全局配置
    /// </summary>
    [CreateAssetMenu(fileName = "BuffSystemConfig", menuName = "BuffSystem/System Config", order = 0)]
    public class BuffSystemConfig : ScriptableObject
    {
        [Header("对象池设置")] [SerializeField] private int defaultPoolCapacity = 32;
        [SerializeField] private int maxPoolSize = 128;

        [Header("更新设置")] [SerializeField] private UpdateMode updateMode = UpdateMode.EveryFrame;
        [SerializeField] private int batchCount = 4;
        [SerializeField] private float updateInterval = 0.1f;

        [Header("调试设置")] [SerializeField] private bool enableDebugLog = false;
        [SerializeField] private bool enableGizmos = false;

        #region Properties

        /// <summary>
        /// 默认对象池容量
        /// </summary>
        public int DefaultPoolCapacity => defaultPoolCapacity;

        /// <summary>
        /// 对象池最大容量
        /// </summary>
        public int MaxPoolSize => maxPoolSize;

        /// <summary>
        /// 更新模式
        /// </summary>
        public UpdateMode UpdateMode => updateMode;

        /// <summary>
        /// 批处理数量
        /// </summary>
        public int BatchCount => batchCount;

        /// <summary>
        /// 更新间隔（用于Interval模式）
        /// </summary>
        public float UpdateInterval => updateInterval;

        /// <summary>
        /// 是否启用调试日志
        /// </summary>
        public bool EnableDebugLog => enableDebugLog;

        /// <summary>
        /// 是否启用Gizmos
        /// </summary>
        public bool EnableGizmos => enableGizmos;

        #endregion

        #region Singleton Access

        private static BuffSystemConfig _instance;

        public static BuffSystemConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    LoadInstance();
                }

                return _instance;
            }
        }

        private static void LoadInstance()
        {
            // 尝试从Resources加载
            _instance = Resources.Load<BuffSystemConfig>("BuffSystem/BuffSystemConfig");

            if (_instance == null)
            {
                // 创建默认配置
                Debug.LogWarning("[BuffSystem] 未找到BuffSystemConfig，使用默认配置");
                _instance = CreateInstance<BuffSystemConfig>();
            }
        }

        #endregion

        private void OnValidate()
        {
            defaultPoolCapacity = Mathf.Max(1, defaultPoolCapacity);
            maxPoolSize = Mathf.Max(defaultPoolCapacity, maxPoolSize);
            batchCount = Mathf.Max(1, batchCount);
            updateInterval = Mathf.Max(0.01f, updateInterval);
        }
    }
}
