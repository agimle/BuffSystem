using System;
using BuffSystem.Core;

namespace BuffSystem.Fusion
{
    /// <summary>
    /// 融合事件系统 - 处理Buff融合相关事件
    /// </summary>
    public static class FusionEventSystem
    {
        #region Events
        
        /// <summary>
        /// 融合开始事件
        /// </summary>
        public static event Action<FusionRecipe, IBuffContainer> OnFusionStarted;
        
        /// <summary>
        /// 融合进度更新事件
        /// </summary>
        public static event Action<FusionRecipe, IBuffContainer, float> OnFusionProgress;
        
        /// <summary>
        /// 融合完成事件
        /// </summary>
        public static event Action<FusionRecipe, IBuffContainer, IBuff> OnFusionCompleted;
        
        /// <summary>
        /// 融合失败事件
        /// </summary>
        public static event Action<FusionRecipe, IBuffContainer, string> OnFusionFailed;
        
        #endregion
        
        #region Trigger Methods
        
        /// <summary>
        /// 触发融合开始事件
        /// </summary>
        public static void TriggerFusionStarted(FusionRecipe recipe, IBuffContainer container)
        {
            OnFusionStarted?.Invoke(recipe, container);
        }
        
        /// <summary>
        /// 触发融合进度更新事件
        /// </summary>
        public static void TriggerFusionProgress(FusionRecipe recipe, IBuffContainer container, float progress)
        {
            OnFusionProgress?.Invoke(recipe, container, progress);
        }
        
        /// <summary>
        /// 触发融合完成事件
        /// </summary>
        public static void TriggerFusionCompleted(FusionRecipe recipe, IBuffContainer container, IBuff resultBuff)
        {
            OnFusionCompleted?.Invoke(recipe, container, resultBuff);
        }
        
        /// <summary>
        /// 触发融合失败事件
        /// </summary>
        public static void TriggerFusionFailed(FusionRecipe recipe, IBuffContainer container, string reason)
        {
            OnFusionFailed?.Invoke(recipe, container, reason);
        }
        
        #endregion
    }
}
