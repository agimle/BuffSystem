using System;
using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;

namespace BuffSystem.Advanced.Combo
{
    /// <summary>
    /// Buff组合数据配置 - ScriptableObject
    /// 定义Buff之间的连携关系和效果
    /// </summary>
    [CreateAssetMenu(fileName = "NewBuffCombo", menuName = "BuffSystem/Buff Combo", order = 2)]
    public class BuffComboData : ScriptableObject
    {
        [Header("基础信息")]
        [SerializeField] private int comboId;
        [SerializeField] private string comboName = "New Combo";
        [SerializeField, TextArea(2, 4)] private string description = "";

        [Header("触发条件")]
        [SerializeField] private List<int> requiredBuffIds = new();
        [SerializeField] private ComboTriggerMode triggerMode = ComboTriggerMode.Default;
        [SerializeField] private bool requireAll = true;

        [Header("优先级")]
        [SerializeField] private int priority = 0;

        [Header("效果列表")]
        [SerializeField] private List<ComboEffect> effects = new();

        [Header("可选：触发新Buff")]
        [SerializeField] private int triggerBuffId;
        [SerializeField] private bool onlyTriggerOnce = true;

        #region Properties

        /// <summary>
        /// Combo唯一ID
        /// </summary>
        public int ComboId => comboId;

        /// <summary>
        /// Combo名称
        /// </summary>
        public string ComboName => comboName;

        /// <summary>
        /// Combo描述
        /// </summary>
        public string Description => description;

        /// <summary>
        /// 必需的Buff ID列表
        /// </summary>
        public IReadOnlyList<int> RequiredBuffIds => requiredBuffIds;

        /// <summary>
        /// 触发模式
        /// </summary>
        public ComboTriggerMode TriggerMode => triggerMode;

        /// <summary>
        /// 是否需要所有Buff都存在（AND逻辑），否则任一存在即可（OR逻辑）
        /// </summary>
        public bool RequireAll => requireAll;

        /// <summary>
        /// 优先级（数值越高优先级越高）
        /// </summary>
        public int Priority => priority;

        /// <summary>
        /// Combo效果列表
        /// </summary>
        public IReadOnlyList<ComboEffect> Effects => effects;

        /// <summary>
        /// 触发的新Buff ID（0表示不触发）
        /// </summary>
        public int TriggerBuffId => triggerBuffId;

        /// <summary>
        /// 是否只触发一次
        /// </summary>
        public bool OnlyTriggerOnce => onlyTriggerOnce;

        /// <summary>
        /// 是否会触发新Buff
        /// </summary>
        public bool HasTriggerBuff => triggerBuffId > 0;

        #endregion

        #region Validation

        /// <summary>
        /// 检查配置是否有效
        /// </summary>
        public bool IsValid()
        {
            if (requiredBuffIds == null || requiredBuffIds.Count == 0)
                return false;

            if (effects == null || effects.Count == 0)
                return false;

            // 检查是否有无效的Buff ID
            foreach (var buffId in requiredBuffIds)
            {
                if (buffId <= 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 检查持有者是否满足触发条件
        /// </summary>
        public bool CheckCondition(IBuffOwner owner)
        {
            if (owner?.BuffContainer == null)
                return false;

            if (requireAll)
            {
                // AND逻辑：所有必需Buff都必须存在
                foreach (var buffId in requiredBuffIds)
                {
                    if (!owner.BuffContainer.HasBuff(buffId))
                        return false;
                }
                return true;
            }
            else
            {
                // OR逻辑：任一必需Buff存在即可
                foreach (var buffId in requiredBuffIds)
                {
                    if (owner.BuffContainer.HasBuff(buffId))
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 获取当前满足的Buff数量
        /// </summary>
        public int GetMatchedBuffCount(IBuffOwner owner)
        {
            if (owner?.BuffContainer == null)
                return 0;

            int count = 0;
            foreach (var buffId in requiredBuffIds)
            {
                if (owner.BuffContainer.HasBuff(buffId))
                    count++;
            }
            return count;
        }

        #endregion

        #region Editor Only

#if UNITY_EDITOR

        private void OnValidate()
        {
            // 确保ID不为0
            if (comboId == 0)
            {
                comboId = Mathf.Abs(comboName.GetHashCode());
            }

            // 确保至少有一个必需Buff
            if (requiredBuffIds == null)
            {
                requiredBuffIds = new List<int>();
            }

            // 清理无效的Buff ID
            requiredBuffIds.RemoveAll(id => id <= 0);
        }

#endif

        #endregion
    }
}
