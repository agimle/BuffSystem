using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Data;

namespace BuffSystem.Advanced.Fusion
{
    /// <summary>
    /// 融合管理器 - 管理所有融合配方和执行
    /// 通过BuffSystemManager.Fusion访问
    /// v7.0: 单例访问改为通过BuffSystemManager
    /// </summary>
    public class FusionManager : MonoBehaviour
    {
        /// <summary>
        /// 全局实例 - 通过BuffSystemManager.Fusion访问
        /// </summary>
        [System.Obsolete("使用 BuffSystemManager.Fusion 替代")]
        public static FusionManager Instance => BuffSystemManager.Fusion;
        
        // 配方注册表
        private Dictionary<string, FusionRecipe> recipes = new();
        
        // 进行中的融合
        private List<ActiveFusion> activeFusions = new();
        
        /// <summary>
        /// 注册配方
        /// </summary>
        public void RegisterRecipe(FusionRecipe recipe)
        {
            if (string.IsNullOrEmpty(recipe.recipeId))
            {
                Debug.LogError("[FusionManager] 配方ID不能为空");
                return;
            }
            
            recipes[recipe.recipeId] = recipe;
            
            if (BuffSystemConfig.Instance.EnableDebugLog)
            {
                Debug.Log($"[FusionManager] 注册配方: {recipe.recipeName} ({recipe.recipeId})");
            }
        }
        
        /// <summary>
        /// 尝试融合
        /// </summary>
        public bool TryFusion(string recipeId, IBuffContainer container, out IBuff resultBuff)
        {
            resultBuff = null;
            
            if (!recipes.TryGetValue(recipeId, out var recipe))
            {
                Debug.LogError($"[FusionManager] 未找到配方: {recipeId}");
                return false;
            }
            
            // 检查材料
            if (!recipe.HasIngredients(container))
            {
                Debug.Log($"[FusionManager] 材料不足，无法融合: {recipe.recipeName}");
                return false;
            }
            
            // 检查条件
            if (!recipe.CheckConditions(container))
            {
                Debug.Log($"[FusionManager] 条件不满足，无法融合: {recipe.recipeName}");
                return false;
            }
            
            // 开始融合
            if (recipe.fusionTime > 0)
            {
                // 延迟融合
                StartDelayedFusion(recipe, container);
                return true;
            }
            else
            {
                // 立即融合
                resultBuff = ExecuteFusion(recipe, container);
                return resultBuff != null;
            }
        }
        
        /// <summary>
        /// 自动检测可融合的配方
        /// </summary>
        public List<FusionRecipe> GetAvailableFusions(IBuffContainer container)
        {
            var available = new List<FusionRecipe>();
            
            foreach (var recipe in recipes.Values)
            {
                if (recipe.HasIngredients(container) && recipe.CheckConditions(container))
                {
                    available.Add(recipe);
                }
            }
            
            return available;
        }
        
        private void StartDelayedFusion(FusionRecipe recipe, IBuffContainer container)
        {
            var activeFusion = new ActiveFusion
            {
                Recipe = recipe,
                Container = container,
                RemainingTime = recipe.fusionTime,
                TotalTime = recipe.fusionTime
            };
            
            activeFusions.Add(activeFusion);
            
            // 触发融合开始事件
            FusionEventSystem.TriggerFusionStarted(recipe, container);
        }
        
        private IBuff ExecuteFusion(FusionRecipe recipe, IBuffContainer container)
        {
            // 消耗材料
            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient.consume)
                {
                    for (int i = 0; i < ingredient.requiredCount; i++)
                    {
                        container.RemoveBuff(ingredient.buffId);
                    }
                }
            }
            
            // 创建结果Buff
            var resultData = BuffDatabase.Instance.GetBuffData(recipe.resultBuffId);
            if (resultData == null)
            {
                Debug.LogError($"[FusionManager] 未找到结果Buff: {recipe.resultBuffId}");
                return null;
            }
            
            var resultBuff = container.AddBuff(resultData, this);
            
            // 触发融合完成事件
            FusionEventSystem.TriggerFusionCompleted(recipe, container, resultBuff);
            
            if (BuffSystemConfig.Instance.EnableDebugLog)
            {
                Debug.Log($"[FusionManager] 融合成功: {recipe.recipeName} -> {resultBuff.Name}");
            }
            
            return resultBuff;
        }
        
        private void Update()
        {
            // 更新进行中的融合
            for (int i = activeFusions.Count - 1; i >= 0; i--)
            {
                var fusion = activeFusions[i];
                fusion.RemainingTime -= Time.deltaTime;
                
                // 触发进度更新
                float progress = 1f - (fusion.RemainingTime / fusion.TotalTime);
                FusionEventSystem.TriggerFusionProgress(fusion.Recipe, fusion.Container, progress);
                
                if (fusion.RemainingTime <= 0)
                {
                    // 融合完成
                    ExecuteFusion(fusion.Recipe, fusion.Container);
                    activeFusions.RemoveAt(i);
                }
            }
        }
        
        private class ActiveFusion
        {
            public FusionRecipe Recipe;
            public IBuffContainer Container;
            public float RemainingTime;
            public float TotalTime;
        }
    }
}
