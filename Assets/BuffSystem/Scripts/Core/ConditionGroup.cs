using System;
using System.Collections.Generic;
using System.Linq;
using BuffSystem.Data;
using UnityEngine;

namespace BuffSystem.Core
{
    /// <summary>
    /// 条件组合类型
    /// 定义条件之间的组合逻辑关系
    /// </summary>
    public enum ConditionCombineType
    {
        /// <summary>所有条件都必须满足（AND逻辑）</summary>
        And,
        /// <summary>任一条件满足即可（OR逻辑）</summary>
        Or,
        /// <summary>条件不满足（NOT逻辑）</summary>
        Not
    }

    /// <summary>
    /// 条件组合 - 支持复杂逻辑组合
    /// 可以嵌套使用，支持And/Or/Not三种逻辑
    /// </summary>
    [Serializable]
    public class ConditionGroup : IBuffCondition
    {
        [SerializeField]
        [Tooltip("条件组合类型：And=全部满足, Or=任一满足, Not=全部不满足")]
        private ConditionCombineType combineType = ConditionCombineType.And;

        [SerializeReference]
        [SubclassSelector]
        [Tooltip("子条件列表，可以是具体条件或其他ConditionGroup")]
        private List<IBuffCondition> conditions = new();

        /// <summary>
        /// 条件组合类型
        /// </summary>
        public ConditionCombineType CombineType
        {
            get => combineType;
            set => combineType = value;
        }

        /// <summary>
        /// 子条件列表
        /// </summary>
        public List<IBuffCondition> Conditions
        {
            get => conditions;
            set => conditions = value ?? new List<IBuffCondition>();
        }

        /// <summary>
        /// 检查条件是否满足
        /// </summary>
        public bool Check(IBuffOwner owner, IBuffData data)
        {
            if (conditions == null || conditions.Count == 0)
            {
                return true;
            }

            return combineType switch
            {
                ConditionCombineType.And => CheckAnd(owner, data),
                ConditionCombineType.Or => CheckOr(owner, data),
                ConditionCombineType.Not => CheckNot(owner, data),
                _ => true
            };
        }

        /// <summary>
        /// AND逻辑：所有条件都必须满足
        /// </summary>
        private bool CheckAnd(IBuffOwner owner, IBuffData data)
        {
            foreach (var condition in conditions)
            {
                if (condition == null) continue;
                if (!condition.Check(owner, data))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// OR逻辑：任一条件满足即可
        /// </summary>
        private bool CheckOr(IBuffOwner owner, IBuffData data)
        {
            foreach (var condition in conditions)
            {
                if (condition == null) continue;
                if (condition.Check(owner, data))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// NOT逻辑：所有条件都不满足
        /// </summary>
        private bool CheckNot(IBuffOwner owner, IBuffData data)
        {
            foreach (var condition in conditions)
            {
                if (condition == null) continue;
                if (condition.Check(owner, data))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 条件描述
        /// </summary>
        public string Description
        {
            get
            {
                if (conditions == null || conditions.Count == 0)
                {
                    return "无条件";
                }

                var validConditions = conditions.Where(c => c != null).ToList();
                if (validConditions.Count == 0)
                {
                    return "无条件";
                }

                var descs = validConditions.Select(c => $"({c.Description})").ToList();
                string operatorStr = combineType switch
                {
                    ConditionCombineType.And => " 且 ",
                    ConditionCombineType.Or => " 或 ",
                    ConditionCombineType.Not => " 非 ",
                    _ => " ? "
                };

                if (combineType == ConditionCombineType.Not)
                {
                    return $"不满足: {string.Join(" 且 ", descs)}";
                }

                return string.Join(operatorStr, descs);
            }
        }

        /// <summary>
        /// 添加子条件
        /// </summary>
        public ConditionGroup AddCondition(IBuffCondition condition)
        {
            if (condition != null)
            {
                conditions ??= new List<IBuffCondition>();
                conditions.Add(condition);
            }
            return this;
        }

        /// <summary>
        /// 移除子条件
        /// </summary>
        public ConditionGroup RemoveCondition(IBuffCondition condition)
        {
            conditions?.Remove(condition);
            return this;
        }

        /// <summary>
        /// 清空所有条件
        /// </summary>
        public ConditionGroup ClearConditions()
        {
            conditions?.Clear();
            return this;
        }
    }

    /// <summary>
    /// 条件扩展方法 - Fluent API
    /// 提供简洁的条件组合语法
    /// </summary>
    public static class ConditionExtensions
    {
        /// <summary>
        /// 创建AND条件组合
        /// 所有条件都必须满足
        /// </summary>
        public static ConditionGroup And(params IBuffCondition[] conditions)
        {
            var group = new ConditionGroup
            {
                CombineType = ConditionCombineType.And,
                Conditions = conditions?.Where(c => c != null).ToList() ?? new List<IBuffCondition>()
            };
            return group;
        }

        /// <summary>
        /// 创建OR条件组合
        /// 任一条件满足即可
        /// </summary>
        public static ConditionGroup Or(params IBuffCondition[] conditions)
        {
            var group = new ConditionGroup
            {
                CombineType = ConditionCombineType.Or,
                Conditions = conditions?.Where(c => c != null).ToList() ?? new List<IBuffCondition>()
            };
            return group;
        }

        /// <summary>
        /// 创建NOT条件
        /// 条件不满足时返回true
        /// </summary>
        public static ConditionGroup Not(this IBuffCondition condition)
        {
            var group = new ConditionGroup
            {
                CombineType = ConditionCombineType.Not
            };
            if (condition != null)
            {
                group.Conditions.Add(condition);
            }
            return group;
        }

        /// <summary>
        /// 与另一个条件进行AND组合
        /// </summary>
        public static ConditionGroup And(this IBuffCondition left, IBuffCondition right)
        {
            var group = new ConditionGroup
            {
                CombineType = ConditionCombineType.And
            };
            if (left != null) group.Conditions.Add(left);
            if (right != null) group.Conditions.Add(right);
            return group;
        }

        /// <summary>
        /// 与多个条件进行AND组合
        /// </summary>
        public static ConditionGroup And(this IBuffCondition left, params IBuffCondition[] right)
        {
            var group = new ConditionGroup
            {
                CombineType = ConditionCombineType.And
            };
            if (left != null) group.Conditions.Add(left);
            if (right != null)
            {
                foreach (var condition in right)
                {
                    if (condition != null) group.Conditions.Add(condition);
                }
            }
            return group;
        }

        /// <summary>
        /// 与另一个条件进行OR组合
        /// </summary>
        public static ConditionGroup Or(this IBuffCondition left, IBuffCondition right)
        {
            var group = new ConditionGroup
            {
                CombineType = ConditionCombineType.Or
            };
            if (left != null) group.Conditions.Add(left);
            if (right != null) group.Conditions.Add(right);
            return group;
        }

        /// <summary>
        /// 与多个条件进行OR组合
        /// </summary>
        public static ConditionGroup Or(this IBuffCondition left, params IBuffCondition[] right)
        {
            var group = new ConditionGroup
            {
                CombineType = ConditionCombineType.Or
            };
            if (left != null) group.Conditions.Add(left);
            if (right != null)
            {
                foreach (var condition in right)
                {
                    if (condition != null) group.Conditions.Add(condition);
                }
            }
            return group;
        }
    }
}
