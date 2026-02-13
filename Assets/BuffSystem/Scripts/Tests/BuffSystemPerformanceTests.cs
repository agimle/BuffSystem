using System;
using System.Collections.Generic;
using System.Linq;
using BuffSystem.Core;
using BuffSystem.Data;
using BuffSystem.Runtime;
using UnityEngine;

namespace BuffSystem.Tests
{
    /// <summary>
    /// Buff系统性能测试
    /// </summary>
    public class BuffSystemPerformanceTests : BuffPerformanceTestBase
    {
        #region 基准测试

        /// <summary>
        /// 添加Buff基准测试
        /// </summary>
        [PerformanceTest("添加Buff基准", 1000)]
        public PerformanceResult Benchmark_AddBuff()
        {
            var owner = CreateTestOwner();
            var data = CreateTestBuffData();

            BeginSample();
            for (int i = 0; i < 1000; i++)
            {
                owner.BuffContainer.AddBuff(data);
                // 清理以避免层数堆叠过多
                if (i % 10 == 0)
                {
                    owner.BuffContainer.ClearAllBuffs();
                }
            }
            return EndSample(1000);
        }

        /// <summary>
        /// Update基准测试
        /// </summary>
        [PerformanceTest("Update基准", 100)]
        public PerformanceResult Benchmark_Update()
        {
            var owner = CreateTestOwner();
            var data = CreateTestBuffData();

            // 预创建100个Buff
            for (int i = 0; i < 100; i++)
            {
                owner.BuffContainer.AddBuff(CreateTestBuffData(i + 1, $"Buff_{i}"));
            }

            BeginSample();
            for (int i = 0; i < 100; i++)
            {
                owner.BuffContainer.Update(0.016f); // 模拟一帧
            }
            return EndSample(100);
        }

        /// <summary>
        /// 查询Buff基准测试
        /// </summary>
        [PerformanceTest("查询Buff基准", 10000)]
        public PerformanceResult Benchmark_Query()
        {
            var owner = CreateTestOwner();
            SetupTestBuffs(owner, 100);

            BeginSample();
            for (int i = 0; i < 10000; i++)
            {
                owner.BuffContainer.GetBuff(50); // 查询中间的Buff
            }
            return EndSample(10000);
        }

        /// <summary>
        /// 移除Buff基准测试
        /// </summary>
        [PerformanceTest("移除Buff基准", 1000)]
        public PerformanceResult Benchmark_RemoveBuff()
        {
            var owner = CreateTestOwner();
            var buffs = new List<IBuff>();

            // 预创建1000个Buff
            for (int i = 0; i < 1000; i++)
            {
                var buff = owner.BuffContainer.AddBuff(CreateTestBuffData(i + 1, $"Buff_{i}"));
                buffs.Add(buff);
            }

            BeginSample();
            for (int i = 0; i < 1000; i++)
            {
                owner.BuffContainer.RemoveBuff(buffs[i]);
            }
            return EndSample(1000);
        }

        /// <summary>
        /// 层数堆叠基准测试
        /// </summary>
        [PerformanceTest("层数堆叠基准", 1000)]
        public PerformanceResult Benchmark_Stacking()
        {
            var owner = CreateTestOwner();
            var data = CreateTestBuffData(1, "StackableBuff");
            data.MaxStack = 100;

            BeginSample();
            for (int i = 0; i < 1000; i++)
            {
                owner.BuffContainer.AddBuff(data);
            }
            return EndSample(1000);
        }

        #endregion

        #region 对比测试

        /// <summary>
        /// Class vs Struct容器对比
        /// </summary>
        [ComparisonTest("Class vs Struct容器")]
        public void Compare_ClassVsStruct()
        {
            var classResult = TestClassContainer();
            var structResult = TestStructContainer();

            LogComparison("Class容器", classResult, "Struct容器", structResult);
        }

        /// <summary>
        /// 频率更新对比
        /// </summary>
        [ComparisonTest("频率更新对比")]
        public void Compare_FrequencyUpdate()
        {
            var normalResult = TestNormalUpdate();
            var frequencyResult = TestFrequencyUpdate();

            LogComparison("普通更新", normalResult, "分层更新", frequencyResult);
        }

