using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BuffSystem.Tests
{
    /// <summary>
    /// 基准测试运行器
    /// </summary>
    public class BenchmarkRunner : MonoBehaviour
    {
        [Header("测试设置")]
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private bool logToConsole = true;
        [SerializeField] private bool generateReport = true;

        [Header("测试筛选")]
        [SerializeField] private bool runBenchmarkTests = true;
        [SerializeField] private bool runComparisonTests = true;
        [SerializeField] private bool runStressTests = true;
        [SerializeField] private bool runMemoryTests = true;

        [Header("输出设置")]
        [SerializeField] private string reportFileName = "BuffPerformanceReport";

        private List<TestResult> allResults = new List<TestResult>();

        private void Start()
        {
            if (runOnStart)
            {
                RunAllTests();
            }
        }

        [ContextMenu("运行所有测试")]
        public void RunAllTests()
        {
            allResults.Clear();
            Debug.Log("=== BuffSystem 性能测试开始 ===");

            var testInstance = new BuffSystemPerformanceTests();
            var type = testInstance.GetType();

            // 运行基准测试
            if (runBenchmarkTests)
            {
                RunBenchmarkTests(testInstance, type);
            }

            // 运行对比测试
            if (runComparisonTests)
            {
                RunComparisonTests(testInstance, type);
            }

            // 运行压力测试
            if (runStressTests)
            {
                RunStressTests(testInstance, type);
            }

            // 运行内存测试
            if (runMemoryTests)
            {
                RunMemoryTests(testInstance, type);
            }

            // 输出结果
            LogResults();

            // 生成报告
            if (generateReport)
            {
                GenerateReport();
            }

            Debug.Log("=== BuffSystem 性能测试完成 ===");
        }

        /// <summary>
        /// 运行基准测试
        /// </summary>
        private void RunBenchmarkTests(BuffSystemPerformanceTests instance, Type type)
        {
            Debug.Log("\n[基准测试]");
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<PerformanceTestAttribute>() != null);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<PerformanceTestAttribute>();
                try
                {
                    var result = (PerformanceResult)method.Invoke(instance, null);
                    var testResult = new TestResult
                    {
                        Name = attr.Name,
                        Category = TestCategory.Benchmark,
                        Performance = result,
                        Passed = true,
                        ExecuteTime = DateTime.Now
                    };
                    allResults.Add(testResult);

                    if (logToConsole)
                    {
                        Debug.Log($"  ✓ {attr.Name}: {result}");
                    }
                }
                catch (Exception ex)
                {
                    allResults.Add(new TestResult
                    {
                        Name = attr.Name,
                        Category = TestCategory.Benchmark,
                        Passed = false,
                        ErrorMessage = ex.InnerException?.Message ?? ex.Message,
                        ExecuteTime = DateTime.Now
                    });

                    if (logToConsole)
                    {
                        Debug.LogError($"  ✗ {attr.Name}: {ex.InnerException?.Message ?? ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 运行对比测试
        /// </summary>
        private void RunComparisonTests(BuffSystemPerformanceTests instance, Type type)
        {
            Debug.Log("\n[对比测试]");
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<ComparisonTestAttribute>() != null);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<ComparisonTestAttribute>();
                try
                {
                    method.Invoke(instance, null);

                    if (logToConsole)
                    {
                        Debug.Log($"  ✓ {attr.Name}");
                    }
                }
                catch (Exception ex)
                {
                    if (logToConsole)
                    {
                        Debug.LogError($"  ✗ {attr.Name}: {ex.InnerException?.Message ?? ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 运行压力测试
        /// </summary>
        private void RunStressTests(BuffSystemPerformanceTests instance, Type type)
        {
            Debug.Log("\n[压力测试]");
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<StressTestAttribute>() != null);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<StressTestAttribute>();
                try
                {
                    var result = (TestResult)method.Invoke(instance, null);
                    allResults.Add(result);

                    if (logToConsole)
                    {
                        var status = result.Passed ? "✓" : "✗";
                        var message = result.Passed
                            ? $"{result.Performance}"
                            : result.ErrorMessage;
                        Debug.Log($"  {status} {attr.Name}: {message}");
                    }
                }
                catch (Exception ex)
                {
                    allResults.Add(new TestResult
                    {
                        Name = attr.Name,
                        Category = TestCategory.Stress,
                        Passed = false,
                        ErrorMessage = ex.InnerException?.Message ?? ex.Message,
                        ExecuteTime = DateTime.Now
                    });

                    if (logToConsole)
                    {
                        Debug.LogError($"  ✗ {attr.Name}: {ex.InnerException?.Message ?? ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 运行内存测试
        /// </summary>
        private void RunMemoryTests(BuffSystemPerformanceTests instance, Type type)
        {
            Debug.Log("\n[内存测试]");
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<MemoryTestAttribute>() != null);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<MemoryTestAttribute>();
                try
                {
                    var result = (TestResult)method.Invoke(instance, null);
                    allResults.Add(result);

                    if (logToConsole)
                    {
                        var status = result.Passed ? "✓" : "✗";
                        var message = result.Passed
                            ? $"{(result.Performance.AverageBytes > 0 ? $"{result.Performance.AverageBytes:F0}B" : "通过")}"
                            : result.ErrorMessage;
                        Debug.Log($"  {status} {attr.Name}: {message}");
                    }
                }
                catch (Exception ex)
                {
                    allResults.Add(new TestResult
                    {
                        Name = attr.Name,
                        Category = TestCategory.Memory,
                        Passed = false,
                        ErrorMessage = ex.InnerException?.Message ?? ex.Message,
                        ExecuteTime = DateTime.Now
                    });

                    if (logToConsole)
                    {
                        Debug.LogError($"  ✗ {attr.Name}: {ex.InnerException?.Message ?? ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 输出测试结果汇总
        /// </summary>
        private void LogResults()
        {
            if (!logToConsole) return;

            Debug.Log("\n[测试结果汇总]");
            int total = allResults.Count;
            int passed = allResults.Count(r => r.Passed);
            int failed = total - passed;

            Debug.Log($"总计: {total} | 通过: {passed} | 失败: {failed}");

            if (failed > 0)
            {
                Debug.Log("失败的测试:");
                foreach (var result in allResults.Where(r => !r.Passed))
                {
                    Debug.LogError($"  - {result.Name}: {result.ErrorMessage}");
                }
            }
        }

        /// <summary>
        /// 生成测试报告
        /// </summary>
        private void GenerateReport()
        {
#if UNITY_EDITOR
            var report = PerformanceReportGenerator.GenerateMarkdownReport(allResults);
            string filePath = $"{Application.dataPath}/BuffSystem/{reportFileName}.md";
            System.IO.File.WriteAllText(filePath, report);
            Debug.Log($"性能报告已保存: {filePath}");
#else
            Debug.LogWarning("报告生成仅在编辑器模式下可用");
#endif
        }

        /// <summary>
        /// 获取所有测试结果
        /// </summary>
        public IReadOnlyList<TestResult> GetResults()
        {
            return allResults;
        }
    }
}
