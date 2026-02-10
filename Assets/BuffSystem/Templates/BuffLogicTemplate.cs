using UnityEngine;
using BuffSystem.Core;

namespace MyNamespace.Buffs
{
    /// <summary>
    /// [Buff名称] Buff 逻辑
    /// 描述：[在此描述 Buff 的效果]
    /// </summary>
    [System.Serializable]
    public class BuffLogicTemplate : BuffLogicBase, 
        IBuffStart,           // Buff 逻辑初始化完成
        IBuffAcquire,         // Buff 被添加到持有者
        IBuffLogicUpdate,     // 每帧逻辑更新
        IBuffVisualUpdate,    // 每帧表现更新
        IBuffRefresh,         // Buff 持续时间刷新
        IBuffStackChange,     // 层数变化
        IBuffReduce,          // 层数减少
        IBuffRemove,          // Buff 被标记移除
        IBuffEnd              // Buff 完全销毁
    {
        #region 配置参数
        
        [Header("基础参数")]
        [SerializeField] private float baseValue = 10f;
        [SerializeField] private float valuePerStack = 5f;
        
        [Header("高级设置")]
        [SerializeField] private bool usePercent = false;
        [SerializeField] private AnimationCurve valueCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        #endregion

        #region 运行时数据
        
        private float currentValue;
        private float timer;
        
        #endregion

        #region 生命周期方法
        
        /// <summary>
        /// Buff 逻辑初始化完成时调用
        /// </summary>
        public void OnStart()
        {
            Debug.Log($"[{Owner.OwnerName}] #BUFFNAME# Buff 逻辑初始化");
            CalculateValue();
        }
        
        /// <summary>
        /// Buff 被添加到持有者时调用
        /// </summary>
        public void OnAcquire()
        {
            Debug.Log($"[{Owner.OwnerName}] 获得 #BUFFNAME# Buff，层数: {CurrentStack}");
            
            // 应用效果
            ApplyEffect();
            
            // 发送事件给持有者
            SendEvent("#BUFFNAME#Started", CurrentStack);
        }
        
        /// <summary>
        /// 每帧逻辑更新
        /// </summary>
        public void OnLogicUpdate(float deltaTime)
        {
            timer += deltaTime;
            
            // 示例：每秒执行一次效果
            if (timer >= 1f)
            {
                OnIntervalEffect();
                timer = 0f;
            }
        }
        
        /// <summary>
        /// 每帧表现更新（用于 UI、特效等）
        /// </summary>
        public void OnVisualUpdate(float deltaTime)
        {
            // 更新 UI、特效等表现
            UpdateVisuals();
        }
        
        /// <summary>
        /// Buff 持续时间刷新时调用
        /// </summary>
        public void OnRefresh()
        {
            Debug.Log($"[{Owner.OwnerName}] #BUFFNAME# Buff 持续时间刷新");
            
            // 刷新时的额外逻辑
            OnRefreshedEffect();
        }
        
        /// <summary>
        /// 层数变化时调用
        /// </summary>
        public void OnStackChanged(int oldStack, int newStack)
        {
            Debug.Log($"[{Owner.OwnerName}] #BUFFNAME# 层数变化: {oldStack} -> {newStack}");
            
            // 重新计算数值
            CalculateValue();
            
            // 更新效果
            UpdateEffect(oldStack, newStack);
            
            // 发送事件
            SendEvent("#BUFFNAME#StackChanged", newStack);
        }
        
        /// <summary>
        /// 层数减少时调用
        /// </summary>
        public void OnReduce()
        {
            Debug.Log($"[{Owner.OwnerName}] #BUFFNAME# 层数减少");
            
            // 层数减少时的额外逻辑
            OnReduceEffect();
        }
        
        /// <summary>
        /// Buff 被标记移除时调用
        /// </summary>
        public void OnRemove()
        {
            Debug.Log($"[{Owner.OwnerName}] #BUFFNAME# Buff 被标记移除");
            
            // 清理效果
            RemoveEffect();
            
            // 发送事件
            SendEvent("#BUFFNAME#Ended", null);
        }
        
        /// <summary>
        /// Buff 完全销毁时调用
        /// </summary>
        public void OnEnd()
        {
            Debug.Log($"[{Owner.OwnerName}] #BUFFNAME# Buff 完全销毁");
            
            // 最终清理
            Cleanup();
        }
        
        #endregion

        #region 效果实现
        
        /// <summary>
        /// 计算当前数值
        /// </summary>
        private void CalculateValue()
        {
            float stackMultiplier = valueCurve.Evaluate((float)(CurrentStack - 1) / (Buff.MaxStack - 1));
            currentValue = baseValue + (valuePerStack * (CurrentStack - 1) * stackMultiplier);
        }
        
        /// <summary>
        /// 应用效果
        /// </summary>
        private void ApplyEffect()
        {
            // 尝试获取持有者组件并应用效果
            if (TryGetOwnerComponent<HealthSystem>(out var health))
            {
                // 示例：修改生命值
            }
            
            // 可以添加更多组件的处理
            if (TryGetOwnerComponent<CombatStats>(out var stats))
            {
                
            }
        }
        
        /// <summary>
        /// 更新效果（层数变化时）
        /// </summary>
        private void UpdateEffect(int oldStack, int newStack)
        {
            // 先移除旧效果
            if (TryGetOwnerComponent<HealthSystem>(out var health))
            {
                
            }
            
            // 再应用新效果
            ApplyEffect();
        }
        
        /// <summary>
        /// 移除效果
        /// </summary>
        private void RemoveEffect()
        {
            if (TryGetOwnerComponent<HealthSystem>(out var health))
            {
                
            }
            
            if (TryGetOwnerComponent<CombatStats>(out var stats))
            {
                
            }
        }
        
        /// <summary>
        /// 间隔效果（每秒执行）
        /// </summary>
        private void OnIntervalEffect()
        {
            // 示例：每秒造成伤害或治疗
            if (TryGetOwnerComponent<HealthSystem>(out var health))
            {
                health.TakeDamage(currentValue * 0.1f); // 每秒 10% 的伤害
            }
        }
        
        /// <summary>
        /// 刷新时的效果
        /// </summary>
        private void OnRefreshedEffect()
        {
            // 刷新时的额外效果
            // 例如：刷新时恢复一定生命值
            if (TryGetOwnerComponent<HealthSystem>(out var health))
            {
                health.Heal(currentValue * 0.5f);
            }
        }
        
        /// <summary>
        /// 层数减少时的效果
        /// </summary>
        private void OnReduceEffect()
        {
            // 层数减少时的特殊效果
        }
        
        /// <summary>
        /// 更新表现（UI、特效等）
        /// </summary>
        private void UpdateVisuals()
        {
            // 更新 UI 显示
            // 调整特效强度等
        }
        
        /// <summary>
        /// 最终清理
        /// </summary>
        private void Cleanup()
        {
            // 清理任何残留数据
            timer = 0f;
            currentValue = 0f;
        }
        
        #endregion
    }
}
