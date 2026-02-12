using System.Collections.Generic;
using UnityEngine;

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
    /// Buff存档数据 - 支持版本管理
    /// </summary>
    public class BuffSaveData
    {
        /// <summary>
        /// 存档格式版本
        /// </summary>
        public int Version = CurrentVersion;
        
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
        /// 当前系统版本
        /// </summary>
        public const int CurrentVersion = 1;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public BuffSaveData()
        {
            CustomData = new Dictionary<string, string>();
        }
        
        /// <summary>
        /// 迁移到最新版本
        /// </summary>
        public void MigrateToLatest()
        {
            while (Version < CurrentVersion)
            {
                MigrateFromVersion(Version);
                Version++;
            }
        }
        
        /// <summary>
        /// 从指定版本迁移
        /// </summary>
        private void MigrateFromVersion(int fromVersion)
        {
            switch (fromVersion)
            {
                // 未来版本添加更多case
                // case 1:
                //     MigrateV1ToV2();
                //     break;
                default:
                    Debug.LogWarning($"[BuffSaveData] 未知的迁移版本: {fromVersion}");
                    break;
            }
        }
        
        /// <summary>
        /// v1 -> v2 迁移示例
        /// </summary>
        private void MigrateV1ToV2()
        {
            // 字段重命名示例
            // if (CustomData.TryGetValue("oldFieldName", out var value))
            // {
            //     CustomData["newFieldName"] = value;
            //     CustomData.Remove("oldFieldName");
            // }
            
            // 数据转换示例
            // if (CustomData.TryGetValue("damageType", out var damageTypeStr))
            // {
            //     if (int.TryParse(damageTypeStr, out var oldValue))
            //     {
            //         var newValue = ConvertDamageType(oldValue);
            //         CustomData["damageType"] = newValue.ToString();
            //     }
            // }
        }
    }
    
    /// <summary>
    /// Buff持有者存档数据
    /// </summary>
    public class BuffOwnerSaveData
    {
        /// <summary>
        /// 系统版本号
        /// </summary>
        public int SystemVersion = BuffSaveData.CurrentVersion;
        
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
        
        /// <summary>
        /// 验证并修复存档数据
        /// </summary>
        public bool Validate()
        {
            // 检查系统版本兼容性
            if (SystemVersion > BuffSaveData.CurrentVersion)
            {
                Debug.LogError($"[BuffSaveData] 存档版本 {SystemVersion} 高于当前系统版本 {BuffSaveData.CurrentVersion}");
                return false;
            }
            
            // 迁移所有Buff数据
            foreach (var buff in Buffs)
            {
                buff.MigrateToLatest();
            }
            
            return true;
        }
    }
}
