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

// 添加Buff
BuffAPI.AddBuff(1001, owner);
BuffAPI.AddBuff("燃烧", owner, damageSource);

// 查询Buff
bool hasBuff = BuffAPI.HasBuff(1001, owner);
IBuff buff = BuffAPI.GetBuff(1001, owner);

// 移除Buff
BuffAPI.RemoveBuff(1001, owner);
BuffAPI.ClearBuffs(owner);
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

### IBuffData - Buff数据

Buff的配置数据，使用ScriptableObject实现：

```csharp
public interface IBuffData
{
    int Id { get; }
    string Name { get; }
    float Duration { get; }
    BuffStackMode StackMode { get; }
    // ...
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
BuffAPI.AddBuff(1001, character);
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
| 持续时间 | Buff持续时间（秒） |
| 可刷新 | 重新添加时是否刷新时间 |
| 移除模式 | Remove（直接移除）/Reduce（逐层移除） |

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
    }
    
    public void OnRemove()
    {
        // 解除眩晕
        if (TryGetOwnerComponent<CharacterController>(out var controller))
        {
            controller.IsStunned = false;
        }
    }
}
```

## 许可证

MIT License
