# Transmission系统使用文档

> Buff传播系统 - 实现Buff在不同目标间传播、传染、连锁的机制

---

## 📖 概述

Transmission系统允许Buff在多个目标之间传播，适用于传染病、连锁反应、范围效果等场景。

**典型应用场景:**
- 🦠 传染病：病毒从感染者传播给附近的人
- ⚡ 连锁闪电：闪电在敌人之间跳跃
- 🔥 火势蔓延：火焰从一个人传给另一个人
- 📢 信息传播：增益效果传递给队友

---

## 🚀 快速开始

### 1. 实现可传播接口

```csharp
using BuffSystem.Advanced.Transmission;

public class VirusBuff : BuffLogicBase, IBuffTransmissible
{
    // 传播配置
    public TransmissionMode Mode => TransmissionMode.Range;
    public float TransmissionRange => 5f;
    public int MaxTransmissionChain => 3;
    public int CurrentChainLength { get; set; }
    
    // 获取传播目标
    public IEnumerable<IBuffOwner> GetTransmissionTargets(IBuff buff)
    {
        // 获取范围内的所有敌人
        return Physics.OverlapSphere(buff.Owner.Position, TransmissionRange)
            .Select(c => c.GetComponent<IBuffOwner>())
            .Where(o => o != null && o != buff.Owner);
    }
    
    // 检查是否可以传播
    public bool CanTransmit(IBuff buff, IBuffOwner target)
    {
        // 检查目标是否已有此Buff
        return !target.BuffContainer.HasBuff(buff.DataId);
    }
    
    // 执行传播
    public void OnTransmit(IBuff buff, IBuffOwner source, IBuffOwner target)
    {
        // 传播给目标
        var newBuff = BuffApi.AddBuff(buff.DataId, target, this);
        
        if (newBuff is IBuffTransmissible transmissible)
        {
            transmissible.CurrentChainLength = CurrentChainLength + 1;
        }
        
        Debug.Log($"病毒从 {source.OwnerName} 传播到 {target.OwnerName}");
    }
}
```

### 2. 请求传播检查

```csharp
public override void OnUpdate(IBuff buff, float deltaTime)
{
    // 定期请求传播检查
    if (Time.time % 1f < deltaTime)  // 每秒检查一次
    {
        BuffSystemManager.Transmission.RequestTransmission(buff);
    }
}
```

### 3. 完成！

系统会自动处理传播队列，在Update中处理传播请求。

---

## 📚 传播模式

### Contact（接触传播）

```csharp
public class ContactVirus : IBuffTransmissible
{
    public TransmissionMode Mode => TransmissionMode.Contact;
    
    public IEnumerable<IBuffOwner> GetTransmissionTargets(IBuff buff)
    {
        // 获取接触的敌人（通过碰撞检测）
        return GetTouchingEnemies(buff.Owner);
    }
}
```

### Range（范围传播）

```csharp
public class RangeVirus : IBuffTransmissible
{
    public TransmissionMode Mode => TransmissionMode.Range;
    public float TransmissionRange => 10f;
    
    public IEnumerable<IBuffOwner> GetTransmissionTargets(IBuff buff)
    {
        // 获取范围内的敌人
        return Physics.OverlapSphere(buff.Owner.Position, TransmissionRange)
            .Select(c => c.GetComponent<IBuffOwner>())
            .Where(o => o != null);
    }
}
```

### Chain（连锁传播）

```csharp
public class ChainLightning : IBuffTransmissible
{
    public TransmissionMode Mode => TransmissionMode.Chain;
    public int MaxTransmissionChain => 5;  // 最多连锁5次
    public int CurrentChainLength { get; set; }
    public float ChainRange => 8f;
    
    public IEnumerable<IBuffOwner> GetTransmissionTargets(IBuff buff)
    {
        if (CurrentChainLength >= MaxTransmissionChain)
            return Enumerable.Empty<IBuffOwner>();
        
        // 获取最近的未感染目标
        return FindNearestEnemies(buff.Owner, ChainRange, 1);
    }
}
```

### Inheritance（继承传播）

```csharp
public class InheritedCurse : IBuffTransmissible
{
    public TransmissionMode Mode => TransmissionMode.Inheritance;
    
    public IEnumerable<IBuffOwner> GetTransmissionTargets(IBuff buff)
    {
        // 传播给召唤者/主人
        if (buff.Owner is Minion minion && minion.Master != null)
        {
            return new[] { minion.Master };
        }
        return Enumerable.Empty<IBuffOwner>();
    }
}
```

---

## 🎯 传播事件

```csharp
void Start()
{
    // 监听传播事件
    TransmissionEventSystem.OnTransmissionStarted += OnTransmissionStarted;
    TransmissionEventSystem.OnTransmissionCompleted += OnTransmissionCompleted;
    TransmissionEventSystem.OnChainTransmission += OnChainTransmission;
}

void OnTransmissionStarted(object sender, TransmissionEventArgs e)
{
    Debug.Log($"传播开始: {e.Buff.Name} -> {e.Target.OwnerName}");
}

void OnTransmissionCompleted(object sender, TransmissionEventArgs e)
{
    Debug.Log($"传播完成: {e.Buff.Name} 已传播到 {e.Target.OwnerName}");
}

void OnChainTransmission(object sender, ChainTransmissionEventArgs e)
{
    Debug.Log($"连锁传播: 第{e.ChainLength}跳，从{e.Source.OwnerName}到{e.Target.OwnerName}");
}
```

---

## 💡 完整示例

### 示例1: 瘟疫系统

