using System;
using System.Collections.Generic;
using BuffSystem.Core;
using BuffSystem.Groups;
using BuffSystem.Modifiers;

namespace BuffSystem.Fluent
{
    /// <summary>
    /// Buff构建器
    /// 使用Fluent API风格链式配置Buff参数
    /// </summary>
    /// <remarks>
    /// 使用示例：
    /// <code>
    /// var buff = owner.CreateBuffBuilder()
    ///     .Add(1001)
    ///     .WithSource(skill)
    ///     .WithStack(3)
    ///     .WithDuration(10f)
    ///     .WithModifier(DurationModifier.Increase(0.5f))
    ///     .AddToGroup("ActiveBuffs")
    ///     .Execute();
    /// </code>
    /// </remarks>
    public class BuffBuilder
    {
        private readonly IBuffOwner owner;
        private int buffId;
        private object source;
        private int stack = 1;
        private float? duration;
        private readonly List<IBuffModifier> modifiers = new();
        private readonly List<string> groupIds = new();
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="owner">Buff持有者</param>
        public BuffBuilder(IBuffOwner owner)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
        
        /// <summary>
        /// 设置Buff ID
        /// </summary>
        /// <param name="buffId">Buff配置ID</param>
        public BuffBuilder Add(int buffId)
        {
            this.buffId = buffId;
            return this;
        }
        
        /// <summary>
        /// 设置Buff来源
        /// </summary>
        /// <param name="source">来源对象</param>
        public BuffBuilder WithSource(object source)
        {
            this.source = source;
            return this;
        }
        
        /// <summary>
        /// 设置初始层数
        /// </summary>
        /// <param name="stack">层数（至少为1）</param>
        public BuffBuilder WithStack(int stack)
        {
            this.stack = Math.Max(1, stack);
            return this;
        }
        
        /// <summary>
        /// 设置持续时间
        /// </summary>
        /// <param name="duration">持续时间（秒）</param>
        public BuffBuilder WithDuration(float duration)
        {
            this.duration = duration;
            return this;
        }
        
        /// <summary>
        /// 添加修饰器
        /// </summary>
        /// <param name="modifier">Buff修饰器</param>
        public BuffBuilder WithModifier(IBuffModifier modifier)
        {
            if (modifier != null)
                modifiers.Add(modifier);
            return this;
        }
        
        /// <summary>
        /// 添加多个修饰器
        /// </summary>
        /// <param name="modifiers">修饰器列表</param>
        public BuffBuilder WithModifiers(IEnumerable<IBuffModifier> modifiers)
        {
            if (modifiers != null)
                this.modifiers.AddRange(modifiers);
            return this;
        }
        
        /// <summary>
        /// 添加到Buff组
        /// </summary>
        /// <param name="groupId">组ID</param>
        public BuffBuilder AddToGroup(string groupId)
        {
            if (!string.IsNullOrEmpty(groupId))
                groupIds.Add(groupId);
            return this;
        }
        
        /// <summary>
        /// 执行构建并添加Buff
        /// </summary>
        /// <returns>创建的Buff实例，失败返回null</returns>
        public IBuff Execute()
        {
            if (buffId <= 0)
                throw new InvalidOperationException("BuffBuilder: 必须先调用Add()设置Buff ID");
            
            IBuff buff;
            
            // 根据是否有修饰器选择调用方式
            if (modifiers.Count > 0)
            {
                buff = BuffApi.AddBuff(buffId, owner, modifiers, source);
            }
            else
            {
                buff = BuffApi.AddBuff(buffId, owner, source);
            }
            
            if (buff == null)
                return null;
            
            // 应用层数设置
            if (stack > 1)
            {
                buff.AddStack(stack - 1);
            }
            
            // 应用持续时间设置
            if (duration.HasValue)
            {
                buff.SetDuration(duration.Value);
            }
            
            // 添加到组
            foreach (var groupId in groupIds)
            {
                owner.BuffContainer.AddBuffToGroup(buff, groupId);
            }
            
            return buff;
        }
        
        /// <summary>
        /// 尝试执行构建
        /// </summary>
        /// <param name="buff">输出的Buff实例</param>
        /// <returns>是否成功</returns>
        public bool TryExecute(out IBuff buff)
        {
            try
            {
                buff = Execute();
                return buff != null;
            }
            catch
            {
                buff = null;
                return false;
            }
        }
    }
    
    /// <summary>
    /// Buff数据构建器
    /// 用于通过Buff数据对象创建Buff
    /// </summary>
    public class BuffDataBuilder
    {
        private readonly IBuffOwner owner;
        private IBuffData data;
        private object source;
        private int stack = 1;
        private float? duration;
        private readonly List<IBuffModifier> modifiers = new();
        private readonly List<string> groupIds = new();
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public BuffDataBuilder(IBuffOwner owner)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
        
        /// <summary>
        /// 设置Buff数据
        /// </summary>
        public BuffDataBuilder Add(IBuffData data)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            return this;
        }
        
        /// <summary>
        /// 设置Buff来源
        /// </summary>
        public BuffDataBuilder WithSource(object source)
        {
            this.source = source;
            return this;
        }
        
        /// <summary>
        /// 设置初始层数
        /// </summary>
        public BuffDataBuilder WithStack(int stack)
        {
            this.stack = Math.Max(1, stack);
            return this;
        }
        
        /// <summary>
        /// 设置持续时间
        /// </summary>
        public BuffDataBuilder WithDuration(float duration)
        {
            this.duration = duration;
            return this;
        }
        
        /// <summary>
        /// 添加修饰器
        /// </summary>
        public BuffDataBuilder WithModifier(IBuffModifier modifier)
        {
            if (modifier != null)
                modifiers.Add(modifier);
            return this;
        }
        
        /// <summary>
        /// 添加多个修饰器
        /// </summary>
        public BuffDataBuilder WithModifiers(IEnumerable<IBuffModifier> modifiers)
        {
            if (modifiers != null)
                this.modifiers.AddRange(modifiers);
            return this;
        }
        
        /// <summary>
        /// 添加到Buff组
        /// </summary>
        public BuffDataBuilder AddToGroup(string groupId)
        {
            if (!string.IsNullOrEmpty(groupId))
                groupIds.Add(groupId);
            return this;
        }
        
        /// <summary>
        /// 执行构建并添加Buff
        /// </summary>
        public IBuff Execute()
        {
            if (data == null)
                throw new InvalidOperationException("BuffDataBuilder: 必须先调用Add()设置Buff数据");
            
            IBuff buff;
            
            if (modifiers.Count > 0)
            {
                buff = BuffApi.AddBuff(data, owner, modifiers, source);
            }
            else
            {
                buff = BuffApi.AddBuff(data, owner, source);
            }
            
            if (buff == null)
                return null;
            
            if (stack > 1)
                buff.AddStack(stack - 1);
            
            if (duration.HasValue)
                buff.SetDuration(duration.Value);
            
            foreach (var groupId in groupIds)
            {
                owner.BuffContainer.AddBuffToGroup(buff, groupId);
            }
            
            return buff;
        }
        
        /// <summary>
        /// 尝试执行构建
        /// </summary>
        public bool TryExecute(out IBuff buff)
        {
            try
            {
                buff = Execute();
                return buff != null;
            }
            catch
            {
                buff = null;
                return false;
            }
        }
    }
}
