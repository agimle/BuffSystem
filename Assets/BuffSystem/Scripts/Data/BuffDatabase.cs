using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;

namespace BuffSystem.Data
{
    /// <summary>
    /// Buff数据库 - 管理所有Buff配置的加载和查询
    /// 饿汉单例模式，线程安全
    /// </summary>
    public class BuffDatabase
    {
        private static readonly BuffDatabase _instance = new();
        public static BuffDatabase Instance => _instance;

        private readonly Dictionary<int, IBuffData> _idToData = new();
        private readonly Dictionary<string, int> _nameToId = new();
        private bool _isInitialized = false;

        private BuffDatabase()
        {
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            LoadAllBuffData();
            _isInitialized = true;

            Debug.Log($"[BuffDatabase] 初始化完成，加载了 {_idToData.Count} 个Buff配置");
        }

        /// <summary>
        /// 重新加载数据
        /// </summary>
        public void Reload()
        {
            _idToData.Clear();
            _nameToId.Clear();
            _isInitialized = false;
            Initialize();
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
        /// 注册Buff数据
        /// </summary>
        private void RegisterBuffData(IBuffData data)
        {
            if (data == null) return;

            if (_idToData.ContainsKey(data.Id))
            {
                Debug.LogWarning($"[BuffDatabase] Buff ID重复: {data.Id} - {data.Name}");
                return;
            }

            _idToData[data.Id] = data;

            if (!string.IsNullOrEmpty(data.Name))
            {
                if (_nameToId.ContainsKey(data.Name))
                {
                    Debug.LogWarning($"[BuffDatabase] Buff名称重复: {data.Name}");
                }
                else
                {
                    _nameToId[data.Name] = data.Id;
                }
            }
        }

        /// <summary>
        /// 根据ID获取Buff数据
        /// </summary>
        public IBuffData GetBuffData(int id)
        {
            EnsureInitialized();
            _idToData.TryGetValue(id, out var data);
            return data;
        }

        /// <summary>
        /// 根据名称获取Buff数据
        /// </summary>
        public IBuffData GetBuffData(string name)
        {
            EnsureInitialized();
            if (_nameToId.TryGetValue(name, out int id))
            {
                return GetBuffData(id);
            }

            return null;
        }

        /// <summary>
        /// 根据名称获取Buff ID
        /// </summary>
        public int GetBuffId(string name)
        {
            EnsureInitialized();
            return _nameToId.TryGetValue(name, out int id) ? id : -1;
        }

        /// <summary>
        /// 是否存在指定Buff
        /// </summary>
        public bool ContainsBuff(int id)
        {
            EnsureInitialized();
            return _idToData.ContainsKey(id);
        }

        /// <summary>
        /// 是否存在指定Buff
        /// </summary>
        public bool ContainsBuff(string name)
        {
            EnsureInitialized();
            return _nameToId.ContainsKey(name);
        }

        /// <summary>
        /// 获取所有Buff数据
        /// </summary>
        public IEnumerable<IBuffData> GetAllBuffData()
        {
            EnsureInitialized();
            return _idToData.Values;
        }

        /// <summary>
        /// 获取Buff数量
        /// </summary>
        public int Count
        {
            get
            {
                EnsureInitialized();
                return _idToData.Count;
            }
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }
    }
}
