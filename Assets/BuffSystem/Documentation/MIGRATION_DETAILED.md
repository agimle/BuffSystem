# BuffSystem v6.x 到 v7.0 详细迁移指南

> 本文档提供详细的迁移步骤和自动化工具使用说明

---

## 📋 迁移前准备

### 1. 备份项目

```bash
# 使用git创建迁移分支
git checkout -b migration/v7.0

# 或者创建完整备份
cp -r MyProject MyProject_Backup_v6
```

### 2. 记录当前状态

创建迁移前的状态记录：

```csharp
// MigrationStatus.cs
public static class MigrationStatus
{
    public static void RecordPreMigrationStatus()
    {
        Debug.Log("=== 迁移前状态记录 ===");
        Debug.Log($"Unity版本: {Application.unityVersion}");
        Debug.Log($"BuffSystem版本: v6.x");
        
        // 记录使用的高级系统
        var usingCombo = FindObjectsOfType<MonoBehaviour>().Any(m => 
            m.GetType().ToString().Contains("Combo"));
        var usingFusion = FindObjectsOfType<MonoBehaviour>().Any(m => 
            m.GetType().ToString().Contains("Fusion"));
        var usingTransmission = FindObjectsOfType<MonoBehaviour>().Any(m => 
            m.GetType().ToString().Contains("Transmission"));
        
        Debug.Log($"使用Combo系统: {usingCombo}");
        Debug.Log($"使用Fusion系统: {usingFusion}");
        Debug.Log($"使用Transmission系统: {usingTransmission}");
    }
}
```

---

## 🔄 迁移步骤详解

### 阶段 1: 环境准备 (5分钟)

#### 1.1 更新BuffSystem包

1. 删除旧版本 `Assets/BuffSystem` 文件夹
2. 导入新版本 v7.0
3. 等待Unity编译完成

#### 1.2 检查编译错误

如果有编译错误，先记录错误信息：
```
Assets/Scripts/MyComboSystem.cs(10,23): error CS0234: 
The type or namespace name 'Combo' does not exist in the namespace 'BuffSystem'
```

### 阶段 2: 启用兼容模式 (2分钟)

这是最关键的一步，可以让项目立即恢复运行。

#### 2.1 添加编译符号

**方法1: Unity编辑器**
1. Edit → Project Settings → Player
2. Scripting Define Symbols
3. 添加: `BUFFSYSTEM_COMPATIBILITY_V6`

**方法2: 修改代码 (推荐用于版本控制)**

创建 `Assets/BuffSystem/Editor/CompatibilitySettings.cs`:

```csharp
using UnityEditor;

namespace BuffSystem.Editor
{
    [InitializeOnLoad]
    public static class CompatibilitySettings
    {
        static CompatibilitySettings()
        {
            // 自动添加兼容模式符号
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);
            
            if (!defines.Contains("BUFFSYSTEM_COMPATIBILITY_V6"))
            {
                defines += ";BUFFSYSTEM_COMPATIBILITY_V6";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    EditorUserBuildSettings.selectedBuildTargetGroup, defines);
                
                UnityEngine.Debug.Log("[BuffSystem] 已自动启用v6兼容模式");
            }
        }
    }
}
```

#### 2.2 验证编译

等待Unity重新编译，确认无错误。

### 阶段 3: 添加BuffSystemManager (5分钟)

#### 3.1 场景配置

**方法1: 手动添加**
1. 在第一个场景中创建空GameObject
2. 命名为 "BuffSystemManager"
3. 添加组件: BuffSystem → Buff System Manager

**方法2: 自动创建 (推荐)**

创建 `Assets/Scripts/BuffSystemInitializer.cs`:

```csharp
using UnityEngine;
using BuffSystem.Core;

public class BuffSystemInitializer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        // 确保BuffSystemManager存在
        var manager = BuffSystemManager.Instance;
        Debug.Log("[BuffSystem] 管理器初始化完成");
    }
}
```

#### 3.2 验证Manager

运行场景，检查Console输出：
```
[BuffSystem] 管理器初始化完成
```

### 阶段 4: 功能测试 (15分钟)

#### 4.1 基础功能测试

创建测试脚本 `Assets/Editor/BuffSystemMigrationTest.cs`:

