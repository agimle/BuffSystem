using System;
using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Data;

namespace BuffSystem.Effects.Core
{
    /// <summary>
    /// 延迟执行Effect - 延迟一段时间后执行其他Effect
    /// </summary>
    [Serializable]
    public class DelayEffect : EffectBase, IBuffLogicUpdate
    {
        [Tooltip("延迟时间（秒）")]
        [SerializeField] private float delay = 1f;

        [Tooltip("延迟后执行的Effect")]
        [SerializeReference, SubclassSelector]
        private List<IEffect> delayedEffects = new();

        private float timer;
        private bool hasTriggered;

        /// <summary>
        /// 所属的Buff实例
        /// </summary>
        public IBuff Buff { get; set; }

        /// <summary>
        /// 初始化时调用
        /// </summary>
        public void Initialize(IBuff buff)
        {
            Buff = buff;
        }

        /// <summary>
        /// 销毁时调用
        /// </summary>
        public void Dispose()
        {
            Buff = null;
        }

        public override void Execute(IBuff buff)
        {
            timer = 0f;
            hasTriggered = false;
        }

        public void OnLogicUpdate(float deltaTime)
        {
            if (hasTriggered) return;

            timer += deltaTime;

            if (timer >= delay)
            {
                hasTriggered = true;

                foreach (var effect in delayedEffects)
                {
                    effect?.Execute(Buff);
                }
            }
        }

        public override void Cancel(IBuff buff)
        {
            foreach (var effect in delayedEffects)
            {
                effect?.Cancel(buff);
            }
        }
    }
}
