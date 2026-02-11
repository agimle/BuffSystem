using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using BuffSystem.Core;

namespace BuffSystem.Data
{
    /// <summary>
    /// Buff数据库 - 管理所有Buff配置的加载和查询
    /// 饿汉单例模式，线程安全，读取无锁
    /// v4.0优化：使用ReadOnlyDictionary实现无锁读取
    /// </summary>
    public class BuffDatabase
    {
        private static readonly BuffDatabase instance = new();
        public static BuffDatabase Instance => instance;

        // 内部可变字典，仅在初始化/Reload时修改
        private Dictionary<int, IBuffData> idToData = new();
        private Dictionary<string, int> nameToId = new();
        
        // 对外暴露的只读字典，读取无需lock
        private IReadOnlyDictionary<int, IBuffData> readonlyIdToData;
        private IReadOnlyDictionary<string, int> readonlyNameToId;
        
        private bool isInitialized;

        // 线程安全锁 - 仅用于初始化和Reload
        private readonly object initLock = new();

        private BuffDatabase()
        {
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;

            lock (initLock)
            {
                if (isInitialized) return;

                LoadAllBuffData();
                
                // 构建完成后转为只读，后续读取无需lock
                readonlyIdToData = new ReadOnlyDictionary<int, IBuffData>(idToData);
                readonlyNameToId = new ReadOnlyDictionary<string, int>(nameToId);
                
                isInitialized = true;

                Debug.Log($"[BuffDatabase] 初始化完成，加载了 {idToData.Count} 个Buff配置");
            }
        }

        /// <summary>
        /// 重新加载数据 - 线程安全
        /// </summary>
        public void Reload()
        {
            lock (initLock)
            {
                // 创建新的字典实例，避免修改正在使用的字典
                var newIdToData = new Dictionary<int, IBuffData>();
                var newNameToId = new Dictionary<string, int>();

                // 从Resources加载
                BuffDataSO[] buffDataArray = Resources.LoadAll<BuffDataSO>("BuffSystem/BuffData");
                foreach (var data in buffDataArray)
                {
                    RegisterBuffDataToDictionary(data, newIdToData, newNameToId);
                }

                // 通过BuffDataCenter加载
                BuffDataCenter center = Resources.Load<BuffDataCenter>("BuffSystem/BuffDataCenter");
                if (center != null)
                {
                    foreach (var data in center.BuffDataList)
                    {
                        RegisterBuffDataToDictionary(data, newIdToData, newNameToId);
                    }
                }

                // 原子性替换引用 - 此时其他线程仍然可以安全读取旧字典
                idToData = newIdToData;
                nameToId = newNameToId;
                readonlyIdToData = new ReadOnlyDictionary<int, IBuffData>(idToData);
                readonlyNameToId = new ReadOnlyDictionary<string, int>(nameToId);

                Debug.Log($"[BuffDatabase] 重新加载完成，当前有 {idToData.Count} 个Buff配置");
            }
        }

        /// <summary>
        /// 加载所有Buff数据
        /// </summary>
        private void LoadAllBuffData()
        {
            // 从Resources加载
            BuffDataSO[] buffDataArray = Resources.LoadAll<BuffDataSO>("BuffSystem/BuffData");
            foreach (var data in buffDataArray)
            {
                RegisterBuffData(data);
            }

            // 也可以通过BuffDataCenter加载
            BuffDataCenter center = Resources.Load<BuffDataCenter>("BuffSystem/BuffDataCenter");
            if (center != null)
            {
                foreach (var data in center.BuffDataList)
                {
                    RegisterBuffData(data);
                }
            }
        }

        /// <summary>
        /// 注册Buff数据到指定字典
        /// </summary>
        private void RegisterBuffDataToDictionary(
            IBuffData data, 
            Dictionary<int, IBuffData> targetIdDict, 
            Dictionary<string, int> targetNameDict)
        {
            if (data == null) return;

            if (targetIdDict.ContainsKey(data.Id))
            {
                Debug.LogWarning($"[BuffDatabase] Buff ID重复: {data.Id} - {data.Name}");
                return;
            }

            targetIdDict[data.Id] = data;

            if (!string.IsNullOrEmpty(data.Name))
            {
                if (targetNameDict.ContainsKey(data.Name))
                {
                    Debug.LogWarning($"[BuffDatabase] Buff名称重复: {data.Name}");
                }
                else
                {
                    targetNameDict[data.Name] = data.Id;
                }
            }
        }

        /// <summary>
        /// 注册Buff数据
        /// </summary>
        private void RegisterBuffData(IBuffData data)
        {
            RegisterBuffDataToDictionary(data, idToData, nameToId);
        }

        /// <summary>
        /// 根据ID获取Buff数据 - 无锁读取
        /// </summary>
        public IBuffData GetBuffData(int id)
        {
            EnsureInitialized();
            // 使用只读字典，无需lock
            readonlyIdToData.TryGetValue(id, out var data);
            return data;
        }

        /// <summary>
        /// 根据名称获取Buff数据 - 无锁读取
        /// </summary>
        public IBuffData GetBuffData(string name)
        {
            EnsureInitialized();
            // 使用只读字典，无需lock
            if (readonlyNameToId.TryGetValue(name, out int id))
            {
                return GetBuffData(id);
            }
            return null;
        }

        /// <summary>
        /// 根据名称获取Buff ID - 无锁读取
        /// </summary>
        public int GetBuffId(string name)
        {
            EnsureInitialized();
            // 使用只读字典，无需lock
            return readonlyNameToId.TryGetValue(name, out int id) ? id : -1;
        }

        /// <summary>
        /// 是否存在指定Buff - 无锁读取
        /// </summary>
        public bool ContainsBuff(int id)
        {
            EnsureInitialized();
            // 使用只读字典，无需lock
            return readonlyIdToData.ContainsKey(id);
        }

        /// <summary>
        /// 是否存在指定Buff - 无锁读取
        /// </summary>
        public bool ContainsBuff(string name)
        {
            EnsureInitialized();
            // 使用只读字典，无需lock
            return readonlyNameToId.ContainsKey(name);
        }

        /// <summary>
        /// 获取所有Buff数据 - 无锁读取
        /// </summary>
        public IEnumerable<IBuffData> GetAllBuffData()
        {
            EnsureInitialized();
            // 使用只读字典的Values，无需lock
            return readonlyIdToData.Values;
        }

        /// <summary>
        /// 获取Buff数量 - 无锁读取
        /// </summary>
        public int Count
        {
            get
            {
                EnsureInitialized();
                // 使用只读字典，无需lock
                return readonlyIdToData.Count;
            }
        }

        /// <summary>
        /// 获取所有Buff ID - 无锁读取
        /// </summary>
        public IEnumerable<int> GetAllBuffIds()
        {
            EnsureInitialized();
            return readonlyIdToData.Keys;
        }

        /// <summary>
        /// 获取所有Buff名称 - 无锁读取
        /// </summary>
        public IEnumerable<string> GetAllBuffNames()
        {
            EnsureInitialized();
            return readonlyNameToId.Keys;
        }

        private void EnsureInitialized()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }
    }
}
