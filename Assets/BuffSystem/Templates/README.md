# BuffSystem 代码模板

本文档说明如何使用 BuffSystem 提供的代码模板快速开发。

## 模板列表

| 模板 | 用途 | 文件名 |
|------|------|--------|
| BuffLogicTemplate | 自定义 Buff 逻辑 | `BuffLogicTemplate.cs` |
| EffectTemplate | 自定义 Effect | `EffectTemplate.cs` |
| BuffOwnerTemplate | 自定义 Buff 持有者 | `BuffOwnerTemplate.cs` |

---

## 使用说明

### 1. BuffLogicTemplate - 自定义 Buff 逻辑

适用于创建复杂的 Buff 行为逻辑。

#### 使用步骤

1. 复制 `BuffLogicTemplate.cs` 到你的项目
2. 重命名文件和类名（例如：`BurningBuffLogic.cs`）
3. 替换模板中的占位符：
   - `#BUFFNAME#` → 你的 Buff 名称（例如：`Burning`）
4. 根据需求实现生命周期方法
5. 在 BuffDataSO 中选择这个逻辑类

#### 模板结构

```csharp
[System.Serializable]
public class BurningBuffLogic : BuffLogicBase, 
    IBuffStart,           // Buff 逻辑初始化完成
    IBuffAcquire,         // Buff 被添加到持有者
    IBuffLogicUpdate,     // 每帧逻辑更新
    // ... 其他生命周期接口
{
    // 配置参数
    [SerializeField] private float baseValue = 10f;
    
    // 生命周期方法
    public void OnAcquire() { }
    public void OnLogicUpdate(float deltaTime) { }
    // ...
}
```

#### 示例：燃烧 Buff

```csharp
[System.Serializable]
public class BurningBuffLogic : BuffLogicBase, 
    IBuffAcquire, IBuffLogicUpdate, IBuffRemove
{
    [SerializeField] private float damagePerSecond = 5f;
    [SerializeField] private float damagePerStack = 2f;
    
    private float damageTimer;
    
    public void OnAcquire()
    {
        Debug.Log($"[{Owner.OwnerName}] 开始燃烧");
    }
    
    public void OnLogicUpdate(float deltaTime)
    {
        damageTimer += deltaTime;
        if (damageTimer >= 1f)
        {
            float damage = damagePerSecond + (CurrentStack - 1) * damagePerStack;
            // 造成伤害...
            damageTimer = 0f;
        }
    }
    
    public void OnRemove()
    {
        Debug.Log($"[{Owner.OwnerName}] 燃烧结束");
    }
}
```

---

### 2. EffectTemplate - 自定义 Effect

适用于 EffectBasedBuffLogic 的效果配置。

#### 使用步骤

1. 复制 `EffectTemplate.cs` 到你的项目
2. 重命名文件和类名（例如：`DamageEffect.cs`）
3. 替换模板中的占位符：
   - `#EFFECTNAME#` → 你的 Effect 名称（例如：`Damage`）
4. 实现 `Execute` 和 `Cancel` 方法
5. 在 EffectBasedBuffLogic 的效果列表中添加这个 Effect

#### 模板结构

```csharp
[System.Serializable]
public class DamageEffect : EffectBase
{
    [Header("基础参数")]
    [SerializeField] private float value = 10f;
    [SerializeField] private bool isPercent = false;
    
    public override void Execute(IBuff buff)
    {
        // 执行效果
    }
    
    public override void Cancel(IBuff buff)
    {
        // 取消效果
    }
}
```

#### 示例：伤害效果

```csharp
[System.Serializable]
public class DamageEffect : EffectBase
{
    [SerializeField] private float damageAmount = 10f;
    
    public override void Execute(IBuff buff)
    {
        if (TryGetOwnerComponent<HealthSystem>(buff, out var health))
        {
            health.TakeDamage(damageAmount);
        }
    }
    
    public override void Cancel(IBuff buff)
    {
        // 伤害效果不需要取消
    }
}
```

---

### 3. BuffOwnerTemplate - 自定义 Buff 持有者

适用于创建自定义的角色/单位类。

#### 使用步骤

