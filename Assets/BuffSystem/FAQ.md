# 常见问题 (FAQ)

本文档整理了 BuffSystem 的常见问题及解决方案。

## 目录

- [基础问题](#基础问题)
- [使用问题](#使用问题)
- [性能问题](#性能问题)
- [扩展开发](#扩展开发)
- [调试技巧](#调试技巧)

---

## 基础问题

### Q: 如何安装 BuffSystem？

**A:** 将 `BuffSystem` 文件夹复制到你的 Unity 项目的 `Assets` 目录下即可。不需要额外的依赖。

### Q: BuffSystem 支持哪些 Unity 版本？

**A:** 支持 Unity 2021.3 LTS 及更高版本。使用 .NET Standard 2.1。

### Q: 是否需要初始化？

**A:** BuffSystem 会在首次使用时自动初始化，但建议在游戏启动时手动调用：

```csharp
void Start()
{
    BuffApi.Initialize();
}
```

### Q: 如何重新加载 Buff 数据？

**A:** 调用 `BuffApi.ReloadData()`：

```csharp
// 运行时重新加载配置
BuffApi.ReloadData();
```

或者在编辑器中点击菜单 `Tools -> BuffSystem -> Reload Buff Database`。

---

## 使用问题

### Q: 如何创建一个简单的 Buff？

**A:** 最简单的 Buff 只需要配置数据：

1. 右键 -> Create -> BuffSystem -> Buff Data
2. 配置 ID、名称、持续时间
3. 挂载 BuffOwner 组件
4. 使用 `BuffApi.AddBuff(id, owner)` 添加

### Q: 如何实现永久 Buff？

**A:** 在 BuffDataSO 中勾选 "Is Permanent"：

```csharp
// 或者在代码中检查
if (buff.IsPermanent)
{
    // 永久 Buff 不会过期
}
```

### Q: 如何实现可叠加的 Buff？

**A:** 
1. 设置 `StackMode` 为 `Stackable`
2. 设置 `MaxStack` 为最大层数
3. 设置 `AddStackCount` 为每次添加的层数

```csharp
// 每次添加会增加层数
BuffApi.AddBuff(1001, owner); // 层数 +1
BuffApi.AddBuff(1001, owner); // 层数 +1
```

### Q: 如何实现独立来源的 Buff？

**A:** 使用 `Independent` 叠加模式：

```csharp
// 配置：StackMode = Independent

// 不同来源的 Buff 可以共存
BuffApi.AddBuff(1001, player, skill1); // 创建实例 1
BuffApi.AddBuff(1001, player, skill2); // 创建实例 2

// 查询特定来源的 Buff
var buff = BuffApi.GetBuff(1001, player, skill1);
```

### Q: 如何手动更新 Buff？

**A:** 

1. 设置 UpdateMode 为 Manual：
```csharp
// 在 BuffSystemConfig 中设置
UpdateMode = UpdateMode.Manual
```

2. 手动调用 Update：
```csharp
// 每帧调用
buffContainer.Update(Time.deltaTime);
```

### Q: 如何移除特定来源的所有 Buff？

**A:**

```csharp
// 当技能结束时，移除该技能添加的所有 Buff
BuffApi.RemoveBuffBySource(skillInstance, player);
```

### Q: 如何监听 Buff 事件？

**A:**

**全局事件：**
```csharp
BuffEventSystem.OnBuffAdded += (sender, e) => {
    Debug.Log($"Buff 添加: {e.Buff.Name}");
};
```

**本地事件：**
```csharp
buffOwner.LocalEvents.OnBuffAdded += (sender, e) => {
    // 只有这个持有者的 Buff 添加时触发
};
```

### Q: 如何在 Buff 逻辑中获取持有者组件？

**A:**

```csharp
public class MyBuffLogic : BuffLogicBase, IBuffAcquire
{
    public void OnAcquire()
    {
        // 尝试获取组件
        if (TryGetOwnerComponent<HealthSystem>(out var health))
        {
            health.TakeDamage(10);
        }
    }
}
```

### Q: 如何发送自定义事件给持有者？

**A:**

```csharp
// 在 BuffLogic 中
public void OnAcquire()
{
    SendEvent("CustomEvent", eventData);
}

// 在持有者中接收
public void OnBuffEvent(IBuff buff, string eventName, object data)
{
    if (eventName == "CustomEvent")
    {
        // 处理事件
    }
}
```

---

## 性能问题

### Q: BuffSystem 的性能如何？

**A:** BuffSystem 针对性能做了以下优化：
- 对象池复用 Buff 实例
- 缓存 Buff 列表避免 GC
- 支持批量更新模式
- 字典索引快速查询

### Q: 如何优化大量 Buff 的性能？

**A:**

1. **使用 Interval 更新模式：**
```csharp
// BuffSystemConfig
UpdateMode = UpdateMode.Interval;
UpdateInterval = 0.1f; // 每 0.1 秒更新一次
```

2. **调整对象池大小：**
```csharp
DefaultPoolCapacity = 64;  // 根据需求调整
MaxPoolSize = 256;
```

3. **避免频繁查询：**
```csharp
// 不好的做法：每帧查询
void Update()
{
    if (BuffApi.HasBuff(1001, player)) // 每帧都查询
    {
        // ...
    }
}

// 好的做法：缓存结果
private bool hasBurningBuff;

void OnBuffAdded(object sender, BuffAddedEventArgs e)
{
    if (e.Buff.DataId == 1001)
    {
        hasBurningBuff = true;
    }
}
```

### Q: 对象池满了会怎样？

**A:** 当对象池达到 MaxPoolSize 时，新释放的对象会被销毁而不是归还池中。可以通过以下方式调整：

```csharp
// 在 BuffSystemConfig 中增大 MaxPoolSize
MaxPoolSize = 256; // 默认是 128
```

### Q: 如何减少 GC？

**A:**

1. 使用对象池
2. 避免在 Update 中创建对象
3. 使用缓存
4. 使用 Struct 代替 Class（小心装箱）

```csharp
// 避免这样
void Update()
{
    var list = new List<IBuff>(); // 每帧创建
    // ...
}

// 应该这样
private static readonly List<IBuff> buffCache = new();

void Update()
{
    buffCache.Clear(); // 清空复用
    // ...
}
```

---

## 扩展开发

### Q: 如何创建自定义 Buff 逻辑？

**A:**

1. 继承 `BuffLogicBase`
2. 实现需要的生命周期接口
3. 标记为 `[System.Serializable]`

```csharp
[System.Serializable]
public class MyBuffLogic : BuffLogicBase, IBuffAcquire, IBuffRemove
{
    [SerializeField] private float damage = 10f;
    
    public void OnAcquire()
    {
        // Buff 被添加时
    }
    
    public void OnRemove()
    {
        // Buff 被移除时
    }
}
```

### Q: 如何创建自定义 Effect？

**A:**

```csharp
[System.Serializable]
public class MyEffect : EffectBase
{
    [SerializeField] private float value = 10f;
    
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

### Q: 自定义 BuffLogic 在 Inspector 中不显示？

**A:** 检查以下几点：

1. 是否继承自 `BuffLogicBase`
2. 是否标记 `[System.Serializable]`
3. 是否是公共类
4. 是否有公共无参构造函数

```csharp
// 正确的写法
namespace MyNamespace
{
    [System.Serializable]
    public class MyBuffLogic : BuffLogicBase
    {
        // 必须有公共无参构造函数
        public MyBuffLogic() { }
    }
}
```

### Q: 如何实现条件触发 Buff？

**A:**

```csharp
public class ConditionalBuffSystem : MonoBehaviour
{
    [SerializeField] private BuffOwner buffOwner;
    [SerializeField] private int buffId;
    [SerializeField] private float healthThreshold = 0.3f;
    
    private HealthSystem healthSystem;
    
    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
        healthSystem.OnHealthChanged += OnHealthChanged;
    }
    
    void OnHealthChanged(float currentHealth, float maxHealth)
    {
        float healthPercent = currentHealth / maxHealth;
        
        // 低血量时添加狂暴 Buff
        if (healthPercent < healthThreshold && !BuffApi.HasBuff(buffId, buffOwner))
        {
            BuffApi.AddBuff(buffId, buffOwner);
        }
        // 血量恢复时移除
        else if (healthPercent >= healthThreshold && BuffApi.HasBuff(buffId, buffOwner))
        {
            BuffApi.RemoveBuff(buffId, buffOwner);
        }
    }
}
```

### Q: 如何实现 Buff 组合效果？

**A:**

```csharp
public class BuffComboSystem : MonoBehaviour
{
    [SerializeField] private BuffOwner buffOwner;
    
    // 燃烧 + 冰冻 = 蒸汽
    void CheckCombo()
    {
        bool hasBurning = BuffApi.HasBuff(1001, buffOwner); // 燃烧
        bool hasFrozen = BuffApi.HasBuff(1002, buffOwner);  // 冰冻
        
        if (hasBurning && hasFrozen)
        {
            // 移除燃烧和冰冻
            BuffApi.RemoveBuff(1001, buffOwner);
            BuffApi.RemoveBuff(1002, buffOwner);
            
            // 添加蒸汽
            BuffApi.AddBuff(1003, buffOwner); // 蒸汽
        }
    }
}
```

---

## 调试技巧

### Q: 如何调试 Buff 问题？

**A:**

1. **启用调试日志：**
```csharp
// BuffSystemConfig
EnableDebugLog = true;
```

2. **使用 BuffOwner 的调试功能：**
```csharp
// 在 Inspector 中勾选
Show Debug Info = true;
```

3. **监听所有事件：**
```csharp
void OnEnable()
{
    BuffEventSystem.OnBuffAdded += (s, e) => Debug.Log($"[DEBUG] Added: {e.Buff.Name}");
    BuffEventSystem.OnBuffRemoved += (s, e) => Debug.Log($"[DEBUG] Removed: {e.Buff.Name}");
    BuffEventSystem.OnStackChanged += (s, e) => Debug.Log($"[DEBUG] Stack: {e.Buff.Name} {e.OldStack}->{e.NewStack}");
}
```

### Q: 运行时查看 Buff 状态？

**A:** 在 Play 模式下，选中带有 BuffOwner 的 GameObject，Inspector 会显示：
- 当前所有 Buff
- 层数和剩余时间
- 移除和刷新按钮

### Q: Buff 不生效怎么办？

**A:** 检查清单：

1. BuffOwner 是否正确初始化？
2. Buff 数据配置是否正确？
3. Buff ID 是否正确？
4. BuffLogic 是否正确配置？
5. 是否被其他系统移除了？

```csharp
// 调试代码
var buff = BuffApi.AddBuff(1001, owner);
if (buff == null)
{
    Debug.LogError("Buff 创建失败！");
    // 检查配置是否存在
    var data = BuffApi.GetBuffData(1001);
    if (data == null)
    {
        Debug.LogError("Buff 数据不存在！");
    }
}
```

### Q: 如何打印 Buff 详细信息？

**A:**

```csharp
void PrintBuffInfo(IBuff buff)
{
    Debug.Log($"=== Buff Info ===");
    Debug.Log($"Instance ID: {buff.InstanceId}");
    Debug.Log($"Data ID: {buff.DataId}");
    Debug.Log($"Name: {buff.Name}");
    Debug.Log($"Stack: {buff.CurrentStack}/{buff.MaxStack}");
    Debug.Log($"Duration: {buff.Duration:F2}/{buff.TotalDuration:F2}");
    Debug.Log($"Remaining: {buff.RemainingTime:F2}");
    Debug.Log($"IsPermanent: {buff.IsPermanent}");
    Debug.Log($"Source: {buff.Source}");
    Debug.Log($"Owner: {buff.Owner.OwnerName}");
}
```

---

## 其他问题

### Q: 如何报告 Bug？

**A:** 请通过 GitHub Issues 报告，包含：
1. 问题描述
2. 复现步骤
3. 期望行为和实际行为
4. Unity 版本和 BuffSystem 版本
5. 相关代码和截图

### Q: 如何请求新功能？

**A:** 请通过 GitHub Issues 提交 Feature Request，说明：
1. 功能描述
2. 使用场景
3. 期望的 API 设计

### Q: 如何贡献代码？

**A:** 请阅读 [CONTRIBUTING.md](CONTRIBUTING.md) 了解贡献流程。

### Q: 有示例项目吗？

**A:** 示例项目正在开发中，请关注仓库更新。

---

## 问题未解决？

如果以上 FAQ 没有解决你的问题：

1. 查阅 [API 文档](API.md)
2. 查阅 [教程](Tutorial.md)
3. 查阅 [架构文档](Architecture.md)
4. 提交 GitHub Issue

我们会在 48 小时内回复。
