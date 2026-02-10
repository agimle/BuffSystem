using System.Collections.Generic;

namespace BuffSystem.Core
{
    /// <summary>
    /// Buff可序列化接口 - 支持自定义存档数据
    /// </summary>
    public interface IBuffSerializable
    {
        /// <summary>
        /// 序列化为存档数据
        /// </summary>
        /// <param name="saveData">存档数据对象</param>
        void Serialize(BuffSaveData saveData);
        
        /// <summary>
        /// 从存档数据反序列化
        /// </summary>
        /// <param name="saveData">存档数据对象</param>
        void Deserialize(BuffSaveData saveData);
    }
    
    /// <summary>
    /// Buff存档数据
    /// </summary>
    public class BuffSaveData
    {
        /// <summary>
        /// Buff ID
        /// </summary>
        public int BuffId;
        
        /// <summary>
        /// 当前层数
        /// </summary>
        public int CurrentStack;
        
        /// <summary>
        /// 已持续时间（用于计算剩余时间）
        /// </summary>
        public float ElapsedDuration;
        
        /// <summary>
        /// 来源对象ID（需要外部映射）
        /// </summary>
        public int SourceId;
        
        /// <summary>
        /// 自定义数据字典（用于存储BuffLogic的自定义状态）
        /// </summary>
        public Dictionary<string, string> CustomData;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public BuffSaveData()
        {
            CustomData = new Dictionary<string, string>();
        }
    }
    
    /// <summary>
    /// Buff持有者存档数据
    /// </summary>
    public class BuffOwnerSaveData
    {
        /// <summary>
        /// 持有者ID
        /// </summary>
        public int OwnerId;
        
        /// <summary>
        /// Buff列表
        /// </summary>
        public List<BuffSaveData> Buffs;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public BuffOwnerSaveData()
        {
            Buffs = new List<BuffSaveData>();
        }
    }
}
