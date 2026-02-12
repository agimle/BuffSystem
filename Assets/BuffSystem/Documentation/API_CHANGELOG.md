# API 变更日志

> 本文档记录BuffSystem API的所有变更历史
> 格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/)

---

## [v7.0] - 2026-02-13

### 🔴 重大变更

#### 命名空间重构
- **影响范围:** Combo, Fusion, Transmission, Area, Snapshot 系统
- **变更内容:** 所有高级系统从根命名空间迁移到 `BuffSystem.Advanced` 子命名空间
- **向后兼容:** 定义 `BUFFSYSTEM_COMPATIBILITY_V6` 可使用旧命名空间

| 旧命名空间 | 新命名空间 | 状态 |
|-----------|-----------|------|
| `BuffSystem.Combo` | `BuffSystem.Advanced.Combo` | ⚠️ 已弃用 |
| `BuffSystem.Fusion` | `BuffSystem.Advanced.Fusion` | ⚠️ 已弃用 |
| `BuffSystem.Transmission` | `BuffSystem.Advanced.Transmission` | ⚠️ 已弃用 |
| `BuffSystem.Area` | `BuffSystem.Advanced.Area` | ⚠️ 已弃用 |
| `BuffSystem.Snapshot` | `BuffSystem.Advanced.Snapshot` | ⚠️ 已弃用 |

#### Manager单例统一
- **影响范围:** BuffComboManager, FusionManager, TransmissionManager
- **变更内容:** 单例访问改为通过 `BuffSystemManager` 统一入口
- **向后兼容:** 旧访问方式仍可用，但显示Obsolete警告

```csharp
// 旧方式（已弃用）
BuffComboManager.Instance.RegisterCombo(data);

// 新方式（推荐）
BuffSystemManager.Combo.RegisterCombo(data);
```

### ✨ 新增 API

#### 核心系统

##### BuffSystemManager
- **命名空间:** `BuffSystem.Core`
- **稳定性:** 👁️ Preview
- **说明:** 统一入口管理器，管理所有子管理器的生命周期
- **新增属性:**
  - `BuffSystemManager.Combo` - Combo管理器访问点
  - `BuffSystemManager.Fusion` - 融合管理器访问点
  - `BuffSystemManager.Transmission` - 传播管理器访问点

##### ApiStabilityAttribute
- **命名空间:** `BuffSystem.Core`
- **稳定性:** 🔒 Stable (v7.0+)
- **说明:** API稳定性标记属性
- **快捷属性:**
  - `[StableApi("版本")]` - 标记稳定API
  - `[PreviewApi]` - 标记预览版API
  - `[ExperimentalApi]` - 标记实验性API
  - `[DeprecatedApi("替代方案", "移除版本")]` - 标记已弃用API

### 📊 API稳定性更新

#### 升级到 Stable
暂无

#### 标记为 Preview
| API | 命名空间 | 说明 |
|-----|---------|------|
| `BuffSystemManager` | `BuffSystem.Core` | 统一入口管理器 |
| `BuffComboManager` | `BuffSystem.Advanced.Combo` | Combo系统管理器 |
| `FusionManager` | `BuffSystem.Advanced.Fusion` | 融合管理器 |
| `TransmissionManager` | `BuffSystem.Advanced.Transmission` | 传播管理器 |

#### 标记为 Deprecated
| API | 替代方案 | 计划移除版本 |
|-----|---------|-------------|
| `BuffComboManager.Instance` | `BuffSystemManager.Combo` | v8.0 |
| `FusionManager.Instance` | `BuffSystemManager.Fusion` | v8.0 |
| `TransmissionManager.Instance` | `BuffSystemManager.Transmission` | v8.0 |
| `BuffSystem.Combo` 命名空间 | `BuffSystem.Advanced.Combo` | v8.0 |
| `BuffSystem.Fusion` 命名空间 | `BuffSystem.Advanced.Fusion` | v8.0 |
| `BuffSystem.Transmission` 命名空间 | `BuffSystem.Advanced.Transmission` | v8.0 |

### 🔧 内部改进

- 优化了Manager的生命周期管理
- 添加了自动化API文档生成工具
- 完善了API稳定性标记系统

---

## [v6.0] - 2026-02-10

