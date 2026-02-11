using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Events;
using BuffSystem.Runtime;

namespace MyNamespace
{
    /// <summary>
    /// [角色名称] Buff 持有者
    /// 实现了 IBuffOwner 和 IBuffEventReceiver 接口
    /// </summary>
    [RequireComponent(typeof(BuffOwner))]
    public class BuffOwnerTemplate : MonoBehaviour, IBuffOwner, IBuffEventReceiver
    {
        #region 组件引用
        
        [Header("Buff 系统")]
        [SerializeField] private BuffOwner buffOwner;
        
        [Header("可选组件")]
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private CombatStats combatStats;
        [SerializeField] private MovementSystem movementSystem;
        
        #endregion

        #region IBuffOwner 实现
        
        /// <summary>
        /// 持有者唯一 ID
        /// </summary>
        public int OwnerId => buffOwner?.OwnerId ?? GetInstanceID();
        
        /// <summary>
        /// 持有者名称
        /// </summary>
        public string OwnerName => buffOwner?.OwnerName ?? gameObject.name;
        
        /// <summary>
        /// Buff 容器
        /// </summary>
        public IBuffContainer BuffContainer => buffOwner?.BuffContainer;
        
        /// <summary>
        /// Buff 事件回调
        /// </summary>
        public void OnBuffEvent(BuffEventType eventType, IBuff buff)
        {
            switch (eventType)
            {
                case BuffEventType.Added:
                    OnBuffAdded(buff);
                    break;
                    
                case BuffEventType.Removed:
                    OnBuffRemoved(buff);
                    break;
                    
                case BuffEventType.StackChanged:
                    OnBuffStackChanged(buff);
                    break;
                    
                case BuffEventType.Refreshed:
                    OnBuffRefreshed(buff);
                    break;
                    
                case BuffEventType.Expired:
                    OnBuffExpired(buff);
                    break;
                    
                case BuffEventType.Cleared:
                    OnBuffCleared();
                    break;
            }
        }

        public bool IsImmuneTo(int buffId)
        {
            throw new System.NotImplementedException();
        }

        public bool IsImmuneToTag(string tag)
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyList<string> ImmuneTags { get; }

        #endregion

        #region IBuffEventReceiver 实现
        
        /// <summary>
        /// 接收 Buff 发送的自定义事件
        /// </summary>
        public void OnBuffEvent(IBuff buff, string eventName, object data)
        {
            switch (eventName)
            {
                // 燃烧相关事件
                case "BurningStarted":
                    OnBurningStarted(buff, data);
                    break;
                    
                case "BurningStackChanged":
                    OnBurningStackChanged(buff, data);
                    break;
                    
                case "BurningEnded":
                    OnBurningEnded(buff);
                    break;
                    
                // 眩晕相关事件
                case "Stun":
                    bool isStunned = (bool)data;
                    OnStunStateChanged(isStunned);
                    break;
                    
                // 沉默相关事件
                case "Silence":
                    bool isSilenced = (bool)data;
                    OnSilenceStateChanged(isSilenced);
                    break;
                    
                // 属性修改事件
                case "AttackChanged":
                    float attackValue = (float)data;
                    OnAttackChanged(attackValue);
                    break;
                    
                case "DefenseChanged":
                    float defenseValue = (float)data;
                    OnDefenseChanged(defenseValue);
                    break;
                    
                case "SpeedChanged":
                    float speedValue = (float)data;
                    OnSpeedChanged(speedValue);
                    break;
                    
                // 自定义事件
                default:
                    OnCustomBuffEvent(buff, eventName, data);
                    break;
            }
        }
        
        #endregion

        #region Unity 生命周期
        
        void Awake()
        {
            // 自动获取组件
            if (buffOwner == null)
            {
                buffOwner = GetComponent<BuffOwner>();
            }
            
            if (healthSystem == null)
            {
                healthSystem = GetComponent<HealthSystem>();
            }
            
            if (combatStats == null)
            {
                combatStats = GetComponent<CombatStats>();
            }
            
            if (movementSystem == null)
            {
                movementSystem = GetComponent<MovementSystem>();
            }
        }
        
        void OnEnable()
        {
            // 订阅本地事件
            if (buffOwner != null)
            {
                buffOwner.LocalEvents.OnBuffAdded += OnLocalBuffAdded;
                buffOwner.LocalEvents.OnBuffRemoved += OnLocalBuffRemoved;
                buffOwner.LocalEvents.OnStackChanged += OnLocalStackChanged;
                buffOwner.LocalEvents.OnBuffRefreshed += OnLocalBuffRefreshed;
                buffOwner.LocalEvents.OnBuffExpired += OnLocalBuffExpired;
                buffOwner.LocalEvents.OnBuffCleared += OnLocalBuffCleared;
            }
        }
        
