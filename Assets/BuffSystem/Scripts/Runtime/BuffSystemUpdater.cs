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
        
        private float _updateTimer;
        private static BuffSystemUpdater _instance;
        
        public static BuffSystemUpdater Instance
        {
            get
            {
                if (_instance == null)
                {
                    CreateInstance();
                }
                return _instance;
            }
        }
        
        private static void CreateInstance()
        {
            var go = new GameObject("BuffSystemUpdater");
            _instance = go.AddComponent<BuffSystemUpdater>();
            DontDestroyOnLoad(go);
        }
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 加载配置
            var config = BuffSystemConfig.Instance;
            updateMode = config.UpdateMode;
            updateInterval = config.UpdateInterval;
        }
        
        private void Update()
        {
            if (updateMode == UpdateMode.Manual) return;
            
            if (updateMode == UpdateMode.EveryFrame)
            {
                UpdateAllContainers(Time.deltaTime);
            }
            else if (updateMode == UpdateMode.Interval)
            {
                _updateTimer += Time.deltaTime;
                if (_updateTimer >= updateInterval)
                {
                    UpdateAllContainers(_updateTimer);
                    _updateTimer = 0f;
                }
            }
        }
        
        /// <summary>
        /// 手动更新所有Buff容器
        /// </summary>
        public static void UpdateAll(float deltaTime)
        {
            if (_instance == null) return;
            _instance.UpdateAllContainers(deltaTime);
        }
        
        private void UpdateAllContainers(float deltaTime)
        {
            // 这里可以通过BuffOwner的静态列表来批量更新
            // 或者让每个BuffOwner自己更新
        }
    }
}
