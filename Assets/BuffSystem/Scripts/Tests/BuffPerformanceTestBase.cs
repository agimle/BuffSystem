using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BuffSystem.Core;
using BuffSystem.Data;
using BuffSystem.Events;
using BuffSystem.Runtime;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BuffSystem.Tests
{
    /// <summary>
    /// 性能测试结果
    /// </summary>
    public struct PerformanceResult
    {
        /// <summary>总耗时（毫秒）</summary>
        public long ElapsedMilliseconds;
        
        /// <summary>分配的内存（字节）</summary>
        public long AllocatedBytes;
        
        /// <summary>采样次数</summary>
        public int SampleCount;
        
        /// <summary>平均耗时（毫秒）</summary>
        public double AverageMs => SampleCount > 0 ? ElapsedMilliseconds / (double)SampleCount : 0;
        
        /// <summary>平均分配内存（字节）</summary>
        public double AverageBytes => SampleCount > 0 ? AllocatedBytes / (double)SampleCount : 0;

        public override string ToString()
        {
            return $"{AverageMs:F3}ms ({AverageBytes:F0}B)";
        }
    }

    /// <summary>
    /// 测试结果
    /// </summary>
    public class TestResult
    {
        /// <summary>测试名称</summary>
        public string Name { get; set; }
        
        /// <summary>测试类别</summary>
        public TestCategory Category { get; set; }
        
        /// <summary>性能结果</summary>
        public PerformanceResult Performance { get; set; }
        
        /// <summary>是否通过</summary>
        public bool Passed { get; set; }
        
        /// <summary>错误信息</summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>执行时间</summary>
        public DateTime ExecuteTime { get; set; }
    }

    /// <summary>
    /// 测试类别
    /// </summary>
    public enum TestCategory
    {
        Benchmark,      // 基准测试
        Comparison,     // 对比测试
        Stress,         // 压力测试
        Memory          // 内存测试
    }

    /// <summary>
    /// 性能测试属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PerformanceTestAttribute : Attribute
    {
        /// <summary>测试名称</summary>
        public string Name { get; }
        
        /// <summary>迭代次数</summary>
        public int Iterations { get; }
        
        /// <summary>超时时间（毫秒）</summary>
        public int Timeout { get; set; } = 30000;

        public PerformanceTestAttribute(string name, int iterations = 1000)
        {
            Name = name;
            Iterations = iterations;
        }
    }

    /// <summary>
    /// 对比测试属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ComparisonTestAttribute : Attribute
    {
        /// <summary>测试名称</summary>
        public string Name { get; }

        public ComparisonTestAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// 压力测试属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class StressTestAttribute : Attribute
    {
        /// <summary>测试名称</summary>
        public string Name { get; }
        
        /// <summary>超时时间（毫秒）</summary>
        public int Timeout { get; set; } = 60000;

        public StressTestAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// 内存测试属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MemoryTestAttribute : Attribute
    {
        /// <summary>测试名称</summary>
        public string Name { get; }

        public MemoryTestAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// 性能测试基类
    /// </summary>
    public abstract class BuffPerformanceTestBase
    {
        protected Stopwatch stopwatch = new Stopwatch();
        protected long gcBefore;
        protected long gcAfter;
        protected int gcCollectionCountBefore;

        /// <summary>
        /// 开始采样
        /// </summary>
        protected void BeginSample()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            gcCollectionCountBefore = GC.CollectionCount(0);
            gcBefore = GC.GetTotalMemory(false);
            stopwatch.Restart();
        }

        /// <summary>
        /// 结束采样
        /// </summary>
        protected PerformanceResult EndSample(int sampleCount = 1)
        {
            stopwatch.Stop();
            gcAfter = GC.GetTotalMemory(false);

            return new PerformanceResult
            {
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                AllocatedBytes = Math.Max(0, gcAfter - gcBefore),
                SampleCount = sampleCount
            };
        }

        /// <summary>
        /// 创建测试用的BuffOwner
        /// </summary>
        protected TestBuffOwner CreateTestOwner(string name = "TestOwner")
        {
            return new TestBuffOwner(name);
        }

        /// <summary>
        /// 创建测试用的BuffData
        /// </summary>
        protected TestBuffData CreateTestBuffData(int id = 1, string buffName = "TestBuff", float duration = 5f)
        {
            return new TestBuffData
            {
                Id = id,
                BuffName = buffName,
                Duration = duration,
                MaxStack = 1,
                UpdateFrequency = UpdateFrequency.EveryFrame
            };
        }

        /// <summary>
        /// 设置测试Buff
        /// </summary>
        protected void SetupTestBuffs(IBuffOwner owner, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var data = CreateTestBuffData(i + 1, $"TestBuff_{i}");
                owner.BuffContainer.AddBuff(data);
            }
        }

        /// <summary>
        /// 测量Update性能
        /// </summary>
        protected PerformanceResult MeasureUpdate(IBuffOwner owner, float deltaTime = 0.016f, int iterations = 100)
        {
            BeginSample();
            for (int i = 0; i < iterations; i++)
            {
                owner.BuffContainer.Update(deltaTime);
            }
            return EndSample(iterations);
        }

        /// <summary>
        /// 测量多个Owner的Update性能
        /// </summary>
        protected PerformanceResult MeasureUpdateAll(List<IBuffOwner> owners, float deltaTime = 0.016f, int iterations = 100)
        {
            BeginSample();
            for (int i = 0; i < iterations; i++)
            {
                foreach (var owner in owners)
                {
                    owner.BuffContainer.Update(deltaTime);
                }
            }
            return EndSample(iterations * owners.Count);
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        protected void Log(string message)
        {
            Debug.Log($"[PerformanceTest] {message}");
        }

        /// <summary>
        /// 记录对比结果
        /// </summary>
        protected void LogComparison(string nameA, PerformanceResult resultA, string nameB, PerformanceResult resultB)
        {
            double speedup = resultA.AverageMs / resultB.AverageMs;
            string comparison = speedup > 1 
                ? $"{nameB} 快 {speedup:F2}x" 
                : $"{nameA} 快 {1/speedup:F2}x";
            
            Log($"对比结果: {nameA}={resultA}, {nameB}={resultB}, {comparison}");
        }

        /// <summary>
        /// 断言：小于
        /// </summary>
        protected void AssertLess(double actual, double expected, string message)
        {
            if (actual >= expected)
            {
                throw new AssertionException($"{message}: 实际值 {actual:F3} 不小于期望值 {expected:F3}");
            }
        }

        /// <summary>
        /// 断言：等于
        /// </summary>
        protected void AssertEqual(int actual, int expected, string message)
        {
            if (actual != expected)
            {
                throw new AssertionException($"{message}: 实际值 {actual} 不等于期望值 {expected}");
            }
        }
    }

    /// <summary>
    /// 断言异常
    /// </summary>
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) { }
    }

    /// <summary>
    /// 测试用的BuffOwner实现
    /// </summary>
    public class TestBuffOwner : IBuffOwner
    {
        public int OwnerId { get; }
        public string OwnerName { get; }
        public IBuffContainer BuffContainer { get; }
        public BuffLocalEventSystem LocalEvents { get; }
        public IReadOnlyList<string> ImmuneTags { get; } = new List<string>();

        private static int _nextId = 1;

        public TestBuffOwner(string name)
        {
            OwnerId = _nextId++;
            OwnerName = name;
            LocalEvents = new BuffLocalEventSystem(this);
            BuffContainer = new BuffContainer(this);
        }

        public void OnBuffEvent(BuffEventType eventType, IBuff buff) { }
        public bool IsImmuneTo(int buffId) => false;
        public bool IsImmuneToTag(string tag) => false;
    }

    /// <summary>
    /// 测试用的BuffData实现
    /// </summary>
    public class TestBuffData : IBuffData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BuffName { get; set; }
        public string Description { get; set; }
        public Sprite Icon { get; set; }
        public float Duration { get; set; }
        public int MaxStack { get; set; }
        public int AddStackCount { get; set; } = 1;
        public int RemoveStackCount { get; set; } = 1;
        public bool IsUnique { get; set; } = false;
        public bool IsPermanent { get; set; } = false;
        public float RemoveInterval { get; set; } = 0f;
        public BuffStackMode StackMode { get; set; } = BuffStackMode.Stackable;
        public BuffEffectType EffectType { get; set; } = BuffEffectType.Neutral;
        public BuffRemoveMode RemoveMode { get; set; } = BuffRemoveMode.Remove;
        public bool CanRefresh { get; set; } = true;
        public UpdateFrequency UpdateFrequency { get; set; } = UpdateFrequency.EveryFrame;
        public IReadOnlyList<IBuffCondition> AddConditions { get; } = new List<IBuffCondition>();
        public IReadOnlyList<IBuffCondition> ActivateConditions { get; } = new List<IBuffCondition>();
        public IReadOnlyList<IBuffCondition> RemoveConditions { get; } = new List<IBuffCondition>();
        public IReadOnlyList<int> MutexBuffIds { get; } = new List<int>();
        public IReadOnlyList<int> DependBuffIds { get; } = new List<int>();
        public IReadOnlyList<string> Tags { get; } = new List<string>();
        public IReadOnlyList<string> GroupIds { get; } = new List<string>();
        public int GroupMaxStack { get; } = 0;
        public GroupStrategyType GroupStrategy { get; } = GroupStrategyType.AddToGroup;

        public bool HasTag(string tag) => false;
        
        public IBuffLogic CreateLogic() => null;
    }
}