        void OnDisable()
        {
            // 取消订阅本地事件
            if (buffOwner != null)
            {
                buffOwner.LocalEvents.OnBuffAdded -= OnLocalBuffAdded;
                buffOwner.LocalEvents.OnBuffRemoved -= OnLocalBuffRemoved;
                buffOwner.LocalEvents.OnStackChanged -= OnLocalStackChanged;
                buffOwner.LocalEvents.OnBuffRefreshed -= OnLocalBuffRefreshed;
                buffOwner.LocalEvents.OnBuffExpired -= OnLocalBuffExpired;
                buffOwner.LocalEvents.OnBuffCleared -= OnLocalBuffCleared;
            }
        }
        
        #endregion

        #region 事件处理方法
        
        // 系统事件
        private void OnBuffAdded(IBuff buff)
        {
            Debug.Log($"[{OwnerName}] 获得 Buff: {buff.Name}");
            
            // 更新 UI
            ShowBuffIcon(buff);
            
            // 播放音效
            PlayBuffSound(buff, true);
        }
        
        private void OnBuffRemoved(IBuff buff)
        {
            Debug.Log($"[{OwnerName}] 失去 Buff: {buff.Name}");
            
            // 更新 UI
            HideBuffIcon(buff);
            
            // 播放音效
            PlayBuffSound(buff, false);
        }
        
        private void OnBuffStackChanged(IBuff buff)
        {
            // 更新层数显示
            UpdateBuffStackDisplay(buff);
        }
        
        private void OnBuffRefreshed(IBuff buff)
        {
            Debug.Log($"[{OwnerName}] Buff 刷新: {buff.Name}");
        }
        
        private void OnBuffExpired(IBuff buff)
        {
            Debug.Log($"[{OwnerName}] Buff 过期: {buff.Name}");
        }
        
        private void OnBuffCleared()
        {
            Debug.Log($"[{OwnerName}] 所有 Buff 被清空");
            
            // 清空 UI
            ClearAllBuffIcons();
        }
        
        // 本地事件（更详细的处理）
        private void OnLocalBuffAdded(object sender, BuffAddedEventArgs e)
        {
            // 可以在这里处理特定 Buff 的逻辑
            if (e.Buff.Data.EffectType == BuffEffectType.Debuff)
            {
                // 受到减益效果
                OnDebuffApplied(e.Buff);
            }
            else if (e.Buff.Data.EffectType == BuffEffectType.Buff)
            {
                // 获得增益效果
                OnBuffApplied(e.Buff);
            }
        }
        
        private void OnLocalBuffRemoved(object sender, BuffRemovedEventArgs e)
        {
            // Buff 移除时的处理
        }
        
        private void OnLocalStackChanged(object sender, BuffStackChangedEventArgs e)
        {
            // 层数变化时的处理
        }
        
        private void OnLocalBuffRefreshed(object sender, BuffRefreshedEventArgs e)
        {
            // 刷新时的处理
        }
        
        private void OnLocalBuffExpired(object sender, BuffExpiredEventArgs e)
        {
            // 过期时的处理
        }
        
        private void OnLocalBuffCleared(object sender, System.EventArgs e)
        {
            // 清空时的处理
        }
        
        #endregion

        #region Buff 事件处理
        
        // 燃烧效果
        private void OnBurningStarted(IBuff buff, object data)
        {
            int stack = (int)data;
            Debug.Log($"[{OwnerName}] 开始燃烧，层数: {stack}");
            
            // 播放燃烧特效
            PlayBurningEffect(stack);
        }
        
        private void OnBurningStackChanged(IBuff buff, object data)
        {
            int newStack = (int)data;
            Debug.Log($"[{OwnerName}] 燃烧层数变化: {newStack}");
            
            // 更新特效强度
            UpdateBurningEffect(newStack);
        }
        
        private void OnBurningEnded(IBuff buff)
        {
            Debug.Log($"[{OwnerName}] 燃烧结束");
            
            // 停止特效
            StopBurningEffect();
        }
        
