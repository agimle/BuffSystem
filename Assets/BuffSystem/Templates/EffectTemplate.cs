using UnityEngine;
using BuffSystem.Core;

namespace MyNamespace.Effects
{
    /// <summary>
    /// [效果名称] 效果
    /// 描述：[在此描述效果的作用]
    /// </summary>
    [System.Serializable]
    public class EffectTemplate : EffectBase
    {
        #region 配置参数
        
        [Header("基础参数")]
        [SerializeField] private float value = 10f;
        [SerializeField] private bool isPercent = false;
        
        [Header("目标设置")]
        [SerializeField] private TargetType targetType = TargetType.Self;
        [SerializeField] private LayerMask affectedLayers = ~0; // 所有层
        
        [Header("可选设置")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private string customEventName = "";
        
        #endregion

        #region 执行效果
        
        /// <summary>
        /// 执行效果
        /// </summary>
        public override void Execute(IBuff buff)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[#EFFECTNAME#Effect] 执行效果 - 目标: {buff.Owner.OwnerName}, 数值: {value}");
            }
            
            switch (targetType)
            {
                case TargetType.Self:
                    ApplyToSelf(buff);
                    break;
                    
                case TargetType.Area:
                    ApplyToArea(buff);
                    break;
                    
                case TargetType.Target:
                    ApplyToTarget(buff);
                    break;
            }
            
            // 发送自定义事件
            if (!string.IsNullOrEmpty(customEventName))
            {
                SendEvent(buff, customEventName, value);
            }
        }
        
        /// <summary>
        /// 取消效果
        /// </summary>
        public override void Cancel(IBuff buff)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[#EFFECTNAME#Effect] 取消效果 - 目标: {buff.Owner.OwnerName}");
            }
            
            // 根据效果类型执行不同的取消逻辑
            switch (targetType)
            {
                case TargetType.Self:
                    CancelFromSelf(buff);
                    break;
                    
                case TargetType.Area:
                    CancelFromArea(buff);
                    break;
                    
                case TargetType.Target:
                    CancelFromTarget(buff);
                    break;
            }
        }
        
        #endregion

        #region 目标类型处理
        
        /// <summary>
        /// 对自身应用效果
        /// </summary>
        private void ApplyToSelf(IBuff buff)
        {
            // 尝试获取各种组件并应用效果
            
            // 生命值系统
            if (TryGetOwnerComponent<HealthSystem>(buff, out var health))
            {
                ApplyHealthEffect(health, value);
            }
            
            // 战斗属性
            if (TryGetOwnerComponent<CombatStats>(buff, out var stats))
            {
                ApplyStatEffect(stats, value);
            }
            
            // 移动系统
            if (TryGetOwnerComponent<MovementSystem>(buff, out var movement))
            {
                ApplyMovementEffect(movement, value);
            }
        }
        
        /// <summary>
        /// 对范围内目标应用效果
        /// </summary>
        private void ApplyToArea(IBuff buff)
        {
            // 获取持有者位置
            if (!(buff.Owner is MonoBehaviour mono)) return;
            
            Vector3 center = mono.transform.position;
            float radius = value; // 使用 value 作为半径
            
            // 检测范围内所有目标
            Collider[] hits = Physics.OverlapSphere(center, radius, affectedLayers);
            
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IBuffOwner>(out var targetOwner))
                {
                    // 对范围内的目标应用效果
                    ApplyEffectToTarget(targetOwner, value * 0.5f); // 范围效果减半
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[#EFFECTNAME#Effect] 范围效果 - 影响目标数: {hits.Length}");
            }
        }
        
        /// <summary>
        /// 对特定目标应用效果
        /// </summary>
        private void ApplyToTarget(IBuff buff)
        {
            // 从 Buff 的来源获取目标
            if (buff.Source is GameObject targetGO)
            {
                if (targetGO.TryGetComponent<IBuffOwner>(out var targetOwner))
                {
                    ApplyEffectToTarget(targetOwner, value);
                }
            }
            else if (buff.Source is IBuffOwner targetOwner)
            {
                ApplyEffectToTarget(targetOwner, value);
            }
        }
        
        #endregion

        #region 取消效果
        
