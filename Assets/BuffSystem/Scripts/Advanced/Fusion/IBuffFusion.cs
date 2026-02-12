using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BuffSystem.Core;

namespace BuffSystem.Advanced.Fusion
{
    /// <summary>
    /// Buff融合接口 - 实现此接口的Buff可以参与融合
    /// </summary>
    public interface IBuffFusion
    {
        /// <summary>
        /// 融合配方ID
        /// </summary>
        string RecipeId { get; }
        
        /// <summary>
        /// 是否可以融合
        /// </summary>
        bool CanFuse(IBuffContainer container);
        
        /// <summary>
        /// 执行融合
        /// </summary>
        /// <returns>融合结果Buff</returns>
        IBuff Fuse(IBuffContainer container);
        
        /// <summary>
        /// 融合前回调
        /// </summary>
        void OnBeforeFusion(IBuffContainer container);
        
        /// <summary>
        /// 融合后回调
        /// </summary>
        void OnAfterFusion(IBuffContainer container, IBuff resultBuff);
    }
    
    /// <summary>
    /// 融合配方
    /// </summary>
    [Serializable]
    public class FusionRecipe
    {
        [Tooltip("配方唯一ID")]
        public string recipeId;
        
        [Tooltip("配方名称")]
        public string recipeName;
        
        [Tooltip("所需Buff列表")]
        public List<Ingredient> ingredients = new();
        
        [Tooltip("融合结果Buff ID")]
        public int resultBuffId;
        
        [Tooltip("融合时间（秒）")]
        public float fusionTime;
        
        [Tooltip("融合条件")]
        [SerializeReference]
        public List<IFusionCondition> conditions = new();
        
        /// <summary>
        /// 检查是否满足融合条件
        /// </summary>
        public bool CheckConditions(IBuffContainer container)
        {
            foreach (var condition in conditions)
            {
                if (!condition.Check(container, this))
                    return false;
            }
            return true;
        }
        
        /// <summary>
        /// 检查是否有所需材料
        /// </summary>
        public bool HasIngredients(IBuffContainer container)
        {
            foreach (var ingredient in ingredients)
            {
                int count = container.GetBuffs(ingredient.buffId).Count();
                if (count < ingredient.requiredCount)
                    return false;
            }
            return true;
        }
    }
    
    /// <summary>
    /// 融合材料
    /// </summary>
    [Serializable]
    public class Ingredient
    {
        [Tooltip("Buff ID")]
        public int buffId;
        
        [Tooltip("所需数量")]
        public int requiredCount = 1;
        
        [Tooltip("是否消耗")]
        public bool consume = true;
    }
    
    /// <summary>
    /// 融合条件接口
    /// </summary>
    public interface IFusionCondition
    {
        bool Check(IBuffContainer container, FusionRecipe recipe);
        string Description { get; }
    }
}