        // 控制效果
        private void OnStunStateChanged(bool isStunned)
        {
            if (isStunned)
            {
                Debug.Log($"[{OwnerName}] 被眩晕！");
                // 禁用移动和攻击
                if (movementSystem != null)
                {
                    movementSystem.SetCanMove(false);
                }
                // 播放眩晕特效
                PlayStunEffect();
            }
            else
            {
                Debug.Log($"[{OwnerName}] 眩晕解除");
                // 恢复移动和攻击
                if (movementSystem != null)
                {
                    movementSystem.SetCanMove(true);
                }
                // 停止眩晕特效
                StopStunEffect();
            }
        }
        
        private void OnSilenceStateChanged(bool isSilenced)
        {
            if (isSilenced)
            {
                Debug.Log($"[{OwnerName}] 被沉默！");
                // 禁用技能
            }
            else
            {
                Debug.Log($"[{OwnerName}] 沉默解除");
                // 恢复技能
            }
        }
        
        // 属性变化
        private void OnAttackChanged(float attackValue)
        {
            Debug.Log($"[{OwnerName}] 攻击力变化: {attackValue}");
            // 更新 UI 显示
        }
        
        private void OnDefenseChanged(float defenseValue)
        {
            Debug.Log($"[{OwnerName}] 防御力变化: {defenseValue}");
        }
        
        private void OnSpeedChanged(float speedValue)
        {
            Debug.Log($"[{OwnerName}] 速度变化: {speedValue}");
        }
        
        // 自定义事件处理
        private void OnCustomBuffEvent(IBuff buff, string eventName, object data)
        {
            Debug.Log($"[{OwnerName}] 自定义事件: {eventName}, 数据: {data}");
        }
        
        // 增益/减益处理
        private void OnBuffApplied(IBuff buff)
        {
            // 获得增益时的通用处理
        }
        
        private void OnDebuffApplied(IBuff buff)
        {
            // 受到减益时的通用处理
        }
        
        #endregion

        #region 公共方法
        
        /// <summary>
        /// 添加 Buff
        /// </summary>
        public IBuff AddBuff(int buffId, object source = null)
        {
            return BuffApi.AddBuff(buffId, this, source);
        }
        
        /// <summary>
        /// 添加 Buff（通过名称）
        /// </summary>
        public IBuff AddBuff(string buffName, object source = null)
        {
            return BuffApi.AddBuff(buffName, this, source);
        }
        
        /// <summary>
        /// 移除 Buff
        /// </summary>
        public void RemoveBuff(int buffId)
        {
            BuffApi.RemoveBuff(buffId, this);
        }
        
        /// <summary>
        /// 检查是否有指定 Buff
        /// </summary>
        public bool HasBuff(int buffId)
        {
            return BuffApi.HasBuff(buffId, this);
        }
        
        /// <summary>
        /// 获取 Buff
        /// </summary>
        public IBuff GetBuff(int buffId)
        {
            return BuffApi.GetBuff(buffId, this);
        }
        
        /// <summary>
        /// 清空所有 Buff
        /// </summary>
        public void ClearAllBuffs()
        {
            BuffApi.ClearBuffs(this);
        }
        
        #endregion

        #region 私有方法（UI 和特效）
        
        private void ShowBuffIcon(IBuff buff)
        {
            // 在 UI 上显示 Buff 图标
        }
        
        private void HideBuffIcon(IBuff buff)
        {
            // 隐藏 Buff 图标
        }
        
        private void UpdateBuffStackDisplay(IBuff buff)
        {
            // 更新层数显示
        }
        
        private void ClearAllBuffIcons()
        {
            // 清空所有 Buff 图标
        }
        
        private void PlayBuffSound(IBuff buff, bool isAdded)
        {
            // 播放 Buff 音效
        }
        
        private void PlayBurningEffect(int stack)
        {
            // 播放燃烧特效
        }
        
        private void UpdateBurningEffect(int stack)
        {
            // 根据层数更新特效强度
        }
        
        private void StopBurningEffect()
        {
            // 停止燃烧特效
        }
        
        private void PlayStunEffect()
        {
            // 播放眩晕特效
        }
        
        private void StopStunEffect()
        {
            // 停止眩晕特效
        }
        
        #endregion
    }
    
    #region 示例接口（根据实际项目调整）
    
    public interface HealthSystem
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        void TakeDamage(float damage);
        void Heal(float amount);
    }
    
    public interface CombatStats
    {
        float Attack { get; }
        float Defense { get; }
        void ModifyAttack(float amount);
        void ModifyDefense(float amount);
    }
    
    public interface MovementSystem
    {
        float Speed { get; }
        void SetCanMove(bool canMove);
        void ModifySpeed(float amount);
    }
    
    #endregion
}
