using System.Collections.Generic;
using UnityEngine;
using BuffSystem.Core;

namespace BuffSystem.Advanced.Area
{
    /// <summary>
    /// Buff区域管理器 - 管理所有BuffArea实例
    /// v4.0新增
    /// </summary>
    public class BuffAreaManager : MonoBehaviour
    {
        private static BuffAreaManager instance;
        public static BuffAreaManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<BuffAreaManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("BuffAreaManager");
                        instance = go.AddComponent<BuffAreaManager>();
                    }
                }
                return instance;
            }
        }

        // 所有注册的区域
        private readonly List<BuffArea> areas = new();

        #region Properties

        /// <summary>
        /// 当前区域数量
        /// </summary>
        public int AreaCount => areas.Count;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Registration

        /// <summary>
        /// 注册区域
        /// </summary>
        internal void RegisterArea(BuffArea area)
        {
            if (area != null && !areas.Contains(area))
            {
                areas.Add(area);
            }
        }

        /// <summary>
        /// 注销区域
        /// </summary>
        internal void UnregisterArea(BuffArea area)
        {
            areas.Remove(area);
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// 获取所有区域
        /// </summary>
        public IReadOnlyList<BuffArea> GetAllAreas()
        {
            return areas;
        }

        /// <summary>
        /// 获取指定Buff ID的所有区域
        /// </summary>
        public IEnumerable<BuffArea> GetAreasByBuffId(int buffId)
        {
            foreach (var area in areas)
            {
                if (area.BuffId == buffId)
                {
                    yield return area;
                }
            }
        }

        /// <summary>
        /// 获取包含指定单位的所有区域
        /// </summary>
        public IEnumerable<BuffArea> GetAreasContainingOwner(IBuffOwner owner)
        {
            foreach (var area in areas)
            {
                if (area.ContainsOwner(owner))
                {
                    yield return area;
                }
            }
        }

        /// <summary>
        /// 检查单位是否在任意区域内
        /// </summary>
        public bool IsOwnerInAnyArea(IBuffOwner owner)
        {
            foreach (var area in areas)
            {
                if (area.ContainsOwner(owner))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取单位所在的所有区域
        /// </summary>
        public List<BuffArea> GetOwnerAreas(IBuffOwner owner, List<BuffArea> result = null)
        {
            result ??= new List<BuffArea>();
            result.Clear();

            foreach (var area in areas)
            {
                if (area.ContainsOwner(owner))
                {
                    result.Add(area);
                }
            }

            return result;
        }

        #endregion

        #region Global Operations

        /// <summary>
        /// 刷新所有区域
        /// </summary>
        public void RefreshAllAreas()
        {
            foreach (var area in areas)
            {
                if (area != null)
                {
                    area.ForceRefresh();
                }
            }
        }

        /// <summary>
        /// 清除所有区域
        /// </summary>
        public void ClearAllAreas()
        {
            foreach (var area in areas)
            {
                if (area != null)
                {
                    area.ClearAllEntries();
                }
            }
        }

        /// <summary>
        /// 销毁所有区域
        /// </summary>
        public void DestroyAllAreas()
        {
            for (int i = areas.Count - 1; i >= 0; i--)
            {
                if (areas[i] != null)
                {
                    Destroy(areas[i].gameObject);
                }
            }
            areas.Clear();
        }

        #endregion
    }
}
