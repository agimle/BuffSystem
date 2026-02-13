#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using BuffSystem.Core;
using BuffSystem.Data;
using UnityEditor;
using UnityEngine;

namespace BuffSystem.Editor
{
    /// <summary>
    /// 验证结果类型
    /// </summary>
    public enum ValidationType
    {
        Error,
        Warning,
        Info
    }

    /// <summary>
    /// 验证结果
    /// </summary>
    public class ValidationResult
    {
        public ValidationType Type;
        public string Message;
        public UnityEngine.Object Target;
        public string BuffName;
        public int BuffId;
        public bool CanAutoFix;
        public Action AutoFixAction;

        public ValidationResult(ValidationType type, string message, UnityEngine.Object target = null, 
            string buffName = "", int buffId = 0, bool canAutoFix = false, Action autoFixAction = null)
        {
            Type = type;
            Message = message;
            Target = target;
            BuffName = buffName;
            BuffId = buffId;
            CanAutoFix = canAutoFix;
            AutoFixAction = autoFixAction;
        }
    }

    /// <summary>
    /// Buff配置验证器
    /// </summary>
    public class BuffConfigValidator : EditorWindow
    {
        private List<ValidationResult> results = new();
        private Vector2 scrollPosition;
        private bool showErrors = true;
        private bool showWarnings = true;
        private bool showInfos = true;
        private string searchFilter = "";

        [MenuItem("BuffSystem/Validate Configurations")]
        public static void ShowWindow()
        {
            GetWindow<BuffConfigValidator>("Config Validator");
        }

        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space();
            DrawToolbar();
            EditorGUILayout.Space();
            DrawResults();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Buff配置验证", EditorStyles.boldLabel, GUILayout.Height(25));
                GUILayout.FlexibleSpace();

                // 统计信息
                int errorCount = results.Count(r => r.Type == ValidationType.Error);
                int warningCount = results.Count(r => r.Type == ValidationType.Warning);
                int infoCount = results.Count(r => r.Type == ValidationType.Info);

                GUI.color = Color.red;
                EditorGUILayout.LabelField($"错误: {errorCount}", EditorStyles.boldLabel, GUILayout.Width(70));
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField($"警告: {warningCount}", EditorStyles.boldLabel, GUILayout.Width(70));
                GUI.color = Color.white;
                EditorGUILayout.LabelField($"信息: {infoCount}", EditorStyles.boldLabel, GUILayout.Width(70));
                GUI.color = Color.white;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("验证所有配置", EditorStyles.toolbarButton, GUILayout.Width(120)))
                {
                    ValidateAll();
                }

