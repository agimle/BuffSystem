# BuffSystem 编辑器工具使用指南

本文档详细说明 BuffSystem 提供的 Unity 编辑器工具使用方法。

## 目录

- [菜单项](#菜单项)
- [BuffDataSO Inspector](#buffdataso-inspector)
- [BuffOwner Inspector](#buffowner-inspector)
- [ScriptableObject 创建](#scriptableobject-创建)
- [自定义编辑器扩展](#自定义编辑器扩展)

---

## 菜单项

BuffSystem 在 Unity 菜单栏中添加了以下菜单项：

### Tools/BuffSystem

```
Tools/
└── BuffSystem/
    ├── Create Buff Data          # 创建 Buff 数据配置
    ├── Create System Config      # 创建系统配置
    ├── Create Data Center        # 创建数据中心
    ├── Open Documentation        # 打开文档
    └── Reload Buff Database      # 重新加载 Buff 数据库
```

#### Create Buff Data

快速创建一个新的 BuffDataSO 文件。

**操作步骤：**
1. 点击菜单 `Tools -> BuffSystem -> Create Buff Data`
2. 在弹出的对话框中选择保存路径
3. 输入文件名
4. 点击保存

**快捷方式：**
- 也可以在 Project 窗口中右键 -> Create -> BuffSystem -> Buff Data

#### Create System Config

创建 BuffSystem 的全局配置文件。

**说明：**
- 配置文件保存在 `Resources/BuffSystem/BuffSystemConfig.asset`
- 如果已存在，会提示是否覆盖

**配置项：**
- Default Pool Capacity: 对象池默认容量
- Max Pool Size: 对象池最大容量
- Update Mode: 更新模式
- Batch Count: 批处理数量
- Update Interval: 更新间隔
- Enable Debug Log: 启用调试日志
- Enable Gizmos: 启用 Gizmos

#### Create Data Center

创建 BuffDataCenter，用于集中管理 Buff 数据资源。

**说明：**
- 数据中心保存在 `Resources/BuffSystem/BuffDataCenter.asset`
- 可以将所有 BuffDataSO 添加到数据中心统一管理

#### Open Documentation

在浏览器中打开 BuffSystem 文档。

**说明：**
- 默认打开 README.md
- 需要系统支持 .md 文件的默认打开方式

#### Reload Buff Database

运行时重新加载 Buff 数据库。

**说明：**
- 仅在 Play 模式下可用
- 用于测试时快速重新加载配置
- 会触发 `BuffApi.ReloadData()`

---

## BuffDataSO Inspector

BuffDataSO 的自定义 Inspector 提供了更友好的编辑体验。

### 界面布局

```
┌─────────────────────────────────────────┐
│ Buff Data                               │
├─────────────────────────────────────────┤
│ [基础信息]                              │
│   ID: [____]                            │
│   名称: [________]                      │
│   描述: [                                │
│         ________]                       │
│   效果类型: [Neutral ▼]                 │
├─────────────────────────────────────────┤
│ [叠加设置]                              │
│   是否唯一: [✓]                         │
│   叠加模式: [Stackable ▼]               │
│   最大层数: [5]                         │
│   每层添加数量: [1]                     │
├─────────────────────────────────────────┤
│ [持续时间]                              │
│   是否永久: [ ]                         │
│   持续时间: [5.0] 秒                    │
│   可刷新: [✓]                           │
├─────────────────────────────────────────┤
│ [移除设置]                              │
│   移除模式: [Reduce ▼]                  │
│   每层移除数量: [1]                     │
│   移除间隔: [1.0] 秒                    │
├─────────────────────────────────────────┤
│ [逻辑脚本]                              │
│   [SubclassSelector]                    │
│   [BuffLogicBase 子类列表 ▼]            │
│                                         │
│   [逻辑参数配置区域]                     │
│   - Damage Per Second: [5]              │
│   - Damage Per Stack: [2]               │
└─────────────────────────────────────────┘
```

### 字段说明

#### 基础信息

| 字段 | 类型 | 说明 | 验证规则 |
|------|------|------|----------|
| ID | int | Buff 唯一标识符 | 自动生成为名称的哈希值，不可为 0 |
| 名称 | string | Buff 显示名称 | 不能为空 |
| 描述 | string | Buff 详细描述 | 多行文本框 |
| 效果类型 | Enum | Neutral/Buff/Debuff/Special | - |

#### 叠加设置

| 字段 | 类型 | 说明 | 验证规则 |
|------|------|------|----------|
| 是否唯一 | bool | 同类型是否只能存在一个 | - |
| 叠加模式 | Enum | None/Stackable/Independent | - |
| 最大层数 | int | Buff 最高可叠加层数 | 最小值为 1 |
| 每层添加数量 | int | 每次添加时增加的层数 | 最小值为 1 |

#### 持续时间

| 字段 | 类型 | 说明 | 验证规则 |
|------|------|------|----------|
| 是否永久 | bool | 是否为永久 Buff | 勾选后持续时间无效 |
| 持续时间 | float | Buff 持续时间（秒） | 最小值为 0.1 |
| 可刷新 | bool | 重新添加时是否刷新时间 | - |

#### 移除设置

| 字段 | 类型 | 说明 | 验证规则 |
|------|------|------|----------|
| 移除模式 | Enum | Remove/Reduce | - |
| 每层移除数量 | int | 每次移除时减少的层数 | 最小值为 1 |
| 移除间隔 | float | 逐层移除时的间隔时间（秒） | 最小值为 0 |

### 逻辑脚本配置

#### SubclassSelector 特性

BuffDataSO 使用 `SubclassSelector` 特性来选择 BuffLogicBase 的子类：

1. 点击下拉框显示所有可序列化的 BuffLogicBase 子类
2. 选择一个子类后，Inspector 会显示该类的序列化字段
3. 可以直接在 Inspector 中配置逻辑参数

#### 支持的逻辑类型

**EmptyBuffLogic**
- 空逻辑，不做任何事情
- 适用于纯标记性的 Buff

**EffectBasedBuffLogic**
- 基于效果的 Buff 逻辑
- 可以在 Inspector 中配置各个生命周期的效果列表
- 无需编写代码即可实现复杂逻辑

**自定义 BuffLogic**
- 继承 BuffLogicBase 的自定义类
- 需要在类上添加 `[System.Serializable]` 特性
- 支持在 Inspector 中配置自定义参数

### 验证和自动修正

BuffDataSO 在 OnValidate 中会自动修正非法值：

```csharp
// ID 为 0 时自动生成
if (id == 0)
{
    id = Mathf.Abs(buffName.GetHashCode());
}

// 确保数值合法
maxStack = Mathf.Max(1, maxStack);
addStackCount = Mathf.Max(1, addStackCount);
duration = Mathf.Max(0.1f, duration);
```

---

## BuffOwner Inspector

BuffOwner 的自定义 Inspector 提供了运行时调试功能。

### 界面布局

```
┌─────────────────────────────────────────┐
│ Buff Owner                              │
├─────────────────────────────────────────┤
│ [设置]                                  │
│   Auto Initialize: [✓]                  │
│   Update In FixedUpdate: [ ]            │
│   Show Debug Info: [✓]                  │
├─────────────────────────────────────────┤
│ [运行时信息] (Play 模式显示)             │
│   Buff 数量: 3                          │
│                                         │
│   ┌─────────────────────────────────┐   │
│   │ 🔥 燃烧 (ID: 1001)              │   │
│   │   层数: 3/5                     │   │
│   │   剩余: 3.2s                    │   │
│   │   来源: FireSkill               │   │
│   │   [移除] [刷新]                 │   │
│   └─────────────────────────────────┘   │
│                                         │
│   ┌─────────────────────────────────┐   │
│   │ ⚡ 加速 (ID: 1002)              │   │
│   │   层数: 1/3                     │   │
│   │   剩余: 8.5s                    │   │
│   │   来源: null                    │   │
│   │   [移除] [刷新]                 │   │
│   └─────────────────────────────────┘   │
│                                         │
│   [添加 Buff] [清空所有]                │
└─────────────────────────────────────────┘
```

### 设置选项

| 选项 | 说明 |
|------|------|
| Auto Initialize | 是否在 Awake 时自动初始化 |
| Update In FixedUpdate | 是否在 FixedUpdate 中更新 Buff |
| Show Debug Info | 是否显示调试日志 |

### 运行时调试功能

在 Play 模式下，Inspector 会显示当前持有的所有 Buff：

#### Buff 卡片

每个 Buff 显示在一个卡片中，包含：
- **图标和名称**：根据 EffectType 显示不同图标
- **ID**：Buff 配置 ID
- **层数**：当前层数/最大层数
- **剩余时间**：剩余时间（永久 Buff 显示 "∞"）
- **来源**：Buff 来源的 ToString()
- **操作按钮**：
  - 移除：立即移除该 Buff
  - 刷新：刷新持续时间

#### 批量操作

- **添加 Buff**：弹出窗口输入 Buff ID 或名称
- **清空所有**：移除所有 Buff

### 调试信息

勾选 "Show Debug Info" 后，BuffOwner 会在控制台输出调试信息：

```
[BuffOwner] Player - 事件: Added, Buff: 燃烧
[BuffOwner] Player - 事件: StackChanged, Buff: 燃烧
[BuffOwner] Player - 事件: Removed, Buff: 燃烧
```

---

## ScriptableObject 创建

BuffSystem 提供了三种 ScriptableObject 的创建菜单：

### 1. Buff Data

**路径：** `Create -> BuffSystem -> Buff Data`

**用途：** 创建 Buff 配置数据

**默认配置：**
- ID: 0（自动生成）
- 名称: "New Buff"
- 效果类型: Neutral
- 叠加模式: Stackable
- 最大层数: 1
- 持续时间: 5 秒

### 2. System Config

**路径：** `Create -> BuffSystem -> System Config`

**用途：** 创建系统全局配置

**默认配置：**
- Default Pool Capacity: 32
- Max Pool Size: 128
- Update Mode: EveryFrame
- Batch Count: 4
- Update Interval: 0.1
- Enable Debug Log: false
- Enable Gizmos: false

**重要：**
- 配置文件应放在 `Resources/BuffSystem/` 目录下
- 文件名应为 `BuffSystemConfig.asset`
- 系统会自动加载该配置

### 3. Data Center

**路径：** `Create -> BuffSystem -> Data Center`

**用途：** 创建数据中心，集中管理 Buff 数据

**使用方式：**
1. 创建 Data Center
2. 将所有 BuffDataSO 添加到 BuffDataList
3. 系统启动时会自动加载列表中的所有 Buff

**重要：**
- 数据中心应放在 `Resources/BuffSystem/` 目录下
- 文件名应为 `BuffDataCenter.asset`

---

## 自定义编辑器扩展

### 创建自定义 BuffDataSO 编辑器

如果你需要扩展 BuffDataSO 的 Inspector，可以继承 `BuffDataSOEditor`：

```csharp
using UnityEngine;
using UnityEditor;
using BuffSystem.Data;
using BuffSystem.Editor;

namespace MyGame.Editor
{
    [CustomEditor(typeof(MyBuffDataSO))]
    public class MyBuffDataSOEditor : BuffDataSOEditor
    {
        public override void OnInspectorGUI()
        {
            // 调用父类绘制默认界面
            base.OnInspectorGUI();
            
            // 添加自定义区域
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("自定义设置", EditorStyles.boldLabel);
            
            // 绘制自定义字段
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("customField"));
            serializedObject.ApplyModifiedProperties();
            
            // 添加自定义按钮
            if (GUILayout.Button("自定义操作"))
            {
                DoCustomAction();
            }
        }
        
        void DoCustomAction()
        {
            // 自定义操作
            Debug.Log("执行自定义操作");
        }
    }
}
```

### 创建自定义 BuffOwner 编辑器

```csharp
using UnityEngine;
using UnityEditor;
using BuffSystem.Runtime;
using BuffSystem.Editor;

namespace MyGame.Editor
{
    [CustomEditor(typeof(MyBuffOwner))]
    public class MyBuffOwnerEditor : BuffOwnerEditor
    {
        public override void OnInspectorGUI()
        {
            // 调用父类绘制默认界面
            base.OnInspectorGUI();
            
            // 添加自定义调试信息
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("自定义调试", EditorStyles.boldLabel);
            
            MyBuffOwner myOwner = (MyBuffOwner)target;
            EditorGUILayout.LabelField("自定义字段:", myOwner.customValue.ToString());
        }
    }
}
```

### 添加自定义菜单项

```csharp
using UnityEngine;
using UnityEditor;

namespace MyGame.Editor
{
    public static class MyBuffSystemMenu
    {
        [MenuItem("Tools/BuffSystem/Custom Action")]
        static void CustomAction()
        {
            // 自定义操作
            Debug.Log("执行自定义操作");
        }
        
        [MenuItem("Tools/BuffSystem/Custom Action", true)]
        static bool ValidateCustomAction()
        {
            // 验证是否可用
            return Application.isPlaying;
        }
    }
}
```

### 自定义属性绘制器

为 Buff 相关类创建自定义 PropertyDrawer：

```csharp
using UnityEngine;
using UnityEditor;

namespace MyGame.Editor
{
    [CustomPropertyDrawer(typeof(MyBuffProperty))]
    public class MyBuffPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 自定义绘制逻辑
            EditorGUI.BeginProperty(position, label, property);
            
            // 绘制字段
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("field1"));
            
            position.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("field2"));
            
            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 返回属性高度
            return EditorGUIUtility.singleLineHeight * 2 + 2;
        }
    }
}
```

---

## 编辑器工具类参考

### BuffSystemMenu

```csharp
public static class BuffSystemMenu
{
    [MenuItem("Tools/BuffSystem/Create Buff Data")]
    static void CreateBuffData()
    
    [MenuItem("Tools/BuffSystem/Create System Config")]
    static void CreateSystemConfig()
    
    [MenuItem("Tools/BuffSystem/Create Data Center")]
    static void CreateDataCenter()
    
    [MenuItem("Tools/BuffSystem/Open Documentation")]
    static void OpenDocumentation()
    
    [MenuItem("Tools/BuffSystem/Reload Buff Database")]
    static void ReloadBuffDatabase()
}
```

### BuffDataSOEditor

```csharp
[CustomEditor(typeof(BuffDataSO))]
public class BuffDataSOEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    
    protected virtual void DrawBasicInfo()
    protected virtual void DrawStackSettings()
    protected virtual void DrawDurationSettings()
    protected virtual void DrawRemoveSettings()
    protected virtual void DrawLogicSettings()
}
```

### BuffOwnerEditor

```csharp
[CustomEditor(typeof(BuffOwner))]
public class BuffOwnerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    
    protected virtual void DrawSettings()
    protected virtual void DrawRuntimeInfo()
    protected virtual void DrawBuffCard(IBuff buff)
    protected virtual void DrawBuffActions(IBuff buff)
}
```

---

## 常见问题

### Q: 为什么 BuffDataSO 的 ID 会自动变化？

A: 当 ID 为 0 时，系统会根据名称自动生成哈希值作为 ID。建议手动设置一个固定的唯一 ID。

### Q: 自定义 BuffLogic 在 Inspector 中不显示？

A: 确保：
1. 类继承自 `BuffLogicBase`
2. 类标记为 `[System.Serializable]`
3. 类不是抽象类
4. 类有公共无参构造函数

### Q: EffectBasedBuffLogic 的效果列表无法添加元素？

A: 确保：
1. 效果类继承自 `EffectBase`
2. 效果类标记为 `[System.Serializable]`
3. 效果类有公共无参构造函数

### Q: 运行时 Inspector 不显示 Buff 列表？

A: 确保：
1. 在 Play 模式下
2. BuffOwner 已初始化
3. 该对象确实有 Buff

### Q: 如何禁用自定义 Inspector？

A: 删除或注释掉 `BuffDataSOEditor.cs` 和 `BuffOwnerEditor.cs` 文件，Unity 会使用默认 Inspector。