```csharp
using UnityEngine;
using UnityEditor;
using BuffSystem.Core;
using BuffSystem.Runtime;

namespace BuffSystem.Editor
{
    public static class MigrationTest
    {
        [MenuItem("BuffSystem/Migration/Run Tests")]
        public static void RunTests()
        {
            Debug.Log("=== BuffSystem v7.0 迁移测试 ===\n");
            
            int passed = 0;
            int failed = 0;
            
            // 测试1: Manager初始化
            try
            {
                var manager = BuffSystemManager.Instance;
                Debug.Log("✅ Test 1: BuffSystemManager 初始化成功");
                passed++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Test 1 失败: {e.Message}");
                failed++;
            }
            
            // 测试2: 向后兼容
            #if BUFFSYSTEM_COMPATIBILITY_V6
            Debug.Log("✅ Test 2: v6兼容模式已启用");
            passed++;
            #else
            Debug.LogWarning("⚠️ Test 2: v6兼容模式未启用");
            #endif
            
            // 测试3: API可用性
            try
            {
                var combo = BuffSystemManager.Combo;
                var fusion = BuffSystemManager.Fusion;
                var transmission = BuffSystemManager.Transmission;
                Debug.Log("✅ Test 3: 所有Manager访问点可用");
                passed++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Test 3 失败: {e.Message}");
                failed++;
            }
            
            Debug.Log($"\n=== 测试结果: {passed} 通过, {failed} 失败 ===");
            
            if (failed == 0)
            {
                Debug.Log("🎉 所有测试通过！迁移成功。");
            }
            else
            {
                Debug.LogError("⚠️ 有测试失败，请检查上述错误。");
            }
        }
    }
}
```

运行测试: `BuffSystem → Migration → Run Tests`

#### 4.2 游戏功能测试

- [ ] Buff添加/移除正常
- [ ] Combo系统正常（如使用）
- [ ] Fusion系统正常（如使用）
- [ ] Transmission系统正常（如使用）
- [ ] 场景切换正常
- [ ] 存档读档正常

### 阶段 5: 代码迁移 (可选，1-2小时)

如果不想看到Obsolete警告，可以迁移代码。

#### 5.1 使用自动迁移工具

创建 `Assets/BuffSystem/Editor/MigrationTools.cs`:

```csharp
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace BuffSystem.Editor
{
    public class MigrationTools : EditorWindow
    {
        private Vector2 scrollPosition;
        private string log = "";
        
        [MenuItem("BuffSystem/Migration/Code Migration Tool")]
        public static void ShowWindow()
        {
            GetWindow<MigrationTools>("Code Migration");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("BuffSystem v7.0 代码迁移工具", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("1. 更新命名空间", GUILayout.Height(30)))
            {
                UpdateNamespaces();
            }
            
            if (GUILayout.Button("2. 更新Manager访问", GUILayout.Height(30)))
            {
                UpdateManagerAccess();
            }
            
            if (GUILayout.Button("3. 清理未使用的using", GUILayout.Height(30)))
            {
                CleanupUsings();
            }
            
            EditorGUILayout.Space();
            
            GUILayout.Label("日志:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.TextArea(log, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
        
        private void UpdateNamespaces()
        {
            log = "";
            int count = 0;
            
            string[] files = Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories);
            
            foreach (string file in files)
            {
                if (file.Contains("BuffSystem")) continue;  // 跳过系统文件
                
                string content = File.ReadAllText(file);
                string original = content;
                
                // 替换命名空间
                content = Regex.Replace(content, 
                    @"using BuffSystem\.Combo;", 
                    "using BuffSystem.Advanced.Combo;");
                content = Regex.Replace(content, 
                    @"using BuffSystem\.Fusion;", 
                    "using BuffSystem.Advanced.Fusion;");
                content = Regex.Replace(content, 
                    @"using BuffSystem\.Transmission;", 
                    "using BuffSystem.Advanced.Transmission;");
                
                if (content != original)
                {
                    File.WriteAllText(file, content);
                    log += $"✅ 已更新: {file}\n";
                    count++;
                }
            }
            
            log += $"\n总共更新了 {count} 个文件";
            AssetDatabase.Refresh();
        }
        
        private void UpdateManagerAccess()
        {
            log = "";
            int count = 0;
            
            string[] files = Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories);
            
            foreach (string file in files)
            {
                if (file.Contains("BuffSystem")) continue;
                
                string content = File.ReadAllText(file);
                string original = content;
                
                // 替换Manager访问
                content = content.Replace(
                    "BuffComboManager.Instance", 
                    "BuffSystemManager.Combo");
                content = content.Replace(
                    "FusionManager.Instance", 
                    "BuffSystemManager.Fusion");
                content = content.Replace(
                    "TransmissionManager.Instance", 
                    "BuffSystemManager.Transmission");
                
                if (content != original)
                {
                    File.WriteAllText(file, content);
                    log += $"✅ 已更新: {file}\n";
                    count++;
                }
            }
            
            log += $"\n总共更新了 {count} 个文件";
            AssetDatabase.Refresh();
        }
        
        private void CleanupUsings()
        {
            log = "清理功能需要更复杂的分析，建议使用IDE的重构功能。\n";
            log += "推荐: Rider → Optimize Usings 或 Visual Studio → Remove and Sort Usings";
        }
    }
}
```

