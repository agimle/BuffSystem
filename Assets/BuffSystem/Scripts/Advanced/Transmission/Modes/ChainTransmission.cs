using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BuffSystem.Core;

namespace BuffSystem.Advanced.Transmission
{
    /// <summary>
    /// 链式传播 - 像闪电链一样在目标间跳跃
    /// </summary>
    [Serializable]
    public class ChainTransmission : IBuffTransmissible
    {
        [Tooltip("最大跳跃次数")]
        [SerializeField]
        private int maxJumps = 3;
        
        [Tooltip("每次跳跃的范围")]
        [SerializeField]
        private float jumpRange = 5f;
        
        [Tooltip("每次跳跃的衰减比例")]
        [SerializeField, Range(0f, 1f)]
        private float decayPerJump = 0.7f;
        
        [Tooltip("目标层")]
        [SerializeField]
        private LayerMask targetLayers = ~0;
        
        [Tooltip("是否优先选择最近目标")]
        [SerializeField]
        private bool preferNearest = true;
        
        public TransmissionMode Mode => TransmissionMode.Chain;
        
        public int MaxTransmissionChain => maxJumps;
        
        public int CurrentChainLength { get; set; }
        
        public bool CanTransmit(IBuff buff, IBuffOwner target)
        {
            if (CurrentChainLength >= maxJumps)
                return false;
            
            if (target.IsImmuneTo(buff.DataId))
                return false;
            
            return true;
        }
        
        public IEnumerable<IBuffOwner> GetTransmissionTargets(IBuff buff)
        {
            if (buff.Owner is not MonoBehaviour ownerMono)
                yield break;
            
            Vector3 position = ownerMono.transform.position;
            
            // 查找范围内所有有效目标
            var targets = Physics.OverlapSphere(position, jumpRange, targetLayers)
                .Select(c => c.GetComponent<IBuffOwner>())
                .Where(o => o != null && o != buff.Owner)
                .Where(o => !o.IsImmuneTo(buff.DataId))
                .ToList();
            
            if (preferNearest)
            {
                targets = targets
                    .OrderBy(t => Vector3.Distance(position, (t as MonoBehaviour).transform.position))
                    .ToList();
            }
            
            // 只返回第一个目标（链式传播一次一个）
            if (targets.Count > 0)
            {
                yield return targets[0];
            }
        }
        
        public void OnTransmit(IBuff buff, IBuffOwner from, IBuffOwner to)
        {
            CurrentChainLength++;
            
            // 应用衰减
            float decayMultiplier = Mathf.Pow(decayPerJump, CurrentChainLength);
            
            // 添加Buff（带衰减参数）
            var newBuff = to.BuffContainer.AddBuff(buff.Data, from);
            if (newBuff != null)
            {
                // 通过事件传递衰减信息
                var eventData = new ChainTransmissionEventData
                {
                    Buff = newBuff,
                    ChainLength = CurrentChainLength,
                    DecayMultiplier = decayMultiplier
                };
                TransmissionEventSystem.TriggerChainJumped(eventData);
            }
            
            TransmissionEventSystem.TriggerTransmitted(buff, from, to, Mode);
        }
    }
    
    /// <summary>
    /// 链式传播事件数据
    /// </summary>
    public class ChainTransmissionEventData
    {
        public IBuff Buff { get; set; }
        public int ChainLength { get; set; }
        public float DecayMultiplier { get; set; }
    }
}