```csharp
public class PlagueBuff : BuffLogicBase, IBuffTransmissible
{
    [SerializeField] private float spreadInterval = 2f;
    [SerializeField] private float spreadRange = 6f;
    [SerializeField] private int maxSpreadChain = 4;
    
    private float lastSpreadTime;
    
    public TransmissionMode Mode => TransmissionMode.Range;
    public float TransmissionRange => spreadRange;
    public int MaxTransmissionChain => maxSpreadChain;
    public int CurrentChainLength { get; set; }
    
    public override void OnUpdate(IBuff buff, float deltaTime)
    {
        // 定期传播
        if (Time.time - lastSpreadTime >= spreadInterval)
        {
            BuffSystemManager.Transmission.RequestTransmission(buff);
            lastSpreadTime = Time.time;
        }
    }
    
    public IEnumerable<IBuffOwner> GetTransmissionTargets(IBuff buff)
    {
        return Physics.OverlapSphere(buff.Owner.Position, spreadRange)
            .Select(c => c.GetComponent<IBuffOwner>())
            .Where(o => o != null 
                && o != buff.Owner 
                && !o.BuffContainer.HasBuff(buff.DataId));
    }
    
    public bool CanTransmit(IBuff buff, IBuffOwner target)
    {
        // 检查目标是否免疫
        if (target.IsImmuneTo(buff.DataId))
            return false;
        
        // 检查连锁次数
        if (CurrentChainLength >= maxSpreadChain)
            return false;
        
        return true;
    }
    
    public void OnTransmit(IBuff buff, IBuffOwner source, IBuffOwner target)
    {
        var newBuff = BuffApi.AddBuff(buff.DataId, target, this);
        
        if (newBuff is IBuffTransmissible transmissible)
        {
            transmissible.CurrentChainLength = CurrentChainLength + 1;
        }
        
        // 播放传播特效
        PlaySpreadEffect(source, target);
    }
}
```

### 示例2: 连锁闪电

```csharp
public class ChainLightningBuff : BuffLogicBase, IBuffTransmissible
{
    public TransmissionMode Mode => TransmissionMode.Chain;
    public int MaxTransmissionChain => 5;
    public int CurrentChainLength { get; set; }
    public float ChainRange => 8f;
    
    public override void OnApply(IBuff buff)
    {
        // 立即开始连锁
        BuffSystemManager.Transmission.RequestTransmission(buff);
    }
    
    public IEnumerable<IBuffOwner> GetTransmissionTargets(IBuff buff)
    {
        if (CurrentChainLength >= MaxTransmissionChain)
            return Enumerable.Empty<IBuffOwner>();
        
        // 找到最近的敌人
        var enemies = FindObjectsOfType<Enemy>()
            .Where(e => e != buff.Owner)
            .Where(e => Vector3.Distance(e.Position, buff.Owner.Position) <= ChainRange)
            .Where(e => !e.BuffContainer.HasBuff(buff.DataId))
            .OrderBy(e => Vector3.Distance(e.Position, buff.Owner.Position));
        
        return enemies.Take(1).Cast<IBuffOwner>();
    }
    
    public void OnTransmit(IBuff buff, IBuffOwner source, IBuffOwner target)
    {
        // 造成伤害
        if (target is IDamageable damageable)
        {
            float damage = 100 * Mathf.Pow(0.8f, CurrentChainLength);  // 每次衰减20%
            damageable.TakeDamage(damage);
        }
        
        // 播放闪电特效
        PlayLightningEffect(source, target);
        
        // 继续连锁
        var newBuff = BuffApi.AddBuff(buff.DataId, target, this);
        if (newBuff is IBuffTransmissible transmissible)
        {
            transmissible.CurrentChainLength = CurrentChainLength + 1;
        }
        BuffSystemManager.Transmission.RequestTransmission(newBuff);
    }
}
```

---

## 📊 性能优化

### 1. 限制传播频率

```csharp
public override void OnUpdate(IBuff buff, float deltaTime)
{
    // 不要每帧都请求传播
    if (Time.time % spreadInterval < deltaTime)
    {
        BuffSystemManager.Transmission.RequestTransmission(buff);
    }
}
```

### 2. 使用分层更新

```csharp
// 在BuffData中设置更新频率
updateMode = UpdateMode.Interval;
updateInterval = 1f;  // 每秒更新一次
```

### 3. 优化目标搜索

```csharp
public IEnumerable<IBuffOwner> GetTransmissionTargets(IBuff buff)
{
    // 使用Physics.OverlapSphereNonAlloc减少GC
    var colliders = new Collider[20];
    int count = Physics.OverlapSphereNonAlloc(
        buff.Owner.Position, 
        TransmissionRange, 
        colliders
    );
    
    for (int i = 0; i < count; i++)
    {
        if (colliders[i].TryGetComponent<IBuffOwner>(out var owner))
        {
            yield return owner;
        }
    }
}
```

---

## 🐛 调试技巧

```csharp
// 可视化传播范围
void OnDrawGizmos()
{
    if (buff?.Owner != null)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(buff.Owner.Position, TransmissionRange);
    }
}

// 打印传播链
void PrintTransmissionChain(IBuff buff)
{
    if (buff is IBuffTransmissible transmissible)
    {
        Debug.Log($"传播链长度: {transmissible.CurrentChainLength}");
        Debug.Log($"最大传播次数: {transmissible.MaxTransmissionChain}");
    }
}
```

---

## 📚 相关文档

- [Combo系统文档](ComboSystem.md)
- [Fusion系统文档](FusionSystem.md)
- [API参考文档](../API_REFERENCE.md)

---

**小心传染！** 🦠
