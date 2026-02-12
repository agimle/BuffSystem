using UnityEngine;
using BuffSystem.Advanced.Combo;
using BuffSystem.Advanced.Fusion;
using BuffSystem.Advanced.Transmission;

namespace BuffSystem.Core
{
    /// <summary>
    /// BuffSystem统一入口管理器
    /// 管理所有子管理器的生命周期和统一访问
    /// v7.0新增
    /// </summary>
    [AddComponentMenu("BuffSystem/Buff System Manager")]
    [DefaultExecutionOrder(-200)]
    public class BuffSystemManager : MonoBehaviour
    {
        private static BuffSystemManager instance;
        
        /// <summary>
        /// 全局实例
        /// </summary>
        public static BuffSystemManager Instance
        {
            get
            {
                if (instance == null) CreateInstance();
                return instance;
            }
        }

        // 子管理器引用
        [SerializeField] private BuffComboManager comboManager;
        [SerializeField] private FusionManager fusionManager;
        [SerializeField] private TransmissionManager transmissionManager;
        
        // 子管理器是否自动创建
        [SerializeField] private bool autoCreateComboManager = true;
        [SerializeField] private bool autoCreateFusionManager = true;
        [SerializeField] private bool autoCreateTransmissionManager = true;

        #region Public Access Points
        
        /// <summary>
        /// Combo管理器统一访问点
        /// </summary>
        public static BuffComboManager Combo => Instance.comboManager;
        
        /// <summary>
        /// 融合管理器统一访问点
        /// </summary>
        public static FusionManager Fusion => Instance.fusionManager;
        
        /// <summary>
        /// 传播管理器统一访问点
        /// </summary>
        public static TransmissionManager Transmission => Instance.transmissionManager;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeManagers();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        #endregion

        #region Initialization

        private static void CreateInstance()
        {
            instance = FindFirstObjectByType<BuffSystemManager>();
            if (instance == null)
            {
                var go = new GameObject("BuffSystemManager");
                instance = go.AddComponent<BuffSystemManager>();
                DontDestroyOnLoad(go);
            }
        }

        private void InitializeManagers()
        {
            // 初始化Combo管理器
            if (autoCreateComboManager && comboManager == null)
            {
                comboManager = gameObject.AddComponent<BuffComboManager>();
            }
            
            // 初始化融合管理器
            if (autoCreateFusionManager && fusionManager == null)
            {
                fusionManager = gameObject.AddComponent<FusionManager>();
            }
            
            // 初始化传播管理器
            if (autoCreateTransmissionManager && transmissionManager == null)
            {
                transmissionManager = gameObject.AddComponent<TransmissionManager>();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 手动设置Combo管理器（用于自定义初始化）
        /// </summary>
        public void SetComboManager(BuffComboManager manager)
        {
            if (comboManager != null && comboManager != manager)
            {
                Destroy(comboManager);
            }
            comboManager = manager;
        }

        /// <summary>
        /// 手动设置融合管理器（用于自定义初始化）
        /// </summary>
        public void SetFusionManager(FusionManager manager)
        {
            if (fusionManager != null && fusionManager != manager)
            {
                Destroy(fusionManager);
            }
            fusionManager = manager;
        }

        /// <summary>
        /// 手动设置传播管理器（用于自定义初始化）
        /// </summary>
        public void SetTransmissionManager(TransmissionManager manager)
        {
            if (transmissionManager != null && transmissionManager != manager)
            {
                Destroy(transmissionManager);
            }
            transmissionManager = manager;
        }

        /// <summary>
        /// 检查所有管理器是否已初始化
        /// </summary>
        public bool AreAllManagersReady()
        {
            return (!autoCreateComboManager || comboManager != null) &&
                   (!autoCreateFusionManager || fusionManager != null) &&
                   (!autoCreateTransmissionManager || transmissionManager != null);
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        /// <summary>
        /// 获取调试信息
        /// </summary>
        public string GetDebugInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== BuffSystemManager Debug Info ===");
            sb.AppendLine($"Instance: {(instance != null ? "Active" : "Null")}");
            sb.AppendLine($"Combo Manager: {(comboManager != null ? "Active" : "Null")}");
            sb.AppendLine($"Fusion Manager: {(fusionManager != null ? "Active" : "Null")}");
            sb.AppendLine($"Transmission Manager: {(transmissionManager != null ? "Active" : "Null")}");
            return sb.ToString();
        }
#endif

        #endregion
    }
}
