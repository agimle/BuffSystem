using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Data;

namespace BuffSystem.Runtime
{
    /// <summary>
    /// Buff存档管理器 - 管理所有Buff持有者的存档和加载
    /// </summary>
    public static class BuffSaveManager
    {
        #region Save

        /// <summary>
        /// 保存所有Buff持有者的状态
        /// </summary>
        /// <returns>所有持有者的存档数据列表</returns>
        public static List<BuffOwnerSaveData> SaveAll()
        {
            var result = new List<BuffOwnerSaveData>();

            foreach (var owner in BuffOwner.AllOwners)
            {
                if (owner != null)
                {
                    var ownerSaveData = SaveOwner(owner);
                    if (ownerSaveData.Buffs.Count > 0)
                    {
                        result.Add(ownerSaveData);
                    }
                }
            }

            if (BuffSystemConfig.Instance.EnableDebugLog)
            {
                Debug.Log($"[BuffSaveManager] 保存了 {result.Count} 个持有者的Buff数据");
            }

            return result;
        }

        /// <summary>
        /// 保存单个持有者的Buff状态
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <returns>持有者的存档数据</returns>
        public static BuffOwnerSaveData SaveOwner(IBuffOwner owner)
        {
            var saveData = new BuffOwnerSaveData { OwnerId = owner.OwnerId };

            if (owner.BuffContainer == null)
            {
                return saveData;
            }

            foreach (var buff in owner.BuffContainer.AllBuffs)
            {
                var buffSaveData = SaveBuff(buff);
                if (buffSaveData != null)
                {
                    saveData.Buffs.Add(buffSaveData);
                }
            }

            return saveData;
        }

        /// <summary>
        /// 保存单个Buff的状态
        /// </summary>
        /// <param name="buff">Buff实例</param>
        /// <returns>Buff存档数据</returns>
        public static BuffSaveData SaveBuff(IBuff buff)
        {
            if (buff == null || buff.Data == null)
            {
                return null;
            }

            var saveData = new BuffSaveData
            {
                BuffId = buff.DataId,
                CurrentStack = buff.CurrentStack,
                ElapsedDuration = buff.Duration,
                SourceId = buff.Source != null ? buff.Source.GetHashCode() : 0
            };

            // 序列化自定义数据
            if (buff.Data.CreateLogic() is IBuffSerializable serializable)
            {
                serializable.Serialize(saveData);
            }

            return saveData;
        }

        #endregion

        #region Load

        /// <summary>
        /// 加载Buff状态到持有者
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <param name="saveData">存档数据</param>
        /// <param name="sourceResolver">来源对象解析器（用于恢复Source引用）</param>
        public static void LoadOwner(IBuffOwner owner, BuffOwnerSaveData saveData, System.Func<int, object> sourceResolver = null)
        {
            if (owner == null || saveData == null)
            {
                return;
            }

            // 先清空现有Buff
            owner.BuffContainer?.ClearAllBuffs();

            foreach (var buffData in saveData.Buffs)
            {
                LoadBuff(owner, buffData, sourceResolver);
            }

            if (BuffSystemConfig.Instance.EnableDebugLog)
            {
                Debug.Log($"[BuffSaveManager] 加载了 {saveData.Buffs.Count} 个Buff到持有者 {owner.OwnerName}");
            }
        }

        /// <summary>
        /// 加载单个Buff到持有者
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        /// <param name="saveData">Buff存档数据</param>
        /// <param name="sourceResolver">来源对象解析器</param>
        /// <returns>加载的Buff实例</returns>
        public static IBuff LoadBuff(IBuffOwner owner, BuffSaveData saveData, System.Func<int, object> sourceResolver = null)
        {
            if (owner?.BuffContainer == null || saveData == null)
            {
                return null;
            }

            var data = BuffDatabase.Instance.GetBuffData(saveData.BuffId);
            if (data == null)
            {
                Debug.LogWarning($"[BuffSaveManager] 加载Buff失败，找不到ID为 {saveData.BuffId} 的Buff数据");
                return null;
            }

            // 解析Source
            object source = sourceResolver?.Invoke(saveData.SourceId);

            // 创建Buff
            var buff = owner.BuffContainer.AddBuff(data, source);
            if (buff == null)
            {
                Debug.LogWarning($"[BuffSaveManager] 加载Buff {data.Name} 失败，AddBuff返回null");
                return null;
            }

            // 恢复状态
            if (buff is BuffEntity entity)
            {
                entity.RestoreState(saveData);
            }

            // 反序列化自定义数据
            if (data.CreateLogic() is IBuffSerializable serializable)
            {
                serializable.Deserialize(saveData);
            }

            return buff;
        }

        #endregion

        #region Utility

        /// <summary>
        /// 从JSON字符串加载所有持有者数据
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns>持有者存档数据列表</returns>
        public static List<BuffOwnerSaveData> LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new List<BuffOwnerSaveData>();
            }

            try
            {
                var wrapper = JsonUtility.FromJson<BuffSaveDataWrapper>(json);
                return wrapper?.Owners ?? new List<BuffOwnerSaveData>();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BuffSaveManager] 从JSON加载失败: {e.Message}");
                return new List<BuffOwnerSaveData>();
            }
        }

        /// <summary>
        /// 保存所有持有者数据到JSON字符串
        /// </summary>
        /// <param name="ownersData">持有者存档数据列表</param>
        /// <returns>JSON字符串</returns>
        public static string SaveToJson(List<BuffOwnerSaveData> ownersData)
        {
            if (ownersData == null)
            {
                return "{}";
            }

            try
            {
                var wrapper = new BuffSaveDataWrapper { Owners = ownersData };
                return JsonUtility.ToJson(wrapper, true);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BuffSaveManager] 保存到JSON失败: {e.Message}");
                return "{}";
            }
        }

        #endregion

        #region Inner Classes

        /// <summary>
        /// 用于JSON序列化的包装类
        /// </summary>
        [System.Serializable]
        private class BuffSaveDataWrapper
        {
            public List<BuffOwnerSaveData> Owners;
        }

        #endregion
    }
}
