# Combo系统使用文档

> Buff组合系统 - 实现Buff之间的连携效果和联动机制

---

## 📖 概述

Combo系统允许你定义Buff之间的组合关系，当特定Buff同时存在时触发额外效果。

**典型应用场景:**
- 🔥 火+风 = 火势蔓延（增强伤害）
- 💧 水+雷 = 感电（额外控制效果）
- 🛡️ 护盾+反击 = 荆棘（反弹伤害）
- ⚡ 多重加速 = 超级速度（突破上限）

---

## 🚀 快速开始

### 1. 创建Combo配置

在Project窗口中右键创建:
```
Create > BuffSystem > Buff Combo
```

### 2. 配置Combo

```csharp
[CreateAssetMenu(fileName = "FireWindCombo", menuName = "BuffSystem/Buff Combo")]
public class BuffComboData : ScriptableObject
{
    public int comboId = 1;                    // Combo唯一ID
    public string comboName = "火势蔓延";       // Combo名称
    public List<int> requiredBuffIds = new() { 1001, 1002 };  // 需要火Buff(1001)和风Buff(1002)
    public ComboTriggerMode triggerMode = ComboTriggerMode.Default;
    public List<ComboEffect> effects = new();  // Combo效果列表
    public int triggerBuffId = 0;              // 触发的新Buff（可选）
}
```

### 3. 注册Combo

```csharp
using BuffSystem.Advanced.Combo;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BuffComboData fireWindCombo;
    
    void Start()
    {
        // 注册Combo
        BuffSystemManager.Combo.RegisterCombo(fireWindCombo);
    }
}
```

### 4. 完成！

当玩家同时拥有火Buff和风Buff时，Combo会自动触发。

---

## 📚 核心概念

### Combo触发条件

#### RequireAll（全部满足）

```csharp
// 需要同时拥有Buff 1001 和 1002
requiredBuffIds = new List<int> { 1001, 1002 };
requireAll = true;  // 默认
```

#### RequireAny（任一满足）

```csharp
// 拥有Buff 1001 或 1002 任一即可
requiredBuffIds = new List<int> { 1001, 1002 };
requireAll = false;
```

### 触发模式 (ComboTriggerMode)

| 模式 | 说明 | 使用场景 |
|------|------|---------|
| `OnBuffAdd` | 当Buff添加时检查 | 大多数Combo |
| `OnBuffRemove` | 当Buff移除时检查 | 解除Combo |
| `OnUpdate` | 每帧检查 | 动态条件Combo |
| `Default` | OnBuffAdd + 全部满足 | 默认推荐 |

```csharp
// 组合模式
triggerMode = ComboTriggerMode.OnBuffAdd | ComboTriggerMode.OnBuffRemove;
```

---

## 🎨 Combo效果类型

### 1. 增强持续时间 (EnhanceDuration)

```csharp
var effect = new ComboEffect
{
    EffectType = ComboEffectType.EnhanceDuration,
    TargetBuffId = 1001,        // 目标Buff
    Value = 50f,                // 增强50%
    UsePercentage = true        // 使用百分比
};
```

### 2. 增强层数效果 (EnhanceStack)

```csharp
var effect = new ComboEffect
{
    EffectType = ComboEffectType.EnhanceStack,
    TargetBuffId = 1001,
    Value = 30f,                // 每层效果增强30%
    UsePercentage = true
};
```

### 3. 减少冷却 (ReduceCooldown)

```csharp
var effect = new ComboEffect
{
    EffectType = ComboEffectType.ReduceCooldown,
    TargetBuffId = 1003,        // 技能Buff
    Value = 20f,                // 减少20%冷却
    UsePercentage = true
};
```

### 4. 触发事件 (TriggerEvent)

```csharp
var effect = new ComboEffect
{
    EffectType = ComboEffectType.TriggerEvent,
    EventName = "FireWindExplosion",  // 自定义事件名
    Value = 100f                       // 事件参数
};

// 监听事件
BuffComboEventSystem.OnComboEffectTriggered += (sender, e) =>
{
    if (e.EventName == "FireWindExplosion")
    {
        // 执行爆炸效果
        CreateExplosion(e.Owner);
    }
};
```

### 5. 修改属性 (ModifyAttribute)

