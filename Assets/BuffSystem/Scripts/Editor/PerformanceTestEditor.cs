#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using BuffSystem.Tests;

namespace BuffSystem.Editor
{
    public class PerformanceTestEditor : EditorWindow
    {
        private bool runBenchmarkTests = true;
        private bool runComparisonTests = true;
        private bool runStressTests = true;
        private bool runMemoryTests = true;
        private bool verboseLogging = true;
        private bool generateReport = true;
        
        private Vector2 scrollPosition;
        private string outputLog = "";
        private bool isRunning = false;

        [MenuItem("Window/BuffSystem/Performance Test")]
        public static void ShowWindow()
        {
            GetWindow<PerformanceTestEditor>("性能测试");
        }

        private void OnGUI()
        {
            GUILayout.Label("BuffSystem 性能测试", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            GUILayout.Label("测试选项", EditorStyles.boldLabel);
            runBenchmarkTests = EditorGUILayout.Toggle("基准测试", runBenchmarkTests);
            runComparisonTests = EditorGUILayout.Toggle("对比测试", runComparisonTests);
            runStressTests = EditorGUILayout.Toggle("压力测试", runStressTests);
            runMemoryTests = EditorGUILayout.Toggle("内存测试", runMemoryTests);
            
            EditorGUILayout.Space();
            
            GUILayout.Label("输出选项", EditorStyles.boldLabel);
            verboseLogging = EditorGUILayout.Toggle("详细日志", verboseLogging);
            generateReport = EditorGUILayout.Toggle("生成报告", generateReport);
            
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(isRunning);
            if (GUILayout.Button(isRunning ? "运行中..." : "运行性能测试", GUILayout.Height(30)))
            {
                RunTests();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("仅基准测试"))
            {
                runBenchmarkTests = true;
                runComparisonTests = false;
                runStressTests = false;
                runMemoryTests = false;
                RunTests();
            }
            if (GUILayout.Button("仅压力测试"))
            {
                runBenchmarkTests = false;
                runComparisonTests = false;
                runStressTests = true;
                runMemoryTests = false;
                RunTests();
            }
            if (GUILayout.Button("仅内存测试"))
            {
                runBenchmarkTests = false;
                runComparisonTests = false;
                runStressTests = false;
                runMemoryTests = true;
                RunTests();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUILayout.Label("输出日志", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            EditorGUILayout.TextArea(outputLog, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            if (GUILayout.Button("清除日志"))
            {
                outputLog = "";
            }
        }

        private void RunTests()
        {
            isRunning = true;
            outputLog += "\n=== 性能测试开始 ===\n";
            
            if (!Application.isPlaying)
            {
                outputLog += "请在播放模式下运行测试\n";
                isRunning = false;
                return;
            }

            var runnerObj = new GameObject("PerformanceTestRunner");
            var runner = runnerObj.AddComponent<BenchmarkRunner>();
            
            ConfigureAndRun(runner);
            
            isRunning = false;
        }

        private void ConfigureAndRun(BenchmarkRunner runner)
        {
            EditorApplication.delayCall += delegate
            {
                var type = runner.GetType();
                
                type.GetField("runBenchmarkTests", System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance)?.SetValue(runner, runBenchmarkTests);
                
                type.GetField("runComparisonTests", System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance)?.SetValue(runner, runComparisonTests);
                
                type.GetField("runStressTests", System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance)?.SetValue(runner, runStressTests);
                
                type.GetField("runMemoryTests", System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance)?.SetValue(runner, runMemoryTests);
                
                type.GetField("logToConsole", System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance)?.SetValue(runner, verboseLogging);
                
                type.GetField("generateReport", System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance)?.SetValue(runner, generateReport);

                Application.logMessageReceived += OnLogMessage;
                
                runner.RunAllTests();
                
                Application.logMessageReceived -= OnLogMessage;
                
                var results = runner.GetResults();
                int passedCount = 0;
                int failedCount = 0;
                foreach (var r in results)
                {
                    if (r.Passed) passedCount++;
                    else failedCount++;
                }
                
                outputLog += "\n测试完成: " + results.Count + " 项测试\n";
                outputLog += "通过: " + passedCount + "\n";
                outputLog += "失败: " + failedCount + "\n";
                outputLog += "=== 性能测试结束 ===\n";
                
                Repaint();
            };
        }

        private void OnLogMessage(string condition, string stackTrace, LogType type)
        {
            if (condition.Contains("[PerformanceTest]") || 
                condition.Contains("BuffSystem 性能测试") ||
                condition.Contains("测试结果"))
            {
                outputLog += condition + "\n";
                Repaint();
            }
        }
    }
}
#endif
