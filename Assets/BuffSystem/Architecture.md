# BuffSystem 架构设计文档

本文档详细说明 BuffSystem 的架构设计、模块划分和设计模式。

## 目录

- [整体架构](#整体架构)
- [模块划分](#模块划分)
- [设计模式](#设计模式)
- [数据流向](#数据流向)
- [类关系图](#类关系图)
- [性能优化](#性能优化)

---

## 整体架构

BuffSystem 采用分层架构设计，各层职责清晰：

```text
┌─────────────────────────────────────────────────────────────┐
│                      表现层 (Presentation)                    │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │  BuffOwner  │  │ BuffSystem  │  │   Editor Tools      │  │
│  │(MonoBehaviour)│  │   Updater   │  │  (Custom Editors)   │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
├─────────────────────────────────────────────────────────────┤
│                       核心层 (Core)                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │   BuffApi   │  │   IBuff     │  │    IBuffLogic       │  │
│  │  (Facade)   │  │  (Entity)   │  │   (Strategy)        │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │  IBuffOwner │  │  IBuffData  │  │      IEffect        │  │
│  │  (Owner)    │  │   (Data)    │  │    (Effect)         │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
├─────────────────────────────────────────────────────────────┤
│                      运行时层 (Runtime)                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │ BuffEntity  │  │BuffContainer│  │   BuffStrategy      │  │
│  │ (Pooled)    │  │  (Manager)  │  │   (Strategy)        │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
├─────────────────────────────────────────────────────────────┤
│                       数据层 (Data)                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │  BuffDataSO │  │ BuffDatabase│  │ BuffSystemConfig    │  │
│  │   (SO)      │  │  (Singleton)│  │    (SO)             │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
│  ┌─────────────┐  ┌─────────────┐                            │
│  │BuffDataCenter│  │  UpdateMode │                            │
│  │   (SO)      │  │   (Enum)    │                            │
│  └─────────────┘  └─────────────┘                            │
├─────────────────────────────────────────────────────────────┤
│                      事件层 (Events)                         │
│  ┌─────────────────────┐  ┌─────────────────────────────┐    │
│  │   BuffEventSystem   │  │    BuffLocalEventSystem     │    │
│  │     (Global)        │  │        (Local)              │    │
│  └─────────────────────┘  └─────────────────────────────┘    │
├─────────────────────────────────────────────────────────────┤
│                      工具层 (Utils)                          │
│  ┌─────────────────────┐                                      │
│  │     ObjectPool<T>   │                                      │
│  │    (Generic Pool)   │                                      │
│  └─────────────────────┘                                      │
└─────────────────────────────────────────────────────────────┘
```

---

## 模块划分

### 1. Core 模块 (核心接口)

**职责：** 定义系统的核心抽象接口

**核心接口：**
- `IBuff` - Buff 实例抽象
- `IBuffData` - Buff 数据抽象
- `IBuffOwner` - Buff 持有者抽象
- `IBuffLogic` - Buff 逻辑抽象
- `IEffect` - 效果抽象
- `BuffApi` - 外观模式，统一入口

**设计原则：**
- 依赖倒置原则 (DIP)：高层模块依赖抽象接口
- 接口隔离原则 (ISP)：细粒度接口，按需实现

### 2. Runtime 模块 (运行时实现)

**职责：** 实现核心接口，管理 Buff 生命周期

**核心类：**
- `BuffEntity` - IBuff 的实现，使用对象池
- `BuffContainer` - IBuffContainer 的实现
- `BuffOwner` - MonoBehaviour 适配器
- `BuffSystemUpdater` - 全局更新管理

**关键特性：**
- 对象池复用 BuffEntity 实例
- BuffContainer 管理持有者的所有 Buff
- 支持三种更新模式

### 3. Data 模块 (数据层)

**职责：** 数据配置和加载

**核心类：**
- `BuffDataSO` - ScriptableObject 配置
- `BuffDatabase` - 数据加载和管理（单例）
- `BuffSystemConfig` - 系统全局配置
- `BuffDataCenter` - 数据资源中心

**数据加载流程：**
```
BuffDatabase.Initialize()
    ├── Resources.LoadAll<BuffDataSO>("BuffSystem/BuffData")
    └── BuffDataCenter.BuffDataList
```

### 4. Events 模块 (事件系统)

**职责：** 提供全局和本地事件通知

**核心类：**
- `BuffEventSystem` - 静态全局事件
- `BuffLocalEventSystem` - 每个持有者的本地事件

**事件类型：**
- Buff 添加/移除
- 层数变化
- 持续时间刷新/过期
- Buff 清空

### 5. Strategy 模块 (策略模式)

**职责：** 定义可配置的 Buff 行为策略

**策略接口：**
- `IStackStrategy` - 叠层策略
- `IRefreshStrategy` - 刷新策略
- `IRemoveStrategy` - 移除策略

**实现类：**
- `StackableStrategy` / `NonStackableStrategy` / `IndependentStrategy`
- `RefreshableStrategy` / `NonRefreshableStrategy`

### 6. Utils 模块 (工具类)

**职责：** 提供通用工具

**核心类：**
- `ObjectPool<T>` - 通用对象池实现

### 7. Editor 模块 (编辑器扩展)

**职责：** Unity 编辑器工具

**核心类：**
- `BuffDataSOEditor` - BuffDataSO 自定义 Inspector
- `BuffOwnerEditor` - BuffOwner 自定义 Inspector
- `BuffSystemMenu` - 菜单项

---

## 设计模式

### 1. 外观模式 (Facade)

**应用：** `BuffApi`

**说明：**
- 为复杂的子系统提供简单的统一接口
- 隐藏内部实现细节
- 降低使用者的学习成本

```csharp
// 使用 Facade，无需了解内部实现
BuffApi.AddBuff(1001, player);

// 而不是
var data = BuffDatabase.Instance.GetBuffData(1001);
var buff = new BuffEntity();
buff.Reset(data, player, null);
player.BuffContainer.AddBuff(data, null);
```

### 2. 策略模式 (Strategy)

**应用：** `BuffStackMode`、`BuffRemoveMode`

**说明：**
- 定义算法族，分别封装起来，让它们可以互相替换
- 叠层、刷新、移除等行为可配置

```csharp
// 不同的叠加策略
public enum BuffStackMode
{
    None,        // 不可叠加
    Stackable,   // 可叠加
    Independent  // 独立实例
}

// BuffContainer 根据配置执行不同策略
if (data.StackMode == BuffStackMode.Stackable)
{
    existingBuff.AddStack(data.AddStackCount);
}
```

### 3. 对象池模式 (Object Pool)

**应用：** `ObjectPool<T>`、`BuffEntity`

**说明：**
- 复用对象，避免频繁创建和销毁
- 减少 GC 压力，提高性能

```csharp
// BuffEntity 使用对象池
private readonly ObjectPool<BuffEntity> buffPool;

public IBuff AddBuff(IBuffData data, object source)
{
    BuffEntity buff = buffPool.Get();  // 从池中获取
    buff.Reset(data, owner, source);   // 重置状态
    // ...
}

internal void ReleaseBuff(BuffEntity buff)
{
    buffPool.Release(buff);  // 归还池中
}
```

### 4. 观察者模式 (Observer)

**应用：** `BuffEventSystem`、`BuffLocalEventSystem`

**说明：**
- 定义对象间的一对多依赖关系
- 当 Buff 状态变化时，自动通知所有观察者

```csharp
// 订阅事件
BuffEventSystem.OnBuffAdded += OnBuffAdded;

// 触发事件（内部）
BuffEventSystem.TriggerBuffAdded(buff);
```

### 5. 单例模式 (Singleton)

**应用：** `BuffDatabase`

**说明：**
- 确保一个类只有一个实例
- 提供全局访问点

```csharp
// 饿汉式单例，线程安全
public class BuffDatabase
{
    private static readonly BuffDatabase instance = new();
    public static BuffDatabase Instance => instance;
    
    private BuffDatabase() { }  // 私有构造函数
}
```

### 6. 组件模式 (Component)

**应用：** `BuffOwner`、`BuffContainer`

**说明：**
- 将功能拆分为独立的组件
- 可以组合使用，提高复用性

```csharp
// BuffOwner 作为组件挂载到 GameObject
public class BuffOwner : MonoBehaviour, IBuffOwner
{
    private BuffContainer buffContainer;  // 组合 BuffContainer
}
```

### 7. 依赖注入 (DI) 风格

**应用：** 接口设计

**说明：**
- 依赖抽象而非具体实现
- 便于测试和扩展

```csharp
// 依赖 IBuffOwner 接口，而非具体类
public class BuffContainer : IBuffContainer
{
    public BuffContainer(IBuffOwner owner) { }
}

// 可以传入任何实现 IBuffOwner 的对象
new BuffContainer(player);      // MonoBehaviour
new BuffContainer(aiCharacter); // 纯代码类
```

---

## 数据流向

### Buff 添加流程

```
使用者调用
    │
    ▼
┌─────────────┐
│  BuffApi    │
│  AddBuff()  │
└──────┬──────┘
       │
       ▼
┌─────────────┐
│ BuffDatabase│
│ GetBuffData │
└──────┬──────┘
       │
       ▼
┌─────────────┐
│BuffContainer│
│  AddBuff()  │
└──────┬──────┘
       │
       ├──────────────┐
       ▼              ▼
┌─────────────┐  ┌─────────────┐
│  已存在？    │  │ 对象池获取   │
│  处理叠加   │  │ BuffEntity  │
└─────────────┘  └──────┬──────┘
                        │
                        ▼
               ┌─────────────────┐
               │  buff.Reset()   │
               │ 初始化 Buff 状态 │
               └────────┬────────┘
                        │
           ┌────────────┼────────────┐
           ▼            ▼            ▼
      ┌─────────┐ ┌──────────┐ ┌──────────┐
      │IBuffStart│ │IBuffAcquire│ │BuffEvent │
      │ OnStart │ │ OnAcquire │ │  Trigger  │
      └─────────┘ └──────────┘ └──────────┘
```

### Buff 更新流程

```
BuffSystemUpdater.Update()
    │
    ▼
BuffOwner.Update()
    │
    ▼
BuffContainer.Update(deltaTime)
    │
    ├─────────────────────────────────────┐
    ▼                                     ▼
foreach (var buff in buffs)      处理待移除队列
    │                                     │
    ▼                                     ▼
buff.Update(deltaTime)           ReleaseBuff()
    │                                     │
    ├──────────────┬──────────────┐       │
    ▼              ▼              ▼       ▼
IBuffLogicUpdate  持续时间更新   IBuffVisualUpdate  对象池归还
OnLogicUpdate()   HandleExpiration() OnVisualUpdate() Cleanup()
                       │
                       ▼
              ┌────────────────┐
              │ 时间到？        │
              │ MarkForRemoval │
              └────────────────┘
```

### 事件通知流程

```
Buff 状态变化
    │
    ▼
┌─────────────────────┐
│ BuffEventSystem     │
│ TriggerXxx()        │
│ (内部静态方法)       │
└──────────┬──────────┘
           │
    ┌──────┴──────┐
    ▼             ▼
全局事件         本地事件
OnBuffXxx       owner.LocalEvents
    │           .TriggerXxx()
    │               │
    ▼               ▼
所有订阅者    该持有者的订阅者
收到通知      收到通知
```

---

## 类关系图

### 核心接口关系

```
                    ┌─────────────────┐
                    │   IBuffLogic    │◄──────────────────┐
                    │  (生命周期接口)  │                   │
                    └────────┬────────┘                   │
                             │                           │
         ┌───────────────────┼───────────────────┐       │
         │                   │                   │       │
         ▼                   ▼                   ▼       │
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐│
│   IBuffStart    │ │  IBuffAcquire   │ │IBuffLogicUpdate ││
│    OnStart()    │ │   OnAcquire()   │ │OnLogicUpdate()  ││
└─────────────────┘ └─────────────────┘ └─────────────────┘│
                                                           │
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐│
│  IBuffRefresh   │ │IBuffStackChange │ │   IBuffRemove   ││
│   OnRefresh()   │ │OnStackChanged() │ │   OnRemove()    ││
└─────────────────┘ └─────────────────┘ └─────────────────┘│
                                                           │
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐│
│    IBuffEnd     │ │   IBuffReduce   │ │IBuffDurationChange│
│    OnEnd()      │ │   OnReduce()    │ │OnDurationChanged()│
└─────────────────┘ └─────────────────┘ └─────────────────┘│
                                                           │
                    ┌─────────────────┐                    │
                    │  IBuffVisualUpdate                  │
                    │ OnVisualUpdate()│────────────────────┘
                    └─────────────────┘


┌─────────────────┐         ┌─────────────────┐
│     IBuff       │◄────────│   BuffEntity    │
│   (Entity)      │         │   (实现类)       │
└────────┬────────┘         └─────────────────┘
         │
         │ 包含
         ▼
┌─────────────────┐         ┌─────────────────┐
│    IBuffData    │◄────────│   BuffDataSO    │
│    (Data)       │         │   (SO 实现)      │
└────────┬────────┘         └─────────────────┘
         │
         │ 创建
         ▼
┌─────────────────┐
│   IBuffLogic    │
│   (Logic)       │
└─────────────────┘


┌─────────────────┐         ┌─────────────────┐
│   IBuffOwner    │◄────────│    BuffOwner    │
│    (Owner)      │         │ (MonoBehaviour) │
└────────┬────────┘         └─────────────────┘
         │
         │ 包含
         ▼
┌─────────────────┐         ┌─────────────────┐
│ IBuffContainer  │◄────────│  BuffContainer  │
│  (Container)    │         │   (实现类)       │
└─────────────────┘         └─────────────────┘
```

### 继承与实现关系

```
BuffLogicBase (抽象基类)
    │
    ├── EmptyBuffLogic (空实现)
    │
    └── EffectBasedBuffLogic (基于效果的实现)
            │
            └── 组合多个 IEffect


EffectBase (抽象基类)
    │
    └── 用户自定义 Effect 实现


System.EventArgs
    │
    └── BuffEventArgs
            │
            ├── BuffAddedEventArgs
            ├── BuffRemovedEventArgs
            ├── BuffStackChangedEventArgs
            ├── BuffRefreshedEventArgs
            ├── BuffExpiredEventArgs
            └── BuffClearedEventArgs
```

---

## 性能优化

### 1. 对象池

**问题：** 频繁创建/销毁 Buff 实例导致 GC 压力

**解决方案：**
```csharp
// 使用 ObjectPool<T> 复用 BuffEntity
private readonly ObjectPool<BuffEntity> buffPool;

// 配置参数
DefaultPoolCapacity = 32;  // 默认容量
MaxPoolSize = 128;         // 最大容量
```

**效果：**
- 避免频繁的内存分配和垃圾回收
- 稳定的运行时性能

### 2. 缓存优化

**问题：** 频繁创建集合类产生 GC

**解决方案：**
```csharp
// BuffCache 复用
private readonly List<IBuff> buffCache = new();

public IReadOnlyCollection<IBuff> AllBuffs
{
    get
    {
        buffCache.Clear();
        foreach (var buff in buffByInstanceId.Values)
        {
            buffCache.Add(buff);
        }
        return buffCache;
    }
}
```

### 3. 批量更新

**问题：** 大量 Buff 每帧更新造成性能瓶颈

**解决方案：**
```csharp
// 三种更新模式
public enum UpdateMode
{
    EveryFrame,  // 每帧更新（精确）
    Interval,    // 间隔更新（性能）
    Manual       // 手动更新（控制）
}

// Interval 模式
updateTimer += Time.deltaTime;
if (updateTimer >= updateInterval)
{
    UpdateAllContainers(updateTimer);
    updateTimer = 0f;
}
```

### 4. 字典索引

**问题：** Buff 查询效率

**解决方案：**
```csharp
// 多维度索引
private readonly Dictionary<int, BuffEntity> buffByInstanceId;     // 实例 ID
private readonly Dictionary<int, List<BuffEntity>> buffsByDataId;  // 数据 ID
private readonly Dictionary<object, List<BuffEntity>> buffsBySource; // 来源

// O(1) 查询
public IBuff GetBuff(int dataId, object source = null)
{
    if (buffsByDataId.TryGetValue(dataId, out var list))
    {
        // ...
    }
}
```

### 5. 延迟移除

**问题：** 遍历时修改集合导致错误

**解决方案：**
```csharp
// 使用队列延迟移除
private readonly Queue<int> removalQueue = new();

// 标记移除
public void MarkForRemoval(IBuff buff)
{
    removalQueue.Enqueue(buff.InstanceId);
}

// 批量处理
private void ProcessRemovals()
{
    while (removalQueue.Count > 0)
    {
        int instanceId = removalQueue.Dequeue();
        RemoveBuffInternal(instanceId);
    }
}
```

### 6. 避免装箱

**问题：** 值类型装箱产生 GC

**解决方案：**
```csharp
// 使用 SourceId (int) 而非 Source (object) 进行比较
public int SourceId => source?.GetHashCode() ?? 0;

// 避免频繁的类型转换和装箱
public bool TryGetSource<T>(out T sourceOut) where T : class
{
    sourceOut = source as T;
    return sourceOut != null;
}
```

---

## 扩展点

### 1. 自定义 Buff 逻辑

继承 `BuffLogicBase`，实现需要的生命周期接口：

```csharp
[System.Serializable]
public class MyBuffLogic : BuffLogicBase, IBuffAcquire, IBuffRemove
{
    public void OnAcquire() { }
    public void OnRemove() { }
}
```

### 2. 自定义 Effect

继承 `EffectBase`，实现效果逻辑：

```csharp
[System.Serializable]
public class MyEffect : EffectBase
{
    public override void Execute(IBuff buff) { }
    public override void Cancel(IBuff buff) { }
}
```

### 3. 自定义策略

实现策略接口，配置到系统中：

```csharp
public class MyStackStrategy : IStackStrategy
{
    public bool HandleStack(IBuff existingBuff, IBuffData newData)
    {
        // 自定义叠加逻辑
        return false;
    }
}
```

### 4. 自定义持有者

实现 `IBuffOwner` 接口：

```csharp
public class MyCharacter : IBuffOwner
{
    public int OwnerId { get; }
    public string OwnerName { get; }
    public IBuffContainer BuffContainer { get; }
    public void OnBuffEvent(BuffEventType eventType, IBuff buff) { }
}
```
