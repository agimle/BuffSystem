# Fusion系统使用文档

> Buff融合系统 - 实现多个Buff合成为新Buff的机制

---

## 📖 概述

Fusion系统允许你将多个Buff作为材料，融合成一个新的Buff。这在炼金、合成、进化等系统中非常有用。

**典型应用场景:**
- ⚗️ 炼金系统：材料A + 材料B = 药水C
- 🧬 基因融合：Buff进化成更高级形态
- ⚔️ 装备合成：多个低级Buff合成高级Buff
- 🍳 烹饪系统：食材组合成料理

---

## 🚀 快速开始

### 1. 创建融合配方

```csharp
using BuffSystem.Advanced.Fusion;

[CreateAssetMenu(fileName = "HealthPotionRecipe", menuName = "BuffSystem/Fusion Recipe")]
public class HealthPotionRecipe : FusionRecipe
{
    void OnEnable()
    {
        recipeId = "health_potion_1";
        recipeName = "生命药水";
        
        // 定义材料
        ingredients = new List<Ingredient>
        {
            new Ingredient { buffId = 1001, requiredStack = 2 },  // 需要2个草药Buff
            new Ingredient { buffId = 1002, requiredStack = 1 }   // 需要1个水晶Buff
        };
        
        // 融合结果
        resultBuffId = 2001;  // 生命药水Buff
        fusionTime = 3f;      // 融合需要3秒
    }
}
```

### 2. 注册配方

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private FusionRecipe healthPotionRecipe;
    
    void Start()
    {
        BuffSystemManager.Fusion.RegisterRecipe(healthPotionRecipe);
    }
}
```

### 3. 执行融合

```csharp
public void TryCraftPotion(IBuffOwner crafter)
{
    var container = crafter.BuffContainer;
    
    // 尝试融合
    if (BuffSystemManager.Fusion.TryFusion("health_potion_1", container, out var resultBuff))
    {
        Debug.Log($"融合成功！获得: {resultBuff.Name}");
    }
    else
    {
        Debug.Log("材料不足，无法融合");
    }
}
```

---

## 📚 核心概念

### 配方 (FusionRecipe)

```csharp
public class FusionRecipe : ScriptableObject
{
    public string recipeId;           // 配方唯一ID
    public string recipeName;         // 配方名称
    public List<Ingredient> ingredients;  // 材料列表
    public int resultBuffId;          // 结果Buff ID
    public float fusionTime;          // 融合时间（0为即时）
    public List<IFusionCondition> conditions;  // 额外条件
}
```

### 材料 (Ingredient)

```csharp
public class Ingredient
{
    public int buffId;           // 需要的Buff ID
    public int requiredStack = 1;    // 需要的层数
    public bool consumeOnFusion = true;  // 融合时是否消耗
}
```

---

## 🎯 融合条件

### 基础条件

```csharp
// 检查材料是否满足
bool canFuse = recipe.HasIngredients(container);
```

### 自定义条件

```csharp
// 等级条件
public class LevelCondition : IFusionCondition
{
    public int requiredLevel = 10;
    
    public bool Check(IBuffOwner owner)
    {
        if (owner is Player player)
        {
            return player.Level >= requiredLevel;
        }
        return false;
    }
}

// 时间条件
public class TimeCondition : IFusionCondition
{
    public bool Check(IBuffOwner owner)
    {
        // 只能在夜晚融合
        return GameTime.IsNight;
    }
}

// 地点条件
public class LocationCondition : IFusionCondition
{
    public string requiredLocation = "AlchemyLab";
    
    public bool Check(IBuffOwner owner)
    {
        return LocationManager.CurrentLocation == requiredLocation;
    }
}
```

### 使用条件

```csharp
var recipe = new FusionRecipe
{
    recipeId = "advanced_potion",
    conditions = new List<IFusionCondition>
    {
        new LevelCondition { requiredLevel = 20 },
        new TimeCondition(),
        new LocationCondition { requiredLocation = "AlchemyLab" }
    }
};
```

---

## ⏱️ 延迟融合

### 即时融合

```csharp
fusionTime = 0f;  // 立即完成
```

### 延迟融合

```csharp
fusionTime = 5f;  // 需要5秒

