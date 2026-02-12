# BuffSystem API 参考文档

> 本文档详细说明 BuffSystem v7.0 的所有公共 API
> 最后更新: 2026-02-13

---

## 📊 API稳定性图例

| 图标 | 级别 | 说明 |
|------|------|------|
| 🔒 | Stable | 稳定API - 保证向后兼容 |
| 👁️ | Preview | 预览版API - 基本稳定但可能有小调整 |
| 🔬 | Experimental | 实验性API - 可能随时更改 |
| ⚠️ | Deprecated | 已弃用 - 将在未来版本移除 |

---

## 📚 目录

- [核心系统](#核心系统)
- [运行时组件](#运行时组件)
- [数据系统](#数据系统)
- [事件系统](#事件系统)
- [高级系统](#高级系统)
- [工具类](#工具类)

---

## 核心系统

### BuffSystem.Core

#### 🔒 BuffApi

**命名空间:** `BuffSystem.Core`  
**稳定性:** 🔒 稳定API (v6.0+) - 保证向后兼容  
**说明:** Buff系统对外API，提供简洁的Buff操作接口

BuffApi是BuffSystem的核心入口类，提供所有Buff操作的静态方法。从v6.0开始保证向后兼容。

**示例:**
```csharp
// 添加Buff
IBuff buff = BuffApi.AddBuff(1001, player);

// 移除Buff
BuffApi.RemoveBuff(buff);

// 检查是否拥有Buff
bool hasBuff = BuffApi.HasBuff(1001, player);
```

##### 方法

| 方法 | 稳定性 | 说明 |
|------|--------|------|
| `Initialize()` | 🔒 | 初始化Buff系统 |
| `ReloadData()` | 🔒 | 重新加载Buff数据 |
| `AddBuff(int, IBuffOwner, object)` | 🔒 | 通过ID添加Buff |
| `AddBuff(string, IBuffOwner, object)` | 🔒 | 通过名称添加Buff |
| `TryAddBuff(int, IBuffOwner, out IBuff, object)` | 🔒 | 尝试添加Buff |
| `RemoveBuff(IBuff)` | 🔒 | 移除指定Buff |
| `RemoveBuff(int, IBuffOwner)` | 🔒 | 通过ID移除Buff |
| `RemoveAllBuffs(IBuffOwner)` | 🔒 | 移除所有Buff |
| `HasBuff(int, IBuffOwner)` | 🔒 | 检查是否拥有Buff |
| `GetBuff(int, IBuffOwner)` | 🔒 | 获取指定Buff |
| `GetAllBuffs(IBuffOwner)` | 🔒 | 获取所有Buff |

---

#### 🔒 IBuff

**命名空间:** `BuffSystem.Core`  
**稳定性:** 🔒 稳定API (v6.0+) - 保证向后兼容  
**说明:** Buff实例接口，运行时Buff实体的抽象

**属性:**

| 属性 | 类型 | 说明 |
|------|------|------|
| `InstanceId` | int | Buff唯一标识符（实例ID） |
| `DataId` | int | Buff数据ID（配置ID） |
| `Name` | string | Buff名称 |
| `CurrentStack` | int | 当前层数 |
| `MaxStack` | int | 最大层数 |
| `Duration` | float | 当前持续时间 |
| `TotalDuration` | float | 总持续时间 |
| `Owner` | IBuffOwner | Buff持有者 |
| `Data` | IBuffData | Buff数据 |

**方法:**

| 方法 | 说明 |
|------|------|
| `RefreshDuration()` | 刷新持续时间 |
| `AddStack(int)` | 添加层数 |
| `RemoveStack(int)` | 移除层数 |
| `Remove()` | 移除Buff |

---

#### 🔒 IBuffOwner

**命名空间:** `BuffSystem.Core`  
**稳定性:** 🔒 稳定API (v6.0+) - 保证向后兼容  
**说明:** Buff持有者接口，解耦MonoBehaviour依赖

任何需要持有Buff的对象都可以实现此接口。

**属性:**

| 属性 | 类型 | 说明 |
|------|------|------|
| `OwnerId` | int | 持有者唯一标识 |
| `OwnerName` | string | 持有者名称（用于调试） |
| `BuffContainer` | IBuffContainer | Buff容器 |
| `LocalEvents` | BuffLocalEventSystem | 本地事件系统 |

**方法:**

| 方法 | 说明 |
|------|------|
| `OnBuffEvent(BuffEventType, IBuff)` | 当Buff事件发生时调用 |
| `IsImmuneTo(int)` | 检查是否对指定Buff免疫 |
| `IsImmuneTo(string)` | 检查是否对指定标签免疫 |

---

#### 🔒 IBuffData

**命名空间:** `BuffSystem.Core`  
**稳定性:** 🔒 稳定API (v6.0+) - 保证向后兼容  
**说明:** Buff数据接口，定义Buff配置数据结构

**属性:**

| 属性 | 类型 | 说明 |
|------|------|------|
| `Id` | int | Buff唯一ID |
| `BuffName` | string | Buff名称 |
| `Description` | string | Buff描述 |
| `Icon` | Sprite | Buff图标 |
| `MaxStack` | int | 最大层数 |
| `Duration` | float | 持续时间 |
| `StackMode` | BuffStackMode | 层数叠加模式 |
| `RemoveMode` | BuffRemoveMode | 移除模式 |
| `UpdateMode` | UpdateMode | 更新模式 |
| `Tags` | string[] | Buff标签 |

**方法:**

| 方法 | 说明 |
|------|------|
| `CreateLogic()` | 创建Buff逻辑实例 |
| `IsValid()` | 检查数据是否有效 |

---

#### 👁️ BuffSystemManager

**命名空间:** `BuffSystem.Core`  
**稳定性:** 👁️ 预览版API (v7.0) - 基本稳定但可能有小调整  
**说明:** BuffSystem统一入口管理器，管理所有子管理器的生命周期

v7.0新增的统一入口类，用于管理所有高级系统的Manager。

**静态属性:**

| 属性 | 类型 | 说明 |
|------|------|------|
| `Instance` | BuffSystemManager | 全局实例 |
| `Combo` | BuffComboManager | Combo管理器访问点 |
| `Fusion` | FusionManager | 融合管理器访问点 |
| `Transmission` | TransmissionManager | 传播管理器访问点 |

**方法:**

| 方法 | 说明 |
|------|------|
| `AreAllManagersReady()` | 检查所有管理器是否已初始化 |
| `SetComboManager(BuffComboManager)` | 手动设置Combo管理器 |
| `SetFusionManager(FusionManager)` | 手动设置融合管理器 |
| `SetTransmissionManager(TransmissionManager)` | 手动设置传播管理器 |

---

## 运行时组件

### BuffSystem.Runtime

#### 🔒 BuffOwner

**命名空间:** `BuffSystem.Runtime`  
**稳定性:** 🔒 稳定API (v6.0+) - 保证向后兼容  
**说明:** Buff持有者组件，MonoBehaviour适配器

挂载到需要持有Buff的GameObject上。

**属性:**

| 属性 | 类型 | 说明 |
|------|------|------|
| `BuffContainer` | IBuffContainer | Buff容器 |
| `LocalEvents` | BuffLocalEventSystem | 本地事件系统 |
| `BuffCount` | int | 当前Buff数量 |
| `AllOwners` | IReadOnlyList<BuffOwner> | 所有Buff持有者 |

**方法:**

| 方法 | 说明 |
|------|------|
| `IsComboActive(int)` | 检查是否激活了指定Combo |
| `GetActiveCombos()` | 获取所有激活的Combo |
| `GetComboTriggerCount(int)` | 获取Combo触发次数 |

---

#### 🔒 BuffEntity

**命名空间:** `BuffSystem.Runtime`  
**稳定性:** 🔒 稳定API (v6.0+) - 保证向后兼容  
**说明:** Buff实体类，实现IBuff接口

Buff系统的核心运行时类。

---

#### 🔒 BuffContainer

**命名空间:** `BuffSystem.Runtime`  
**稳定性:** 🔒 稳定API (v6.0+) - 保证向后兼容  
**说明:** Buff容器实现，管理持有者的所有Buff

---

## 数据系统

### BuffSystem.Data

#### 🔒 BuffStackMode (枚举)

**命名空间:** `BuffSystem.Data`  
**稳定性:** 🔒 稳定API (v6.0+) - 保证向后兼容

| 值 | 说明 |
|----|------|
| `Stackable` | 可叠加，层数增加 |
| `NonStackable` | 不可叠加，刷新持续时间 |
| `Independent` | 独立实例，每次添加都创建新Buff |

---

#### 🔒 BuffRemoveMode (枚举)

**命名空间:** `BuffSystem.Data`  
**稳定性:** 🔒 稳定API (v6.0+) - 保证向后兼容

| 值 | 说明 |
|----|------|
| `Remove` | 直接移除 |
| `Reduce` | 减少层数，层数为0时移除 |

---

#### 🔒 UpdateMode (枚举)

**命名空间:** `BuffSystem.Data`  
**稳定性:** 🔒 稳定API (v6.0+) - 保证向后兼容

| 值 | 说明 |
|----|------|
| `EveryFrame` | 每帧更新 |
| `Interval` | 按间隔更新 |
| `Manual` | 手动更新 |

---

#### 🔒 UpdateFrequency (枚举)

**命名空间:** `BuffSystem.Data`  
**稳定性:** 🔒 稳定API (v6.0+) - 保证向后兼容  
**说明:** v6.0新增 - 分层更新频率

| 值 | 说明 |
|----|------|
| `High` | 高频 - 每帧更新 |
| `Normal` | 正常 - 每2帧更新 |
| `Low` | 低频 - 每4帧更新 |
| `VeryLow` | 极低频 - 每8帧更新 |

---

## 事件系统

### BuffSystem.Events

#### 🔒 BuffEventType (枚举)

**命名空间:** `BuffSystem.Core`  
**稳定性:** 🔒 稳定API (v6.0+) - 保证向后兼容

| 值 | 说明 |
|----|------|
| `Added` | Buff添加 |
| `Removed` | Buff移除 |
| `StackChanged` | 层数变化 |
| `Refreshed` | 持续时间刷新 |
| `Expired` | Buff过期 |
| `Cleared` | 所有Buff清除 |

---

#### 🔒 BuffEventSystem

**命名空间:** `BuffSystem.Events`  
**稳定性:** 🔒 稳定API (v6.0+) - 保证向后兼容  
**说明:** 全局事件系统

**事件:**

| 事件 | 说明 |
|------|------|
| `OnBuffAdded` | Buff添加事件 |
| `OnBuffRemoved` | Buff移除事件 |
| `OnBuffStackChanged` | 层数变化事件 |
| `OnBuffRefreshed` | 刷新事件 |
| `OnBuffExpired` | 过期事件 |

---

## 高级系统

### BuffSystem.Advanced.Combo

#### 👁️ BuffComboManager

**命名空间:** `BuffSystem.Advanced.Combo`  
**稳定性:** 👁️ 预览版API (v7.0) - 基本稳定但可能有小调整  
**说明:** Combo系统管理器

v7.0从 `BuffSystem.Combo` 迁移到 `BuffSystem.Advanced.Combo`。

**访问方式:**
```csharp
// 新方式（推荐）
BuffSystemManager.Combo.RegisterCombo(data);

// 旧方式（兼容，显示Obsolete警告）
BuffComboManager.Instance.RegisterCombo(data);
```

---

### BuffSystem.Advanced.Fusion

#### 👁️ FusionManager

**命名空间:** `BuffSystem.Advanced.Fusion`  
**稳定性:** 👁️ 预览版API (v7.0) - 基本稳定但可能有小调整  
**说明:** Buff融合管理器

**访问方式:**
```csharp
// 新方式（推荐）
BuffSystemManager.Fusion.TryFusion(recipeId, container, out result);

// 旧方式（兼容，显示Obsolete警告）
FusionManager.Instance.TryFusion(recipeId, container, out result);
```

---

### BuffSystem.Advanced.Transmission

#### 👁️ TransmissionManager

**命名空间:** `BuffSystem.Advanced.Transmission`  
**稳定性:** 👁️ 预览版API (v7.0) - 基本稳定但可能有小调整  
**说明:** Buff传播管理器

**访问方式:**
```csharp
// 新方式（推荐）
BuffSystemManager.Transmission.RequestTransmission(buff);

// 旧方式（兼容，显示Obsolete警告）
TransmissionManager.Instance.RequestTransmission(buff);
```

---

## 工具类

### 属性类

#### ApiStabilityAttribute

**命名空间:** `BuffSystem.Core`  
**说明:** API稳定性标记属性

用于标记API的稳定性级别和版本信息。

**快捷属性:**

| 属性 | 说明 |
|------|------|
| `[StableApi("6.0")]` | 标记稳定API |
| `[PreviewApi]` | 标记预览版API |
| `[ExperimentalApi]` | 标记实验性API |
| `[DeprecatedApi("替代方案", "8.0")]` | 标记已弃用API |

---

## 🔄 版本迁移指南

### 从 v6.x 迁移到 v7.0

#### 1. 命名空间更新（可选）

```csharp
// 旧代码
using BuffSystem.Combo;
using BuffSystem.Fusion;

// 新代码
using BuffSystem.Advanced.Combo;
using BuffSystem.Advanced.Fusion;
```

或者启用兼容模式：在Project Settings中定义 `BUFFSYSTEM_COMPATIBILITY_V6`

#### 2. Manager访问更新（可选）

```csharp
// 旧代码
BuffComboManager.Instance.RegisterCombo(data);

// 新代码
BuffSystemManager.Combo.RegisterCombo(data);
```

---

## 📖 相关文档

- [API版本文档](../Scripts/Core/API_VERSIONS.md) - 详细的版本历史
- [API变更日志](API_CHANGELOG.md) - API变更记录
- [开发者迁移指南](MIGRATION_GUIDE.md) - 详细的迁移步骤
- [使用指南](../Tutorial.md) - 快速入门教程
- [架构设计](../Architecture.md) - 系统架构说明

---

## 💬 反馈

如有关于API的疑问或建议，请提交Issue或联系维护团队。