使用工具: `BuffSystem → Migration → Code Migration Tool`

#### 5.2 手动检查关键点

即使使用自动工具，也需要手动检查以下文件：

1. **自定义Manager继承**
   ```csharp
   // 检查是否有继承旧Manager的代码
   public class MyComboManager : BuffComboManager { }  // 可能需要更新
   ```

2. **反射调用**
   ```csharp
   // 检查字符串反射
   var type = Type.GetType("BuffSystem.Combo.BuffComboManager");
   // 需要更新为
   var type = Type.GetType("BuffSystem.Advanced.Combo.BuffComboManager");
   ```

3. **序列化数据**
   ```csharp
   // 检查ScriptableObject引用
   [SerializeField] private BuffComboData comboData;  // 通常自动处理
   ```

### 阶段 6: 最终验证 (15分钟)

#### 6.1 编译检查

确保无编译错误和警告（Obsolete警告可接受）。

#### 6.2 运行时检查

运行完整游戏流程，确保：
- [ ] 无NullReferenceException
- [ ] 所有Buff功能正常
- [ ] 性能无明显下降
- [ ] 存档系统正常

#### 6.3 提交代码

```bash
git add .
git commit -m "chore: migrate BuffSystem to v7.0

- 启用v6兼容模式
- 添加BuffSystemManager
- 更新命名空间 (可选)
- 所有测试通过"
```

---

## 🐛 常见问题解决

### Q1: 编译错误 "命名空间不存在"

**原因:** 兼容模式未启用或命名空间错误

**解决:**
```csharp
// 确认在Project Settings中添加了:
BUFFSYSTEM_COMPATIBILITY_V6

// 或者更新using语句:
using BuffSystem.Advanced.Combo;  // 新命名空间
```

### Q2: 运行时NullReferenceException

**原因:** BuffSystemManager未初始化

**解决:**
```csharp
// 在场景中添加BuffSystemManager
// 或使用自动初始化脚本
```

### Q3: Obsolete警告太多

**解决:**
1. 启用自动迁移工具更新代码
2. 或者暂时忽略警告（不影响功能）

### Q4: 第三方插件报错

**解决:**
- 确保启用了兼容模式
- 联系插件作者更新
- 或者使用assembly定义隔离

---

## 📊 迁移检查清单

### 迁移前
- [ ] 项目已备份
- [ ] 团队成员已通知
- [ ] 迁移时间窗口已确定

### 迁移中
- [ ] BuffSystem包已更新
- [ ] 兼容模式已启用
- [ ] 编译无错误
- [ ] BuffSystemManager已添加
- [ ] 基础功能测试通过

### 迁移后
- [ ] 完整游戏流程测试通过
- [ ] 性能测试通过
- [ ] 存档系统测试通过
- [ ] 代码已提交
- [ ] 文档已更新

---

## 📚 相关文档

- [快速迁移指南](MIGRATION_GUIDE.md)
- [API变更日志](API_CHANGELOG.md)
- [API参考文档](API_REFERENCE.md)

---

**祝你迁移顺利！** 🚀