        /// <summary>
        /// 查询方式对比
        /// </summary>
        [ComparisonTest("查询方式对比")]
        public void Compare_QueryMethods()
        {
            var owner = CreateTestOwner();
            SetupTestBuffs(owner, 100);

            // 按ID查询
            BeginSample();
            for (int i = 0; i < 10000; i++)
            {
                owner.BuffContainer.GetBuff(50);
            }
            var byIdResult = EndSample(10000);

            // 按标签查询（如果支持）
            BeginSample();
            var allBuffs = owner.BuffContainer.AllBuffs.ToList();
            for (int i = 0; i < 10000; i++)
            {
                var found = allBuffs.FirstOrDefault(b => b.DataId == 50);
            }
            var byLinqResult = EndSample(10000);

            LogComparison("按ID查询", byIdResult, "LINQ查询", byLinqResult);
        }

        private PerformanceResult TestClassContainer()
        {
            var owner = CreateTestOwner();
            var data = CreateTestBuffData();

            BeginSample();
            for (int i = 0; i < 1000; i++)
            {
                owner.BuffContainer.AddBuff(data);
                if (i % 10 == 0) owner.BuffContainer.ClearAllBuffs();
            }
            return EndSample(1000);
        }

        private PerformanceResult TestStructContainer()
        {
            // 如果实现了Struct容器，在这里测试
            // 目前使用相同的容器作为对比
            var owner = CreateTestOwner();
            var data = CreateTestBuffData();

            BeginSample();
            for (int i = 0; i < 1000; i++)
            {
                owner.BuffContainer.AddBuff(data);
                if (i % 10 == 0) owner.BuffContainer.ClearAllBuffs();
            }
            return EndSample(1000);
        }

        private PerformanceResult TestNormalUpdate()
        {
            var owner = CreateTestOwner();
            for (int i = 0; i < 100; i++)
            {
                owner.BuffContainer.AddBuff(CreateTestBuffData(i + 1, $"Buff_{i}", 10f));
            }

            BeginSample();
            for (int i = 0; i < 100; i++)
            {
                owner.BuffContainer.Update(0.016f);
            }
            return EndSample(100);
        }

        private PerformanceResult TestFrequencyUpdate()
        {
            var owner = CreateTestOwner();
            for (int i = 0; i < 100; i++)
            {
                // 使用不同的更新频率
                var freq = (UpdateFrequency)(i % 4);
                var data = CreateTestBuffData(i + 1, $"Buff_{i}", 10f);
                data.UpdateFrequency = freq;
                owner.BuffContainer.AddBuff(data);
            }

            BeginSample();
            for (int i = 0; i < 100; i++)
            {
                owner.BuffContainer.Update(0.016f);
            }
            return EndSample(100);
        }

        #endregion

        #region 压力测试

        /// <summary>
        /// 10000 Buff压力测试
        /// </summary>
        [StressTest("10000 Buff压力测试")]
        public TestResult StressTest_10000Buffs()
        {
            var owner = CreateTestOwner();
            var data = CreateTestBuffData();

            // 创建10000个Buff
            for (int i = 0; i < 10000; i++)
            {
                owner.BuffContainer.AddBuff(CreateTestBuffData(i + 1, $"Buff_{i}"));
            }

            var result = MeasureUpdate(owner, 0.016f, 10);
            bool passed = result.AverageMs < 5.0;

            return new TestResult
            {
                Name = "10000 Buff压力测试",
                Category = TestCategory.Stress,
                Performance = result,
                Passed = passed,
                ErrorMessage = passed ? null : $"10000 Buff更新耗时 {result.AverageMs:F3}ms，超过5ms阈值",
                ExecuteTime = System.DateTime.Now
            };
        }

        /// <summary>
        /// 100 Owner压力测试
        /// </summary>
        [StressTest("100 Owner压力测试")]
        public TestResult StressTest_100Owners()
        {
            var owners = new List<IBuffOwner>();
            for (int i = 0; i < 100; i++)
            {
                owners.Add(CreateTestOwner($"Owner_{i}"));
            }

            // 每个Owner添加100个Buff
            foreach (var owner in owners)
            {
                for (int j = 0; j < 100; j++)
                {
                    owner.BuffContainer.AddBuff(CreateTestBuffData(j + 1, $"Buff_{j}"));
                }
            }

            var result = MeasureUpdateAll(owners, 0.016f, 10);
            bool passed = result.AverageMs < 10.0;

            return new TestResult
            {
                Name = "100 Owner压力测试",
                Category = TestCategory.Stress,
                Performance = result,
                Passed = passed,
                ErrorMessage = passed ? null : $"100 Owner更新耗时 {result.AverageMs:F3}ms，超过10ms阈值",
                ExecuteTime = System.DateTime.Now
            };
        }

