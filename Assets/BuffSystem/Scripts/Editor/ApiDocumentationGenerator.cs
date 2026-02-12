using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using BuffSystem.Core;

namespace BuffSystem.Editor
{
    /// <summary>
    /// API文档自动生成工具
    /// 扫描代码中的API稳定性属性，生成markdown文档
    /// v7.0新增
    /// </summary>
    public class ApiDocumentationGenerator
    {
        private const string OutputPath = "Assets/BuffSystem/Documentation/API_REFERENCE.md";
        private const string ChangeLogPath = "Assets/BuffSystem/Documentation/API_CHANGELOG.md";
        
        private readonly StringBuilder sb = new();
        private readonly List<TypeInfo> apiTypes = new();
        
        private class TypeInfo
        {
            public Type Type;
            public ApiStabilityAttribute Stability;
            public string Summary;
            public string Remarks;
            public List<MemberInfo> Members = new();
        }
        
        private class MemberInfo
        {
            public string Name;
            public string Type;
            public string Summary;
            public ApiStabilityAttribute Stability;
            public List<ParameterInfo> Parameters = new();
            public string Returns;
        }
        
        private class ParameterInfo
        {
            public string Name;
            public string Type;
            public string Description;
        }

        [MenuItem("BuffSystem/Tools/Generate API Documentation", priority = 100)]
        public static void GenerateDocumentation()
        {
            var generator = new ApiDocumentationGenerator();
            generator.ScanAssemblies();
            generator.GenerateApiReference();
            generator.GenerateChangeLog();
            
            Debug.Log("[ApiDocumentationGenerator] API文档生成完成");
            EditorUtility.RevealInFinder(OutputPath);
        }
        
        [MenuItem("BuffSystem/Tools/Generate API Documentation (Preview)", priority = 101)]
        public static void PreviewDocumentation()
        {
            var generator = new ApiDocumentationGenerator();
            generator.ScanAssemblies();
            var content = generator.GeneratePreview();
            
            // 显示在编辑器窗口中
            EditorWindow.GetWindow<ApiDocPreviewWindow>("API Documentation Preview").SetContent(content);
        }

        private void ScanAssemblies()
        {
            apiTypes.Clear();
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name.StartsWith("BuffSystem"));
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsPublic && !t.IsNested)
                        .Where(t => t.Namespace?.StartsWith("BuffSystem") == true);
                    