```csharp
var effect = new ComboEffect
{
    EffectType = ComboEffectType.ModifyAttribute,
    EventName = "AttackSpeed",   // 属性名
    Value = 50f                  // 增加50点攻速
};
```

### 6. 添加额外Buff (AddExtraBuff)

```csharp
var effect = new ComboEffect
{
    EffectType = ComboEffectType.AddExtraBuff,
    ExtraBuffId = 2001           // 添加燃烧Buff
};
```

### 7. 移除Buff (RemoveBuff)

```csharp
var effect = new ComboEffect
{
    EffectType = ComboEffectType.RemoveBuff,
    TargetBuffId = 1001,         // 移除火Buff
    TargetType = ComboTargetType.SpecificBuff
};
```

### 8. 刷新持续时间 (RefreshDuration)

```csharp
var effect = new ComboEffect
{
    EffectType = ComboEffectType.RefreshDuration,
    TargetBuffId = 1001
};
```

### 9. 添加/移除层数

```csharp
// 添加层数
var effect = new ComboEffect
{
    EffectType = ComboEffectType.AddStack,
    TargetBuffId = 1001,
    Value = 2                    // 添加2层
};

// 移除层数
var effect = new ComboEffect
{
    EffectType = ComboEffectType.RemoveStack,
    TargetBuffId = 1001,
    Value = 1                    // 移除1层
};
```

---

## 🎯 目标类型 (ComboTargetType)

```csharp
public enum ComboTargetType
{
    SpecificBuff,    // 特定Buff
    AllComboBuffs,   // 所有Combo中的Buff
    Owner,           // 持有者本身
    NewestBuff       // 最新的Buff
}
```

---

## 💡 完整示例

### 示例1: 元素反应系统

```csharp
// 创建火+风 = 火势蔓延 Combo
[CreateAssetMenu(fileName = "FireWindCombo", menuName = "BuffSystem/Combos/FireWind")]
public class FireWindCombo : BuffComboData
{
    void OnEnable()
    {
        comboId = 1001;
        comboName = "火势蔓延";
        description = "火与风结合，火势更加猛烈";
        
        requiredBuffIds = new List<int> { 1001, 1002 };  // 火Buff + 风Buff
        triggerMode = ComboTriggerMode.OnBuffAdd;
        requireAll = true;
        priority = 100;
        
        effects = new List<ComboEffect>
        {
            new ComboEffect
            {
                EffectType = ComboEffectType.EnhanceDuration,
                TargetBuffId = 1001,        // 增强火Buff持续时间
                Value = 100f,               // 延长100%
                UsePercentage = true,
                TargetType = ComboTargetType.SpecificBuff
            },
            new ComboEffect
            {
                EffectType = ComboEffectType.EnhanceStack,
                TargetBuffId = 1001,        // 增强火Buff层数效果
                Value = 50f,                // 增强50%
                UsePercentage = true,
                TargetType = ComboTargetType.SpecificBuff
            },
            new ComboEffect
            {
                EffectType = ComboEffectType.TriggerEvent,
                EventName = "FireSpread",
                Value = 1f
            }
        };
        
        triggerBuffId = 2001;  // 触发"猛烈燃烧"Buff
        onlyTriggerOnce = false;  // 每次满足条件都触发
    }
}
```

### 示例2: 连击系统

```csharp
public class ComboSystemExample : MonoBehaviour
{
    [SerializeField] private BuffComboData threeHitCombo;
    [SerializeField] private BuffComboData fiveHitCombo;
    
    void Start()
    {
        // 注册Combo
        BuffSystemManager.Combo.RegisterCombo(threeHitCombo);
        BuffSystemManager.Combo.RegisterCombo(fiveHitCombo);
        
        // 监听Combo事件
        BuffComboEventSystem.OnComboActivated += OnComboActivated;
        BuffComboEventSystem.OnComboDeactivated += OnComboDeactivated;
    }
    
    void OnComboActivated(object sender, ComboEventArgs e)
    {
        Debug.Log($"Combo激活: {e.Combo.ComboName} on {e.Owner.OwnerName}");
        
        // 播放特效
        PlayComboEffect(e.Combo.ComboId, e.Owner);
    }
    
    void OnComboDeactivated(object sender, ComboEventArgs e)
    {
        Debug.Log($"Combo停用: {e.Combo.ComboName}");
    }
    
    void OnDestroy()
    {
        BuffComboEventSystem.OnComboActivated -= OnComboActivated;
        BuffComboEventSystem.OnComboDeactivated -= OnComboDeactivated;
    }
}
```

