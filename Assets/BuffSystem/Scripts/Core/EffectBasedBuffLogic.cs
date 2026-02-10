using System.Collections.Generic;
using BuffSystem.Data;
using UnityEngine;

namespace BuffSystem.Core
{
    /// <summary>
    /// 基于效果的Buff逻辑
    /// 支持在Inspector中配置多个Effect，在对应的生命周期方法中执行
    /// </summary>
    [System.Serializable]
    public class EffectBasedBuffLogic : BuffLogicBase, 
        IBuffAcquire, IBuffRemove, IBuffRefresh, IBuffDurationChange,IBuffVisualUpdate,
        IBuffStackChange, IBuffReduce, IBuffEnd, IBuffLogicUpdate
    {
        [Header("效果配置")]
        [SerializeReference, SubclassSelector]
        [Tooltip("Buff添加时执行的效果")]
        private List<IEffect> onAcquireEffects = new List<IEffect>();

        [SerializeReference, SubclassSelector]
        [Tooltip("Buff移除时执行的效果")]
        private List<IEffect> onRemoveEffects = new List<IEffect>();

        [SerializeReference, SubclassSelector]
        [Tooltip("Buff刷新时执行的效果")]
        private List<IEffect> onRefreshEffects = new List<IEffect>();

        [SerializeReference, SubclassSelector]
        [Tooltip("持续时间变化时执行的效果")]
        private List<IEffect> onDurationChangeEffects = new List<IEffect>();

        [SerializeReference, SubclassSelector]
        [Tooltip("Buff层数变化时执行的效果")]
        private List<IEffect> onStackChangeEffects = new List<IEffect>();

        [SerializeReference, SubclassSelector]
        [Tooltip("Buff层数减少时执行的效果")]
        private List<IEffect> onReduceEffects = new List<IEffect>();

        [SerializeReference, SubclassSelector]
        [Tooltip("Buff结束时执行的效果")]
        private List<IEffect> onEndEffects = new List<IEffect>();

        [SerializeReference, SubclassSelector]
        [Tooltip("每帧逻辑更新时执行的效果")]
        private List<IEffect> onUpdateEffects = new List<IEffect>();

        [SerializeReference, SubclassSelector]
        [Tooltip("每帧可视化更新时执行的效果")]
        private List<IEffect> onVisualUpdateEffects = new List<IEffect>();
        
        // IBuffAcquire 实现
        public void OnAcquire()
        {
            ExecuteEffects(onAcquireEffects);
        }

        // IBuffRemove 实现
        public void OnRemove()
        {
            ExecuteEffects(onRemoveEffects);
        }

        // IBuffRefresh 实现
        public void OnRefresh()
        {
            ExecuteEffects(onRefreshEffects);
        }

        // IBuffDurationChange 实现
        public void OnDurationChanged(float oldDuration, float newDuration)
        {
            ExecuteEffects(onDurationChangeEffects);
        }

        // IBuffStackChange 实现
        public void OnStackChanged(int oldStack, int newStack)
        {
            ExecuteEffects(onStackChangeEffects);
        }

        // IBuffReduce 实现
        public void OnReduce()
        {
            ExecuteEffects(onReduceEffects);
        }

        // IBuffEnd 实现
        public void OnEnd()
        {
            ExecuteEffects(onEndEffects);
        }

        // IBuffLogicUpdate 实现
        public void OnLogicUpdate(float deltaTime)
        {
            ExecuteEffects(onUpdateEffects);
        }
        
        // IBuffVisualUpdate 实现
        public void OnVisualUpdate(float deltaTime)
        {
            ExecuteEffects(onVisualUpdateEffects);
        }

        /// <summary>
        /// 执行效果列表
        /// </summary>
        private void ExecuteEffects(List<IEffect> effects)
        {
            if (effects == null) return;

            foreach (var effect in effects)
            {
                effect?.Execute(Buff);
            }
        }

        /// <summary>
        /// 取消效果列表
        /// </summary>
        private void CancelEffects(List<IEffect> effects)
        {
            if (effects == null) return;

            foreach (var effect in effects)
            {
                effect?.Cancel(Buff);
            }
        }
    }
}
