using System;
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Events;

namespace BuffSystem.Effects.Core
{
    /// <summary>
    /// 触发事件Effect - 核心解耦机制
    /// 只触发事件，不执行任何数值逻辑
    /// </summary>
    [Serializable]
    public class TriggerEventEffect : EffectBase
    {
        [Tooltip("事件名称")]
        [SerializeField] private string eventName = "BuffEvent";

        [Tooltip("字符串参数")]
        [SerializeField] private string stringParam;

        [Tooltip("数值参数")]
        [SerializeField] private float floatParam;

        [Tooltip("整数参数")]
        [SerializeField] private int intParam;

        [Tooltip("布尔参数")]
        [SerializeField] private bool boolParam;

        public override void Execute(IBuff buff)
        {
            var eventData = new BuffEventData
            {
                Buff = buff,
                EventName = eventName,
                StringParam = stringParam,
                FloatParam = floatParam,
                IntParam = intParam,
                BoolParam = boolParam
            };

            // 触发全局事件
            BuffEventSystem.Trigger($"Buff.{eventName}", eventData);

            // 触发持有者本地事件
            if (buff.Owner is IBuffEventReceiver receiver)
            {
                receiver.OnBuffEvent(buff, eventName, eventData);
            }
        }

        public override void Cancel(IBuff buff)
        {
            var eventData = new BuffEventData
            {
                Buff = buff,
                EventName = $"{eventName}Cancelled"
            };

            BuffEventSystem.Trigger($"Buff.{eventName}Cancelled", eventData);

            // 触发持有者本地取消事件
            if (buff.Owner is IBuffEventReceiver receiver)
            {
                receiver.OnBuffEvent(buff, $"{eventName}Cancelled", eventData);
            }
        }
    }

    /// <summary>
    /// Buff事件数据
    /// </summary>
    public class BuffEventData
    {
        public IBuff Buff { get; set; }
        public string EventName { get; set; }
        public string StringParam { get; set; }
        public float FloatParam { get; set; }
        public int IntParam { get; set; }
        public bool BoolParam { get; set; }
    }
}