### ✨ 新增 API

#### 核心系统
- `IBuff` - Buff实例接口
- `IBuffOwner` - Buff持有者接口
- `IBuffData` - Buff数据接口
- `BuffApi` - 核心API类

#### 运行时组件
- `BuffOwner` - MonoBehaviour适配器
- `BuffEntity` - Buff实体类
- `BuffContainer` - Buff容器
- `BuffContainerNativeArray` - NativeArray优化版本
- `FrequencyBasedUpdater` - 分层更新管理器
- `FrequencyAssigner` - 频率分配器

#### 数据类型
- `BuffStackMode` - 层数叠加模式枚举
- `BuffRemoveMode` - 移除模式枚举
- `UpdateMode` - 更新模式枚举
- `UpdateFrequency` - 更新频率枚举

#### 事件系统
- `BuffEventType` - 事件类型枚举
- `BuffEventSystem` - 全局事件系统
- `BuffLocalEventSystem` - 本地事件系统

#### 高级系统
- `BuffComboManager` - Combo系统管理器
- `FusionManager` - 融合管理器
- `TransmissionManager` - 传播管理器
- `BuffArea` - 区域Buff系统
- `BuffSnapshot` - Buff快照系统

### 📊 API稳定性基线

从v6.0开始，以下API标记为 **🔒 Stable**，保证向后兼容：

- 所有核心接口 (`IBuff`, `IBuffOwner`, `IBuffData`)
- 核心API类 (`BuffApi`)
- 运行时组件 (`BuffOwner`, `BuffEntity`, `BuffContainer`)
- 数据类型枚举 (`BuffStackMode`, `BuffRemoveMode`, `UpdateMode`)
- 事件系统 (`BuffEventType`, `BuffEventSystem`)

---

## 版本说明

### 版本号规则

本项目使用 [语义化版本](https://semver.org/lang/zh-CN/)：

- **主版本号 (X.0.0):** 不兼容的API修改
- **次版本号 (0.X.0):** 向下兼容的功能性新增
- **修订号 (0.0.X):** 向下兼容的问题修正

### 稳定性级别

| 级别 | 说明 | 兼容性保证 |
|------|------|-----------|
| 🔒 Stable | 稳定API | 主版本号不变，保证向后兼容 |
| 👁️ Preview | 预览版API | 次版本号不变，可能有小调整 |
| 🔬 Experimental | 实验性API | 无保证，可能随时更改 |
| ⚠️ Deprecated | 已弃用API | 计划移除，提供替代方案 |

---

## 迁移指南

### 从 v6.x 迁移到 v7.0

#### 步骤 1: 更新命名空间（可选）

如果你的代码使用了高级系统，可以更新命名空间：

```csharp
// 旧代码
using BuffSystem.Combo;
using BuffSystem.Fusion;
using BuffSystem.Transmission;

// 新代码
using BuffSystem.Advanced.Combo;
using BuffSystem.Advanced.Fusion;
using BuffSystem.Advanced.Transmission;
```

或者启用兼容模式，在Unity的 **Project Settings > Player > Scripting Define Symbols** 中添加：
```
BUFFSYSTEM_COMPATIBILITY_V6
```

#### 步骤 2: 更新Manager访问方式（可选）

```csharp
// 旧代码
BuffComboManager.Instance.RegisterCombo(data);
FusionManager.Instance.TryFusion(id, container, out result);
TransmissionManager.Instance.RequestTransmission(buff);

// 新代码
BuffSystemManager.Combo.RegisterCombo(data);
BuffSystemManager.Fusion.TryFusion(id, container, out result);
BuffSystemManager.Transmission.RequestTransmission(buff);
```

旧访问方式仍然可用，但会显示Obsolete警告。

#### 步骤 3: 测试

1. 更新后运行所有单元测试
2. 检查控制台是否有Obsolete警告
3. 测试高级系统功能是否正常

---

## 相关文档

- [API参考文档](API_REFERENCE.md) - 完整的API参考
- [API版本文档](../Scripts/Core/API_VERSIONS.md) - API稳定性状态
- [开发者迁移指南](MIGRATION_GUIDE.md) - 详细的迁移步骤
- [CHANGELOG](../CHANGELOG.md) - 完整更新日志
