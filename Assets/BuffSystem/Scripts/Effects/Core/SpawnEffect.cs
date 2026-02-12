using System;
using UnityEngine;
using BuffSystem.Core;

namespace BuffSystem.Effects.Core
{
    /// <summary>
    /// 生成预制体Effect - 完全通用
    /// </summary>
    [Serializable]
    public class SpawnEffect : EffectBase
    {
        [Tooltip("预制体")]
        [SerializeField] private GameObject prefab;

        [Tooltip("生成位置偏移")]
        [SerializeField] private Vector3 offset;

        [Tooltip("是否附加到持有者")]
        [SerializeField] private bool attachToOwner = true;

        [Tooltip("自动销毁时间（秒，-1表示不销毁）")]
        [SerializeField] private float destroyAfter = -1f;

        private GameObject spawnedInstance;

        public override void Execute(IBuff buff)
        {
            if (prefab == null) return;

            Vector3 spawnPosition = buff.Owner is MonoBehaviour mono
                ? mono.transform.position + offset
                : offset;

            spawnedInstance = UnityEngine.Object.Instantiate(prefab, spawnPosition, Quaternion.identity);

            if (attachToOwner && buff.Owner is MonoBehaviour ownerMono)
            {
                spawnedInstance.transform.SetParent(ownerMono.transform);
            }

            if (destroyAfter > 0)
            {
                UnityEngine.Object.Destroy(spawnedInstance, destroyAfter);
            }
        }

        public override void Cancel(IBuff buff)
        {
            if (spawnedInstance != null && destroyAfter <= 0)
            {
                UnityEngine.Object.Destroy(spawnedInstance);
            }
        }
    }
}
