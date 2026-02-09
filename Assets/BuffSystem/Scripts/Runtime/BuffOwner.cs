using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Events;

namespace BuffSystem.Runtime
{
    /// <summary>
    /// Buff持有者组件 - MonoBehaviour适配器
    /// 挂载到需要持有Buff的GameObject上
    /// </summary>
    [AddComponentMenu("BuffSystem/Buff Owner")]
    [DisallowMultipleComponent]
    public class BuffOwner : MonoBehaviour, IBuffOwner
    {
        [Header("设置")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool updateInFixedUpdate = false;
        [SerializeField] private bool showDebugInfo = false;
        
        [Header("事件")]
        [SerializeField] private bool useUnityEvents = false;
        
        // 内部组件
        private BuffContainer _buffContainer;
        private BuffLocalEventSystem _localEvents;
        
        #region Properties
        
        /// <summary>
        /// Buff容器
        /// </summary>
        public IBuffContainer BuffContainer => _buffContainer;
        
        /// <summary>
        /// 本地事件系统
        /// </summary>
        public BuffLocalEventSystem LocalEvents => _localEvents;
        
        /// <summary>
        /// 当前Buff数量
        /// </summary>
        public int BuffCount => _buffContainer?.Count ?? 0;
        
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
                    _localEvents?.TriggerBuffAdded(buff);
                    break;
                case BuffEventType.Removed:
                    _localEvents?.TriggerBuffRemoved(buff);
                    break;
                case BuffEventType.StackChanged:
                    // 层数变化在Buff内部处理
                    break;
                case BuffEventType.Refreshed:
                    _localEvents?.TriggerRefreshed(buff);
                    break;
                case BuffEventType.Expired:
                    _localEvents?.TriggerExpired(buff);
                    break;
                case BuffEventType.Cleared:
                    _localEvents?.TriggerCleared();
                    break;
            }
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (autoInitialize)
            {
                Initialize();
            }
        }
        
        private void Update()
        {
            if (!updateInFixedUpdate && _buffContainer != null)
            {
                _buffContainer.Update(Time.deltaTime);
            }
        }
        
        private void FixedUpdate()
        {
            if (updateInFixedUpdate && _buffContainer != null)
            {
                _buffContainer.Update(Time.fixedDeltaTime);
            }
        }
        
        private void OnDestroy()
        {
            // 清理所有Buff
            _buffContainer?.ClearAllBuffs();
            _buffContainer = null;
            _localEvents = null;
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// 初始化Buff持有者
        /// </summary>
        public void Initialize()
        {
            if (_buffContainer != null) return;
            
            _buffContainer = new BuffContainer(this);
            _localEvents = new BuffLocalEventSystem(this);
            
            // 初始化Buff系统
            BuffApi.Initialize();
            
            if (showDebugInfo)
            {
                Debug.Log($"[BuffOwner] {gameObject.name} 初始化完成");
            }
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
        
        #region Debug

        private static readonly Rect DebugWindowRect = new(10, 10, 250, 300);

        private void OnGUI()
        {
            if (!showDebugInfo || _buffContainer == null) return;

            GUILayout.BeginArea(DebugWindowRect);
            GUILayout.BeginVertical("box");

            GUILayout.Label($"<b>{gameObject.name}</b>");
            GUILayout.Label($"Buff数量: {BuffCount}");

            if (BuffCount > 0)
            {
                GUILayout.Space(5);
                GUILayout.Label("<b>当前Buff:</b>");

                foreach (var buff in _buffContainer.AllBuffs)
                {
                    string timeText = buff.IsPermanent ? "∞" : $"{buff.RemainingTime:F1}s";
                    GUILayout.Label($"  • {buff.Name} ({buff.CurrentStack}/{buff.MaxStack}) [{timeText}]");
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion
    }
}
