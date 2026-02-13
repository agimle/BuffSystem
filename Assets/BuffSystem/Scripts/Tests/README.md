# BuffSystem 性能测试框架

性能基准测试框架用于测量和验证BuffSystem的性能表现。

## 功能特性

- **基准测试**: 测量核心操作的性能（添加、移除、更新、查询）
- **对比测试**: 对比不同实现方案的性能差异
- **压力测试**: 测试系统在极端条件下的表现
- **内存测试**: 检测内存占用和GC Alloc
- **报告生成**: 自动生成Markdown/HTML/JSON格式的性能报告

## 文件结构

```
Tests/
├── BuffPerformanceTestBase.cs      # 性能测试基类
├── BuffSystemPerformanceTests.cs   # 具体测试用例
├── BenchmarkRunner.cs              # 测试运行器
├── PerformanceReportGenerator.cs   # 报告生成器
├── ExamplePerformanceTest.cs       # 使用示例
└── README.md                       # 本文档
```

## 使用方法

### 1. 在编辑器中运行

通过菜单 `Window/BuffSystem/Performance Test` 打开性能测试窗口：

1. 选择要运行的测试类别
2. 点击"运行性能测试"按钮
3. 查看输出日志和生成的报告

### 2. 在场景中运行

将 `ExamplePerformanceTest` 组件添加到场景中的GameObject：

```csharp
// 自动运行测试
var tester = gameObject.AddComponent<ExamplePerformanceTest>();
tester.RunPerformanceTests();
```

### 3. 程序化运行

```csharp
// 创建测试运行器
var runner = gameObject.AddComponent<BenchmarkRunner>();

// 运行所有测试
runner.RunAllTests();

// 获取结果
var results = runner.GetResults();
foreach (var result in results)
{
    Debug.Log($"{result.Name}: {result.Performance.AverageMs}ms");
}
```

### 4. 编写自定义测试

继承 `BuffPerformanceTestBase` 并添加测试方法：

```csharp
public class MyPerformanceTests : BuffPerformanceTestBase
{
    [PerformanceTest("我的测试", 1000)]
    public PerformanceResult Benchmark_MyFeature()
    {
        var owner = CreateTestOwner();
        var data = CreateTestBuffData();
        
        BeginSample();
        // 执行要测试的操作
        for (int i = 0; i < 1000; i++)
        {
            owner.BuffContainer.AddBuff(data);
        }
        return EndSample(1000);
    }
}
```

## 测试属性

### [PerformanceTest]
基准测试属性
```csharp
[PerformanceTest("测试名称", iterations: 1000)]
public PerformanceResult MyTest() { }
```

### [ComparisonTest]
对比测试属性
```csharp
[ComparisonTest("对比测试名称")]
public void MyComparison() { }
```

### [StressTest]
压力测试属性
```csharp
[StressTest("压力测试名称")]
public TestResult MyStressTest() { }
```

### [MemoryTest]
内存测试属性
```csharp
[MemoryTest("内存测试名称")]
public TestResult MyMemoryTest() { }
```

## 性能指标

### 基准指标

| 操作 | 目标性能 | 说明 |
|------|----------|------|
| 添加Buff | < 0.1ms | 单次添加操作 |
| 更新Buff | < 0.05ms/Buff | 每帧更新 |
| 查询Buff | < 0.01ms | 按ID查询 |
| 移除Buff | < 0.1ms | 单次移除操作 |

### 压力测试指标

| 场景 | 目标性能 | 说明 |
|------|----------|------|
| 10000 Buff | < 5ms/帧 | 单Owner大量Buff |
| 100 Owner | < 10ms/帧 | 多Owner场景 |
| 高频操作 | < 0.1ms | 快速添加移除 |

### 内存指标

| 指标 | 目标值 | 说明 |
|------|--------|------|
| 单Buff内存 | < 256B | 每个Buff实例 |
| GC Alloc | 0 | 运行时无GC分配 |
| 内存泄漏 | 0 | 反复操作无增长 |

## 报告格式

### Markdown报告

生成的Markdown报告包含：
- 测试元信息（时间、版本、平台）
- 汇总统计
- 详细测试结果表格
- 失败测试详情
- 性能优化建议

### HTML报告

可视化HTML报告，包含：
- 美观的表格展示
- 颜色标识通过/失败
- 响应式布局

### JSON报告

结构化JSON数据，便于：
- 自动化分析
- 历史对比
- CI/CD集成

## 最佳实践

1. **定期运行**: 在每次重大更改后运行性能测试
2. **对比基线**: 保存基线结果用于对比
3. **关注趋势**: 跟踪性能指标的变化趋势
4. **自动化**: 将性能测试集成到CI流程

## 注意事项

- 性能测试应在发布模式下运行
- 关闭其他应用程序以获得准确结果
- 多次运行取平均值
- 注意平台差异（Editor vs Build）
