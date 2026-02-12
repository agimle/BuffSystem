using System;
using UnityEngine;
using BuffSystem.Core;

namespace BuffSystem.Effects.Core
{
    /// <summary>
    /// 播放动画Effect - 完全通用
    /// </summary>
    [Serializable]
    public class PlayAnimationEffect : EffectBase
    {
        [Tooltip("动画状态名")]
        [SerializeField] private string animationName;

        [Tooltip("动画层")]
        [SerializeField] private int layer = 0;

        [Tooltip("淡入时间")]
        [SerializeField] private float crossFadeTime = 0.1f;

        public override void Execute(IBuff buff)
        {
            if (buff.Owner is not MonoBehaviour mono) return;

            var animator = mono.GetComponent<Animator>();
            if (animator != null && !string.IsNullOrEmpty(animationName))
            {
                animator.CrossFade(animationName, crossFadeTime, layer);
            }
        }

        public override void Cancel(IBuff buff)
        {
            // 可选：恢复默认动画
        }
    }
}
