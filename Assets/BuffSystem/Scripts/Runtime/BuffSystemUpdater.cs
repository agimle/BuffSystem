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

        [Header("批处理设置")]
        [SerializeField] private bool enableBatchUpdate = false;
        [SerializeField] private int batchCount = 4;
        [SerializeField] private int batchThreshold = 100;

        private float updateTimer;
        private int currentBatchIndex = 0;
        private static BuffSystemUpdater instance;

        // 线程安全锁
        private static readonly object instanceLock = new();

        /// <summary>
        /// 当前更新模式
        /// </summary>
        public static UpdateMode CurrentUpdateMode => instance != null ? instance.updateMode : UpdateMode.Manual;

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

                // 加载配置
                var config = BuffSystemConfig.Instance;
                updateMode = config.UpdateMode;
                updateInterval = config.UpdateInterval;
                enableBatchUpdate = config.EnableBatchUpdate;
                batchCount = config.BatchCount;
                batchThreshold = config.BatchThreshold;
            }
        }

        private void Update()
        {
            // 手动模式和回合制模式不自动更新
            if (updateMode == UpdateMode.Manual || updateMode == UpdateMode.TurnBased) return;

            if (updateMode == UpdateMode.EveryFrame)
            {
                UpdateAllContainers(Time.deltaTime);
            }
            else if (updateMode == UpdateMode.Interval)
            {
                updateTimer += Time.deltaTime;
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
            instance.UpdateAllContainers(deltaTime);
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
    }
}
