using System;
using UnityEngine;
using BuffSystem.Core;

namespace BuffSystem.Effects.Core
{
    /// <summary>
    /// 播放音效Effect - 完全通用
    /// </summary>
    [Serializable]
    public class PlaySoundEffect : EffectBase
    {
        [Tooltip("音效剪辑")]
        [SerializeField] private AudioClip clip;

        [Tooltip("音量")]
        [SerializeField] private float volume = 1f;

        [Tooltip("是否循环")]
        [SerializeField] private bool loop = false;

        private AudioSource audioSource;

        public override void Execute(IBuff buff)
        {
            if (clip == null) return;

            if (buff.Owner is MonoBehaviour mono)
            {
                audioSource = mono.gameObject.AddComponent<AudioSource>();
                audioSource.clip = clip;
                audioSource.volume = volume;
                audioSource.loop = loop;
                audioSource.Play();
            }
        }

        public override void Cancel(IBuff buff)
        {
            if (audioSource != null)
            {
                if (loop)
                {
                    audioSource.Stop();
                }
                UnityEngine.Object.Destroy(audioSource);
            }
        }
    }
}
