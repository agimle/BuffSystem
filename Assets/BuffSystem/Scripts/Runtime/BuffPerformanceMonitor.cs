using System;
using System.Collections.Generic;
using System.Diagnostics;
using BuffSystem.Core;
using BuffSystem.Data;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BuffSystem.Runtime
{
    /// <summary>
    /// Buff性能监控器 - 运行时性能显示
    /// </summary>
    [AddComponentMenu("BuffSystem/Performance Monitor")]
    public class BuffPerformanceMonitor : MonoBehaviour
    {
        [Header("显示设置")]
        [Tooltip("是否在屏幕上显示监控信息")]
        [SerializeField] private bool showOnScreen = true;
        
        [Tooltip("切换显示的快捷键")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F12;
        
        [Tooltip("显示区域位置和大小")]
        [SerializeField] private Rect displayRect = new Rect(10, 10, 320, 250);
        
        [Tooltip("背景透明度")]
        [SerializeField] [Range(0f, 1f)] private float backgroundAlpha = 0.8f;

        [Header("监控项")]
        [Tooltip("显示Buff总数")]
        [SerializeField] private bool showBuffCount = true;
        
        [Tooltip("显示Owner数量")]
        [SerializeField] private bool showOwnerCount = true;
        
        [Tooltip("显示更新时间")]
        [SerializeField] private bool showUpdateTime = true;
        
        [Tooltip("显示FPS")]
        [SerializeField] private bool showFPS = true;
        
        [Tooltip("显示内存使用")]
        [SerializeField] private bool showMemoryUsage = true;
        
        [Tooltip("显示对象池状态")]
        [SerializeField] private bool showPoolStats = true;

        // 性能统计
        private float lastUpdateTime;
        private float currentUpdateTime;
        private float smoothedUpdateTime;
        private const float SmoothFactor = 0.1f;
        
        // FPS计算
        private float fpsUpdateInterval = 0.5f;
        private float fpsAccumulatedTime;
        private int fpsFrameCount;
        private float currentFPS;
        
        // 历史数据
        private Queue<float> updateTimeHistory = new Queue<float>(60);
        private float maxUpdateTime;
        private float avgUpdateTime;

        // GUI样式
        private GUIStyle boxStyle;
        private GUIStyle headerStyle;
        private GUIStyle labelStyle;
        private GUIStyle valueStyle;
        private bool stylesInitialized;

        private void Update()
        {
            // 处理快捷键
            if (Input.GetKeyDown(toggleKey))
            {
                showOnScreen = !showOnScreen;
            }
            
            // 计算FPS
            CalculateFPS();
        }

        private void CalculateFPS()
        {
            fpsAccumulatedTime += Time.unscaledDeltaTime;
            fpsFrameCount++;

            if (fpsAccumulatedTime >= fpsUpdateInterval)
            {
                currentFPS = fpsFrameCount / fpsAccumulatedTime;
                fpsFrameCount = 0;
                fpsAccumulatedTime = 0f;
            }
        }

        private void OnGUI()
        {
            if (!showOnScreen) return;
            
            InitializeStyles();
            
            // 保存原始GUI颜色
            Color originalColor = GUI.color;
            Color originalBackgroundColor = GUI.backgroundColor;
            
            // 设置背景透明度
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, backgroundAlpha);
            
            GUILayout.BeginArea(displayRect, boxStyle);
            {
                GUI.backgroundColor = originalBackgroundColor;
                
                // 标题
                GUILayout.Label("═══ BuffSystem 性能监控 ═══", headerStyle);
                GUILayout.Space(5);
                
                // 显示各项监控数据
                if (showOwnerCount)
                {
                    DrawStatLine("Owner数量", GetOwnerCount().ToString(), Color.cyan);
                }
                
                if (showBuffCount)
                {
                    var (totalBuffs, activeBuffs) = GetBuffCounts();
                    DrawStatLine("Buff总数", $"{totalBuffs} (活跃: {activeBuffs})", Color.green);
                }
                
                if (showUpdateTime)
                {
                    MeasureUpdateTime();
                    DrawStatLine("更新时间", $"{smoothedUpdateTime:F2}ms", GetUpdateTimeColor(smoothedUpdateTime));
                    DrawStatLine("  平均", $"{avgUpdateTime:F2}ms", Color.gray);
                    DrawStatLine("  峰值", $"{maxUpdateTime:F2}ms", GetUpdateTimeColor(maxUpdateTime));
                }
                
                if (showFPS)
                {
                    DrawStatLine("FPS", $"{currentFPS:F0}", GetFPSColor(currentFPS));
                }
                
                if (showMemoryUsage)
                {
                    DrawStatLine("内存使用", GetMemoryUsage(), Color.yellow);
                }
                
                if (showPoolStats)
                {
                    var (poolSize, activeCount) = GetPoolStats();
                    DrawStatLine("对象池", $"{activeCount}/{poolSize}", Color.magenta);
                }
                
                GUILayout.Space(5);
                GUILayout.Label($"按 [{toggleKey}] 切换显示", labelStyle);
            }
            GUILayout.EndArea();
            
            // 恢复原始颜色
            GUI.color = originalColor;
            GUI.backgroundColor = originalBackgroundColor;
        }

        private void InitializeStyles()
        {
            if (stylesInitialized) return;
            
            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
            
            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                normal = { textColor = Color.white }
            };
            
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = Color.white }
            };
            
            valueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            };
            
            stylesInitialized = true;
        }

        private void DrawStatLine(string label, string value, Color valueColor)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(label + ":", labelStyle, GUILayout.Width(100));
                GUILayout.FlexibleSpace();
                
                GUI.color = valueColor;
                GUILayout.Label(value, valueStyle, GUILayout.Width(120));
                GUI.color = Color.white;
            }
            GUILayout.EndHorizontal();
        }

        private int GetOwnerCount()
        {
            return BuffOwner.ActiveOwnerCount;
        }

        private (int total, int active) GetBuffCounts()
        {
            int total = 0;
            int active = 0;
            
            var owners = BuffOwner.AllOwners;
            for (int i = 0; i < owners.Count; i++)
            {
                var owner = owners[i];
                if (owner == null) continue;
                
                var container = owner.BuffContainer;
                if (container == null) continue;
                
                total += container.AllBuffs.Count;
                
                // 统计活跃Buff
                foreach (var buff in container.AllBuffs)
                {
                    if (buff != null && buff.IsActive && !buff.IsMarkedForRemoval)
                    {
                        active++;
                    }
                }
            }
            
            return (total, active);
        }

        private void MeasureUpdateTime()
        {
            // 测量Buff系统更新时间
            var stopwatch = Stopwatch.StartNew();
            
            // 注意：这里只是估算，实际更新时间分散在Update/FixedUpdate中
            // 更准确的测量需要在BuffSystemUpdater中添加性能计数器
            
            stopwatch.Stop();
            currentUpdateTime = (float)stopwatch.Elapsed.TotalMilliseconds;
            
            // 平滑处理
            if (smoothedUpdateTime == 0)
            {
                smoothedUpdateTime = currentUpdateTime;
            }
            else
            {
                smoothedUpdateTime = smoothedUpdateTime * (1 - SmoothFactor) + currentUpdateTime * SmoothFactor;
            }
            
            // 更新历史数据
            updateTimeHistory.Enqueue(currentUpdateTime);
            if (updateTimeHistory.Count > 60)
            {
                updateTimeHistory.Dequeue();
            }
            
            // 计算统计值
            CalculateUpdateTimeStats();
        }

        private void CalculateUpdateTimeStats()
        {
            if (updateTimeHistory.Count == 0) return;
            
            float sum = 0;
            maxUpdateTime = 0;
            
            foreach (var time in updateTimeHistory)
            {
                sum += time;
                if (time > maxUpdateTime)
                {
                    maxUpdateTime = time;
                }
            }
            
            avgUpdateTime = sum / updateTimeHistory.Count;
        }

        private string GetMemoryUsage()
        {
            long totalMemory = GC.GetTotalMemory(false);
            return $"{totalMemory / 1024 / 1024} MB";
        }

        private (int poolSize, int activeCount) GetPoolStats()
        {
            // 获取对象池统计
            // 注意：需要从BuffContainer获取对象池信息
            // 这里使用估算值
            int poolSize = 0;
            int activeCount = 0;
            
            var owners = BuffOwner.AllOwners;
            for (int i = 0; i < owners.Count; i++)
            {
                var owner = owners[i];
                if (owner == null) continue;
                
                var container = owner.BuffContainer;
                if (container == null) continue;
                
                activeCount += container.AllBuffs.Count;
            }
            
            // 估算池大小（活跃数量 + 预留给新Buff的空间）
            poolSize = activeCount + 32;
            
            return (poolSize, activeCount);
        }

        private Color GetUpdateTimeColor(float time)
        {
            if (time < 1f) return Color.green;
            if (time < 5f) return Color.yellow;
            return Color.red;
        }

        private Color GetFPSColor(float fps)
        {
            if (fps >= 55) return Color.green;
            if (fps >= 30) return Color.yellow;
            return Color.red;
        }

        #region Public API

        /// <summary>
        /// 获取性能报告
        /// </summary>
        public PerformanceReport GetPerformanceReport()
        {
            var (totalBuffs, activeBuffs) = GetBuffCounts();
            
            return new PerformanceReport
            {
                Timestamp = Time.realtimeSinceStartup,
                OwnerCount = GetOwnerCount(),
                TotalBuffCount = totalBuffs,
                ActiveBuffCount = activeBuffs,
                AverageUpdateTime = avgUpdateTime,
                MaxUpdateTime = maxUpdateTime,
                CurrentFPS = currentFPS,
                MemoryUsageMB = GC.GetTotalMemory(false) / 1024 / 1024
            };
        }

        /// <summary>
        /// 导出性能报告到日志
        /// </summary>
        public void LogPerformanceReport()
        {
            var report = GetPerformanceReport();
            Debug.Log($"[BuffPerformanceMonitor] 性能报告:\n{report}");
        }

        #endregion
    }

    /// <summary>
    /// 性能报告数据结构
    /// </summary>
    public struct PerformanceReport
    {
        public float Timestamp;
        public int OwnerCount;
        public int TotalBuffCount;
        public int ActiveBuffCount;
        public float AverageUpdateTime;
        public float MaxUpdateTime;
        public float CurrentFPS;
        public long MemoryUsageMB;

        public override string ToString()
        {
            return $"时间: {Timestamp:F1}s\n" +
                   $"Owner数量: {OwnerCount}\n" +
                   $"Buff总数: {TotalBuffCount} (活跃: {ActiveBuffCount})\n" +
                   $"平均更新时间: {AverageUpdateTime:F2}ms\n" +
                   $"最大更新时间: {MaxUpdateTime:F2}ms\n" +
                   $"当前FPS: {CurrentFPS:F0}\n" +
                   $"内存使用: {MemoryUsageMB} MB";
        }
    }
}
