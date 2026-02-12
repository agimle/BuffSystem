using System;
using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Data;
using BuffSystem.Events;
using BuffSystem.Runtime;
using BuffAddedEventArgs = BuffSystem.Events.BuffAddedEventArgs;
using BuffRemovedEventArgs = BuffSystem.Events.BuffRemovedEventArgs;
using BuffSystem.Runtime;

namespace BuffSystem.Advanced.Combo
{
    /// <summary>
    /// Buff组合管理器 - 管理所有BuffCombo的注册、检测和执行
    /// 通过BuffSystemManager.Combo访问
    /// v7.0: 单例访问改为通过BuffSystemManager
    /// </summary>
    public class BuffComboManager : MonoBehaviour
    {
        // Combo注册表
        private readonly List<BuffComboData> registeredCombos = new();
        private readonly Dictionary<int, List<BuffComboData>> combosByBuffId = new();

        // 运行时状态
        private readonly Dictionary<IBuffOwner, HashSet<int>> activeCombos = new();
        private readonly Dictionary<IBuffOwner, Dictionary<int, int>> comboTriggerCounts = new();

        // 事件监听标记
        private bool isListeningEvents;

        #region Singleton (via BuffSystemManager)

        /// <summary>
        /// 全局实例 - 通过BuffSystemManager.Combo访问
        /// </summary>
        [System.Obsolete("使用 BuffSystemManager.Combo 替代")]
        public static BuffComboManager Instance => BuffSystemManager.Combo;

