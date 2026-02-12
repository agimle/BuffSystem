using System.Collections.Generic;
using BuffSystem.Core;

namespace BuffSystem.Transmission
{
    /// <summary>
    /// Buff传播接口 - 实现此接口的Buff可以在单位间传播
    /// </summary>
    public interface IBuffTransmissible
    {
        /// <summary>
        /// 传播模式
        /// </summary>
        TransmissionMode Mode { get; }
        
        /// <summary>
        /// 是否可以传播给目标
        /// </summary>
        bool CanTransmit(IBuff buff, IBuffOwner target);
        
        /// <summary>
        /// 获取传播目标
        /// </summary>
        IEnumerable<IBuffOwner> GetTransmissionTargets(IBuff buff);
        
        /// <summary>
        /// 传播发生时
        /// </summary>
        void OnTransmit(IBuff buff, IBuffOwner from, IBuffOwner to);
        
        /// <summary>
        /// 最大传播链长度
        /// </summary>
        int MaxTransmissionChain { get; }
        
        /// <summary>
        /// 当前传播链长度
        /// </summary>
        int CurrentChainLength { get; set; }
    }
    
    /// <summary>
    /// 传播模式
    /// </summary>
    public enum TransmissionMode
    {
        /// <summary>
        /// 接触传播 - 与感染者接触时传播
        /// </summary>
        Contact,
        
        /// <summary>
        /// 范围传播 - 范围内所有目标
        /// </summary>
        Range,
        
        /// <summary>
        /// 链式传播 - 从一个目标跳到另一个
        /// </summary>
        Chain,
        
        /// <summary>
        /// 继承传播 - 原持有者死亡时转移
        /// </summary>
        Inheritance
    }
}
