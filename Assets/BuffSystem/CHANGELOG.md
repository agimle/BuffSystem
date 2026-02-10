# 更新日志

本文档记录 BuffSystem 的所有版本更新内容。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/)。

## [未发布]

### 计划中
- [ ] Buff 可视化编辑器
- [ ] Buff 组合系统（Combo Buff）
- [ ] Buff 条件触发器
- [ ] Buff 网络同步支持
- [ ] Buff 存档/读档支持

---

## [1.0.0] - 2026-02-10

### 新增
- **核心系统**
  - 完整的 Buff 生命周期管理
  - 支持叠加、刷新、移除等多种行为
  - 三种更新模式：EveryFrame、Interval、Manual
  - 对象池管理，避免频繁 GC

- **数据系统**
  - ScriptableObject 配置支持
  - BuffDatabase 数据加载和管理
  - BuffDataCenter 数据中心
  - BuffSystemConfig 全局配置

- **事件系统**
  - 全局事件：BuffEventSystem
  - 本地事件：BuffLocalEventSystem
  - 完整的事件类型：Added、Removed、StackChanged、Refreshed、Expired、Cleared

- **策略系统**
  - 叠层策略：Stackable、NonStackable、Independent
  - 刷新策略：Refreshable、NonRefreshable
  - 移除策略：Remove、Reduce

- **效果系统**
  - EffectBasedBuffLogic 基于效果的 Buff 逻辑
  - IEffect 接口，支持自定义效果
  - 可在 Inspector 中配置效果列表

- **编辑器工具**
  - BuffDataSO 自定义 Inspector
  - BuffOwner 运行时调试 Inspector
  - 菜单项快速创建资源
  - SubclassSelector 特性支持

- **工具类**
  - ObjectPool<T> 通用对象池
  - 完整的 API 封装：BuffApi

### 设计特点
- **解耦设计**：不依赖 MonoBehaviour，支持纯代码使用
- **接口抽象**：完整的接口定义，便于扩展和测试
- **类型安全**：泛型接口支持
- **性能优化**：对象池、缓存、延迟移除等优化手段

---

## 版本说明

### 版本号规则

本项目使用 [语义化版本](https://semver.org/lang/zh-CN/)：

版本格式：主版本号.次版本号.修订号

- **主版本号**：不兼容的 API 修改
- **次版本号**：向下兼容的功能性新增
- **修订号**：向下兼容的问题修正

### 版本标签说明

- **[未发布]**：正在开发中，尚未发布
- **[YANKED]**：已撤回版本，不要使用

---

## 如何更新

### 从旧版本升级

1. 备份项目
2. 删除旧版本 BuffSystem 文件夹
3. 导入新版本
4. 检查 API 变更，修改代码
5. 测试所有功能

### 兼容性说明

| 版本 | 兼容性 |
|------|--------|
| 1.0.0 | Unity 2021.3+ |

---

## 贡献

欢迎提交 Issue 和 Pull Request！

请阅读 [CONTRIBUTING.md](CONTRIBUTING.md) 了解如何贡献代码。
