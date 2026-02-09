using System.Collections.Generic;
using UnityEngine;

namespace BuffSystem.Data
{
    /// <summary>
    /// Buff数据中心 - 用于集中管理Buff数据资源
    /// </summary>
    [CreateAssetMenu(fileName = "BuffDataCenter", menuName = "BuffSystem/Data Center", order = 0)]
    public class BuffDataCenter : ScriptableObject
    {
        [SerializeField] private List<BuffDataSO> buffDataList = new();
        [SerializeField] private BuffSystemConfig systemConfig;

        public List<BuffDataSO> BuffDataList => buffDataList;
        public BuffSystemConfig SystemConfig => systemConfig;
    }
}
