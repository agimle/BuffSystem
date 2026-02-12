# BuffSystem API稳定性保证

**文档版本：** v6.0  
**生效日期：** 2026-02-12  
**适用范围：** BuffSystem v6.x 及后续版本

---

## 向后兼容承诺

BuffSystem v6.0+ 对以下API提供**100%向后兼容保证**。这意味着：

- 这些API在 v6.x、v7.x、v8.x 及更高版本中保持不变
- 您的代码无需修改即可升级到新版本
- 所有破坏性变更都将通过 `[Obsolete]` 标记提供迁移期

---

## 冻结的API列表

### 核心接口 (BuffSystem.Core)

| API | 类型 | 保证内容 |
|-----|------|----------|
| `IBuff` | 接口 | 所有属性、方法签名 |
| `IBuffData` | 接口 | 所有属性、方法签名 |
| `IBuffLogic` | 接口 | 所有属性、方法签名 |
| `IEffect` | 接口 | 所有属性、方法签名 |
| `IBuffOwner` | 接口 | 所有属性、方法签名 |
| `IBuffContainer` | 接口 | 所有属性、方法签名 |

### 核心基类 (BuffSystem.Core)

| API | 类型 | 保证内容 |
|-----|------|----------|
| `BuffLogicBase` | 抽象类 | 所有公开/保护成员 |
| `EffectBase` | 抽象类 | 所有公开/保护成员 |
| `BuffApi` | 静态类 | 所有公开静态方法 |

### 运行时类 (BuffSystem.Runtime)

| API | 类型 | 保证内容 |
|-----|------|----------|
| `BuffOwner` | 类 | 所有公开方法/属性 |
| `BuffEntity` | 类 | 所有公开方法/属性 |
| `BuffContainer` | 类 | 所有公开方法/属性 |

### 枚举类型

| 枚举 | 保证内容 |
|------|----------|
| `BuffEventType` | 所有枚举值及数值 |
| `BuffStackMode` | 所有枚举值及数值 |
| `BuffRemoveMode` | 所有枚举值及数值 |
| `UpdateMode` | 所有枚举值及数值 |
| `BuffEffectType` | 所有枚举值及数值 |

---

## 版本升级策略

### 如果必须修改冻结API

在极端情况下，如果必须修改以上API，我们将遵循以下流程：

1. **提供兼容层**
   ```csharp
   [Obsolete("此方法在v7.0中将被移除，请使用NewMethod()代替")]
   public void OldMethod() { /* ... */ }
   
   public void NewMethod() { /* ... */ }
   ```

2. **保留期**
   - 标记为 `[Obsolete]` 的API至少保留**2个主版本**
   - 例如：v6.0标记废弃，最早v8.0才能移除

3. **自动迁移工具**
   - 提供代码自动迁移脚本
   - 包含在升级指南中

---

## 不受冻结限制的API

以下API可能在未来版本中变更，但会提前通知：

- **内部实现细节**：私有/内部方法
- **编辑器工具**：Editor命名空间下的类
- **实验性功能**：标记为 `[Experimental]` 的功能
- **扩展功能**：Combo、Area、Networking等高级模块的API

---

## 如何验证兼容性

### 使用编译时检查

所有冻结的API都带有稳定性标记：

```csharp
/// <remarks>
/// 🔒 稳定API: v6.0后保证向后兼容
/// 版本历史: v1.0-v6.0 逐步完善
/// 修改策略: 只允许bug修复，不允许破坏性变更
/// </remarks>
public interface IBuff { /* ... */ }
```

### 使用单元测试

建议为您的Buff逻辑编写单元测试，确保升级后行为一致：

```csharp
[Test]
public void BuffSystem_Upgrade_ShouldMaintainCompatibility()
{
    // 测试核心API调用
    var owner = CreateTestOwner();
    var buff = BuffApi.AddBuff(1, owner);
    
    Assert.IsNotNull(buff);
    Assert.AreEqual(1, buff.DataId);
}
```

---

## 历史版本说明

| 版本 | API变更 | 说明 |
|------|---------|------|
| v1.0 | 初始API | 基础接口设计 |
| v2.0 | 添加Stack系统 | 层数管理 |
| v3.0 | 添加Event系统 | 事件驱动 |
| v4.0 | 添加Immunity系统 | 免疫机制 |
| v5.0 | 完善Combo/Area | 高级功能 |
| **v6.0** | **API冻结** | **稳定性保证开始** |

---

## 反馈与支持

如果您发现冻结API的变更，请提交Issue：

1. 标记为 `api-compatibility` 标签
2. 提供变更前后的代码对比
3. 说明对项目的影响

我们将在24小时内响应，并在必要时发布热修复版本。

---

**BuffSystem 团队承诺：稳定优先，永不背弃。**
