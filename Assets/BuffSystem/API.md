# BuffSystem API 文档

本文档详细说明 BuffSystem 的所有公共 API。

## 目录

- [BuffApi](#buffapi) - 核心 API 类
- [BuffDatabase](#buffdatabase) - Buff 数据库
- [BuffEventSystem](#buffeventsystem) - 全局事件系统
- [BuffLocalEventSystem](#bufflocaleventsystem) - 本地事件系统
- [接口参考](#接口参考)
- [枚举类型](#枚举类型)

---

## BuffApi

`BuffSystem.Core.BuffApi`

Buff 系统的核心静态 API 类，提供所有 Buff 操作的入口。

### 初始化

#### Initialize
```csharp
public static void Initialize()
```
初始化 Buff 系统。首次使用时会自动初始化，但你可以手动调用以确保初始化时机。

**示例：**
```csharp
// 在游戏启动时初始化
void Start()
{
    BuffApi.Initialize();
}
```

#### ReloadData
```csharp
public static void ReloadData()
```
重新加载所有 Buff 数据。适用于运行时重新加载配置。

---

### 添加 Buff

#### AddBuff (通过 ID)
```csharp
public static IBuff AddBuff(int buffId, IBuffOwner target, object source = null)
```
| 参数 | 类型 | 说明 |
|------|------|------|
| buffId | int | Buff 配置 ID |
| target | IBuffOwner | 目标持有者 |
| source | object | 可选，Buff 来源 |

**返回值：** `IBuff` - 创建的 Buff 实例，失败返回 null

**示例：**
```csharp
IBuff buff = BuffApi.AddBuff(1001, player);
if (buff != null)
{
    Debug.Log($"添加成功: {buff.Name}");
}
```

#### AddBuff (通过名称)
```csharp
public static IBuff AddBuff(string buffName, IBuffOwner target, object source = null)
```
| 参数 | 类型 | 说明 |
|------|------|------|
| buffName | string | Buff 名称 |
| target | IBuffOwner | 目标持有者 |
| source | object | 可选，Buff 来源 |

**示例：**
```csharp
IBuff buff = BuffApi.AddBuff("燃烧", player, fireSkill);
```

#### AddBuff (通过数据)
```csharp
public static IBuff AddBuff(IBuffData data, IBuffOwner target, object source = null)
```
| 参数 | 类型 | 说明 |
|------|------|------|
| data | IBuffData | Buff 数据 |
| target | IBuffOwner | 目标持有者 |
| source | object | 可选，Buff 来源 |

#### TryAddBuff
```csharp
public static bool TryAddBuff(int buffId, IBuffOwner target, out IBuff buff, object source = null)
public static bool TryAddBuff(string buffName, IBuffOwner target, out IBuff buff, object source = null)
```
尝试添加 Buff，返回是否成功。

**示例：**
```csharp
if (BuffApi.TryAddBuff(1001, player, out var buff))
{
    // 添加成功
}
else
{
    // 添加失败（可能是配置不存在或目标为空）
}
```

---

### 移除 Buff

#### RemoveBuff (通过实例)
```csharp
public static void RemoveBuff(IBuff buff)
```
移除指定的 Buff 实例。

#### RemoveBuff (通过 ID)
```csharp
public static void RemoveBuff(int buffId, IBuffOwner target)
```
移除目标上指定 ID 的所有 Buff。

#### RemoveBuff (通过名称)
```csharp
public static void RemoveBuff(string buffName, IBuffOwner target)
```
移除目标上指定名称的所有 Buff。

#### RemoveBuffBySource
```csharp
public static void RemoveBuffBySource(object source, IBuffOwner target)
```
移除目标上来自指定来源的所有 Buff。

**示例：**
```csharp
// 当技能结束时，移除该技能添加的所有 Buff
BuffApi.RemoveBuffBySource(fireSkill, player);
```

#### ClearBuffs
```csharp
public static void ClearBuffs(IBuffOwner target)
```
清空目标上的所有 Buff。

---

### 查询 Buff

#### HasBuff
```csharp
public static bool HasBuff(int buffId, IBuffOwner target)
public static bool HasBuff(string buffName, IBuffOwner target)
public static bool HasBuff(int buffId, object source, IBuffOwner target)
```
检查目标是否拥有指定 Buff。

**示例：**
```csharp
if (BuffApi.HasBuff("燃烧", player))
{
    // 玩家正在燃烧
}

// 检查特定来源的 Buff
if (BuffApi.HasBuff(1001, fireSkill, player))
{
    // 玩家有来自 fireSkill 的 1001 号 Buff
}
```

#### GetBuff
```csharp
public static IBuff GetBuff(int buffId, IBuffOwner target, object source = null)
public static IBuff GetBuff(string buffName, IBuffOwner target, object source = null)
```
获取指定的 Buff 实例。

**示例：**
```csharp
IBuff burnBuff = BuffApi.GetBuff("燃烧", player);
if (burnBuff != null)
{
    Debug.Log($"燃烧层数: {burnBuff.CurrentStack}");
}
```

#### GetBuffs
```csharp
public static IEnumerable<IBuff> GetBuffs(int buffId, IBuffOwner target)
public static IEnumerable<IBuff> GetBuffs(string buffName, IBuffOwner target)
public static IEnumerable<IBuff> GetBuffsBySource(object source, IBuffOwner target)
```
获取 Buff 列表。

**示例：**
```csharp
// 获取所有燃烧 Buff（可能是不同来源的）
var burnBuffs = BuffApi.GetBuffs("燃烧", player);
foreach (var buff in burnBuffs)
{
    Debug.Log($"来源: {buff.Source}, 层数: {buff.CurrentStack}");
}
```

#### GetAllBuffs
```csharp
public static IReadOnlyCollection<IBuff> GetAllBuffs(IBuffOwner target)
```
获取目标上的所有 Buff。

#### GetBuffCount
```csharp
public static int GetBuffCount(IBuffOwner target)
```
获取目标上的 Buff 数量。

---

### 修改 Buff

#### AddStack
```csharp
public static void AddStack(IBuff buff, int amount)
```
增加 Buff 层数。

**示例：**
```csharp
IBuff buff = BuffApi.GetBuff("燃烧", player);
BuffApi.AddStack(buff, 2); // 增加 2 层
```

#### RemoveStack
```csharp
public static void RemoveStack(IBuff buff, int amount)
```
减少 Buff 层数。

#### RefreshBuff
```csharp
public static void RefreshBuff(IBuff buff)
```
刷新 Buff 持续时间。

---

### 数据查询

#### GetBuffData
```csharp
public static IBuffData GetBuffData(int buffId)
public static IBuffData GetBuffData(string buffName)
```
获取 Buff 配置数据。

#### HasBuffData
```csharp
public static bool HasBuffData(int buffId)
public static bool HasBuffData(string buffName)
```
检查 Buff 数据是否存在。

#### GetAllBuffData
```csharp
public static IEnumerable<IBuffData> GetAllBuffData()
```
获取所有 Buff 数据。

#### GetBuffId
```csharp
public static int GetBuffId(string buffName)
```
通过名称获取 Buff ID，不存在返回 -1。

---

## BuffDatabase

`BuffSystem.Data.BuffDatabase`

Buff 数据库，管理所有 Buff 配置的加载和查询。使用单例模式。

### 访问实例
```csharp
BuffDatabase db = BuffDatabase.Instance;
```

### 方法

#### Initialize
```csharp
public void Initialize()
```
初始化数据库，加载所有 Buff 数据。

#### Reload
```csharp
public void Reload()
```
重新加载所有数据。

#### GetBuffData
```csharp
public IBuffData GetBuffData(int buffId)
public IBuffData GetBuffData(string buffName)
```
获取 Buff 数据。

#### GetBuffId
```csharp
public int GetBuffId(string buffName)
```
通过名称获取 ID。

---

## BuffEventSystem

`BuffSystem.Events.BuffEventSystem`

全局 Buff 事件系统，所有 Buff 操作都会触发相应事件。

### 事件列表

```csharp
// Buff 被添加
public static event EventHandler<BuffAddedEventArgs> OnBuffAdded;

// Buff 被移除
public static event EventHandler<BuffRemovedEventArgs> OnBuffRemoved;

// Buff 层数变化
public static event EventHandler<BuffStackChangedEventArgs> OnStackChanged;

// Buff 被刷新
public static event EventHandler<BuffRefreshedEventArgs> OnBuffRefreshed;

// Buff 过期
public static event EventHandler<BuffExpiredEventArgs> OnBuffExpired;

// Buff 被清空
public static event EventHandler<BuffClearedEventArgs> OnBuffCleared;
```

### 使用示例

```csharp
public class GameManager : MonoBehaviour
{
    void OnEnable()
    {
        BuffEventSystem.OnBuffAdded += OnBuffAdded;
        BuffEventSystem.OnBuffRemoved += OnBuffRemoved;
        BuffEventSystem.OnStackChanged += OnStackChanged;
    }
    
    void OnDisable()
    {
        BuffEventSystem.OnBuffAdded -= OnBuffAdded;
        BuffEventSystem.OnBuffRemoved -= OnBuffRemoved;
        BuffEventSystem.OnStackChanged -= OnStackChanged;
    }
    
    void OnBuffAdded(object sender, BuffAddedEventArgs e)
    {
        Debug.Log($"[全局] Buff 添加: {e.Buff.Name} -> {e.Buff.Owner.OwnerName}");
    }
    
    void OnBuffRemoved(object sender, BuffRemovedEventArgs e)
    {
        Debug.Log($"[全局] Buff 移除: {e.Buff.Name}");
    }
    
    void OnStackChanged(object sender, BuffStackChangedEventArgs e)
    {
        Debug.Log($"[全局] 层数变化: {e.Buff.Name} {e.OldStack} -> {e.NewStack}");
    }
}
```

---

## BuffLocalEventSystem

`BuffSystem.Events.BuffLocalEventSystem`

每个 BuffOwner 独立的本地事件系统。

### 访问方式
```csharp
BuffOwner owner = GetComponent<BuffOwner>();
BuffLocalEventSystem localEvents = owner.LocalEvents;
```

### 事件列表

```csharp
public event EventHandler<BuffAddedEventArgs> OnBuffAdded;
public event EventHandler<BuffRemovedEventArgs> OnBuffRemoved;
public event EventHandler<BuffStackChangedEventArgs> OnStackChanged;
public event EventHandler<BuffRefreshedEventArgs> OnBuffRefreshed;
public event EventHandler<BuffExpiredEventArgs> OnBuffExpired;
public event EventHandler OnBuffCleared;
```

### 使用示例

```csharp
public class Character : MonoBehaviour
{
    [SerializeField] private BuffOwner buffOwner;
    
    void Start()
    {
        // 只监听这个角色的 Buff 事件
        buffOwner.LocalEvents.OnBuffAdded += OnBuffAdded;
        buffOwner.LocalEvents.OnBuffRemoved += OnBuffRemoved;
    }
    
    void OnDestroy()
    {
        buffOwner.LocalEvents.OnBuffAdded -= OnBuffAdded;
        buffOwner.LocalEvents.OnBuffRemoved -= OnBuffRemoved;
    }
    
    void OnBuffAdded(object sender, BuffAddedEventArgs e)
    {
        // 只有这个角色的 Buff 添加时触发
        ShowBuffIcon(e.Buff);
    }
    
    void OnBuffRemoved(object sender, BuffRemovedEventArgs e)
    {
        HideBuffIcon(e.Buff);
    }
}
```

---

## 接口参考

### IBuff

Buff 实例接口。

| 属性/方法 | 类型 | 说明 |
|-----------|------|------|
| InstanceId | int | Buff 实例唯一 ID |
| DataId | int | Buff 配置 ID |
| Name | string | Buff 名称 |
| CurrentStack | int | 当前层数 |
| MaxStack | int | 最大层数 |
| Duration | float | 当前持续时间 |
| TotalDuration | float | 总持续时间 |
| RemainingTime | float | 剩余时间 |
| IsPermanent | bool | 是否永久 Buff |
| IsMarkedForRemoval | bool | 是否标记为移除 |
| Source | object | Buff 来源 |
| SourceId | int | 来源 ID（哈希值） |
| Owner | IBuffOwner | 所属持有者 |
| Data | IBuffData | Buff 数据 |
| GetSource<T>() | T | 获取并转换来源 |
| TryGetSource<T>(out T) | bool | 尝试获取来源 |
| AddStack(int) | void | 增加层数 |
| RemoveStack(int) | void | 减少层数 |
| RefreshDuration() | void | 刷新持续时间 |
| MarkForRemoval() | void | 标记为移除 |

### IBuffOwner

Buff 持有者接口。

| 属性/方法 | 类型 | 说明 |
|-----------|------|------|
| OwnerId | int | 持有者唯一 ID |
| OwnerName | string | 持有者名称 |
| BuffContainer | IBuffContainer | Buff 容器 |
| OnBuffEvent(BuffEventType, IBuff) | void | Buff 事件回调 |

### IBuffContainer

Buff 容器接口。

| 属性/方法 | 类型 | 说明 |
|-----------|------|------|
| Owner | IBuffOwner | 持有者 |
| AllBuffs | IReadOnlyCollection<IBuff> | 所有 Buff |
| Count | int | Buff 数量 |
| AddBuff(IBuffData, object) | IBuff | 添加 Buff |
| RemoveBuff(IBuff) | void | 移除 Buff |
| RemoveBuff(int) | void | 通过 ID 移除 |
| RemoveBuffBySource(object) | void | 通过来源移除 |
| ClearAllBuffs() | void | 清空所有 |
| GetBuff(int, object) | IBuff | 获取 Buff |
| GetBuffs(int) | IEnumerable<IBuff> | 获取 Buff 列表 |
| GetBuffsBySource(object) | IEnumerable<IBuff> | 通过来源获取 |
| HasBuff(int) | bool | 是否有指定 Buff |
| HasBuff(int, object) | bool | 是否有指定来源的 Buff |
| Update(float) | void | 更新 |

### IBuffData

Buff 数据接口。

| 属性 | 类型 | 说明 |
|------|------|------|
| Id | int | 唯一 ID |
| Name | string | 名称 |
| Description | string | 描述 |
| EffectType | BuffEffectType | 效果类型 |
| IsUnique | bool | 是否唯一 |
| StackMode | BuffStackMode | 叠加模式 |
| MaxStack | int | 最大层数 |
| AddStackCount | int | 每次添加层数 |
| IsPermanent | bool | 是否永久 |
| Duration | float | 持续时间 |
| CanRefresh | bool | 是否可刷新 |
| RemoveMode | BuffRemoveMode | 移除模式 |
| RemoveStackCount | int | 每次移除层数 |
| RemoveInterval | float | 移除间隔 |
| CreateLogic() | IBuffLogic | 创建逻辑实例 |

### IBuffLogic

Buff 逻辑接口。

| 属性/方法 | 类型 | 说明 |
|-----------|------|------|
| Buff | IBuff | 所属 Buff |
| Initialize(IBuff) | void | 初始化 |
| Dispose() | void | 销毁 |

#### 生命周期接口

- `IBuffStart` - `void OnStart()`
- `IBuffAcquire` - `void OnAcquire()`
- `IBuffLogicUpdate` - `void OnLogicUpdate(float deltaTime)`
- `IBuffVisualUpdate` - `void OnVisualUpdate(float deltaTime)`
- `IBuffRefresh` - `void OnRefresh()`
- `IBuffStackChange` - `void OnStackChanged(int oldStack, int newStack)`
- `IBuffReduce` - `void OnReduce()`
- `IBuffRemove` - `void OnRemove()`
- `IBuffEnd` - `void OnEnd()`
- `IBuffDurationChange` - `void OnDurationChanged(float oldDuration, float newDuration)`

---

## 枚举类型

### BuffEffectType

```csharp
public enum BuffEffectType
{
    Neutral = 0,  // 中性
    Buff = 1,     // 增益
    Debuff = 2,   // 减益
    Special = 3   // 特殊
}
```

### BuffStackMode

```csharp
public enum BuffStackMode
{
    None = 0,        // 不可叠加（新 Buff 会替换或忽略）
    Stackable = 1,   // 可叠加（层数增加）
    Independent = 2  // 独立（同 ID 可同时存在多个实例）
}
```

### BuffRemoveMode

```csharp
public enum BuffRemoveMode
{
    Remove = 0,  // 直接移除
    Reduce = 1   // 逐层移除
}
```

### BuffEventType

```csharp
public enum BuffEventType
{
    Added = 0,       // Buff 被添加
    Removed = 1,     // Buff 被移除
    StackChanged = 2,// 层数变化
    Refreshed = 3,   // 持续时间刷新
    Expired = 4,     // Buff 过期
    Cleared = 5      // Buff 被清空
}
```

### UpdateMode

```csharp
public enum UpdateMode
{
    EveryFrame = 0,  // 每帧更新
    Interval = 1,    // 按固定间隔更新
    Manual = 2       // 手动更新
}
```

---

## 事件参数类

### BuffEventArgs

所有事件参数的基类。

```csharp
public class BuffEventArgs : EventArgs
{
    public IBuff Buff { get; }
}
```

### BuffAddedEventArgs

```csharp
public class BuffAddedEventArgs : BuffEventArgs
{
    public BuffAddedEventArgs(IBuff buff) : base(buff) { }
}
```

### BuffRemovedEventArgs

```csharp
public class BuffRemovedEventArgs : BuffEventArgs
{
    public BuffRemovedEventArgs(IBuff buff) : base(buff) { }
}
```

### BuffStackChangedEventArgs

```csharp
public class BuffStackChangedEventArgs : BuffEventArgs
{
    public int OldStack { get; }
    public int NewStack { get; }
}
```

### BuffRefreshedEventArgs

```csharp
public class BuffRefreshedEventArgs : BuffEventArgs
{
    public BuffRefreshedEventArgs(IBuff buff) : base(buff) { }
}
```

### BuffExpiredEventArgs

```csharp
public class BuffExpiredEventArgs : BuffEventArgs
{
    public BuffExpiredEventArgs(IBuff buff) : base(buff) { }
}
```

### BuffClearedEventArgs

```csharp
public class BuffClearedEventArgs : EventArgs
{
    public IBuffOwner Owner { get; }
}
```