1. 复制 `BuffOwnerTemplate.cs` 到你的项目
2. 重命名文件和类名（例如：`PlayerCharacter.cs`）
3. 替换模板中的占位符：
   - `#CHARACTERNAME#` → 你的角色名称（例如：`PlayerCharacter`）
4. 根据需要实现事件处理方法
5. 将脚本挂载到 GameObject 上

#### 模板结构

```csharp
[RequireComponent(typeof(BuffOwner))]
public class PlayerCharacter : MonoBehaviour, IBuffOwner, IBuffEventReceiver
{
    [SerializeField] private BuffOwner buffOwner;
    
    // IBuffOwner 实现
    public int OwnerId => buffOwner?.OwnerId ?? GetInstanceID();
    public string OwnerName => buffOwner?.OwnerName ?? gameObject.name;
    public IBuffContainer BuffContainer => buffOwner?.BuffContainer;
    
    // 事件处理...
}
```

#### 示例：玩家角色

```csharp
[RequireComponent(typeof(BuffOwner))]
public class PlayerCharacter : MonoBehaviour, IBuffOwner, IBuffEventReceiver
{
    [SerializeField] private BuffOwner buffOwner;
    [SerializeField] private HealthSystem healthSystem;
    
    public int OwnerId => buffOwner.OwnerId;
    public string OwnerName => buffOwner.OwnerName;
    public IBuffContainer BuffContainer => buffOwner.BuffContainer;
    
    void OnEnable()
    {
        buffOwner.LocalEvents.OnBuffAdded += OnBuffAdded;
    }
    
    void OnDisable()
    {
        buffOwner.LocalEvents.OnBuffAdded -= OnBuffAdded;
    }
    
    void OnBuffAdded(object sender, BuffAddedEventArgs e)
    {
        Debug.Log($"获得 Buff: {e.Buff.Name}");
        ShowBuffIcon(e.Buff);
    }
    
    public void OnBuffEvent(IBuff buff, string eventName, object data)
    {
        if (eventName == "Damage")
        {
            float damage = (float)data;
            healthSystem?.TakeDamage(damage);
        }
    }
    
    public void OnBuffEvent(BuffEventType eventType, IBuff buff) { }
}
```

---

## 快速开始清单

### 创建一个完整的 Buff 系统

1. **创建 Buff 数据配置**
   - 右键 -> Create -> BuffSystem -> Buff Data
   - 配置 ID、名称、持续时间等参数

2. **（可选）创建自定义 Buff 逻辑**
   - 复制 `BuffLogicTemplate.cs`
   - 重命名并实现逻辑
   - 在 BuffDataSO 中选择逻辑类

3. **创建角色类**
   - 复制 `BuffOwnerTemplate.cs`
   - 重命名并实现事件处理
   - 挂载到 GameObject

4. **创建技能/道具系统**
   - 编写添加 Buff 的代码
   - 使用 `BuffApi.AddBuff()`

5. **创建 UI 系统**
   - 监听 Buff 事件
   - 显示/隐藏 Buff 图标

---

## 占位符替换说明

| 占位符 | 替换为 | 示例 |
|--------|--------|------|
| `#BUFFNAME#` | Buff 名称 | `Burning`、`Poison`、`PowerUp` |
| `#EFFECTNAME#` | Effect 名称 | `Damage`、`Heal`、`SpeedUp` |
| `#CHARACTERNAME#` | 角色名称 | `PlayerCharacter`、`Enemy`、`NPC` |

### 替换示例

**BuffLogicTemplate.cs**
```csharp
// 替换前
public class #BUFFNAME#BuffLogic : BuffLogicBase

// 替换后
public class BurningBuffLogic : BuffLogicBase
```

**EffectTemplate.cs**
```csharp
// 替换前
public class #EFFECTNAME#Effect : EffectBase

// 替换后
public class DamageEffect : EffectBase
```

**BuffOwnerTemplate.cs**
```csharp
// 替换前
public class #CHARACTERNAME# : MonoBehaviour

// 替换后
public class PlayerCharacter : MonoBehaviour
```

---

## 最佳实践

### 1. 命名规范

