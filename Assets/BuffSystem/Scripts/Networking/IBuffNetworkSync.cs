using BuffSystem.Core;

namespace BuffSystem.Networking
{
    /// <summary>
    /// Buff网络同步接口
    /// 定义网络同步组件的基本功能
    /// </summary>
    public interface IBuffNetworkSync
    {
        /// <summary>
        /// 是否已连接到网络
        /// </summary>
        bool IsConnected { get; }
        
        /// <summary>
        /// 是否是服务器/主机
        /// </summary>
        bool IsServer { get; }
        
        /// <summary>
        /// 是否是客户端
        /// </summary>
        bool IsClient { get; }
        
        /// <summary>
        /// 是否拥有该对象的权限（控制权限）
        /// </summary>
        bool HasAuthority { get; }
        
        /// <summary>
        /// 请求添加Buff（客户端调用，服务器执行）
        /// </summary>
        /// <param name="buffId">Buff配置ID</param>
        void RequestAddBuff(int buffId);
        
        /// <summary>
        /// 请求移除Buff（客户端调用，服务器执行）
        /// </summary>
        /// <param name="buffId">Buff配置ID</param>
        void RequestRemoveBuff(int buffId);
    }
    
    /// <summary>
    /// Buff同步模式
    /// </summary>
    public enum BuffSyncMode
    {
        /// <summary>
        /// 服务器权威模式（推荐）
        /// 所有Buff变更由服务器控制，客户端只同步状态
        /// </summary>
        ServerAuthority,
        
        /// <summary>
        /// 客户端预测模式
        /// 客户端立即响应，服务器验证并纠正
        /// </summary>
        ClientPrediction,
        
        /// <summary>
        /// 主机模式
        /// 主机玩家拥有权威
        /// </summary>
        HostAuthority
    }
    
    /// <summary>
    /// 网络同步配置
    /// </summary>
    public class BuffNetworkSyncConfig
    {
        /// <summary>
        /// 同步模式
        /// </summary>
        public BuffSyncMode SyncMode = BuffSyncMode.ServerAuthority;
        
        /// <summary>
        /// 同步间隔（秒）
        /// 0表示实时同步（事件驱动）
        /// </summary>
        public float SyncInterval = 0f;
        
        /// <summary>
        /// 是否同步持续时间
        /// </summary>
        public bool SyncDuration = true;
        
        /// <summary>
        /// 是否同步层数
        /// </summary>
        public bool SyncStack = true;
        
        /// <summary>
        /// 是否启用客户端预测
        /// </summary>
        public bool EnableClientPrediction = false;
        
        /// <summary>
        /// 预测容错时间（秒）
        /// 服务器状态与客户端预测差异在此范围内不纠正
        /// </summary>
        public float PredictionTolerance = 0.1f;
    }
}
