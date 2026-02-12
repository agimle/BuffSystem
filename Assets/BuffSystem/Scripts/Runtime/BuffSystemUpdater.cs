using UnityEngine;
using BuffSystem.Data;

namespace BuffSystem.Runtime
{
    /// <summary>
    /// Buff系统全局更新器
    /// 管理所有Buff持有者的更新
    /// </summary>
    [AddComponentMenu("BuffSystem/Buff System Updater")]
    [DefaultExecutionOrder(-100)]
    public class BuffSystemUpdater : MonoBehaviour
    {
        [Header("更新设置")]
        [SerializeField] private UpdateMode updateMode = UpdateMode.EveryFrame;
        [SerializeField] private float updateInterval = 0.1f;

        [Header("分层更新设置")]
        [SerializeField] private bool enableFrequencyBasedUpdate = true;
        [Tooltip("自动根据Buff类型分配更新频率")]
        [SerializeField] private bool autoAssignFrequency = true;

        [Header("批处理设置")]
        [SerializeField] private bool enableBatchUpdate = false;
        [SerializeField] private int batchCount = 4;
        [SerializeField] private int batchThreshold = 100;

        private float updateTimer;
        private int currentBatchIndex = 0;
        private static BuffSystemUpdater instance;
        private FrequencyBasedUpdater frequencyUpdater;

        // 线程安全锁
        private static readonly object instanceLock = new();

        /// <summary>
        /// 当前更新模式
        /// </summary>
        public static UpdateMode CurrentUpdateMode => instance != null ? instance.updateMode : UpdateMode.Manual;

        /// <summary>
        /// 是否启用分层更新
        /// </summary>
        public static bool EnableFrequencyBasedUpdate => instance != null && instance.enableFrequencyBasedUpdate;

        /// <summary>
        /// 分层更新器实例
        /// </summary>
        public static FrequencyBasedUpdater FrequencyUpdater => instance?.frequencyUpdater;

        public static BuffSystemUpdater Instance
        {
            get
            {
                if (instance == null)
                {
                    CreateInstance();
                }
                return instance;
            }
        }

        private static void CreateInstance()
        {
            if (instance != null) return;

            lock (instanceLock)
            {
                if (instance != null) return;

                var go = new GameObject("BuffSystemUpdater");
                instance = go.AddComponent<BuffSystemUpdater>();
                DontDestroyOnLoad(go);
            }
        }

        private void Awake()
        {
            lock (instanceLock)
            {
                if (instance != null && instance != this)
                {
                    Destroy(gameObject);
                    return;
                }

                instance = this;
                DontDestroyOnLoad(gameObject);

                // 初始化分层更新器
                frequencyUpdater = new FrequencyBasedUpdater();

                // 加载配置
                var config = BuffSystemConfig.Instance;
                updateMode = config.UpdateMode;
                updateInterval = config.UpdateInterval;
                enableBatchUpdate = config.EnableBatchUpdate;
                batchCount = config.BatchCount;
                batchThreshold = config.BatchThreshold;
                enableFrequencyBasedUpdate = config.EnableFrequencyBasedUpdate;
                autoAssignFrequency = config.AutoAssignFrequency;
            }
        }

        private void Update()
        {
            // 手动模式和回合制模式不自动更新
            if (updateMode == UpdateMode.Manual || updateMode == UpdateMode.TurnBased) return;

            float deltaTime = Time.deltaTime;

            if (enableFrequencyBasedUpdate && frequencyUpdater != null)
            {
                // 使用分层更新
                frequencyUpdater.Update(deltaTime);
            }
            else if (updateMode == UpdateMode.EveryFrame)
            {
                UpdateAllContainers(deltaTime);
            }
            else if (updateMode == UpdateMode.Interval)
            {
                updateTimer += deltaTime;
                if (updateTimer >= updateInterval)
                {
                    UpdateAllContainers(updateTimer);
                    updateTimer = 0f;
                }
            }
        }

        /// <summary>
        /// 手动更新所有Buff容器
        /// </summary>
        public static void UpdateAll(float deltaTime)
        {
            if (instance == null) return;
            
            if (instance.enableFrequencyBasedUpdate && instance.frequencyUpdater != null)
            {
                instance.frequencyUpdater.Update(deltaTime);
            }
            else
            {
                instance.UpdateAllContainers(deltaTime);
            }
        }

        /// <summary>
        /// 推进一个回合（用于回合制游戏）
        /// 调用此方法会更新所有Buff的持续时间
        /// </summary>
        /// <param name="turnDuration">一个回合的持续时间（通常是1）</param>
        public static void AdvanceTurn(float turnDuration = 1f)
        {
            if (instance == null) return;
            if (instance.updateMode != UpdateMode.TurnBased)
            {
                Debug.LogWarning("[BuffSystemUpdater] 当前不是回合制模式，AdvanceTurn可能不会产生预期效果");
            }
            instance.UpdateAllContainers(turnDuration);
        }

        /// <summary>
        /// 设置更新模式（运行时切换）
        /// </summary>
        public static void SetUpdateMode(UpdateMode mode)
        {
            if (instance == null) return;
            instance.updateMode = mode;
            instance.updateTimer = 0f;
        }

        /// <summary>
        /// 设置是否启用分层更新
        /// </summary>
        public static void SetFrequencyBasedUpdate(bool enable)
        {
            if (instance == null) return;
            instance.enableFrequencyBasedUpdate = enable;
        }

        /// <summary>
        /// 注册Buff到分层更新器
        /// </summary>
        public static void RegisterBuff(Core.IBuff buff, UpdateFrequency frequency)
        {
            if (instance == null || instance.frequencyUpdater == null) return;
            instance.frequencyUpdater.Register(buff, frequency);
        }

        /// <summary>
        /// 从分层更新器注销Buff
        /// </summary>
        public static void UnregisterBuff(Core.IBuff buff)
        {
            if (instance == null || instance.frequencyUpdater == null) return;
            instance.frequencyUpdater.Unregister(buff);
        }

        /// <summary>
        /// 获取分层更新器的调试信息
        /// </summary>
        public static string GetFrequencyDebugInfo()
        {
            if (instance == null || instance.frequencyUpdater == null) 
                return "FrequencyUpdater not initialized";
            return instance.frequencyUpdater.GetDebugInfo();
        }

        private void UpdateAllContainers(float deltaTime)
        {
            // 检查是否启用批处理更新
            bool shouldUseBatchUpdate = enableBatchUpdate && BuffOwner.ActiveOwnerCount >= batchThreshold && batchCount > 1;

            if (shouldUseBatchUpdate)
            {
                // 分批更新
                BuffOwner.UpdateBatch(deltaTime, currentBatchIndex, batchCount);
                currentBatchIndex = (currentBatchIndex + 1) % batchCount;
            }
            else
            {
                // 不分批，全部更新
                BuffOwner.UpdateAll(deltaTime);
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                frequencyUpdater?.Clear();
                instance = null;
            }
        }
    }
}
