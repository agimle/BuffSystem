# 贡献指南

感谢您对 BuffSystem 的兴趣！本文档将帮助您了解如何为项目做出贡献。

## 目录

- [行为准则](#行为准则)
- [如何贡献](#如何贡献)
- [开发环境](#开发环境)
- [代码规范](#代码规范)
- [提交规范](#提交规范)
- [版本发布](#版本发布)

---

## 行为准则

### 我们的承诺

为了营造一个开放和友好的环境，我们作为贡献者和维护者承诺：

- 尊重不同的观点和经验
- 接受建设性的批评
- 关注对社区最有利的事情
- 对其他社区成员表示同理心

### 不可接受的行为

- 使用带有性暗示的语言或图像
- 挑衅、侮辱/贬损的评论，以及个人或政治攻击
- 公开或私下的骚扰
- 未经明确许可发布他人的私人信息
- 其他不道德或不专业的行为

---

## 如何贡献

### 报告 Bug

如果您发现了 Bug，请通过 GitHub Issues 报告，并包含以下信息：

1. **问题描述**：清晰简洁地描述 Bug
2. **复现步骤**：详细说明如何复现问题
3. **期望行为**：描述您期望发生的行为
4. **实际行为**：描述实际发生的行为
5. **环境信息**：
   - Unity 版本
   - BuffSystem 版本
   - 平台（Windows/Mac/Linux）
6. **附加信息**：截图、代码片段等

**Bug 报告模板：**

```markdown
## Bug 描述
[清晰简洁地描述 Bug]

## 复现步骤
1. 步骤 1
2. 步骤 2
3. 步骤 3

## 期望行为
[描述您期望发生的行为]

## 实际行为
[描述实际发生的行为]

## 环境信息
- Unity 版本：
- BuffSystem 版本：
- 平台：

## 附加信息
[截图、代码片段等]
```

### 建议新功能

如果您有新功能建议，请通过 GitHub Issues 提交，并包含：

1. **功能描述**：清晰简洁地描述功能
2. **使用场景**：说明这个功能会在什么场景下使用
3. **期望行为**：描述您期望这个功能如何工作
4. **可能的实现**：如果您有实现思路，可以分享
5. **替代方案**：您是否考虑过其他解决方案

### 提交代码

1. Fork 本仓库
2. 创建您的功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交您的更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打开一个 Pull Request

**Pull Request 流程：**

1. 确保您的代码符合代码规范
2. 更新相关文档（README、API 文档等）
3. 确保所有测试通过
4. 在 PR 描述中说明更改内容
5. 等待维护者审核

---

## 开发环境

### 要求

- Unity 2021.3 LTS 或更高版本
- .NET Standard 2.1
- Git

### 设置开发环境

1. Fork 并克隆仓库
```bash
git clone https://github.com/YOUR_USERNAME/BuffSystem.git
```

2. 在 Unity 中打开项目
```
Unity -> Open Project -> 选择 BuffSystem 文件夹
```

3. 等待 Unity 导入完成

4. 运行测试场景验证环境

### 项目结构

```
BuffSystem/
├── Assets/
│   └── BuffSystem/
│       ├── Scripts/          # 源代码
│       ├── Examples/         # 示例场景
│       ├── Tests/            # 测试代码
│       └── Documentation/    # 文档
├── Packages/
├── ProjectSettings/
└── README.md
```

---

## 代码规范

### C# 编码规范

#### 命名规范

| 类型 | 规范 | 示例 |
|------|------|------|
| 类名 | PascalCase | `BuffManager` |
| 接口名 | PascalCase，以 I 开头 | `IBuff` |
| 方法名 | PascalCase | `AddBuff()` |
| 属性名 | PascalCase | `CurrentStack` |
| 字段名 | camelCase，私有字段加 _ 前缀 | `_buffContainer` |
| 常量名 | PascalCase | `MaxStackCount` |
| 枚举名 | PascalCase | `BuffEffectType` |
| 事件名 | PascalCase，以 On 开头 | `OnBuffAdded` |

#### 代码风格

```csharp
// 使用显式访问修饰符
public class BuffEntity : IBuff
{
    // 私有字段
    private int instanceId;
    private IBuffData data;
    
    // 属性
    public int InstanceId => instanceId;
    public int CurrentStack { get; private set; }
    
    // 方法
    public void AddStack(int amount)
    {
        if (amount <= 0) return;
        
        int oldStack = CurrentStack;
        CurrentStack = Mathf.Min(CurrentStack + amount, MaxStack);
        
        if (CurrentStack != oldStack)
        {
            OnStackChanged(oldStack, CurrentStack);
        }
    }
    
    // 私有辅助方法
    private void OnStackChanged(int oldStack, int newStack)
    {
        // ...
    }
}
```

#### 注释规范

```csharp
/// <summary>
/// Buff 实体类，实现 IBuff 接口
/// 使用对象池管理生命周期
/// </summary>
public class BuffEntity : IBuff
{
    /// <summary>
    /// 增加层数
    /// </summary>
    /// <param name="amount">增加的数量</param>
    /// <remarks>层数不会超过最大层数</remarks>
    public void AddStack(int amount)
    {
        // 实现代码
    }
}
```

### 接口设计规范

1. **单一职责**：每个接口只做一件事
2. **接口隔离**：避免胖接口，拆分为小接口
3. **依赖抽象**：依赖接口而非具体实现

```csharp
// 好的设计：细粒度接口
public interface IBuffAcquire
{
    void OnAcquire();
}

public interface IBuffRemove
{
    void OnRemove();
}

// 不好的设计：胖接口
public interface IBuffLifecycle  // 避免这样
{
    void OnAcquire();
    void OnRemove();
    void OnUpdate();
    void OnRefresh();
    // ... 太多方法
}
```

### 性能考虑

1. **避免装箱**：使用泛型避免值类型装箱
2. **对象池**：频繁创建的对象使用对象池
3. **缓存**：避免重复计算和获取
4. **延迟执行**：使用队列延迟处理

```csharp
// 好的做法：使用对象池
private readonly ObjectPool<BuffEntity> buffPool;

public IBuff CreateBuff()
{
    return buffPool.Get();  // 从池中获取
}

// 不好的做法：直接 new
public IBuff CreateBuff()
{
    return new BuffEntity();  // 避免这样
}
```

---

## 提交规范

### 提交信息格式

```
<type>(<scope>): <subject>

<body>

<footer>
```

#### Type（类型）

| 类型 | 说明 |
|------|------|
| feat | 新功能 |
| fix | 修复 Bug |
| docs | 文档更新 |
| style | 代码格式（不影响功能） |
| refactor | 重构 |
| perf | 性能优化 |
| test | 测试相关 |
| chore | 构建过程或辅助工具的变动 |

#### Scope（范围）

可选，说明影响的范围：
- core
- data
- runtime
- events
- editor
- docs

#### Subject（主题）

简短描述，不超过 50 个字符：
- 使用祈使句，现在时
- 首字母小写
- 不以句号结尾

#### Body（正文）

详细描述，可分成多行：
- 说明修改的动机
- 与之前行为的对比

#### Footer（页脚）

- **Breaking Changes**：不兼容的修改
- **Closes**：关闭的 Issue

### 提交示例

```
feat(core): 添加 Buff 持续时间变化事件

添加 IBuffDurationChange 接口，用于监听 Buff 持续时间变化。
这在需要精确控制 Buff 倒计时的 UI 中很有用。

Closes #123
```

```
fix(runtime): 修复 Buff 层数计算错误

当 AddStackCount 大于 1 时，层数计算不正确。
现在正确处理多层叠加的情况。

Closes #456
```

```
docs(api): 更新 API 文档

添加 BuffApi.TryAddBuff 方法的说明和示例。
```

```
refactor(data): 重构 BuffDatabase 数据加载

使用异步加载替代同步加载，减少启动时的卡顿。

BREAKING CHANGE: BuffDatabase.Initialize() 现在是异步方法
```

---

## 版本发布

### 版本号规则

使用 [语义化版本](https://semver.org/lang/zh-CN/)：

- **主版本号**：不兼容的 API 修改
- **次版本号**：向下兼容的功能性新增
- **修订号**：向下兼容的问题修正

### 发布流程

1. 更新版本号
   - 更新 `package.json`（如果有）
   - 更新 `CHANGELOG.md`

2. 创建发布分支
```bash
git checkout -b release/v1.0.0
```

3. 进行最终测试

4. 合并到主分支
```bash
git checkout main
git merge release/v1.0.0
```

5. 打标签
```bash
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

6. 创建 GitHub Release
   - 填写发布说明
   - 上传打包文件

---

## 代码审查

### 审查清单

- [ ] 代码是否符合编码规范
- [ ] 是否有适当的注释
- [ ] 是否有单元测试
- [ ] 性能是否可接受
- [ ] 是否破坏了向后兼容性
- [ ] 文档是否已更新

### 审查原则

1. **尊重**：对事不对人，尊重贡献者的努力
2. **建设性**：提供改进建议，而非单纯批评
3. **及时性**：尽快完成审查，避免阻塞
4. **学习**：互相学习，共同进步

---

## 社区

### 交流渠道

- GitHub Issues：Bug 报告和功能建议
- GitHub Discussions：一般性讨论
- 邮件：xxx@example.com

### 贡献者

感谢所有为 BuffSystem 做出贡献的人！

[贡献者列表]

---

## 许可证

通过贡献代码，您同意您的贡献将在 MIT 许可证下发布。