        private void Awake()
        {
            // 由BuffSystemManager管理生命周期
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        #endregion

        #region Initialization

        private void Start()
        {
            SubscribeEvents();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        /// <summary>
        /// 订阅Buff事件
        /// </summary>
        private void SubscribeEvents()
        {
            if (isListeningEvents) return;

            BuffEventSystem.OnBuffAdded += OnBuffAdded;
            BuffEventSystem.OnBuffRemoved += OnBuffRemoved;
            isListeningEvents = true;
        }

        /// <summary>
        /// 取消订阅Buff事件
        /// </summary>
        private void UnsubscribeEvents()
        {
            if (!isListeningEvents) return;

            BuffEventSystem.OnBuffAdded -= OnBuffAdded;
            BuffEventSystem.OnBuffRemoved -= OnBuffRemoved;
            isListeningEvents = false;
        }

        #endregion

        #region Combo Registration

        /// <summary>
        /// 注册一个BuffCombo
        /// </summary>
        /// <param name="combo">Combo配置</param>
        public void RegisterCombo(BuffComboData combo)
        {
            if (combo == null || !combo.IsValid())
            {
                Debug.LogWarning("[BuffComboManager] 尝试注册无效的Combo");
                return;
            }

            if (registeredCombos.Contains(combo))
            {
                Debug.LogWarning($"[BuffComboManager] Combo {combo.ComboName} 已经注册");
                return;
            }

            registeredCombos.Add(combo);

            // 建立Buff ID到Combo的映射
            foreach (var buffId in combo.RequiredBuffIds)
            {
                if (!combosByBuffId.TryGetValue(buffId, out var comboList))
                {
                    comboList = new List<BuffComboData>();
                    combosByBuffId[buffId] = comboList;
                }

                if (!comboList.Contains(combo))
                {
                    comboList.Add(combo);
                }
            }

            // 按优先级排序
            registeredCombos.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            if (BuffSystemConfig.Instance.EnableDebugLog)
            {
                Debug.Log($"[BuffComboManager] 注册Combo: {combo.ComboName}");
            }
        }

        /// <summary>
        /// 注销一个BuffCombo
        /// </summary>
        public void UnregisterCombo(BuffComboData combo)
        {
            if (combo == null) return;

            registeredCombos.Remove(combo);

            // 从映射中移除
            foreach (var buffId in combo.RequiredBuffIds)
            {
                if (combosByBuffId.TryGetValue(buffId, out var comboList))
                {
                    comboList.Remove(combo);
                }
            }

            // 从所有持有者的激活列表中移除
            foreach (var ownerCombos in activeCombos.Values)
            {
                ownerCombos.Remove(combo.ComboId);
            }

            if (BuffSystemConfig.Instance.EnableDebugLog)
            {
                Debug.Log($"[BuffComboManager] 注销Combo: {combo.ComboName}");
            }
        }

        /// <summary>
        /// 批量注册Combo
        /// </summary>
        public void RegisterCombos(IEnumerable<BuffComboData> combos)
        {
            foreach (var combo in combos)
            {
                RegisterCombo(combo);
            }
        }

        /// <summary>
        /// 清空所有注册的Combo
        /// </summary>
        public void ClearAllCombos()
        {
            registeredCombos.Clear();
            combosByBuffId.Clear();
            activeCombos.Clear();
            comboTriggerCounts.Clear();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Buff添加事件处理
        /// </summary>
        private void OnBuffAdded(object sender, BuffAddedEventArgs e)
        {
            var buff = e.Buff;
            if (buff?.Owner == null) return;

            // 检查与此Buff相关的所有Combo
            if (combosByBuffId.TryGetValue(buff.DataId, out var relatedCombos))
            {
                foreach (var combo in relatedCombos)
                {
                    if ((combo.TriggerMode & ComboTriggerMode.OnBuffAdd) != 0)
                    {
                        TryActivateCombo(combo, buff.Owner);
                    }
                }
            }
        }

        /// <summary>
        /// Buff移除事件处理
        /// </summary>
        private void OnBuffRemoved(object sender, BuffRemovedEventArgs e)
        {
            var buff = e.Buff;
            if (buff?.Owner == null) return;

            var owner = buff.Owner;

            // 检查与此Buff相关的所有Combo
            if (combosByBuffId.TryGetValue(buff.DataId, out var relatedCombos))
            {
                foreach (var combo in relatedCombos)
                {
                    if ((combo.TriggerMode & ComboTriggerMode.OnBuffRemove) != 0)
                    {
                        CheckComboDeactivation(combo, owner);
                    }
                }
            }

            // 清理已移除的Combo激活状态
            CleanupOwnerCombos(owner);
        }

        #endregion

        #region Combo Activation

        /// <summary>
        /// 尝试激活Combo
        /// </summary>
        private void TryActivateCombo(BuffComboData combo, IBuffOwner owner)
        {
            if (!combo.CheckCondition(owner))
                return;

            // 获取或创建持有者的激活Combo集合
            if (!activeCombos.TryGetValue(owner, out var ownerActiveCombos))
            {
                ownerActiveCombos = new HashSet<int>();
                activeCombos[owner] = ownerActiveCombos;
            }

            // 检查是否已经激活
            bool wasActive = ownerActiveCombos.Contains(combo.ComboId);

            // 如果只触发一次且已经触发过，则不再触发
            if (combo.OnlyTriggerOnce && wasActive)
                return;

            // 添加到激活集合
            ownerActiveCombos.Add(combo.ComboId);

            // 增加触发计数
            IncrementTriggerCount(owner, combo.ComboId);

            // 执行Combo效果
            ExecuteComboEffects(combo, owner);

            // 触发新Buff
            if (combo.HasTriggerBuff && !wasActive)
            {
                BuffApi.AddBuff(combo.TriggerBuffId, owner, source: this);
            }

            // 触发全局事件
            BuffComboEventSystem.TriggerComboActivated(combo, owner);

            if (BuffSystemConfig.Instance.EnableDebugLog)
            {
                Debug.Log($"[BuffComboManager] Combo激活: {combo.ComboName} on {owner.OwnerName}");
            }
        }

        /// <summary>
        /// 检查Combo是否需要停用
        /// </summary>
        private void CheckComboDeactivation(BuffComboData combo, IBuffOwner owner)
        {
            if (!activeCombos.TryGetValue(owner, out var ownerActiveCombos))
                return;

            if (!ownerActiveCombos.Contains(combo.ComboId))
                return;

            // 检查条件是否不再满足
            if (!combo.CheckCondition(owner))
            {
                ownerActiveCombos.Remove(combo.ComboId);

                // 触发停用事件
                BuffComboEventSystem.TriggerComboDeactivated(combo, owner);

                if (BuffSystemConfig.Instance.EnableDebugLog)
                {
                    Debug.Log($"[BuffComboManager] Combo停用: {combo.ComboName} on {owner.OwnerName}");
                }
            }
        }

        /// <summary>
        /// 清理持有者的Combo状态
        /// </summary>
        private void CleanupOwnerCombos(IBuffOwner owner)
        {
            if (owner?.BuffContainer == null)
            {
                activeCombos.Remove(owner);
                comboTriggerCounts.Remove(owner);
                return;
            }

            // 检查所有激活的Combo是否仍然有效
            if (activeCombos.TryGetValue(owner, out var ownerActiveCombos))
            {
                var combosToRemove = new List<int>();

                foreach (var comboId in ownerActiveCombos)
                {
                    var combo = GetComboById(comboId);
                    if (combo == null || !combo.CheckCondition(owner))
                    {
                        combosToRemove.Add(comboId);
                    }
                }

                foreach (var comboId in combosToRemove)
                {
                    ownerActiveCombos.Remove(comboId);
                }

                if (ownerActiveCombos.Count == 0)
                {
                    activeCombos.Remove(owner);
                }
            }
        }

        /// <summary>
        /// 增加Combo触发计数
        /// </summary>
        private void IncrementTriggerCount(IBuffOwner owner, int comboId)
        {
            if (!comboTriggerCounts.TryGetValue(owner, out var counts))
            {
                counts = new Dictionary<int, int>();
                comboTriggerCounts[owner] = counts;
            }

            if (!counts.TryGetValue(comboId, out var count))
            {
                count = 0;
            }

            counts[comboId] = count + 1;
        }

        #endregion

        #region Effect Execution

        /// <summary>
        /// 执行Combo的所有效果
        /// </summary>
        private void ExecuteComboEffects(BuffComboData combo, IBuffOwner owner)
        {
            foreach (var effect in combo.Effects)
            {
                ExecuteEffect(effect, combo, owner);
            }
        }

        /// <summary>
        /// 执行单个效果
        /// </summary>
        private void ExecuteEffect(ComboEffect effect, BuffComboData combo, IBuffOwner owner)
        {
            try
            {
                switch (effect.EffectType)
                {
                    case ComboEffectType.EnhanceDuration:
                        ExecuteEnhanceDuration(effect, owner);
                        break;

                    case ComboEffectType.EnhanceStack:
                        ExecuteEnhanceStack(effect, owner);
                        break;

                    case ComboEffectType.ReduceCooldown:
                        ExecuteReduceCooldown(effect, owner);
                        break;

                    case ComboEffectType.TriggerEvent:
                        ExecuteTriggerEvent(effect, combo, owner);
                        break;

                    case ComboEffectType.ModifyAttribute:
                        ExecuteModifyAttribute(effect, owner);
                        break;

                    case ComboEffectType.AddExtraBuff:
                        ExecuteAddExtraBuff(effect, owner);
                        break;

                    case ComboEffectType.RemoveBuff:
                        ExecuteRemoveBuff(effect, owner);
                        break;

                    case ComboEffectType.RefreshDuration:
                        ExecuteRefreshDuration(effect, owner);
                        break;

                    case ComboEffectType.AddStack:
                        ExecuteAddStack(effect, owner);
                        break;

                    case ComboEffectType.RemoveStack:
                        ExecuteRemoveStack(effect, owner);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[BuffComboManager] 执行Combo效果失败: {e.Message}");
            }
        }

        private void ExecuteEnhanceDuration(ComboEffect effect, IBuffOwner owner)
        {
            var buff = GetTargetBuff(effect, owner);
            if (buff == null) return;

            // 通过事件系统通知持续时间增强
            float multiplier = effect.UsePercentage ? effect.Value : effect.Value / 100f;
            BuffComboEventSystem.TriggerEnhanceDuration(buff, multiplier);
        }

        private void ExecuteEnhanceStack(ComboEffect effect, IBuffOwner owner)
        {
            var buff = GetTargetBuff(effect, owner);
            if (buff == null) return;

            float multiplier = effect.UsePercentage ? effect.Value : effect.Value / 100f;
            BuffComboEventSystem.TriggerEnhanceStack(buff, multiplier);
        }

        private void ExecuteReduceCooldown(ComboEffect effect, IBuffOwner owner)
        {
            float multiplier = effect.UsePercentage ? effect.Value : effect.Value / 100f;
            BuffComboEventSystem.TriggerReduceCooldown(owner, effect.TargetBuffId, multiplier);
        }

        private void ExecuteTriggerEvent(ComboEffect effect, BuffComboData combo, IBuffOwner owner)
        {
            BuffComboEventSystem.TriggerCustomEvent(effect.EventName, combo, owner, effect.Value);
        }

        private void ExecuteModifyAttribute(ComboEffect effect, IBuffOwner owner)
        {
            BuffComboEventSystem.TriggerModifyAttribute(owner, effect.EventName, effect.Value);
        }

        private void ExecuteAddExtraBuff(ComboEffect effect, IBuffOwner owner)
        {
            if (effect.ExtraBuffId > 0)
            {
                BuffApi.AddBuff(effect.ExtraBuffId, owner, source: this);
            }
        }

        private void ExecuteRemoveBuff(ComboEffect effect, IBuffOwner owner)
        {
            var buff = GetTargetBuff(effect, owner);
            if (buff != null)
            {
                BuffApi.RemoveBuff(buff);
            }
        }

        private void ExecuteRefreshDuration(ComboEffect effect, IBuffOwner owner)
        {
            var buff = GetTargetBuff(effect, owner);
            if (buff is BuffEntity entity)
            {
                entity.RefreshDuration();
            }
        }

        private void ExecuteAddStack(ComboEffect effect, IBuffOwner owner)
        {
            var buff = GetTargetBuff(effect, owner);
            if (buff is BuffEntity entity)
            {
                int amount = effect.UsePercentage ? Mathf.RoundToInt(buff.MaxStack * effect.Value) : Mathf.RoundToInt(effect.Value);
                entity.AddStack(Mathf.Max(1, amount));
            }
        }

        private void ExecuteRemoveStack(ComboEffect effect, IBuffOwner owner)
        {
            var buff = GetTargetBuff(effect, owner);
            if (buff is BuffEntity entity)
            {
                int amount = effect.UsePercentage ? Mathf.RoundToInt(buff.CurrentStack * effect.Value) : Mathf.RoundToInt(effect.Value);
                entity.RemoveStack(Mathf.Max(1, amount));
            }
        }

        /// <summary>
        /// 获取效果的目标Buff
        /// </summary>
        private IBuff GetTargetBuff(ComboEffect effect, IBuffOwner owner)
        {
            if (owner?.BuffContainer == null) return null;

            switch (effect.TargetType)
            {
                case ComboTargetType.SpecificBuff:
                    return owner.BuffContainer.GetBuff(effect.TargetBuffId);

                case ComboTargetType.NewestBuff:
                    IBuff newestBuff = null;
                    float maxDuration = float.MinValue;
                    foreach (var buff in owner.BuffContainer.AllBuffs)
                    {
                        if (buff.Duration > maxDuration)
                        {
                            maxDuration = buff.Duration;
                            newestBuff = buff;
                        }
                    }
                    return newestBuff;

                default:
                    return null;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// 获取所有注册的Combo
        /// </summary>
        public IReadOnlyList<BuffComboData> GetAllCombos() => registeredCombos;

        /// <summary>
        /// 根据ID获取Combo
        /// </summary>
        public BuffComboData GetComboById(int comboId)
        {
            foreach (var combo in registeredCombos)
            {
                if (combo.ComboId == comboId)
                    return combo;
            }
            return null;
        }

        /// <summary>
        /// 检查持有者是否激活了指定Combo
        /// </summary>
        public bool IsComboActive(int comboId, IBuffOwner owner)
        {
            return activeCombos.TryGetValue(owner, out var ownerActiveCombos) &&
                   ownerActiveCombos.Contains(comboId);
        }

        /// <summary>
        /// 获取持有者的所有激活Combo
        /// </summary>
        public IEnumerable<BuffComboData> GetActiveCombos(IBuffOwner owner)
        {
            if (!activeCombos.TryGetValue(owner, out var ownerActiveCombos))
                yield break;

            foreach (var comboId in ownerActiveCombos)
            {
                var combo = GetComboById(comboId);
                if (combo != null)
                    yield return combo;
            }
        }

        /// <summary>
        /// 获取Combo的触发次数
        /// </summary>
        public int GetTriggerCount(IBuffOwner owner, int comboId)
        {
            if (comboTriggerCounts.TryGetValue(owner, out var counts) &&
                counts.TryGetValue(comboId, out var count))
            {
                return count;
            }
            return 0;
        }

        /// <summary>
        /// 手动触发检查所有Combo（用于Update模式）
        /// </summary>
        public void CheckAllCombos(IBuffOwner owner)
        {
            foreach (var combo in registeredCombos)
            {
                if ((combo.TriggerMode & ComboTriggerMode.OnUpdate) != 0)
                {
                    TryActivateCombo(combo, owner);
                }
            }
        }

        /// <summary>
        /// 清除持有者的所有Combo状态
        /// </summary>
        public void ClearOwnerCombos(IBuffOwner owner)
        {
            if (activeCombos.TryGetValue(owner, out var ownerActiveCombos))
            {
                ownerActiveCombos.Clear();
                activeCombos.Remove(owner);
            }
            comboTriggerCounts.Remove(owner);
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
            sb.AppendLine($"=== BuffComboManager Debug Info ===");
            sb.AppendLine($"Registered Combos: {registeredCombos.Count}");
            sb.AppendLine($"Active Owners: {activeCombos.Count}");

            foreach (var kvp in activeCombos)
            {
                sb.AppendLine($"  Owner: {kvp.Key.OwnerName}");
                foreach (var comboId in kvp.Value)
                {
                    var combo = GetComboById(comboId);
                    sb.AppendLine($"    - {combo?.ComboName ?? "Unknown"} (ID: {comboId})");
                }
            }

            return sb.ToString();
        }
#endif

        #endregion
    }
}