        /// <summary>
        /// 高频添加移除压力测试
        /// </summary>
        [StressTest("高频添加移除压力测试")]
        public TestResult StressTest_HighFrequencyAddRemove()
        {
            var owner = CreateTestOwner();
            var data = CreateTestBuffData();

            BeginSample();
            for (int i = 0; i < 10000; i++)
            {
                var buff = owner.BuffContainer.AddBuff(data);
                owner.BuffContainer.RemoveBuff(buff);
            }
            var result = EndSample(10000);

            bool passed = result.AverageMs < 0.1;

            return new TestResult
            {
                Name = "高频添加移除压力测试",
                Category = TestCategory.Stress,
                Performance = result,
                Passed = passed,
                ErrorMessage = passed ? null : $"高频添加移除平均耗时 {result.AverageMs:F3}ms，超过0.1ms阈值",
                ExecuteTime = System.DateTime.Now
            };
        }

        #endregion

        #region 内存测试

        /// <summary>
        /// Buff内存占用测试
        /// </summary>
        [MemoryTest("Buff内存占用")]
        public TestResult Memory_BuffSize()
        {
            var owner = CreateTestOwner();
            var data = CreateTestBuffData();

            GC.Collect();
            long before = GC.GetTotalMemory(true);

            // 创建1000个Buff
            for (int i = 0; i < 1000; i++)
            {
                owner.BuffContainer.AddBuff(CreateTestBuffData(i + 1, $"Buff_{i}"));
            }

            long after = GC.GetTotalMemory(true);
            long totalSize = after - before;
            float averageSize = totalSize / 1000f;

            bool passed = averageSize < 256; // 每个Buff应小于256字节

            return new TestResult
            {
                Name = "Buff内存占用",
                Category = TestCategory.Memory,
                Performance = new PerformanceResult
                {
                    AllocatedBytes = totalSize,
                    SampleCount = 1000
                },
                Passed = passed,
                ErrorMessage = passed ? null : $"单个Buff平均占用 {averageSize:F0}B，超过256B阈值",
                ExecuteTime = System.DateTime.Now
            };
        }

        /// <summary>
        /// GC Alloc测试
        /// </summary>
        [MemoryTest("GC Alloc测试")]
        public TestResult Memory_GCAlloc()
        {
            var owner = CreateTestOwner();
            var data = CreateTestBuffData();

            // 预热
            for (int i = 0; i < 100; i++)
            {
                var buff = owner.BuffContainer.AddBuff(data);
                owner.BuffContainer.RemoveBuff(buff);
            }

            GC.Collect();
            int before = GC.CollectionCount(0);

            // 执行1000次操作
            for (int i = 0; i < 1000; i++)
            {
                var buff = owner.BuffContainer.AddBuff(data);
                owner.BuffContainer.RemoveBuff(buff);
            }

            int after = GC.CollectionCount(0);
            bool passed = before == after;

            return new TestResult
            {
                Name = "GC Alloc测试",
                Category = TestCategory.Memory,
                Passed = passed,
                ErrorMessage = passed ? null : $"GC触发次数: 前={before}, 后={after}",
                ExecuteTime = System.DateTime.Now
            };
        }

        /// <summary>
        /// 内存泄漏测试
        /// </summary>
        [MemoryTest("内存泄漏测试")]
        public TestResult Memory_LeakTest()
        {
            var owner = CreateTestOwner();

            GC.Collect();
            long before = GC.GetTotalMemory(true);

            // 反复添加和清空
            for (int cycle = 0; cycle < 10; cycle++)
            {
                for (int i = 0; i < 1000; i++)
                {
                    owner.BuffContainer.AddBuff(CreateTestBuffData(i + 1, $"Buff_{i}"));
                }
                owner.BuffContainer.ClearAllBuffs();
                GC.Collect();
            }

            long after = GC.GetTotalMemory(true);
            long growth = after - before;
            bool passed = growth < 1024; // 增长应小于1KB

            return new TestResult
            {
                Name = "内存泄漏测试",
                Category = TestCategory.Memory,
                Performance = new PerformanceResult
                {
                    AllocatedBytes = growth
                },
                Passed = passed,
                ErrorMessage = passed ? null : $"内存增长 {growth}B，可能存在泄漏",
                ExecuteTime = System.DateTime.Now
            };
        }

        #endregion
    }
}
