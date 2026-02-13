using UnityEngine;

namespace BuffSystem.Tests
{
    /// <summary>
    /// 示例性能测试组件
    /// 展示如何在场景中使用性能测试框架
    /// </summary>
    public class ExamplePerformanceTest : MonoBehaviour
    {
        [Header("测试配置")]
        [Tooltip("是否在游戏开始时自动运行测试")]
        [SerializeField] private bool autoRunOnStart = true;
        
        [Tooltip("是否在控制台输出详细日志")]
        [SerializeField] private bool verboseLogging = true;
        
        [Tooltip("是否生成性能报告文件")]
        [SerializeField] private bool generateReportFile = true;

        [Header("测试选项")]
        [SerializeField] private bool runBenchmarkTests = true;
        [SerializeField] private bool runComparisonTests = true;
        [SerializeField] private bool runStressTests = true;
        [SerializeField] private bool runMemoryTests = true;

        private BenchmarkRunner runner;

        private void Start()
        {
            if (autoRunOnStart)
            {
                RunPerformanceTests();
            }
        }

        [ContextMenu("运行性能测试")]
        public void RunPerformanceTests()
        {
            Debug.Log("<color=cyan>===== BuffSystem 性能测试 =====</color>");
            
            // 创建测试运行器
            GameObject runnerObj = new GameObject("BenchmarkRunner");
            runner = runnerObj.AddComponent<BenchmarkRunner>();
            
            // 配置测试选项
            ConfigureRunner();
            
            // 运行测试
            runner.RunAllTests();
            
            // 显示结果
            DisplayResults();
        }

        /// <summary>
        /// 配置测试运行器
        /// </summary>
        private void ConfigureRunner()
        {
            // 使用反射设置私有字段（实际使用时可通过公共API配置）
            var type = runner.GetType();
            
            type.GetField("logToConsole", System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance)?.SetValue(runner, verboseLogging);
            
            type.GetField("generateReport", System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance)?.SetValue(runner, generateReportFile);
            
            type.GetField("runBenchmarkTests", System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance)?.SetValue(runner, runBenchmarkTests);
            
            type.GetField("runComparisonTests", System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance)?.SetValue(runner, runComparisonTests);
            
            type.GetField("runStressTests", System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance)?.SetValue(runner, runStressTests);
            
            type.GetField("runMemoryTests", System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance)?.SetValue(runner, runMemoryTests);
        }

        /// <summary>
        /// 显示测试结果
        /// </summary>
        private void DisplayResults()
        {
            if (runner == null) return;
            
            var results = runner.GetResults();
            
            Debug.Log("<color=yellow>----- 测试结果摘要 -----</color>");
            
            int total = results.Count;
            int passed = 0;
            int failed = 0;
            
            foreach (var result in results)
            {
                if (result.Passed) passed++;
                else failed++;
            }
            
            Debug.Log($"总测试数: {total}");
            Debug.Log($"<color=green>通过: {passed}</color>");
            Debug.Log($"<color=red>失败: {failed}</color>");
            Debug.Log($"通过率: {(total > 0 ? (passed * 100 / total) : 0)}%");
            
            if (failed > 0)
            {
                Debug.Log("<color=red>失败的测试:</color>");
                foreach (var result in results)
                {
                    if (!result.Passed)
                    {
                        Debug.Log($"  - {result.Name}: {result.ErrorMessage}");
                    }
                }
            }
            
            Debug.Log("<color=cyan>===========================</color>");
        }

        [ContextMenu("仅运行基准测试")]
        public void RunBenchmarkOnly()
        {
            runBenchmarkTests = true;
            runComparisonTests = false;
            runStressTests = false;
            runMemoryTests = false;
            RunPerformanceTests();
        }

        [ContextMenu("仅运行压力测试")]
        public void RunStressOnly()
        {
            runBenchmarkTests = false;
            runComparisonTests = false;
            runStressTests = true;
            runMemoryTests = false;
            RunPerformanceTests();
        }

        [ContextMenu("仅运行内存测试")]
        public void RunMemoryOnly()
        {
            runBenchmarkTests = false;
            runComparisonTests = false;
            runStressTests = false;
            runMemoryTests = true;
            RunPerformanceTests();
        }
    }
}
