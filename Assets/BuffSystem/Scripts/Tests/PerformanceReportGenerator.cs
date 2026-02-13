#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BuffSystem.Tests
{
    /// <summary>
    /// 性能报告生成器
    /// </summary>
    public static class PerformanceReportGenerator
    {
        /// <summary>
        /// 生成Markdown格式报告
        /// </summary>
        public static string GenerateMarkdownReport(List<TestResult> results)
        {
            var sb = new StringBuilder();
            
            // 报告标题
            sb.AppendLine("# BuffSystem 性能测试报告");
            sb.AppendLine();
            
            // 元信息
            sb.AppendLine("## 测试元信息");
            sb.AppendLine($"- **生成时间**: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"- **Unity版本**: {Application.unityVersion}");
            sb.AppendLine($"- **平台**: {Application.platform}");
            sb.AppendLine($"- **系统内存**: {SystemInfo.systemMemorySize} MB");
            sb.AppendLine($"- **CPU**: {SystemInfo.processorType}");
            sb.AppendLine();
            
            // 汇总统计
            sb.AppendLine("## 测试结果汇总");
            sb.AppendLine();
            
            int total = results.Count;
            int passed = results.Count(r => r.Passed);
            int failed = total - passed;
            
            sb.AppendLine($"- **总测试数**: {total}");
            sb.AppendLine($"- **通过**: {passed} ✅");
            sb.AppendLine($"- **失败**: {failed} ❌");
            sb.AppendLine($"- **通过率**: {(total > 0 ? passed * 100 / total : 0)}%");
            sb.AppendLine();
            
            // 分类统计
            sb.AppendLine("### 分类统计");
            sb.AppendLine();
            sb.AppendLine("| 类别 | 数量 | 通过 | 失败 |");
            sb.AppendLine("|------|------|------|------|");
            
            foreach (TestCategory category in Enum.GetValues(typeof(TestCategory)))
            {
                var categoryResults = results.Where(r => r.Category == category).ToList();
                if (categoryResults.Count > 0)
                {
                    int catPassed = categoryResults.Count(r => r.Passed);
                    int catFailed = categoryResults.Count - catPassed;
                    sb.AppendLine($"| {GetCategoryName(category)} | {categoryResults.Count} | {catPassed} | {catFailed} |");
                }
            }
            sb.AppendLine();
            
            // 详细结果
            sb.AppendLine("## 详细测试结果");
            sb.AppendLine();
            
            // 基准测试详情
            var benchmarkResults = results.Where(r => r.Category == TestCategory.Benchmark).ToList();
            if (benchmarkResults.Count > 0)
            {
                sb.AppendLine("### 基准测试");
                sb.AppendLine();
                sb.AppendLine("| 测试项 | 平均耗时 | 内存分配 | 状态 |");
                sb.AppendLine("|--------|----------|----------|------|");
                
                foreach (var result in benchmarkResults)
                {
                    string status = result.Passed ? "✅ 通过" : "❌ 失败";
                    string time = $"{result.Performance.AverageMs:F3} ms";
                    string memory = $"{result.Performance.AverageBytes:F0} B";
                    sb.AppendLine($"| {result.Name} | {time} | {memory} | {status} |");
                }
                sb.AppendLine();
            }
            
            // 压力测试详情
            var stressResults = results.Where(r => r.Category == TestCategory.Stress).ToList();
            if (stressResults.Count > 0)
            {
                sb.AppendLine("### 压力测试");
                sb.AppendLine();
                sb.AppendLine("| 测试项 | 平均耗时 | 内存分配 | 状态 | 备注 |");
                sb.AppendLine("|--------|----------|----------|------|------|");
                
                foreach (var result in stressResults)
                {
                    string status = result.Passed ? "✅ 通过" : "❌ 失败";
                    string time = $"{result.Performance.AverageMs:F3} ms";
                    string memory = result.Performance.AverageBytes > 0 
                        ? $"{result.Performance.AverageBytes:F0} B" 
                        : "-";
                    string note = result.ErrorMessage ?? "-";
                    sb.AppendLine($"| {result.Name} | {time} | {memory} | {status} | {note} |");
                }
                sb.AppendLine();
            }
            
            // 内存测试详情
            var memoryResults = results.Where(r => r.Category == TestCategory.Memory).ToList();
            if (memoryResults.Count > 0)
            {
                sb.AppendLine("### 内存测试");
                sb.AppendLine();
                sb.AppendLine("| 测试项 | 内存占用 | 状态 | 备注 |");
                sb.AppendLine("|--------|----------|------|------|");
                
                foreach (var result in memoryResults)
                {
                    string status = result.Passed ? "✅ 通过" : "❌ 失败";
                    string memory = result.Performance.AverageBytes > 0 
                        ? $"{result.Performance.AverageBytes:F0} B/操作" 
                        : "-";
                    string note = result.ErrorMessage ?? "-";
                    sb.AppendLine($"| {result.Name} | {memory} | {status} | {note} |");
                }
                sb.AppendLine();
            }
            
            // 失败测试详情
            var failedResults = results.Where(r => !r.Passed).ToList();
            if (failedResults.Count > 0)
            {
                sb.AppendLine("## 失败测试详情");
                sb.AppendLine();
                
                foreach (var result in failedResults)
                {
                    sb.AppendLine($"### {result.Name}");
                    sb.AppendLine($"- **类别**: {GetCategoryName(result.Category)}");
                    sb.AppendLine($"- **错误**: {result.ErrorMessage}");
                    sb.AppendLine($"- **执行时间**: {result.ExecuteTime:HH:mm:ss}");
                    sb.AppendLine();
                }
            }
            
            // 性能建议
            sb.AppendLine("## 性能优化建议");
            sb.AppendLine();
            
            var slowTests = results
                .Where(r => r.Performance.AverageMs > 1.0)
                .OrderByDescending(r => r.Performance.AverageMs)
                .ToList();
            
            if (slowTests.Count > 0)
            {
                sb.AppendLine("### 耗时较长的操作");
                sb.AppendLine();
                foreach (var result in slowTests)
                {
                    sb.AppendLine($"- **{result.Name}**: {result.Performance.AverageMs:F3} ms - 建议优化");
                }
                sb.AppendLine();
            }
            
            var highMemoryTests = results
                .Where(r => r.Performance.AverageBytes > 1024)
                .OrderByDescending(r => r.Performance.AverageBytes)
                .ToList();
            
            if (highMemoryTests.Count > 0)
            {
                sb.AppendLine("### 内存占用较高的操作");
                sb.AppendLine();
                foreach (var result in highMemoryTests)
                {
                    sb.AppendLine($"- **{result.Name}**: {result.Performance.AverageBytes:F0} B - 建议减少GC Alloc");
                }
                sb.AppendLine();
            }
            
            // 历史对比（如果有）
            sb.AppendLine("## 版本历史");
            sb.AppendLine();
            sb.AppendLine($"- **当前版本**: v{GetSystemVersion()}");
            sb.AppendLine($"- **测试框架版本**: 1.0");
            sb.AppendLine();
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 生成HTML格式报告
        /// </summary>
        public static string GenerateHtmlReport(List<TestResult> results)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"UTF-8\">");
            sb.AppendLine("<title>BuffSystem 性能测试报告</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(@"
                body { font-family: Arial, sans-serif; margin: 20px; background: #f5f5f5; }
                .container { max-width: 1200px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; }
                h1 { color: #333; border-bottom: 2px solid #4CAF50; padding-bottom: 10px; }
                h2 { color: #555; margin-top: 30px; }
                h3 { color: #666; }
                table { width: 100%; border-collapse: collapse; margin: 15px 0; }
                th, td { padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }
                th { background: #4CAF50; color: white; }
                tr:hover { background: #f5f5f5; }
                .pass { color: #4CAF50; }
                .fail { color: #f44336; }
                .summary { background: #e3f2fd; padding: 15px; border-radius: 5px; margin: 15px 0; }
                .error { background: #ffebee; padding: 10px; border-radius: 5px; }
                .meta { color: #666; font-size: 14px; }
            ");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            
            // 标题
            sb.AppendLine("<h1>BuffSystem 性能测试报告</h1>");
            
            // 元信息
            sb.AppendLine("<div class='meta'>");
            sb.AppendLine($"<p>生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine($"<p>Unity版本: {Application.unityVersion}</p>");
            sb.AppendLine($"<p>平台: {Application.platform}</p>");
            sb.AppendLine("</div>");
            
            // 汇总
            int total = results.Count;
            int passed = results.Count(r => r.Passed);
            int failed = total - passed;
            
            sb.AppendLine("<div class='summary'>");
            sb.AppendLine($"<h2>测试结果汇总</h2>");
            sb.AppendLine($"<p>总测试数: <strong>{total}</strong></p>");
            sb.AppendLine($"<p class='pass'>通过: {passed}</p>");
            sb.AppendLine($"<p class='fail'>失败: {failed}</p>");
            sb.AppendLine($"<p>通过率: {(total > 0 ? passed * 100 / total : 0)}%</p>");
            sb.AppendLine("</div>");
            
            // 详细结果表格
            sb.AppendLine("<h2>详细测试结果</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>测试项</th><th>类别</th><th>平均耗时</th><th>内存分配</th><th>状态</th></tr>");
            
            foreach (var result in results)
            {
                string statusClass = result.Passed ? "pass" : "fail";
                string status = result.Passed ? "✓ 通过" : "✗ 失败";
                string time = $"{result.Performance.AverageMs:F3} ms";
                string memory = result.Performance.AverageBytes > 0 
                    ? $"{result.Performance.AverageBytes:F0} B" 
                    : "-";
                
                sb.AppendLine($"<tr>");
                sb.AppendLine($"<td>{result.Name}</td>");
                sb.AppendLine($"<td>{GetCategoryName(result.Category)}</td>");
                sb.AppendLine($"<td>{time}</td>");
                sb.AppendLine($"<td>{memory}</td>");
                sb.AppendLine($"<td class='{statusClass}'>{status}</td>");
                sb.AppendLine($"</tr>");
            }
            
            sb.AppendLine("</table>");
            
            // 失败详情
            var failedResults = results.Where(r => !r.Passed).ToList();
            if (failedResults.Count > 0)
            {
                sb.AppendLine("<h2>失败测试详情</h2>");
                foreach (var result in failedResults)
                {
                    sb.AppendLine("<div class='error'>");
                    sb.AppendLine($"<h3>{result.Name}</h3>");
                    sb.AppendLine($"<p>{result.ErrorMessage}</p>");
                    sb.AppendLine("</div>");
                }
            }
            
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 生成JSON格式报告
        /// </summary>
        public static string GenerateJsonReport(List<TestResult> results)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"timestamp\": \"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\",");
            sb.AppendLine($"  \"unityVersion\": \"{Application.unityVersion}\",");
            sb.AppendLine($"  \"platform\": \"{Application.platform}\",");
            sb.AppendLine($"  \"totalTests\": {results.Count},");
            sb.AppendLine($"  \"passed\": {results.Count(r => r.Passed)},");
            sb.AppendLine($"  \"failed\": {results.Count(r => !r.Passed)},");
            sb.AppendLine("  \"results\": [");
            
            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                sb.AppendLine("    {");
                sb.AppendLine($"      \"name\": \"{result.Name}\",");
                sb.AppendLine($"      \"category\": \"{result.Category}\",");
                sb.AppendLine($"      \"passed\": {result.Passed.ToString().ToLower()},");
                sb.AppendLine($"      \"averageMs\": {result.Performance.AverageMs:F6},");
                sb.AppendLine($"      \"averageBytes\": {result.Performance.AverageBytes:F2},");
                sb.AppendLine($"      \"errorMessage\": \"{result.ErrorMessage?.Replace("\"", "\\\"") ?? ""}\",");
                sb.AppendLine($"      \"executeTime\": \"{result.ExecuteTime:yyyy-MM-dd HH:mm:ss}\"");
                sb.AppendLine($"    }}{(i < results.Count - 1 ? "," : "")}");
            }
            
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 获取类别名称
        /// </summary>
        private static string GetCategoryName(TestCategory category)
        {
            return category switch
            {
                TestCategory.Benchmark => "基准测试",
                TestCategory.Comparison => "对比测试",
                TestCategory.Stress => "压力测试",
                TestCategory.Memory => "内存测试",
                _ => category.ToString()
            };
        }
        
        /// <summary>
        /// 获取系统版本
        /// </summary>
        private static string GetSystemVersion()
        {
            // 从版本文件中读取或使用默认值
            return "8.0";
        }
        
        [MenuItem("BuffSystem/Tests/Generate Performance Report")]
        private static void GenerateReportMenu()
        {
            // 创建临时运行器来执行测试
            var runner = new GameObject("BenchmarkRunner").AddComponent<BenchmarkRunner>();
            runner.RunAllTests();
            
            var results = runner.GetResults().ToList();
            if (results.Count > 0)
            {
                string report = GenerateMarkdownReport(results);
                string filePath = $"{Application.dataPath}/BuffSystem/PerformanceReport.md";
                System.IO.File.WriteAllText(filePath, report);
                Debug.Log($"性能报告已生成: {filePath}");
                
                // 打开报告
                EditorUtility.RevealInFinder(filePath);
            }
            else
            {
                Debug.LogWarning("没有测试结果可生成报告");
            }
            
            // 清理
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(runner.gameObject);
            }
        }
    }
}
#endif