                if (GUILayout.Button("一键修复", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    AutoFixAll();
                }

                if (GUILayout.Button("清除结果", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    results.Clear();
                }

                GUILayout.FlexibleSpace();

                // 过滤器
                showErrors = GUILayout.Toggle(showErrors, "错误", EditorStyles.toolbarButton, GUILayout.Width(50));
                showWarnings = GUILayout.Toggle(showWarnings, "警告", EditorStyles.toolbarButton, GUILayout.Width(50));
                showInfos = GUILayout.Toggle(showInfos, "信息", EditorStyles.toolbarButton, GUILayout.Width(50));
            }
            EditorGUILayout.EndHorizontal();

            // 搜索框
            EditorGUILayout.BeginHorizontal();
            {
                searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);
                if (GUILayout.Button("×", GUILayout.Width(25)))
                {
                    searchFilter = "";
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawResults()
        {
            if (results.Count == 0)
            {
                EditorGUILayout.HelpBox("点击【验证所有配置】开始检查", MessageType.Info);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUI.skin.box);
            {
                var filteredResults = GetFilteredResults();

                if (filteredResults.Count == 0)
                {
                    EditorGUILayout.HelpBox("没有匹配的结果", MessageType.Info);
                }
                else
                {
                    foreach (var result in filteredResults)
                    {
                        DrawValidationResult(result);
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private List<ValidationResult> GetFilteredResults()
        {
            return results.Where(r =>
            {
                // 类型过滤
                if (r.Type == ValidationType.Error && !showErrors) return false;
                if (r.Type == ValidationType.Warning && !showWarnings) return false;
                if (r.Type == ValidationType.Info && !showInfos) return false;

                // 搜索过滤
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    string lowerFilter = searchFilter.ToLower();
                    if (!r.Message.ToLower().Contains(lowerFilter) &&
                        !r.BuffName.ToLower().Contains(lowerFilter) &&
                        r.BuffId.ToString() != searchFilter)
                    {
                        return false;
                    }
                }

                return true;
            }).ToList();
        }

        private void DrawValidationResult(ValidationResult result)
        {
            // 根据类型设置颜色
            Color iconColor = result.Type switch
            {
                ValidationType.Error => Color.red,
                ValidationType.Warning => Color.yellow,
                ValidationType.Info => Color.cyan,
                _ => Color.white
            };

            string icon = result.Type switch
            {
                ValidationType.Error => "✗",
                ValidationType.Warning => "⚠",
                ValidationType.Info => "ℹ",
                _ => "•"
            };

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                GUI.color = iconColor;
                EditorGUILayout.LabelField(icon, GUILayout.Width(20));
                GUI.color = Color.white;

                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField(result.Message, EditorStyles.wordWrappedLabel);

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField($"Buff: {result.BuffName} (ID: {result.BuffId})", EditorStyles.miniLabel);

                        GUILayout.FlexibleSpace();

                        if (result.Target != null)
                        {
                            if (GUILayout.Button("定位", EditorStyles.miniButton, GUILayout.Width(50)))
                            {
                                EditorGUIUtility.PingObject(result.Target);
                                Selection.activeObject = result.Target;
                            }
                        }

                        if (result.CanAutoFix && result.AutoFixAction != null)
                        {
                            GUI.color = Color.green;
                            if (GUILayout.Button("修复", EditorStyles.miniButton, GUILayout.Width(50)))
                            {
                                result.AutoFixAction();
                                ValidateAll(); // 重新验证
                            }
                            GUI.color = Color.white;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);
        }

        private void ValidateAll()
        {
            results.Clear();

            var allBuffs = LoadAllBuffData();

            foreach (var buff in allBuffs)
            {
                // 基础验证
                CheckBasicValidation(buff);

                // 检查ID冲突
                CheckIdConflict(buff, allBuffs);

                // 检查名称冲突
                CheckNameConflict(buff, allBuffs);

                // 检查循环依赖
                CheckCircularDependency(buff, new List<int>(), allBuffs);

                // 检查无效引用
                CheckInvalidReferences(buff);

                // 检查性能问题
                CheckPerformanceIssues(buff);

                // 检查配置合理性
                CheckConfigurationIssues(buff);
            }

            // 信息汇总
            results.Add(new ValidationResult(
                ValidationType.Info,
                $"验证完成，共检查 {allBuffs.Count} 个Buff配置",
                null,
                "",
                0
            ));
        }

        private List<BuffDataSO> LoadAllBuffData()
        {
            var guids = AssetDatabase.FindAssets("t:BuffDataSO");
            var buffs = new List<BuffDataSO>();

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var buff = AssetDatabase.LoadAssetAtPath<BuffDataSO>(path);
                if (buff != null)
                {
                    buffs.Add(buff);
                }
            }

            return buffs;
        }

        private void CheckBasicValidation(BuffDataSO buff)
        {
            // ID检查
            if (buff.Id <= 0)
            {
                results.Add(new ValidationResult(
                    ValidationType.Error,
                    $"Buff ID必须大于0",
                    buff,
                    buff.Name,
                    buff.Id
                ));
            }

            // 名称检查
            if (string.IsNullOrEmpty(buff.Name))
            {
                results.Add(new ValidationResult(
                    ValidationType.Error,
                    $"Buff名称不能为空",
                    buff,
                    "未命名",
                    buff.Id
                ));
            }

            // 持续时间检查
            if (!buff.IsPermanent && buff.Duration <= 0)
            {
                results.Add(new ValidationResult(
                    ValidationType.Error,
                    $"非永久Buff的持续时间必须大于0",
                    buff,
                    buff.Name,
                    buff.Id
                ));
            }

            // 层数检查
            if (buff.MaxStack <= 0)
            {
                results.Add(new ValidationResult(
                    ValidationType.Error,
                    $"最大层数必须大于0",
                    buff,
                    buff.Name,
                    buff.Id
                ));
            }

            if (buff.AddStackCount <= 0)
            {
                results.Add(new ValidationResult(
                    ValidationType.Error,
                    $"添加层数必须大于0",
                    buff,
                    buff.Name,
                    buff.Id
                ));
            }
        }

        private void CheckIdConflict(BuffDataSO buff, List<BuffDataSO> allBuffs)
        {
            var duplicates = allBuffs.Where(b => b != buff && b.Id == buff.Id).ToList();
            if (duplicates.Any())
            {
                string duplicateNames = string.Join(", ", duplicates.Select(d => $"{d.Name}(ID:{d.Id})"));
                results.Add(new ValidationResult(
                    ValidationType.Error,
                    $"ID冲突: 与 {duplicateNames} 使用相同ID",
                    buff,
                    buff.Name,
                    buff.Id,
                    true,
                    () => FixIdConflict(buff, allBuffs)
                ));
            }
        }

        private void CheckNameConflict(BuffDataSO buff, List<BuffDataSO> allBuffs)
        {
            var duplicates = allBuffs.Where(b => b != buff && b.Name == buff.Name).ToList();
            if (duplicates.Any())
            {
                string duplicateNames = string.Join(", ", duplicates.Select(d => $"{d.Name}(ID:{d.Id})"));
                results.Add(new ValidationResult(
                    ValidationType.Warning,
                    $"名称冲突: 与 {duplicateNames} 使用相同名称",
                    buff,
                    buff.Name,
                    buff.Id
                ));
            }
        }

        private void CheckCircularDependency(BuffDataSO buff, List<int> visited, List<BuffDataSO> allBuffs)
        {
            if (visited.Contains(buff.Id))
            {
                string cycle = string.Join(" -> ", visited) + " -> " + buff.Id;
                results.Add(new ValidationResult(
                    ValidationType.Error,
                    $"循环依赖检测: {cycle}",
                    buff,
                    buff.Name,
                    buff.Id
                ));
                return;
            }

            visited.Add(buff.Id);

            foreach (var depId in buff.DependBuffIds)
            {
                var dep = allBuffs.FirstOrDefault(b => b.Id == depId);
                if (dep != null)
                {
                    CheckCircularDependency(dep, new List<int>(visited), allBuffs);
                }
            }
        }

        private void CheckInvalidReferences(BuffDataSO buff)
        {
            // 检查互斥Buff引用
            foreach (var mutexId in buff.MutexBuffIds)
            {
                if (BuffDatabase.Instance.GetBuffData(mutexId) == null)
                {
                    results.Add(new ValidationResult(
                        ValidationType.Warning,
                        $"无效引用: 互斥Buff ID {mutexId} 不存在",
                        buff,
                        buff.Name,
                        buff.Id,
                        true,
                        () => FixInvalidMutexRef(buff, mutexId)
                    ));
                }
            }

            // 检查依赖Buff引用
            foreach (var dependId in buff.DependBuffIds)
            {
                if (BuffDatabase.Instance.GetBuffData(dependId) == null)
                {
                    results.Add(new ValidationResult(
                        ValidationType.Warning,
                        $"无效引用: 依赖Buff ID {dependId} 不存在",
                        buff,
                        buff.Name,
                        buff.Id,
                        true,
                        () => FixInvalidDependRef(buff, dependId)
                    ));
                }
            }

            // 检查自引用
            if (buff.MutexBuffIds.Contains(buff.Id))
            {
                results.Add(new ValidationResult(
                    ValidationType.Error,
                    $"自引用错误: Buff不能与自己互斥",
                    buff,
                    buff.Name,
                    buff.Id,
                    true,
                    () => FixSelfMutex(buff)
                ));
            }

            if (buff.DependBuffIds.Contains(buff.Id))
            {
                results.Add(new ValidationResult(
                    ValidationType.Error,
                    $"自引用错误: Buff不能依赖自己",
                    buff,
                    buff.Name,
                    buff.Id,
                    true,
                    () => FixSelfDepend(buff)
                ));
            }
        }

        private void CheckPerformanceIssues(BuffDataSO buff)
        {
            // 检查更新频率设置
            if (buff.UpdateFrequency == UpdateFrequency.EveryFrame && !buff.IsPermanent && buff.Duration > 60f)
            {
                results.Add(new ValidationResult(
                    ValidationType.Warning,
                    $"性能建议: 持续时间较长({buff.Duration:F1}s)但使用每帧更新，建议降低更新频率",
                    buff,
                    buff.Name,
                    buff.Id
                ));
            }

            // 检查层数设置
            if (buff.MaxStack > 100)
            {
                results.Add(new ValidationResult(
                    ValidationType.Warning,
                    $"性能建议: 最大层数({buff.MaxStack})过大，可能影响性能",
                    buff,
                    buff.Name,
                    buff.Id
                ));
            }
        }

        private void CheckConfigurationIssues(BuffDataSO buff)
        {
            // 检查叠加模式与层数设置
            if (buff.StackMode == BuffStackMode.None && buff.MaxStack > 1)
            {
                results.Add(new ValidationResult(
                    ValidationType.Warning,
                    $"配置建议: 不可叠加Buff但最大层数>1",
                    buff,
                    buff.Name,
                    buff.Id
                ));
            }

            // 检查永久Buff的刷新设置
            if (buff.IsPermanent && buff.CanRefresh)
            {
                results.Add(new ValidationResult(
                    ValidationType.Info,
                    $"配置信息: 永久Buff不需要刷新持续时间",
                    buff,
                    buff.Name,
                    buff.Id
                ));
            }

            // 检查添加层数与最大层数
            if (buff.AddStackCount > buff.MaxStack)
            {
                results.Add(new ValidationResult(
                    ValidationType.Warning,
                    $"配置建议: 添加层数({buff.AddStackCount})大于最大层数({buff.MaxStack})",
                    buff,
                    buff.Name,
                    buff.Id
                ));
            }
        }

        #region Auto Fix Methods

        private void AutoFixAll()
        {
            int fixedCount = 0;
            foreach (var result in results.Where(r => r.CanAutoFix && r.AutoFixAction != null))
            {
                try
                {
                    result.AutoFixAction();
                    fixedCount++;
                }
                catch (Exception e)
                {
                    Debug.LogError($"自动修复失败: {e.Message}");
                }
            }

            if (fixedCount > 0)
            {
                EditorUtility.DisplayDialog("自动修复", $"成功修复 {fixedCount} 个问题", "确定");
                ValidateAll(); // 重新验证
            }
        }

        private void FixIdConflict(BuffDataSO buff, List<BuffDataSO> allBuffs)
        {
            // 生成新的唯一ID
            int newId = allBuffs.Max(b => b.Id) + 1;
            
            SerializedObject so = new SerializedObject(buff);
            SerializedProperty idProp = so.FindProperty("id");
            idProp.intValue = newId;
            so.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(buff);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"[BuffConfigValidator] 修复ID冲突: {buff.Name} ID {buff.Id} -> {newId}");
        }

        private void FixInvalidMutexRef(BuffDataSO buff, int invalidId)
        {
            SerializedObject so = new SerializedObject(buff);
            SerializedProperty mutexProp = so.FindProperty("mutexBuffIds");
            
            for (int i = 0; i < mutexProp.arraySize; i++)
            {
                if (mutexProp.GetArrayElementAtIndex(i).intValue == invalidId)
                {
                    mutexProp.DeleteArrayElementAtIndex(i);
                    break;
                }
            }
            
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(buff);
            AssetDatabase.SaveAssets();
        }

        private void FixInvalidDependRef(BuffDataSO buff, int invalidId)
        {
            SerializedObject so = new SerializedObject(buff);
            SerializedProperty dependProp = so.FindProperty("dependBuffIds");
            
            for (int i = 0; i < dependProp.arraySize; i++)
            {
                if (dependProp.GetArrayElementAtIndex(i).intValue == invalidId)
                {
                    dependProp.DeleteArrayElementAtIndex(i);
                    break;
                }
            }
            
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(buff);
            AssetDatabase.SaveAssets();
        }

        private void FixSelfMutex(BuffDataSO buff)
        {
            SerializedObject so = new SerializedObject(buff);
            SerializedProperty mutexProp = so.FindProperty("mutexBuffIds");
            
            for (int i = 0; i < mutexProp.arraySize; i++)
            {
                if (mutexProp.GetArrayElementAtIndex(i).intValue == buff.Id)
                {
                    mutexProp.DeleteArrayElementAtIndex(i);
                    break;
                }
            }
            
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(buff);
            AssetDatabase.SaveAssets();
        }

        private void FixSelfDepend(BuffDataSO buff)
        {
            SerializedObject so = new SerializedObject(buff);
            SerializedProperty dependProp = so.FindProperty("dependBuffIds");
            
            for (int i = 0; i < dependProp.arraySize; i++)
            {
                if (dependProp.GetArrayElementAtIndex(i).intValue == buff.Id)
                {
                    dependProp.DeleteArrayElementAtIndex(i);
                    break;
                }
            }
            
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(buff);
            AssetDatabase.SaveAssets();
        }

        #endregion
    }
}
#endif
