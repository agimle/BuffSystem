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

        private float updateTimer;
        private static BuffSystemUpdater instance;

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
            var go = new GameObject("BuffSystemUpdater");
            instance = go.AddComponent<BuffSystemUpdater>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
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
            // 通过BuffOwner的静态列表批量更新所有Buff容器
            BuffOwner.UpdateAll(deltaTime);
        }
    }
}
