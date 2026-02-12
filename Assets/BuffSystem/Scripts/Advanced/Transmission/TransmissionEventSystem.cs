using System;
using BuffSystem.Core;

namespace BuffSystem.Advanced.Transmission
{
    /// <summary>
    /// 传播事件系统 - 处理Buff传播相关事件
    /// </summary>
    public static class TransmissionEventSystem
    {
        #region Events
        
        /// <summary>
        /// Buff传播事件
        /// </summary>
        public static event Action<IBuff, IBuffOwner, IBuffOwner, TransmissionMode> OnTransmitted;
        
        /// <summary>
        /// 链式传播跳跃事件
        /// </summary>
        public static event Action<ChainTransmissionEventData> OnChainJumped;
        
        /// <summary>
        /// 阵营检查委托 - 由游戏端实现
        /// </summary>
        public static Func<IBuffOwner, IBuffOwner, bool> IsEnemyChecker { get; set; }
        
        #endregion
        
        #region Trigger Methods
        
        /// <summary>
        /// 触发传播事件
        /// </summary>
        public static void TriggerTransmitted(IBuff buff, IBuffOwner from, IBuffOwner to, TransmissionMode mode)
        {
            OnTransmitted?.Invoke(buff, from, to, mode);
        }
        
        /// <summary>
        /// 触发链式跳跃事件
        /// </summary>
        public static void TriggerChainJumped(ChainTransmissionEventData eventData)
        {
            OnChainJumped?.Invoke(eventData);
        }
        
        /// <summary>
        /// 检查是否为敌人
        /// </summary>
        public static bool CheckIsEnemy(IBuffOwner a, IBuffOwner b)
        {
            if (IsEnemyChecker == null)
            {
                // 默认实现：不同对象视为敌人
                return a != b;
            }
            return IsEnemyChecker(a, b);
        }
        
        #endregion
    }
}
