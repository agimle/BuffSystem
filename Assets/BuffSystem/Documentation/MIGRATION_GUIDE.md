# BuffSystem v6.x 到 v7.0 迁移指南

> 本文档帮助开发者从 v6.x 版本平滑迁移到 v7.0 版本

---

## 📋 迁移概览

### 主要变更

| 变更类型 | 影响程度 | 向后兼容 |
|---------|---------|---------|
| 命名空间重构 | 中 | ✅ 兼容模式可用 |
| Manager单例统一 | 低 | ✅ 旧方式仍可用 |
| API稳定性标记 | 无 | ✅ 纯新增功能 |

### 迁移时间预估

- **快速迁移（启用兼容模式）**: 5分钟
- **完整迁移（更新所有代码）**: 1-2小时
- **大型项目**: 半天到一天

---

## 🚀 快速迁移（推荐）

如果你希望快速迁移且暂时不想修改代码，只需启用兼容模式：

### 步骤 1: 启用兼容模式

1. 打开 Unity 编辑器
2. 进入 **Edit > Project Settings > Player**
3. 找到 **Scripting Define Symbols**
4. 添加符号：`BUFFSYSTEM_COMPATIBILITY_V6`
5. 点击 Apply

```
BUFFSYSTEM_COMPATIBILITY_V6
```

### 步骤 2: 更新BuffSystemManager

在场景中添加 `BuffSystemManager`：

1. 创建空 GameObject，命名为 "BuffSystemManager"
2. 添加组件 **BuffSystem > Buff System Manager**
3. 或者使用代码自动创建：

```csharp
// 首次访问时会自动创建
var comboManager = BuffSystemManager.Combo;
```

### 完成！

你的代码无需任何修改即可在 v7.0 上运行。

---

## 🔧 完整迁移

如果你希望完全迁移到新API，按照以下步骤操作：

### 步骤 1: 更新命名空间

#### 使用IDE批量替换

**Visual Studio:**
1. 按 `Ctrl+Shift+H` 打开替换窗口
2. 启用 "正则表达式"
3. 查找：`using BuffSystem\.(Combo|Fusion|Transmission|Area|Snapshot);`
4. 替换：`using BuffSystem.Advanced.$1;`
5. 点击 "全部替换"

**Rider:**
1. 按 `Ctrl+Shift+R` 打开重构菜单
2. 选择 "Adjust Namespaces"
3. 按提示操作

#### 手动替换清单

| 旧命名空间 | 新命名空间 |
|-----------|-----------|
| `BuffSystem.Combo` | `BuffSystem.Advanced.Combo` |
| `BuffSystem.Fusion` | `BuffSystem.Advanced.Fusion` |
| `BuffSystem.Transmission` | `BuffSystem.Advanced.Transmission` |
| `BuffSystem.Area` | `BuffSystem.Advanced.Area` |
| `BuffSystem.Snapshot` | `BuffSystem.Advanced.Snapshot` |

### 步骤 2: 更新Manager访问方式

#### 批量替换

**查找:**
```csharp
(BuffComboManager|FusionManager|TransmissionManager)\.Instance
```

**替换为:**
```csharp
BuffSystemManager.$1
```

注意：需要手动调整大小写：
- `BuffComboManager` → `Combo`
- `FusionManager` → `Fusion`
- `TransmissionManager` → `Transmission`

#### 替换示例

```csharp
// ========== 旧代码 ==========
using BuffSystem.Combo;
using BuffSystem.Fusion;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // 注册Combo
        BuffComboManager.Instance.RegisterCombo(comboData);
        
        // 尝试融合
        FusionManager.Instance.TryFusion("recipe1", container, out result);
    }
}

// ========== 新代码 ==========
using BuffSystem.Advanced.Combo;
using BuffSystem.Advanced.Fusion;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // 注册Combo
        BuffSystemManager.Combo.RegisterCombo(comboData);
        
        // 尝试融合
        BuffSystemManager.Fusion.TryFusion("recipe1", container, out result);
    }
}
```

### 步骤 3: 处理Obsolete警告

编译项目后，你可能会看到一些 Obsolete 警告。按照警告信息提示进行更新：

```
Warning CS0618: 'BuffComboManager.Instance' is obsolete: '使用 BuffSystemManager.Combo 替代'
```

### 步骤 4: 测试

#### 测试清单

- [ ] 项目能正常编译
- [ ] 无 Obsolete 警告（或已确认可忽略）
- [ ] Buff系统基础功能正常
- [ ] Combo系统功能正常（如使用）
- [ ] 融合系统功能正常（如使用）
- [ ] 传播系统功能正常（如使用）
- [ ] 场景切换后Manager仍然存在

