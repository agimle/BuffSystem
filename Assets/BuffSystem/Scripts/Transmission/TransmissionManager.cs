using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Data;

namespace BuffSystem.Transmission
{
    /// <summary>
    /// 传播管理器 - 管理所有Buff的传播逻辑
    /// </summary>
    public class TransmissionManager : MonoBehaviour
    {
        private static TransmissionManager instance;
        public static TransmissionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("TransmissionManager");
                    instance = go.AddComponent<TransmissionManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }
        
        // 待处理的传播请求队列
        private Queue<TransmissionRequest> transmissionQueue = new();
        
        private void Update()
        {
            ProcessTransmissionQueue();
        }
        
        /// <summary>
        /// 请求传播检查
        /// </summary>
        public void RequestTransmission(IBuff buff)
        {
            if (buff.Data.CreateLogic() is not IBuffTransmissible transmissible)
                return;
            
            transmissionQueue.Enqueue(new TransmissionRequest
            {
                Buff = buff,
                Transmissible = transmissible
            });
        }
        
        private void ProcessTransmissionQueue()
        {
            int processCount = 0;
            int maxPerFrame = BuffSystemConfig.Instance.TransmissionMaxPerFrame;
            
            while (transmissionQueue.Count > 0 && processCount < maxPerFrame)
            {
                var request = transmissionQueue.Dequeue();
                ProcessTransmission(request);
                processCount++;
            }
        }
        
        private void ProcessTransmission(TransmissionRequest request)
        {
            var buff = request.Buff;
            var transmissible = request.Transmissible;
            
            foreach (var target in transmissible.GetTransmissionTargets(buff))
            {
                if (transmissible.CanTransmit(buff, target))
                {
                    transmissible.OnTransmit(buff, buff.Owner, target);
                    
                    // 链式传播需要继续传播新Buff
                    if (transmissible.Mode == TransmissionMode.Chain && 
                        transmissible.CurrentChainLength < transmissible.MaxTransmissionChain)
                    {
                        var newBuff = target.BuffContainer.GetBuff(buff.DataId);
                        if (newBuff != null)
                        {
                            RequestTransmission(newBuff);
                        }
                    }
                }
            }
        }
        
        private class TransmissionRequest
        {
            public IBuff Buff;
            public IBuffTransmissible Transmissible;
        }
    }
}