### 示例3: 动态检查Combo

```csharp
public class ComboChecker : MonoBehaviour
{
    void Update()
    {
        // 手动检查所有Combo（用于Update模式）
        if (owner != null)
        {
            BuffSystemManager.Combo.CheckAllCombos(owner);
        }
    }
    
    void CheckSpecificCombo()
    {
        // 检查特定Combo是否激活
        bool isActive = BuffSystemManager.Combo.IsComboActive(1001, owner);
        
        // 获取所有激活的Combo
        var activeCombos = BuffSystemManager.Combo.GetActiveCombos(owner);
        foreach (var combo in activeCombos)
        {
            Debug.Log($"激活的Combo: {combo.ComboName}");
        }
        
        // 获取Combo触发次数
        int triggerCount = BuffSystemManager.Combo.GetTriggerCount(owner, 1001);
    }
}
```

---

## 🔧 高级用法

### 优先级系统

当多个Combo可能同时触发时，优先级决定执行顺序：

```csharp
// 高优先级Combo先执行
combo1.Priority = 100;  // 先执行
combo2.Priority = 50;   // 后执行
```

### 只触发一次

```csharp
// Combo激活后不再重复触发
onlyTriggerOnce = true;

// 每次满足条件都触发
onlyTriggerOnce = false;
```

### 自定义触发条件

```csharp
// 继承BuffComboData实现自定义条件
public class CustomComboData : BuffComboData
{
    public override bool CheckCondition(IBuffOwner owner)
    {
        // 基础条件
        if (!base.CheckCondition(owner)) return false;
        
        // 自定义条件：持有者血量低于50%
        if (owner is Player player)
        {
            return player.HealthPercent < 0.5f;
        }
        
        return true;
    }
}
```

---

## 📊 性能优化

### 1. 合理使用触发模式

```csharp
// 推荐：只在Buff添加时检查
triggerMode = ComboTriggerMode.OnBuffAdd;

// 避免：每帧检查（除非必要）
// triggerMode = ComboTriggerMode.OnUpdate;
```

### 2. 控制Combo数量

```csharp
// 及时注销不需要的Combo
BuffSystemManager.Combo.UnregisterCombo(comboData);
```

### 3. 使用优先级

```csharp
// 为高频Combo设置高优先级，减少不必要的检查
highFrequencyCombo.Priority = 1000;
```

---

## 🐛 调试技巧

### 查看激活的Combo

```csharp
void PrintActiveCombos(IBuffOwner owner)
{
    var activeCombos = BuffSystemManager.Combo.GetActiveCombos(owner);
    Debug.Log($"=== {owner.OwnerName} 的激活Combo ===");
    foreach (var combo in activeCombos)
    {
        Debug.Log($"- {combo.ComboName} (ID: {combo.ComboId})");
    }
}
```

### 监听所有Combo事件

```csharp
void Start()
{
    BuffComboEventSystem.OnComboActivated += (sender, e) =>
    {
        Debug.Log($"[Combo] 激活: {e.Combo.ComboName}");
    };
    
    BuffComboEventSystem.OnComboDeactivated += (sender, e) =>
    {
        Debug.Log($"[Combo] 停用: {e.Combo.ComboName}");
    };
    
    BuffComboEventSystem.OnComboEffectTriggered += (sender, e) =>
    {
        Debug.Log($"[Combo] 效果触发: {e.EventName}, 值: {e.Value}");
    };
}
```

---

## 📚 相关文档

- [API参考文档](../API_REFERENCE.md)
- [Fusion系统文档](FusionSystem.md)
- [Transmission系统文档](TransmissionSystem.md)
- [高级特性示例](Examples.md)

---

## 💬 常见问题

**Q: Combo不触发怎么办？**

检查清单：
1. Combo是否已注册？
2. 必需的Buff是否都存在？
3. triggerMode设置是否正确？
4. requireAll设置是否符合预期？

**Q: 多个Combo同时触发，如何控制顺序？**

使用Priority属性，数值越高优先级越高。

**Q: Combo触发后如何取消效果？**

Combo停用时效果会自动取消。可以通过移除必需的Buff来停用Combo。

---

**祝你使用愉快！** 🎮
