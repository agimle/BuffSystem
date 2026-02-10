# BuffSystem 使用教程

本文档提供从入门到进阶的完整使用教程。

## 目录

- [基础教程](#基础教程)
- [进阶教程](#进阶教程)
- [实战案例](#实战案例)
- [最佳实践](#最佳实践)

---

## 基础教程

### 第 1 课：快速开始

#### 1.1 安装 BuffSystem

1. 将 `BuffSystem` 文件夹复制到项目的 `Assets` 目录
2. 等待 Unity 编译完成
3. 开始使用！

#### 1.2 创建第一个 Buff

**步骤 1：创建 Buff 数据配置**

1. 在 Project 窗口中，右键点击 -> Create -> BuffSystem -> Buff Data
2. 命名为 "BurningBuff"
3. 配置参数：
   - ID: 1001
   - 名称: 燃烧
   - 描述: 每秒造成火焰伤害
   - 效果类型: Debuff
   - 是否唯一: ✓
   - 叠加模式: Stackable
   - 最大层数: 5
   - 每层添加数量: 1
   - 是否永久: ✗
   - 持续时间: 5
   - 可刷新: ✓
   - 移除模式: Reduce
   - 每层移除数量: 1
   - 移除间隔: 1

**步骤 2：挂载 BuffOwner**

1. 在场景中创建一个 Cube（或其他 GameObject）
2. 添加 `BuffOwner` 组件
3. 勾选 "Auto Initialize"

**步骤 3：编写测试脚本**

```csharp
using UnityEngine;
using BuffSystem.Core;

public class BuffTest : MonoBehaviour
{
    [SerializeField] private BuffOwner buffOwner;
    
    void Update()
    {
        // 按空格键添加 Buff
        if (Input.GetKeyDown(KeyCode.Space))
        {
            BuffApi.AddBuff(1001, buffOwner);
            Debug.Log("添加了燃烧 Buff！");
        }
        
        // 按 R 键移除 Buff
        if (Input.GetKeyDown(KeyCode.R))
        {
            BuffApi.RemoveBuff(1001, buffOwner);
            Debug.Log("移除了燃烧 Buff！");
        }
    }
}
```

4. 将脚本挂载到 Cube 上
5. 将 BuffOwner 拖到脚本的序列化字段
6. 运行游戏，按空格键测试！

---

### 第 2 课：理解核心概念

#### 2.1 Buff 的生命周期

```
创建 -> 初始化 -> 添加层数 -> 获得事件 -> 逻辑更新 -> ... -> 标记移除 -> 移除事件 -> 销毁
```

**关键时间点：**
- **OnStart**: Buff 逻辑初始化完成
- **OnAcquire**: Buff 被添加到持有者
- **OnLogicUpdate**: 每帧逻辑更新
- **OnVisualUpdate**: 每帧表现更新（UI、特效）
- **OnRefresh**: Buff 持续时间刷新
- **OnStackChanged**: 层数变化
- **OnReduce**: 层数减少
- **OnRemove**: Buff 被标记移除
- **OnEnd**: Buff 完全销毁

#### 2.2 叠加模式详解

**None（不可叠加）**
```
已有燃烧 Buff，再次添加：
- 如果 CanRefresh = true: 刷新持续时间
- 如果 CanRefresh = false: 忽略新 Buff
```

**Stackable（可叠加）**
```
已有 2 层燃烧，再次添加（AddStackCount = 1）：
- 层数变为 3
- 如果 CanRefresh = true: 同时刷新持续时间
```

**Independent（独立）**
```
已有燃烧 Buff，再次添加：
- 创建新的 Buff 实例
- 两个 Buff 独立存在、独立计时
- 适用于不同来源的同类型 Buff
```

#### 2.3 移除模式详解

**Remove（直接移除）**
```
持续时间结束：
- 直接标记为移除
- 立即消失
```

**Reduce（逐层移除）**
```
持续时间结束：
- 每 RemoveInterval 秒减少 RemoveStackCount 层
- 层数归零后才真正移除
- 适用于持续衰减的效果
```

---

### 第 3 课：使用事件系统

#### 3.1 监听全局事件

```csharp
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Events;

public class GlobalBuffListener : MonoBehaviour
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
        Debug.Log($"[全局] {e.Buff.Owner.OwnerName} 获得了 {e.Buff.Name}");
    }
    
    void OnBuffRemoved(object sender, BuffRemovedEventArgs e)
    {
        Debug.Log($"[全局] {e.Buff.Name} 从 {e.Buff.Owner.OwnerName} 移除");
    }
    
    void OnStackChanged(object sender, BuffStackChangedEventArgs e)
    {
        Debug.Log($"[全局] {e.Buff.Name} 层数: {e.OldStack} -> {e.NewStack}");
    }
}
```

#### 3.2 监听本地事件

```csharp
using UnityEngine;
using BuffSystem.Core;
using BuffSystem.Events;

public class LocalBuffListener : MonoBehaviour
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
        // 只在这个角色获得 Buff 时触发
        ShowBuffIcon(e.Buff);
    }
    
    void OnBuffRemoved(object sender, BuffRemovedEventArgs e)
    {
        HideBuffIcon(e.Buff);
    }
    
    void ShowBuffIcon(IBuff buff)
    {
        // 在 UI 上显示 Buff 图标
        Debug.Log($"显示图标: {buff.Name}");
    }
    
    void HideBuffIcon(IBuff buff)
    {
        // 隐藏 Buff 图标
        Debug.Log($"隐藏图标: {buff.Name}");
    }
}
```

---

## 进阶教程

### 第 4 课：创建自定义 Buff 逻辑

#### 4.1 编写 Buff 逻辑脚本

创建一个燃烧伤害 Buff：

```csharp
using UnityEngine;
using BuffSystem.Core;

namespace MyGame.Buffs
{
    [System.Serializable]
    public class BurningBuffLogic : BuffLogicBase, 
        IBuffAcquire, IBuffLogicUpdate, IBuffStackChange, IBuffRemove
    {
        [SerializeField] private float damagePerSecond = 5f;
        [SerializeField] private float damagePerStack = 2f;
        
        private float damageTimer;
        private float currentDamage;
        
        // Buff 被添加时
        public void OnAcquire()
        {
            CalculateDamage();
            
            // 发送事件给持有者
            SendEvent("BurningStarted", currentDamage);
            
            Debug.Log($"[{Owner.OwnerName}] 开始燃烧，每秒伤害: {currentDamage}");
        }
        
        // 每帧逻辑更新
        public void OnLogicUpdate(float deltaTime)
        {
            damageTimer += deltaTime;
            
            // 每秒造成伤害
            if (damageTimer >= 1f)
            {
                ApplyDamage();
                damageTimer -= 1f;
            }
        }
        
        // 层数变化时
        public void OnStackChanged(int oldStack, int newStack)
        {
            CalculateDamage();
            Debug.Log($"[{Owner.OwnerName}] 燃烧层数变化: {oldStack} -> {newStack}, 新伤害: {currentDamage}");
        }
        
        // Buff 被移除时
        public void OnRemove()
        {
            SendEvent("BurningEnded", null);
            Debug.Log($"[{Owner.OwnerName}] 燃烧结束");
        }
        
        private void CalculateDamage()
        {
            currentDamage = damagePerSecond + (CurrentStack - 1) * damagePerStack;
        }
        
        private void ApplyDamage()
        {
            // 尝试获取 HealthComponent
            if (TryGetOwnerComponent<HealthComponent>(out var health))
            {
                health.TakeDamage(currentDamage);
                Debug.Log($"[{Owner.OwnerName}] 受到燃烧伤害: {currentDamage}");
            }
        }
    }
}
```

#### 4.2 配置 Buff 使用自定义逻辑

1. 选中之前创建的 BurningBuff 配置
2. 在 Inspector 中找到 "逻辑脚本" 字段
3. 选择 `MyGame.Buffs.BurningBuffLogic`
4. 配置参数：
   - Damage Per Second: 5
   - Damage Per Stack: 2

#### 4.3 接收 Buff 事件

在角色脚本中接收 Buff 发送的事件：

```csharp
using UnityEngine;
using BuffSystem.Core;

public class PlayerCharacter : MonoBehaviour, IBuffOwner, IBuffEventReceiver
{
    [SerializeField] private BuffOwner buffOwner;
    
    public int OwnerId => buffOwner.OwnerId;
    public string OwnerName => buffOwner.OwnerName;
    public IBuffContainer BuffContainer => buffOwner.BuffContainer;
    
    public void OnBuffEvent(BuffEventType eventType, IBuff buff)
    {
        // 处理系统事件
    }
    
    // 接收 Buff 发送的自定义事件
    public void OnBuffEvent(IBuff buff, string eventName, object data)
    {
        switch (eventName)
        {
            case "BurningStarted":
                float damage = (float)data;
                Debug.Log($"开始燃烧！每秒伤害: {damage}");
                // 播放燃烧特效
                PlayBurningEffect();
                break;
                
            case "BurningEnded":
                Debug.Log("燃烧结束！");
                // 停止燃烧特效
                StopBurningEffect();
                break;
        }
    }
    
    void PlayBurningEffect()
    {
        // 实例化火焰特效
    }
    
    void StopBurningEffect()
    {
        // 销毁火焰特效
    }
}
```

---

### 第 5 课：使用 EffectBasedBuffLogic

如果不想编写代码，可以使用 `EffectBasedBuffLogic` 在 Inspector 中配置效果。

#### 5.1 创建自定义 Effect

```csharp
using UnityEngine;
using BuffSystem.Core;

namespace MyGame.Effects
{
    [System.Serializable]
    public class DamageEffect : EffectBase
    {
        [SerializeField] private float damageAmount = 10f;
        [SerializeField] private bool isPercent = false;
        
        public override void Execute(IBuff buff)
        {
            if (TryGetOwnerComponent<HealthComponent>(buff, out var health))
            {
                float finalDamage = isPercent 
                    ? health.MaxHealth * damageAmount / 100f 
                    : damageAmount;
                    
                health.TakeDamage(finalDamage);
                
                Debug.Log($"[{buff.Owner.OwnerName}] 受到 {finalDamage} 点伤害");
            }
        }
        
        public override void Cancel(IBuff buff)
        {
            // 伤害效果不需要取消
        }
    }
    
    [System.Serializable]
    public class HealEffect : EffectBase
    {
        [SerializeField] private float healAmount = 10f;
        
        public override void Execute(IBuff buff)
        {
            if (TryGetOwnerComponent<HealthComponent>(buff, out var health))
            {
                health.Heal(healAmount);
                Debug.Log($"[{buff.Owner.OwnerName}] 恢复 {healAmount} 点生命");
            }
        }
        
        public override void Cancel(IBuff buff)
        {
            // 治疗不需要取消
        }
    }
    
    [System.Serializable]
    public class ModifySpeedEffect : EffectBase
    {
        [SerializeField] private float speedMultiplier = 1.5f;
        
        private float originalSpeed;
        
        public override void Execute(IBuff buff)
        {
            if (TryGetOwnerComponent<MovementComponent>(buff, out var movement))
            {
                originalSpeed = movement.Speed;
                movement.Speed *= speedMultiplier;
                
                Debug.Log($"[{buff.Owner.OwnerName}] 速度变为 {movement.Speed}");
            }
        }
        
        public override void Cancel(IBuff buff)
        {
            if (TryGetOwnerComponent<MovementComponent>(buff, out var movement))
            {
                movement.Speed = originalSpeed;
                
                Debug.Log($"[{buff.Owner.OwnerName}] 速度恢复为 {movement.Speed}");
            }
        }
    }
}
```

#### 5.2 在 Inspector 中配置

1. 创建 BuffDataSO
2. 逻辑脚本选择 `EffectBasedBuffLogic`
3. 配置各个生命周期的效果：

**On Acquire Effects（添加时执行）：**
- DamageEffect: damageAmount = 5

**On Logic Update Effects（每帧执行）：**
- DamageEffect: damageAmount = 2

**On Remove Effects（移除时执行）：**
- HealEffect: healAmount = 10

**On Stack Change Effects（层数变化时执行）：**
- 可以配置播放特效等

---

### 第 6 课：纯代码使用

不依赖 MonoBehaviour 的使用方式：

```csharp
using BuffSystem.Core;
using BuffSystem.Runtime;

public class GameCharacter : IBuffOwner
{
    public int OwnerId { get; private set; }
    public string OwnerName { get; private set; }
    public IBuffContainer BuffContainer { get; private set; }
    
    private static int idCounter;
    
    public GameCharacter(string name)
    {
        OwnerId = ++idCounter;
        OwnerName = name;
        BuffContainer = new BuffContainer(this);
    }
    
    public void OnBuffEvent(BuffEventType eventType, IBuff buff)
    {
        Debug.Log($"[{OwnerName}] 事件: {eventType}, Buff: {buff?.Name}");
    }
    
    // 手动更新 Buff
    public void Update(float deltaTime)
    {
        BuffContainer.Update(deltaTime);
    }
}

// 使用
public class GameManager
{
    private GameCharacter player;
    private GameCharacter enemy;
    
    void Initialize()
    {
        // 初始化 Buff 系统
        BuffApi.Initialize();
        
        // 创建角色
        player = new GameCharacter("Player");
        enemy = new GameCharacter("Enemy");
        
        // 添加 Buff
        BuffApi.AddBuff(1001, player);
        BuffApi.AddBuff(1002, enemy);
    }
    
    void Update()
    {
        float deltaTime = Time.deltaTime;
        
        // 手动更新所有角色
        player.Update(deltaTime);
        enemy.Update(deltaTime);
    }
}
```

---

## 实战案例

### 案例 1：实现一个完整的燃烧系统

#### 需求分析
- 燃烧可以叠加，最多 5 层
- 每层增加每秒伤害
- 燃烧期间播放火焰特效
- 燃烧结束时熄灭特效
- 水属性攻击可以熄灭燃烧

#### 实现步骤

**步骤 1：创建燃烧 Buff 配置**
```
ID: 1001
名称: 燃烧
描述: 每秒造成火焰伤害，可叠加
效果类型: Debuff
是否唯一: ✓
叠加模式: Stackable
最大层数: 5
每层添加数量: 1
是否永久: ✗
持续时间: 5
可刷新: ✓
移除模式: Reduce
每层移除数量: 1
移除间隔: 1
```

**步骤 2：编写燃烧逻辑**
```csharp
using UnityEngine;
using BuffSystem.Core;

[System.Serializable]
public class BurningLogic : BuffLogicBase, 
    IBuffAcquire, IBuffLogicUpdate, IBuffStackChange, IBuffRemove
{
    [SerializeField] private float baseDamage = 3f;
    [SerializeField] private float damagePerStack = 2f;
    
    private float damageTimer;
    
    public void OnAcquire()
    {
        SendEvent("BurningStart", CurrentStack);
    }
    
    public void OnLogicUpdate(float deltaTime)
    {
        damageTimer += deltaTime;
        if (damageTimer >= 1f)
        {
            float damage = baseDamage + (CurrentStack - 1) * damagePerStack;
            
            if (TryGetOwnerComponent<HealthSystem>(out var health))
            {
                health.TakeDamage(damage, DamageType.Fire);
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
        SendEvent("BurningEnd", null);
    }
}
```

**步骤 3：编写角色控制器**
```csharp
using UnityEngine;
using BuffSystem.Core;

public class CharacterController : MonoBehaviour, IBuffOwner, IBuffEventReceiver
{
    [SerializeField] private BuffOwner buffOwner;
    [SerializeField] private ParticleSystem burningEffect;
    
    public int OwnerId => buffOwner.OwnerId;
    public string OwnerName => buffOwner.OwnerName;
    public IBuffContainer BuffContainer => buffOwner.BuffContainer;
    
    void Start()
    {
        buffOwner.LocalEvents.OnBuffAdded += OnBuffAdded;
    }
    
    void OnDestroy()
    {
        buffOwner.LocalEvents.OnBuffAdded -= OnBuffAdded;
    }
    
    void OnBuffAdded(object sender, BuffAddedEventArgs e)
    {
        if (e.Buff.Name == "燃烧")
        {
            // 播放燃烧特效
            if (burningEffect != null)
            {
                burningEffect.Play();
            }
        }
    }
    
    public void OnBuffEvent(IBuff buff, string eventName, object data)
    {
        switch (eventName)
        {
            case "BurningStart":
                int stack = (int)data;
                UpdateBurningEffect(stack);
                break;
                
            case "BurningStackChanged":
                int newStack = (int)data;
                UpdateBurningEffect(newStack);
                break;
                
            case "BurningEnd":
                if (burningEffect != null)
                {
                    burningEffect.Stop();
                }
                break;
        }
    }
    
    void UpdateBurningEffect(int stack)
    {
        // 根据层数调整特效强度
        var emission = burningEffect.emission;
        emission.rateOverTime = stack * 10f;
    }
    
    // 被水属性攻击命中
    public void OnWaterHit()
    {
        // 熄灭燃烧
        BuffApi.RemoveBuff(1001, this);
    }
    
    public void OnBuffEvent(BuffEventType eventType, IBuff buff)
    {
        // 处理系统事件
    }
}
```

**步骤 4：编写技能系统**
```csharp
using UnityEngine;
using BuffSystem.Core;

public class FireSkill : MonoBehaviour
{
    [SerializeField] private int burningBuffId = 1001;
    [SerializeField] private float skillRange = 10f;
    [SerializeField] private LayerMask enemyLayer;
    
    public void Cast(Vector3 targetPosition)
    {
        // 检测范围内的敌人
        Collider[] hits = Physics.OverlapSphere(targetPosition, skillRange, enemyLayer);
        
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IBuffOwner>(out var owner))
            {
                // 添加燃烧 Buff，来源是这个技能
                BuffApi.AddBuff(burningBuffId, owner, this);
            }
        }
    }
}
```

---

### 案例 2：实现增益/减益系统

#### 需求分析
- 力量 Buff：增加攻击力
- 虚弱 Debuff：减少攻击力
- 可以叠加，有最大层数限制

#### 实现

```csharp
using UnityEngine;
using BuffSystem.Core;

// 力量 Buff
[System.Serializable]
public class PowerBuffLogic : BuffLogicBase, 
    IBuffAcquire, IBuffStackChange, IBuffRemove
{
    [SerializeField] private float attackBonusPerStack = 5f;
    
    private float totalBonus;
    
    public void OnAcquire()
    {
        ApplyBonus();
    }
    
    public void OnStackChanged(int oldStack, int newStack)
    {
        RemoveBonus();
        ApplyBonus();
    }
    
    public void OnRemove()
    {
        RemoveBonus();
    }
    
    void ApplyBonus()
    {
        totalBonus = CurrentStack * attackBonusPerStack;
        
        if (TryGetOwnerComponent<CombatStats>(out var stats))
        {
            stats.Attack += totalBonus;
        }
        
        SendEvent("AttackChanged", stats.Attack);
    }
    
    void RemoveBonus()
    {
        if (TryGetOwnerComponent<CombatStats>(out var stats))
        {
            stats.Attack -= totalBonus;
        }
    }
}

// 虚弱 Debuff
[System.Serializable]
public class WeaknessDebuffLogic : BuffLogicBase, 
    IBuffAcquire, IBuffStackChange, IBuffRemove
{
    [SerializeField] private float attackReductionPerStack = 0.1f; // 10%
    
    private float originalAttack;
    
    public void OnAcquire()
    {
        ApplyReduction();
    }
    
    public void OnStackChanged(int oldStack, int newStack)
    {
        // 先恢复
        if (TryGetOwnerComponent<CombatStats>(out var stats))
        {
            stats.Attack = originalAttack;
        }
        
        // 再应用新的减益
        ApplyReduction();
    }
    
    public void OnRemove()
    {
        if (TryGetOwnerComponent<CombatStats>(out var stats))
        {
            stats.Attack = originalAttack;
        }
    }
    
    void ApplyReduction()
    {
        if (TryGetOwnerComponent<CombatStats>(out var stats))
        {
            if (CurrentStack == 1)
            {
                originalAttack = stats.Attack;
            }
            
            float reduction = 1f - (CurrentStack * attackReductionPerStack);
            reduction = Mathf.Max(reduction, 0.1f); // 最少保留 10%
            
            stats.Attack = originalAttack * reduction;
        }
    }
}
```

---

## 最佳实践

### 1. ID 管理

```csharp
// 使用常量或枚举管理 Buff ID
public static class BuffIds
{
    public const int Burning = 1001;
    public const int Poison = 1002;
    public const int Power = 1003;
    public const int Weakness = 1004;
    public const int Shield = 1005;
    // ...
}

// 使用
BuffApi.AddBuff(BuffIds.Burning, target);
```

### 2. 来源管理

```csharp
// 定义来源类型，便于追踪和管理
public class SkillSource
{
    public int SkillId { get; set; }
    public string SkillName { get; set; }
    public GameObject Caster { get; set; }
}

// 使用
var source = new SkillSource 
{ 
    SkillId = 1, 
    SkillName = "火球术",
    Caster = gameObject 
};

BuffApi.AddBuff(BuffIds.Burning, target, source);

// 技能结束时移除该技能添加的所有 Buff
BuffApi.RemoveBuffBySource(source, target);
```

### 3. 批量操作

```csharp
// 同时添加多个 Buff
public void ApplyComboBuffs(IBuffOwner target)
{
    StartCoroutine(ApplyBuffsCoroutine(target));
}

IEnumerator ApplyBuffsCoroutine(IBuffOwner target)
{
    BuffApi.AddBuff(BuffIds.Power, target);
    yield return new WaitForSeconds(0.1f);
    
    BuffApi.AddBuff(BuffIds.Shield, target);
    yield return new WaitForSeconds(0.1f);
    
    BuffApi.AddBuff(BuffIds.Haste, target);
}
```

### 4. 条件判断

```csharp
// 添加 Buff 前检查条件
public void TryApplyPoison(IBuffOwner target)
{
    // 检查是否免疫
    if (BuffApi.HasBuff(BuffIds.PoisonImmunity, target))
    {
        Debug.Log("目标免疫中毒！");
        return;
    }
    
    // 检查已有层数
    var poison = BuffApi.GetBuff(BuffIds.Poison, target);
    if (poison != null && poison.CurrentStack >= 5)
    {
        // 已满层，改为刷新时间
        BuffApi.RefreshBuff(poison);
    }
    else
    {
        BuffApi.AddBuff(BuffIds.Poison, target);
    }
}
```

### 5. 生命周期管理

```csharp
public class BuffManager : MonoBehaviour
{
    void OnEnable()
    {
        BuffEventSystem.OnBuffAdded += OnBuffAdded;
        BuffEventSystem.OnBuffExpired += OnBuffExpired;
    }
    
    void OnDisable()
    {
        BuffEventSystem.OnBuffAdded -= OnBuffAdded;
        BuffEventSystem.OnBuffExpired -= OnBuffExpired;
    }
    
    void OnBuffAdded(object sender, BuffAddedEventArgs e)
    {
        // 记录日志
        // 更新 UI
        // 播放音效
    }
    
    void OnBuffExpired(object sender, BuffExpiredEventArgs e)
    {
        // 清理资源
        // 更新 UI
    }
}
```

### 6. 性能优化

```csharp
// 避免频繁的 GetComponent
public class CachedBuffOwner : MonoBehaviour, IBuffOwner, IBuffEventReceiver
{
    private HealthSystem healthSystem;
    private CombatStats combatStats;
    
    void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        combatStats = GetComponent<CombatStats>();
    }
    
    public void OnBuffEvent(IBuff buff, string eventName, object data)
    {
        // 直接使用缓存的组件
        switch (eventName)
        {
            case "Damage":
                healthSystem?.TakeDamage((float)data);
                break;
            case "ModifyAttack":
                combatStats?.ModifyAttack((float)data);
                break;
        }
    }
}
```
