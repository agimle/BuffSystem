# BuffSystem - Unity Buff系统插件

一个即插即用的Unity Buff系统插件，支持多种游戏类型（RPG、MOBA、卡牌等）。

## 特性

- **解耦设计** - 不依赖MonoBehaviour，支持纯代码使用
- **对象池** - 内置对象池，避免频繁GC
- **数据驱动** - 使用ScriptableObject配置Buff
- **策略模式** - 叠层、刷新、移除等行为可配置
- **生命周期** - 完整的Buff生命周期回调
- **事件系统** - 全局和本地事件支持
- **类型安全** - 泛型接口支持

## 快速开始

### 1. 安装

将 `BuffSystem` 文件夹复制到你的项目的 `Assets` 目录下。

### 2. 创建Buff配置

1. 在Project窗口右键 -> Create -> BuffSystem -> Buff Data
2. 配置Buff的各项参数
3. （可选）创建Buff逻辑脚本

### 3. 挂载BuffOwner

在需要持有Buff的GameObject上添加 `BuffOwner` 组件：

```csharp
// 获取BuffOwner
BuffOwner owner = GetComponent<BuffOwner>();

// 添加Buff
owner.AddBuff(1001);  // 通过ID
owner.AddBuff("燃烧"); // 通过名称
```

### 4. 使用API

```csharp
using BuffSystem.Core;

// 初始化Buff系统（首次使用时自动初始化）
BuffApi.Initialize();

// 添加Buff
BuffApi.AddBuff(1001, owner);
BuffApi.AddBuff("燃烧", owner, damageSource);

// 查询Buff
bool hasBuff = BuffApi.HasBuff(1001, owner);
IBuff buff = BuffApi.GetBuff(1001, owner);

// 移除Buff
BuffApi.RemoveBuff(1001, owner);
BuffApi.ClearBuffs(owner);

// 重新加载Buff数据
BuffApi.ReloadData();
```

## 目录结构

```text
BuffSystem/
├── Scripts/
│   ├── Core/           # 核心接口和API
│   ├── Data/           # 数据层（SO、Database）
│   ├── Runtime/        # 运行时实体
│   ├── Strategy/       # 策略模式
│   ├── Events/         # 事件系统
│   ├── Utils/          # 工具类
│   └── Editor/         # 编辑器工具
└── Examples/           # 使用示例
```

## 核心概念

### IBuffOwner - Buff持有者

任何需要持有Buff的对象都可以实现此接口：

```csharp
public interface IBuffOwner
{
    int OwnerId { get; }
    string OwnerName { get; }
    IBuffContainer BuffContainer { get; }
    void OnBuffEvent(BuffEventType eventType, IBuff buff);
}
```

### IBuffContainer - Buff容器

管理Buff的添加、移除、查询：

```csharp
public interface IBuffContainer
{
    IBuffOwner Owner { get; }
    IReadOnlyCollection<IBuff> AllBuffs { get; }
    IBuff AddBuff(IBuffData data, object source = null);
    void RemoveBuff(IBuff buff);
    void RemoveBuff(int dataId);
    void RemoveBuffBySource(object source);
    void ClearAllBuffs();
    IBuff GetBuff(int dataId, object source = null);
    IEnumerable<IBuff> GetBuffs(int dataId);
    IEnumerable<IBuff> GetBuffsBySource(object source);
    bool HasBuff(int dataId);
    bool HasBuff(int dataId, object source);
    void Update(float deltaTime);
}
```

### IBuff - Buff实例

运行时Buff实体的抽象：

```csharp
public interface IBuff
{
    int InstanceId { get; }
    int DataId { get; }
    string Name { get; }
    int CurrentStack { get; }
    int MaxStack { get; }
    float Duration { get; }
    float TotalDuration { get; }
    float RemainingTime { get; }
    bool IsPermanent { get; }
    bool IsMarkedForRemoval { get; }
    object Source { get; }
    int SourceId { get; }
    IBuffOwner Owner { get; }
    IBuffData Data { get; }
    T GetSource<T>() where T : class;
    bool TryGetSource<T>(out T source) where T : class;
    void AddStack(int amount);
    void RemoveStack(int amount);
    void RefreshDuration();
    void MarkForRemoval();
    void Reset(IBuffData data, IBuffOwner owner, object source);
}
```

### IBuffData - Buff数据

Buff的配置数据，使用ScriptableObject实现：

```csharp
public interface IBuffData
{
    int Id { get; }
    string Name { get; }
    string Description { get; }
    BuffEffectType EffectType { get; }
    bool IsUnique { get; }
    BuffStackMode StackMode { get; }
    int MaxStack { get; }
    int AddStackCount { get; }
    bool IsPermanent { get; }
    float Duration { get; }
    bool CanRefresh { get; }
    BuffRemoveMode RemoveMode { get; }
    int RemoveStackCount { get; }
    float RemoveInterval { get; }
    IBuffLogic CreateLogic();
}
```

### IBuffLogic - Buff逻辑

Buff的具体行为逻辑：

```csharp
[System.Serializable]
public class MyBuffLogic : BuffLogicBase, IBuffAcquire, IBuffRemove
{
    public void OnAcquire()
    {
        // Buff被添加时调用
    }
    
    public void OnRemove()
    {
        // Buff被移除时调用
    }
}
```

