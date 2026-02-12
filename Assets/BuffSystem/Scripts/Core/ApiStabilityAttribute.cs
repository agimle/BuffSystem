using System;

namespace BuffSystem.Core
{
    /// <summary>
    /// API稳定性级别
    /// </summary>
    public enum ApiStabilityLevel
    {
        /// <summary>
        /// 实验性API - 可能随时更改
        /// </summary>
        Experimental = 0,
        
        /// <summary>
        /// 预览版API - 基本稳定但可能有小调整
        /// </summary>
        Preview = 1,
        
        /// <summary>
        /// 稳定API - 保证向后兼容
        /// </summary>
        Stable = 2,
        
        /// <summary>
        /// 已弃用 - 将在未来版本移除
        /// </summary>
        Deprecated = 3
    }

    /// <summary>
    /// API稳定性标记属性
    /// 用于标记API的稳定性级别和版本信息
    /// v7.0新增
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | 
                    AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | 
                    AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Delegate,
                    AllowMultiple = false, Inherited = true)]
    public class ApiStabilityAttribute : Attribute
    {
        /// <summary>
        /// 稳定性级别
        /// </summary>
        public ApiStabilityLevel Level { get; }
        
        /// <summary>
        /// 稳定版本（从哪个版本开始稳定）
        /// </summary>
        public string StableSince { get; }
        
        /// <summary>
        /// 版本历史
        /// </summary>
        public string VersionHistory { get; set; }
        
        /// <summary>
        /// 修改策略说明
        /// </summary>
        public string ChangePolicy { get; set; }
        
        /// <summary>
        /// 替代方案（如果是Deprecated）
        /// </summary>
        public string Replacement { get; set; }
        
        /// <summary>
        /// 计划移除版本（如果是Deprecated）
        /// </summary>
        public string RemoveInVersion { get; set; }

        /// <summary>
        /// 创建API稳定性标记
        /// </summary>
        /// <param name="level">稳定性级别</param>
        /// <param name="stableSince">稳定版本</param>
        public ApiStabilityAttribute(ApiStabilityLevel level, string stableSince = null)
        {
            Level = level;
            StableSince = stableSince;
        }

        /// <summary>
        /// 获取稳定性描述
        /// </summary>
        public string GetStabilityDescription()
        {
            return Level switch
            {
                ApiStabilityLevel.Experimental => "🔬 实验性API - 可能随时更改",
                ApiStabilityLevel.Preview => "👁️ 预览版API - 基本稳定但可能有小调整",
                ApiStabilityLevel.Stable => $"🔒 稳定API{(StableSince != null ? $" (v{StableSince}+)" : "")} - 保证向后兼容",
                ApiStabilityLevel.Deprecated => $"⚠️ 已弃用{(RemoveInVersion != null ? $" (将在v{RemoveInVersion}移除)" : "")}",
                _ => "未知"
            };
        }
    }

    /// <summary>
    /// 稳定API快捷属性
    /// </summary>
    public class StableApiAttribute : ApiStabilityAttribute
    {
        public StableApiAttribute(string stableSince) : base(ApiStabilityLevel.Stable, stableSince)
        {
            ChangePolicy = "只允许bug修复，不允许破坏性变更";
        }
    }

    /// <summary>
    /// 实验性API快捷属性
    /// </summary>
    public class ExperimentalApiAttribute : ApiStabilityAttribute
    {
        public ExperimentalApiAttribute() : base(ApiStabilityLevel.Experimental)
        {
            ChangePolicy = "可能随时更改，不建议在生产环境使用";
        }
    }

    /// <summary>
    /// 预览版API快捷属性
    /// </summary>
    public class PreviewApiAttribute : ApiStabilityAttribute
    {
        public PreviewApiAttribute() : base(ApiStabilityLevel.Preview)
        {
            ChangePolicy = "基本稳定但可能有小调整";
        }
    }

    /// <summary>
    /// 已弃用API快捷属性
    /// </summary>
    public class DeprecatedApiAttribute : ApiStabilityAttribute
    {
        public DeprecatedApiAttribute(string replacement, string removeInVersion = null) 
            : base(ApiStabilityLevel.Deprecated)
        {
            Replacement = replacement;
            RemoveInVersion = removeInVersion;
        }
    }
}
