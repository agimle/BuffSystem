---
name: csharp-code-review
description: "Unity C# 项目级自动化代码审查主控技能（依赖分析 → 审查执行 → 自动修复 → 进度管理 → 最终报告）"
---

# 技能定位

`csharp-code-review` 是 Unity C# 项目的 **系统级代码审查主控技能（Entry Skill）**。

负责统筹完整审查生命周期：

- 调度依赖分析
- 构建审查优先级
- 管控逐文件审查
- 触发自动修复
- 维护审查进度
- 汇总最终报告
- 控制系统阻断与恢复

---

# 审查目标

覆盖 Unity C# 项目以下质量维度：

- 架构结构合理性
- 类型依赖健康度
- 命名规范统一性
- Unity API 使用规范
- 生命周期执行顺序
- 序列化策略合规
- 性能与 GC 风险
- 空引用安全
- 编辑器代码隔离
- 注释文档完整性
- 协程与异步规范
- 常量与只读优化
- 访问级别控制

---

# 输入要求（完整约束）

支持输入：

- Unity 项目根目录
- `Assets/Scripts` 目录
- 任意包含 `.cs` 文件的源码目录
- 单个 `.cs` 文件

---

## 输入限制

以下输入 **拒绝执行**：

- 伪代码
- 非 C# 代码
- 仅片段方法逻辑
- 不完整类定义
- 反编译代码
- 混合多语言源码

---

# 子技能组件

| 子技能 | 路径 | 职责 |
|--------|------|------|
| csharp-dependency-analyzer | Analyzer/ | 构建依赖图与审查顺序 |
| csharp-code-review-executor | Executor/ | 逐文件审查与自动修复 |
| review-progress-manager | ProgressManager/ | 进度管理与报告生成 |

---

# 技能协作协议

遵循 `skill-linking-protocol`。

执行链路：

1. Analyzer
2. Executor
3. ProgressManager
4. Final Report

---

# 跨组件数据移交契约（新增）

---

## Analyzer → Executor

移交数据：

- DependencyGraph
- ReviewOrderQueue
- ArchitectureLayerMap
- ReferenceFrequencyMap
- CycleDependencyList
- ArchitectureViolationList

---

## Executor → ProgressManager

回传数据：

- FileName
- Reviewed
- AutoFixCount
- ManualFixCount
- RiskCount
- RiskLevel
- BlockerFound
- ExecutionTime

---

# 执行流程

## 1️⃣ 输入阶段

解析输入路径 / 文件：

- 校验合法性
- 识别文件数量
- 判断是否批次执行

---

## 2️⃣ 系统扫描阶段

调用：`csharp-dependency-analyzer`

新增检测：

- 循环依赖检测
- 架构违规检测
- 层级越权引用检测

---

## 3️⃣ 审查执行阶段

调用：`csharp-code-review-executor`

按 DAG 拓扑顺序逐文件执行。

---

# 审查顺序锁定协议（新增）

Executor 必须遵循：

- 禁止跳过依赖顺序
- 禁止逆序执行
- 禁止并行跨层审查

违规时：

- 系统中止审查
- 记录协议违规

---

## 4️⃣ 自动修复阶段

触发条件：发现可安全修复问题即执行。

---

# AutoFix 总原则（系统级约束）

自动修改必须遵循：

- 不改变业务逻辑
- 不改变变量语义
- 不破坏 Inspector 序列化
- 不修改 public API
- 不破坏 ScriptableObject 数据
- 不引入性能回退
- 最小侵入式修改

---

# AutoFix 安全闸门

以下情况 **禁止自动修改**：

- 跨程序集类型重命名
- public 字段序列化重命名
- ScriptableObject 字段结构修改
- 反射依赖字段修改
- JSON 序列化字段修改

---

# 系统阻断控制（新增）

---

## 阻断触发源

- Dependency Analyzer
- Code Review Executor

---

## 阻断类型

- 循环依赖
- 架构层级违规
- 线程安全违规
- Addressables 资源错误
- 严重内存泄漏风险

---

## 阻断处理流程

1. 发现阻断
2. 写入风险文档
3. ProgressManager 标记 ⛔
4. 冻结 Executor
5. 等待修复
6. 恢复审查

---

# 批次审查策略

当脚本数量 > 30 自动分批：

- Batch_01_Core
- Batch_02_Framework
- Batch_03_Gameplay
- Batch_04_UI

---

# 断点恢复机制

输入：`01_审查进度.md`

恢复：

1. 读取阶段
2. 定位文件
3. 跳过已完成
4. 恢复 Executor

---

# 输出文档体系

| 文件名 | 内容 |
|--------|------|
| 00_检查顺序.md | 审查优先级队列 |
| 01_审查进度.md | 实时进度状态 |
| 02_修改记录.md | 自动修复记录 |
| 03_风险问题.md | 风险汇总 |
| 04_架构分析.md | 命名空间分层 |
| 05_依赖图谱.md | 类型依赖关系 |
| 最终审查报告.md | 综合审查报告 |

---

# 执行状态机定义

| 状态 | 含义 |
|------|------|
| INIT | 初始化 |
| ANALYZING | 依赖分析中 |
| REVIEWING | 审查执行中 |
| FIXING | 自动修复中 |
| TRACKING | 进度更新中 |
| AGGREGATING | 报告聚合中 |
| BLOCKED | 阻断暂停 |
| COMPLETED | 审查完成 |

---

# 最终报告聚合职责

由 ProgressManager 汇总：

- 修改总数
- 风险总数
- 架构违规数
- GC 风险密度
- 命名违规密度
- 生命周期违规数
- 架构健康评分

---

# 完成判定

必须满足：

- 全文件审查完成
- 无阻断残留
- 风险已记录
- 修改已记录
- 已生成最终报告

---

# 执行约束

- 必须先执行 Analyzer
- 禁止跳过依赖排序
- Executor 必须按顺序执行
- 所有修改必须记录
- 进度必须实时更新

---

# 技能边界

本技能不负责：

- 运行时代码执行
- 单元测试生成
- CI/CD 集成
- 非 Unity C# 项目
- IL2CPP 产物审查

---

# 结束语义

当全部批次审查完成后：

1. 汇总修改统计
2. 汇总风险统计
3. 生成架构评分
4. 输出最终审查报告

系统流程结束。