        private void CancelFromSelf(IBuff buff)
        {
            // 恢复生命值
            if (TryGetOwnerComponent<HealthSystem>(buff, out var health))
            {
                CancelHealthEffect(health, value);
            }
            
            // 恢复属性
            if (TryGetOwnerComponent<CombatStats>(buff, out var stats))
            {
                CancelStatEffect(stats, value);
            }
            
            // 恢复移动
            if (TryGetOwnerComponent<MovementSystem>(buff, out var movement))
            {
                CancelMovementEffect(movement, value);
            }
        }
        
        private void CancelFromArea(IBuff buff)
        {
            // 范围效果的取消逻辑
            // 通常不需要特别处理，因为效果已经应用过了
        }
        
        private void CancelFromTarget(IBuff buff)
        {
            // 特定目标效果的取消逻辑
        }
        
        #endregion

        #region 具体效果实现
        
        /// <summary>
        /// 应用生命值效果
        /// </summary>
        private void ApplyHealthEffect(HealthSystem health, float amount)
        {
            if (isPercent)
            {
                float damage = health.MaxHealth * amount / 100f;
                health.TakeDamage(damage);
            }
            else
            {
                if (amount > 0)
                {
                    health.TakeDamage(amount);
                }
                else
                {
                    health.Heal(-amount);
                }
            }
        }
        
        /// <summary>
        /// 取消生命值效果
        /// </summary>
        private void CancelHealthEffect(HealthSystem health, float amount)
        {
            // 如果是持续伤害/治疗，可能需要恢复
            // 具体逻辑根据需求实现
        }
        
        /// <summary>
        /// 应用属性效果
        /// </summary>
        private void ApplyStatEffect(CombatStats stats, float amount)
        {
            // 示例：增加攻击力
            stats.ModifyStat("Attack", amount);
            
            // 示例：增加防御力
            // stats.ModifyStat("Defense", amount);
            
            // 示例：增加速度
            // stats.ModifyStat("Speed", amount);
        }
        
        /// <summary>
        /// 取消属性效果
        /// </summary>
        private void CancelStatEffect(CombatStats stats, float amount)
        {
            stats.ModifyStat("Attack", -amount);
        }
        
        /// <summary>
        /// 应用移动效果
        /// </summary>
        private void ApplyMovementEffect(MovementSystem movement, float amount)
        {
            if (isPercent)
            {
                movement.ModifySpeedPercent(amount);
            }
            else
            {
                movement.ModifySpeed(amount);
            }
        }
        
        /// <summary>
        /// 取消移动效果
        /// </summary>
        private void CancelMovementEffect(MovementSystem movement, float amount)
        {
            if (isPercent)
            {
                movement.ModifySpeedPercent(-amount);
            }
            else
            {
                movement.ModifySpeed(-amount);
            }
        }
        
        /// <summary>
        /// 对目标应用效果
        /// </summary>
        private void ApplyEffectToTarget(IBuffOwner target, float amount)
        {
            // 可以添加 Buff 到目标
            // BuffApi.AddBuff(buffId, target, source);
            
            // 或者直接修改属性
            if (target is MonoBehaviour mono)
            {
                if (mono.TryGetComponent<HealthSystem>(out var health))
                {
                    health.TakeDamage(amount);
                }
            }
        }
        
        #endregion

        #region 枚举定义
        
        /// <summary>
        /// 目标类型
        /// </summary>
        public enum TargetType
        {
            /// <summary>
            /// 自身
            /// </summary>
            Self,
            
            /// <summary>
            /// 范围
            /// </summary>
            Area,
            
            /// <summary>
            /// 特定目标
            /// </summary>
            Target
        }
        
        #endregion
    }
    
    #region 示例组件接口（根据实际项目调整）
    
    /// <summary>
    /// 生命值系统接口示例
    /// </summary>
    public interface HealthSystem
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        void TakeDamage(float damage);
        void Heal(float amount);
        void ModifyHealth(float amount);
        void ModifyHealthPercent(float percent);
    }
    
    /// <summary>
    /// 战斗属性接口示例
    /// </summary>
    public interface CombatStats
    {
        void ModifyStat(string statName, float value);
        float GetStat(string statName);
    }
    
    /// <summary>
    /// 移动系统接口示例
    /// </summary>
    public interface MovementSystem
    {
        float Speed { get; }
        void ModifySpeed(float amount);
        void ModifySpeedPercent(float percent);
    }
    
    #endregion
}