                    foreach (var type in types)
                    {
                        var stability = type.GetCustomAttribute<ApiStabilityAttribute>();
                        var typeInfo = new TypeInfo
                        {
                            Type = type,
                            Stability = stability,
                            Summary = GetXmlSummary(type),
                            Remarks = GetXmlRemarks(type)
                        };
                        
                        // 扫描成员
                        ScanMembers(typeInfo);
                        
                        apiTypes.Add(typeInfo);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ApiDocumentationGenerator] 扫描程序集失败: {assembly.GetName().Name}, {e.Message}");
                }
            }
            
            // 按稳定性排序
            apiTypes.Sort((a, b) =>
            {
                var levelA = a.Stability?.Level ?? ApiStabilityLevel.Stable;
                var levelB = b.Stability?.Level ?? ApiStabilityLevel.Stable;
                return levelA.CompareTo(levelB);
            });
        }

        private void ScanMembers(TypeInfo typeInfo)
        {
            // 扫描方法
            var methods = typeInfo.Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName);
            
            foreach (var method in methods)
            {
                var member = new MemberInfo
                {
                    Name = method.Name,
                    Type = "Method",
                    Summary = GetXmlSummary(method),
                    Stability = method.GetCustomAttribute<ApiStabilityAttribute>(),
                    Returns = GetXmlReturns(method)
                };
                
                foreach (var param in method.GetParameters())
                {
                    member.Parameters.Add(new ParameterInfo
                    {
                        Name = param.Name,
                        Type = GetFriendlyTypeName(param.ParameterType),
                        Description = GetXmlParam(method, param.Name)
                    });
                }
                
                typeInfo.Members.Add(member);
            }
            
            // 扫描属性
            var properties = typeInfo.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
            foreach (var prop in properties)
            {
                typeInfo.Members.Add(new MemberInfo
                {
                    Name = prop.Name,
                    Type = "Property",
                    Summary = GetXmlSummary(prop),
                    Stability = prop.GetCustomAttribute<ApiStabilityAttribute>()
                });
            }
        }

        private void GenerateApiReference()
        {
            sb.Clear();
            
            sb.AppendLine("# BuffSystem API 参考文档");
            sb.AppendLine();
            sb.AppendLine("> 本文档由自动化工具生成，最后更新: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("> 生成工具: ApiDocumentationGenerator");
            sb.AppendLine();
            
            // 稳定性图例
            sb.AppendLine("## 📊 API稳定性图例");
            sb.AppendLine();
            sb.AppendLine("| 图标 | 级别 | 说明 |");
            sb.AppendLine("|------|------|------|");
            sb.AppendLine("| 🔒 | Stable | 稳定API - 保证向后兼容 |");
            sb.AppendLine("| 👁️ | Preview | 预览版API - 基本稳定但可能有小调整 |");
            sb.AppendLine("| 🔬 | Experimental | 实验性API - 可能随时更改 |");
            sb.AppendLine("| ⚠️ | Deprecated | 已弃用 - 将在未来版本移除 |");
            sb.AppendLine();
            
            // 按命名空间分组
            var namespaceGroups = apiTypes.GroupBy(t => t.Type.Namespace).OrderBy(g => g.Key);
            
            foreach (var group in namespaceGroups)
            {
                sb.AppendLine($"## {group.Key}");
                sb.AppendLine();
                
                foreach (var typeInfo in group)
                {
                    GenerateTypeDocumentation(typeInfo);
                }
            }
            
            // 确保目录存在
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            File.WriteAllText(OutputPath, sb.ToString());
            
            Debug.Log($"[ApiDocumentationGenerator] API参考文档已生成: {OutputPath}");
        }

        private void GenerateTypeDocumentation(TypeInfo typeInfo)
        {
            var stabilityIcon = GetStabilityIcon(typeInfo.Stability);
            var stabilityText = typeInfo.Stability?.GetStabilityDescription() ?? "🔒 稳定API";
            
            sb.AppendLine($"### {stabilityIcon} {typeInfo.Type.Name}");
            sb.AppendLine();
            sb.AppendLine($"**命名空间:** `{typeInfo.Type.Namespace}`");
            sb.AppendLine();
            sb.AppendLine($"**稳定性:** {stabilityText}");
            sb.AppendLine();
            
            if (!string.IsNullOrEmpty(typeInfo.Summary))
            {
                sb.AppendLine(typeInfo.Summary);
                sb.AppendLine();
            }
            
            if (!string.IsNullOrEmpty(typeInfo.Remarks))
            {
                sb.AppendLine("> **备注:** " + typeInfo.Remarks.Replace("\n", "\n> "));
                sb.AppendLine();
            }
            
            // 成员
            if (typeInfo.Members.Count > 0)
            {
                sb.AppendLine("#### 成员");
                sb.AppendLine();
                
                foreach (var member in typeInfo.Members)
                {
                    GenerateMemberDocumentation(member);
                }
            }
            
            sb.AppendLine();
        }

        private void GenerateMemberDocumentation(MemberInfo member)
        {
            var stabilityIcon = GetStabilityIcon(member.Stability);
            
            if (member.Type == "Method")
            {
                var paramList = string.Join(", ", member.Parameters.Select(p => $"{p.Type} {p.Name}"));
                sb.AppendLine($"- **{stabilityIcon} {member.Name}**({paramList})");
                
                if (!string.IsNullOrEmpty(member.Summary))
                {
                    sb.AppendLine($"  - {member.Summary}");
                }
                
                if (member.Parameters.Count > 0)
                {
                    sb.AppendLine("  - 参数:");
                    foreach (var param in member.Parameters)
                    {
                        sb.AppendLine($"    - `{param.Name}` ({param.Type}): {param.Description}");
                    }
                }
                
                if (!string.IsNullOrEmpty(member.Returns))
                {
                    sb.AppendLine($"  - 返回: {member.Returns}");
                }
            }
            else if (member.Type == "Property")
            {
                sb.AppendLine($"- **{stabilityIcon} {member.Name}**");
                if (!string.IsNullOrEmpty(member.Summary))
                {
                    sb.AppendLine($"  - {member.Summary}");
                }
            }
        }

        private void GenerateChangeLog()
        {
            sb.Clear();
            
            sb.AppendLine("# API 变更日志");
            sb.AppendLine();
            sb.AppendLine("> 本文档记录BuffSystem API的所有变更");
            sb.AppendLine("> 最后更新: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine();
            
            // v7.0 变更
            sb.AppendLine("## [v7.0] - " + DateTime.Now.ToString("yyyy-MM-dd"));
            sb.AppendLine();
            
            // 新增API
            var newApis = apiTypes.Where(t => t.Stability?.StableSince == "7.0" || 
                                              (t.Stability?.Level == ApiStabilityLevel.Preview && t.Stability?.VersionHistory?.Contains("v7.0") == true));
            if (newApis.Any())
            {
                sb.AppendLine("### 新增 API");
                sb.AppendLine();
                foreach (var api in newApis)
                {
                    sb.AppendLine($"- `{api.Type.FullName}` - {api.Summary}");
                }
                sb.AppendLine();
            }
            
            // 已弃用API
            var deprecatedApis = apiTypes.Where(t => t.Stability?.Level == ApiStabilityLevel.Deprecated);
            if (deprecatedApis.Any())
            {
                sb.AppendLine("### 已弃用 API");
                sb.AppendLine();
                foreach (var api in deprecatedApis)
                {
                    sb.AppendLine($"- `{api.Type.FullName}`");
                    sb.AppendLine($"  - 替代方案: {api.Stability?.Replacement}");
                    if (!string.IsNullOrEmpty(api.Stability?.RemoveInVersion))
                    {
                        sb.AppendLine($"  - 计划移除版本: {api.Stability?.RemoveInVersion}");
                    }
                }
                sb.AppendLine();
            }
            
            // 稳定化API
            var stabilizedApis = apiTypes.Where(t => t.Stability?.Level == ApiStabilityLevel.Stable && 
                                                     t.Stability?.VersionHistory?.Contains("v7.0") == true);
            if (stabilizedApis.Any())
            {
                sb.AppendLine("### 稳定化 API");
                sb.AppendLine();
                foreach (var api in stabilizedApis)
                {
                    sb.AppendLine($"- `{api.Type.FullName}` - 从v7.0开始标记为稳定");
                }
                sb.AppendLine();
            }
            
            // v6.0 稳定API
            sb.AppendLine("## [v6.0] 及之前 - 稳定API基线");
            sb.AppendLine();
            sb.AppendLine("以下API从v6.0开始保证向后兼容:");
            sb.AppendLine();
            
            var stableApis = apiTypes.Where(t => t.Stability?.Level == ApiStabilityLevel.Stable && 
                                                 (t.Stability?.StableSince == "6.0" || string.IsNullOrEmpty(t.Stability?.StableSince)));
            foreach (var api in stableApis.Take(20))
            {
                sb.AppendLine($"- `{api.Type.FullName}`");
            }
            if (stableApis.Count() > 20)
            {
                sb.AppendLine($"- ... 还有 {stableApis.Count() - 20} 个稳定API");
            }
            
            // 确保目录存在
            Directory.CreateDirectory(Path.GetDirectoryName(ChangeLogPath));
            File.WriteAllText(ChangeLogPath, sb.ToString());
            
            Debug.Log($"[ApiDocumentationGenerator] API变更日志已生成: {ChangeLogPath}");
        }

        private string GeneratePreview()
        {
            sb.Clear();
            sb.AppendLine("API Documentation Preview");
            sb.AppendLine("========================");
            sb.AppendLine();
            sb.AppendLine($"Total API Types: {apiTypes.Count}");
            sb.AppendLine();
            
            var stabilityGroups = apiTypes.GroupBy(t => t.Stability?.Level ?? ApiStabilityLevel.Stable);
            foreach (var group in stabilityGroups.OrderBy(g => g.Key))
            {
                sb.AppendLine($"{group.Key}: {group.Count()} types");
                foreach (var type in group.Take(5))
                {
                    sb.AppendLine($"  - {type.Type.Name}");
                }
                if (group.Count() > 5)
                {
                    sb.AppendLine($"  ... and {group.Count() - 5} more");
                }
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        #region Helper Methods

        private string GetStabilityIcon(ApiStabilityAttribute stability)
        {
            return stability?.Level switch
            {
                ApiStabilityLevel.Experimental => "🔬",
                ApiStabilityLevel.Preview => "👁️",
                ApiStabilityLevel.Stable => "🔒",
                ApiStabilityLevel.Deprecated => "⚠️",
                _ => "🔒"
            };
        }

        private string GetFriendlyTypeName(Type type)
        {
            if (type == null) return "void";
            if (type == typeof(void)) return "void";
            if (type == typeof(int)) return "int";
            if (type == typeof(float)) return "float";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(string)) return "string";
            if (type.IsGenericType)
            {
                var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetFriendlyTypeName));
                return $"{type.Name.Split('`')[0]}<{genericArgs}>";
            }
            return type.Name;
        }

        private string GetXmlSummary(System.Reflection.MemberInfo member)
        {
            // 从XML文档注释中提取summary
            // 实际实现需要解析XML文档文件
            return "";
        }

        private string GetXmlSummary(Type type)
        {
            // 从XML文档注释中提取summary
            return "";
        }

        private string GetXmlRemarks(Type type)
        {
            return "";
        }

        private string GetXmlReturns(MethodInfo method)
        {
            return "";
        }

        private string GetXmlParam(MethodInfo method, string paramName)
        {
            return "";
        }

        #endregion
    }

    /// <summary>
    /// API文档预览窗口
    /// </summary>
    public class ApiDocPreviewWindow : EditorWindow
    {
        private string content;
        private Vector2 scrollPosition;

        public void SetContent(string text)
        {
            content = text;
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.TextArea(content, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }
}