## 生命周期接口

| 接口 | 触发时机 |
| ------ | ---------- |
| `IBuffStart` | Buff逻辑初始化完成 |
| `IBuffAcquire` | Buff被添加到持有者 |
| `IBuffLogicUpdate` | 每帧逻辑更新 |
| `IBuffVisualUpdate` | 每帧表现更新 |
| `IBuffRefresh` | Buff持续时间刷新 |
| `IBuffStackChange` | Buff层数变化 |
| `IBuffReduce` | Buff层数减少 |
| `IBuffRemove` | Buff被标记移除 |
| `IBuffEnd` | Buff完全销毁 |

## 事件系统

### 全局事件

```csharp
BuffEventSystem.OnBuffAdded += (sender, e) => {
    Debug.Log($"Buff添加: {e.Buff.Name}");
};

BuffEventSystem.OnBuffRemoved += (sender, e) => {
    Debug.Log($"Buff移除: {e.Buff.Name}");
};

BuffEventSystem.OnStackChanged += (sender, e) => {
    Debug.Log($"层数变化: {e.Buff.Name} {e.OldStack} -> {e.NewStack}");
};
```

### 本地事件

```csharp
buffOwner.LocalEvents.OnBuffAdded += (sender, e) => {
    // 只有这个持有者的Buff添加时触发
};
```

### Buff事件接收

持有者可以实现 `IBuffEventReceiver` 接口来接收Buff发送的事件：

```csharp
public class Character : MonoBehaviour, IBuffOwner, IBuffEventReceiver
{
    // ...
    
    public void OnBuffEvent(IBuff buff, string eventName, object data)
    {
        // 处理Buff发送的事件
        if (eventName == "Stun")
        {
            // 处理眩晕效果
        }
    }
}
```

## 纯代码使用

不依赖MonoBehaviour的使用方式：

```csharp
// 实现IBuffOwner
public class GameCharacter : IBuffOwner
{
    public int OwnerId { get; private set; }
    public string OwnerName { get; private set; }
    public IBuffContainer BuffContainer { get; private set; }
    
    public GameCharacter(string name)
    {
        OwnerId = GenerateId();
        OwnerName = name;
        BuffContainer = new BuffContainer(this);
    }
    
    public void OnBuffEvent(BuffEventType eventType, IBuff buff)
    {
        // 处理Buff事件
    }
    
    public void Update(float deltaTime)
    {
        // 手动更新Buff
        BuffContainer.Update(deltaTime);
    }
}

// 使用
var character = new GameCharacter("Player");
BuffApi.AddBuff(1001, character);
```

## 配置说明

### BuffDataSO 配置项

| 配置项 | 说明 |
| ------ | ------ |
| ID | Buff唯一标识符 |
| 名称 | Buff显示名称 |
| 描述 | Buff详细描述 |
| 效果类型 | Neutral/Buff/Debuff/Special |
| 唯一性 | 同类型是否只能存在一个 |
| 叠加模式 | None/Stackable/Independent |
| 最大层数 | Buff最高可叠加层数 |
| 每层添加数量 | 每次添加时增加的层数 |
| 是否永久 | 是否为永久Buff |
| 持续时间 | Buff持续时间（秒） |
| 可刷新 | 重新添加时是否刷新时间 |
| 移除模式 | Remove（直接移除）/Reduce（逐层移除） |
| 每层移除数量 | 每次移除时减少的层数 |
| 移除间隔 | 逐层移除时的间隔时间（秒） |
| 逻辑脚本 | Buff的自定义逻辑脚本 |

### BuffSystemConfig 全局配置

| 配置项 | 说明 |
| ------ | ------ |
| DefaultPoolCapacity | 默认对象池容量 |
| MaxPoolSize | 对象池最大容量 |
| UpdateMode | 更新模式（EveryFrame/Interval/Manual） |
| EnableDebugLog | 是否启用调试日志 |

## 扩展开发

### 创建自定义Buff逻辑

1. 继承 `BuffLogicBase`
2. 实现需要的生命周期接口
3. 在BuffDataSO中配置

```csharp
[System.Serializable]
public class StunBuffLogic : BuffLogicBase, IBuffAcquire, IBuffRemove
{
    public void OnAcquire()
    {
        // 眩晕目标
        if (TryGetOwnerComponent<CharacterController>(out var controller))
        {
            controller.IsStunned = true;
        }
        
        // 发送事件给持有者
        SendEvent("Stun", true);
    }
    
    public void OnRemove()
    {
        // 解除眩晕
        if (TryGetOwnerComponent<CharacterController>(out var controller))
        {
            controller.IsStunned = false;
        }
        
        // 发送事件给持有者
        SendEvent("Stun", false);
    }
}
```

### 高级API使用

```csharp
// 获取Buff数据
IBuffData buffData = BuffApi.GetBuffData(1001);

// 检查Buff数据是否存在
if (BuffApi.HasBuffData("燃烧"))
{
    // Buff数据存在
}

// 获取所有Buff数据
IEnumerable<IBuffData> allBuffs = BuffApi.GetAllBuffData();

// 增加Buff层数
BuffApi.AddStack(buff, 1);

// 减少Buff层数
BuffApi.RemoveStack(buff, 1);

// 刷新Buff持续时间
BuffApi.RefreshBuff(buff);
```

## 许可证

MIT License
