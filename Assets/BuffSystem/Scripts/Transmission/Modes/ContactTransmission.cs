using System;
using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Data;

namespace BuffSystem.Transmission
{
    /// <summary>
    /// 接触传播 - 当两个单位接触时传播Buff
    /// </summary>
    [Serializable]
    public class ContactTransmission : IBuffTransmissible
    {
        [Tooltip("传播概率 0-1")]
        [SerializeField, Range(0f, 1f)]
        private float transmissionProbability = 0.5f;
        
        [Tooltip("传播冷却时间（秒）")]
        [SerializeField]
        private float transmissionCooldown = 1f;
        
        [Tooltip("接触检测半径")]
        [SerializeField]
        private float contactRadius = 2f;
        
        [Tooltip("目标层")]
        [SerializeField]
        private LayerMask targetLayers = ~0;
        
        [Tooltip("是否只对敌人生效")]
        [SerializeField]
        private bool onlyAffectEnemies = false;
        
        private float lastTransmissionTime;
        
        public TransmissionMode Mode => TransmissionMode.Contact;
        
        public int MaxTransmissionChain => 1; // 接触传播不连锁
        
        public int CurrentChainLength { get; set; }
        
        public bool CanTransmit(IBuff buff, IBuffOwner target)
        {
            // 检查冷却
            if (Time.time - lastTransmissionTime < transmissionCooldown)
                return false;
            
            // 检查概率
            if (UnityEngine.Random.value > transmissionProbability)
                return false;
            
            // 检查免疫
            if (target.IsImmuneTo(buff.DataId))
                return false;
            
            return true;
        }
        
        public IEnumerable<IBuffOwner> GetTransmissionTargets(IBuff buff)
        {
            if (buff.Owner is not MonoBehaviour ownerMono)
                yield break;
            
            Vector3 position = ownerMono.transform.position;
            
            // 检测范围内碰撞体
            Collider[] colliders = Physics.OverlapSphere(position, contactRadius, targetLayers);
            
            foreach (var collider in colliders)
            {
                var targetOwner = collider.GetComponent<IBuffOwner>();
                if (targetOwner == null || targetOwner == buff.Owner)
                    continue;
                
                // 阵营检查
                if (onlyAffectEnemies && !IsEnemy(buff.Owner, targetOwner))
                    continue;
                
                yield return targetOwner;
            }
        }
        
        public void OnTransmit(IBuff buff, IBuffOwner from, IBuffOwner to)
        {
            lastTransmissionTime = Time.time;
            
            // 传播Buff
            to.BuffContainer.AddBuff(buff.Data, from);
            
            // 触发事件
            TransmissionEventSystem.TriggerTransmitted(buff, from, to, Mode);
            
            if (BuffSystemConfig.Instance.EnableDebugLog)
            {
                Debug.Log($"[ContactTransmission] Buff {buff.Name} 从 {from.OwnerName} 传播到 {to.OwnerName}");
            }
        }
        
        private bool IsEnemy(IBuffOwner a, IBuffOwner b)
        {
            // 通过事件系统让游戏端判断阵营
            return TransmissionEventSystem.CheckIsEnemy(a, b);
        }
    }
}