- Buff 逻辑类：`[Name]BuffLogic`（例如：`BurningBuffLogic`）
- Effect 类：`[Name]Effect`（例如：`DamageEffect`）
- 角色类：`[Name]Character` 或 `[Name]`（例如：`PlayerCharacter`）

### 2. 文件组织

```
MyGame/
├── Scripts/
│   ├── Buffs/
│   │   ├── Logic/          # Buff 逻辑类
│   │   │   ├── BurningBuffLogic.cs
│   │   │   └── PoisonBuffLogic.cs
│   │   └── Effects/        # Effect 类
│   │       ├── DamageEffect.cs
│   │       └── HealEffect.cs
│   ├── Characters/         # 角色类
│   │   ├── PlayerCharacter.cs
│   │   └── EnemyCharacter.cs
│   └── Skills/            # 技能系统
│       └── FireballSkill.cs
└── Data/
    └── Buffs/             # BuffDataSO 配置
        ├── BurningBuff.asset
        └── PoisonBuff.asset
```

### 3. ID 管理

使用常量管理 Buff ID：

```csharp
public static class BuffIds
{
    public const int Burning = 1001;
    public const int Poison = 1002;
    public const int PowerUp = 1003;
    public const int SpeedUp = 1004;
    // ...
}

// 使用
BuffApi.AddBuff(BuffIds.Burning, target);
```

---

## 示例项目

### 完整的燃烧系统

**1. BurningBuffLogic.cs**
```csharp
[System.Serializable]
public class BurningBuffLogic : BuffLogicBase, 
    IBuffAcquire, IBuffLogicUpdate, IBuffStackChange, IBuffRemove
{
    [SerializeField] private float damagePerSecond = 5f;
    [SerializeField] private float damagePerStack = 2f;
    
    private float damageTimer;
    
    public void OnAcquire()
    {
        SendEvent("BurningStarted", CurrentStack);
    }
    
    public void OnLogicUpdate(float deltaTime)
    {
        damageTimer += deltaTime;
        if (damageTimer >= 1f)
        {
            float damage = damagePerSecond + (CurrentStack - 1) * damagePerStack;
            if (TryGetOwnerComponent<HealthSystem>(out var health))
            {
                health.TakeDamage(damage);
            }
            damageTimer = 0f;
        }
    }
    
    public void OnStackChanged(int oldStack, int newStack)
    {
        SendEvent("BurningStackChanged", newStack);
    }
    
    public void OnRemove()
    {
        SendEvent("BurningEnded", null);
    }
}
```

**2. PlayerCharacter.cs**
```csharp
[RequireComponent(typeof(BuffOwner))]
public class PlayerCharacter : MonoBehaviour, IBuffOwner, IBuffEventReceiver
{
    [SerializeField] private BuffOwner buffOwner;
    [SerializeField] private ParticleSystem burningEffect;
    
    public int OwnerId => buffOwner.OwnerId;
    public string OwnerName => buffOwner.OwnerName;
    public IBuffContainer BuffContainer => buffOwner.BuffContainer;
    
    public void OnBuffEvent(IBuff buff, string eventName, object data)
    {
        switch (eventName)
        {
            case "BurningStarted":
                burningEffect?.Play();
                break;
            case "BurningEnded":
                burningEffect?.Stop();
                break;
        }
    }
    
    public void OnBuffEvent(BuffEventType eventType, IBuff buff) { }
}
```

**3. FireballSkill.cs**
```csharp
public class FireballSkill : MonoBehaviour
{
    [SerializeField] private int burningBuffId = 1001;
    
    public void Cast(IBuffOwner target)
    {
        BuffApi.AddBuff(burningBuffId, target, this);
    }
}
```

---

## 常见问题

### Q: 模板中的接口（HealthSystem 等）在哪里？

**A:** 这些是示例接口，你需要根据实际项目替换为你自己的组件接口。

### Q: 可以同时实现多个生命周期接口吗？

**A:** 可以，根据需求选择实现需要的接口。

### Q: 模板需要修改多少内容？

**A:** 最少只需要：
1. 重命名类名
2. 替换占位符
3. 在关键方法中添加你的逻辑

### Q: 可以删除不需要的生命周期方法吗？

**A:** 可以，只保留你需要实现的接口和方法。