// 开始延迟融合
BuffSystemManager.Fusion.TryFusion("recipe_id", container, out _);

// 监听融合完成事件
FusionEventSystem.OnFusionCompleted += (sender, e) =>
{
    Debug.Log($"融合完成: {e.Recipe.recipeName}");
};

// 监听融合取消事件
FusionEventSystem.OnFusionCancelled += (sender, e) =>
{
    Debug.Log($"融合取消: {e.Recipe.recipeName}");
};
```

### 取消融合

```csharp
// 取消进行中的融合
BuffSystemManager.Fusion.CancelFusion(fusionId);
```

---

## 💡 完整示例

### 示例1: 炼金系统

```csharp
public class AlchemySystem : MonoBehaviour
{
    [SerializeField] private List<FusionRecipe> recipes;
    
    void Start()
    {
        // 注册所有配方
        foreach (var recipe in recipes)
        {
            BuffSystemManager.Fusion.RegisterRecipe(recipe);
        }
    }
    
    // 显示可融合的配方
    public List<FusionRecipe> GetAvailableRecipes(IBuffOwner owner)
    {
        return BuffSystemManager.Fusion.GetAvailableFusions(owner.BuffContainer);
    }
    
    // 执行融合
    public bool CraftItem(string recipeId, IBuffOwner crafter)
    {
        return BuffSystemManager.Fusion.TryFusion(recipeId, crafter.BuffContainer, out _);
    }
}
```

### 示例2: 自动融合

```csharp
public class AutoFusion : MonoBehaviour
{
    void Update()
    {
        // 自动检测并执行所有可融合的配方
        var availableFusions = BuffSystemManager.Fusion
            .GetAvailableFusions(player.BuffContainer);
        
        foreach (var recipe in availableFusions)
        {
            if (recipe.autoCraft)  // 标记为自动合成的配方
            {
                BuffSystemManager.Fusion.TryFusion(
                    recipe.recipeId, 
                    player.BuffContainer, 
                    out _
                );
            }
        }
    }
}
```

---

## 📊 性能优化

### 1. 缓存配方检查

```csharp
private List<FusionRecipe> cachedAvailableRecipes;
private float lastCheckTime;

void Update()
{
    // 每0.5秒检查一次，而非每帧
    if (Time.time - lastCheckTime > 0.5f)
    {
        cachedAvailableRecipes = BuffSystemManager.Fusion
            .GetAvailableFusions(container);
        lastCheckTime = Time.time;
    }
}
```

### 2. 批量注册配方

```csharp
// 一次性注册所有配方
BuffSystemManager.Fusion.RegisterRecipes(recipeList);
```

---

## 🐛 调试技巧

```csharp
// 打印所有配方
void PrintAllRecipes()
{
    var recipes = BuffSystemManager.Fusion.GetAllRecipes();
    foreach (var recipe in recipes)
    {
        Debug.Log($"配方: {recipe.recipeName} ({recipe.recipeId})");
        Debug.Log($"  材料: {string.Join(", ", recipe.ingredients.Select(i => $"Buff{i.buffId}x{i.requiredStack}"))}");
        Debug.Log($"  结果: Buff{recipe.resultBuffId}");
    }
}

// 检查特定配方
void CheckRecipe(string recipeId, IBuffOwner owner)
{
    var recipe = BuffSystemManager.Fusion.GetRecipe(recipeId);
    if (recipe == null)
    {
        Debug.LogError($"配方不存在: {recipeId}");
        return;
    }
    
    bool hasIngredients = recipe.HasIngredients(owner.BuffContainer);
    bool conditionsMet = recipe.CheckConditions(owner.BuffContainer);
    
    Debug.Log($"配方 {recipeId}:");
    Debug.Log($"  材料满足: {hasIngredients}");
    Debug.Log($"  条件满足: {conditionsMet}");
}
```

---

## 📚 相关文档

- [Combo系统文档](ComboSystem.md)
- [Transmission系统文档](TransmissionSystem.md)
- [API参考文档](../API_REFERENCE.md)

---

**祝你炼金成功！** ⚗️