#### 调试技巧

如果出现问题，可以查看BuffSystemManager状态：

```csharp
#if UNITY_EDITOR
// 打印调试信息
Debug.Log(BuffSystemManager.Instance.GetDebugInfo());
#endif
```

---

## 📊 迁移检查清单

### 代码检查

- [ ] 所有 `using BuffSystem.Combo` 已更新
- [ ] 所有 `using BuffSystem.Fusion` 已更新
- [ ] 所有 `using BuffSystem.Transmission` 已更新
- [ ] 所有 `BuffComboManager.Instance` 已更新
- [ ] 所有 `FusionManager.Instance` 已更新
- [ ] 所有 `TransmissionManager.Instance` 已更新

### 场景检查

- [ ] 场景中有BuffSystemManager（或确认会自动创建）
- [ ] 场景中无重复的Manager（旧Manager已移除）

### 资源检查

- [ ] 所有BuffDataSO引用正常
- [ ] 所有Combo配置引用正常
- [ ] 所有融合配方引用正常

### 测试检查

- [ ] 单元测试通过
- [ ] 集成测试通过
- [ ] 场景测试通过

---

## 🐛 常见问题

### Q: 编译错误 "BuffComboManager.Instance 不存在"

**原因:** 使用了新的命名空间但没有启用兼容模式

**解决:** 
- 方案1: 启用兼容模式（添加 `BUFFSYSTEM_COMPATIBILITY_V6`）
- 方案2: 更新代码使用 `BuffSystemManager.Combo`

### Q: 运行时错误 "BuffSystemManager.Instance 为 null"

**原因:** BuffSystemManager 未正确初始化

**解决:**
```csharp
// 在首次访问前确保初始化
void Start()
{
    // 这会触发自动创建
    _ = BuffSystemManager.Instance;
    
    // 然后再使用
    BuffSystemManager.Combo.RegisterCombo(data);
}
```

### Q: 场景切换后Manager失效

**原因:** 旧Manager被销毁，新场景没有Manager

**解决:**
确保BuffSystemManager在第一个场景中创建，且启用了 `DontDestroyOnLoad`（这是默认行为）。

### Q: 如何同时使用新旧两种方式？

**回答:** 可以，但不推荐。旧方式会显示Obsolete警告。

```csharp
// 新旧方式可以混用
BuffSystemManager.Combo.RegisterCombo(data);  // 新方式
BuffComboManager.Instance.ClearOwnerCombos(owner);  // 旧方式（显示警告）
```

### Q: 第三方插件依赖BuffSystem怎么办？

**回答:** 启用兼容模式即可，无需修改第三方插件。

---

## 📝 版本对比

### v6.x 代码风格

```csharp
using BuffSystem.Combo;
using BuffSystem.Fusion;

public class MyManager : MonoBehaviour
{
    void Start()
    {
        // 各自独立的单例
        BuffComboManager.Instance.RegisterCombo(myCombo);
        FusionManager.Instance.RegisterRecipe(recipe);
        
        // 访问其他Manager
        var combo = BuffComboManager.Instance.GetComboById(1);
    }
}
```

### v7.0 代码风格

```csharp
using BuffSystem.Advanced.Combo;
using BuffSystem.Advanced.Fusion;

public class MyManager : MonoBehaviour
{
    void Start()
    {
        // 统一入口
        BuffSystemManager.Combo.RegisterCombo(myCombo);
        BuffSystemManager.Fusion.RegisterRecipe(recipe);
        
        // 访问其他Manager
        var combo = BuffSystemManager.Combo.GetComboById(1);
    }
}
```

---

## 🎯 最佳实践

### 1. 新项目建议

- 直接使用 v7.0 API
- 使用 `BuffSystemManager` 统一访问
- 使用新的命名空间

### 2. 现有项目建议

- **短期:** 启用兼容模式，暂不修改代码
- **中期:** 逐步更新代码，消除Obsolete警告
- **长期:** 完全迁移到新API

### 3. 团队协作

- 在团队文档中记录迁移状态
- 使用代码审查确保新代码使用新API
- 设置CI检查，防止回退到旧API

---

## 📚 相关文档

- [API参考文档](API_REFERENCE.md) - 完整的API参考
- [API变更日志](API_CHANGELOG.md) - API变更记录
- [API版本文档](../Scripts/Core/API_VERSIONS.md) - API稳定性状态
- [CHANGELOG](../CHANGELOG.md) - 完整更新日志

---

## 💬 获取帮助

如果在迁移过程中遇到问题：

1. 查看 [FAQ](../FAQ.md)
2. 提交Issue到项目仓库
3. 联系维护团队

---

**祝你迁移顺利！** 🎉
