using UnityEngine;
using UnityEditor;
using BuffSystem.Data;
using BuffSystem.Core;

namespace BuffSystem.Editor
{
    /// <summary>
    /// BuffDataSO自定义编辑器
    /// </summary>
    [CustomEditor(typeof(BuffDataSO))]
    public class BuffDataSOEditor : UnityEditor.Editor
    {
        private SerializedProperty _id;
        private SerializedProperty _buffName;
        private SerializedProperty _description;
        private SerializedProperty _effectType;
        private SerializedProperty _isUnique;
        private SerializedProperty _stackMode;
        private SerializedProperty _maxStack;
        private SerializedProperty _addStackCount;
        private SerializedProperty _isPermanent;
        private SerializedProperty _duration;
        private SerializedProperty _canRefresh;
        private SerializedProperty _removeMode;
        private SerializedProperty _removeStackCount;
        private SerializedProperty _removeInterval;
        private SerializedProperty _buffLogicInstance;
        
        private bool _showBasicInfo = true;
        private bool _showStackSettings = true;
        private bool _showDurationSettings = true;
        private bool _showRemoveSettings = true;
        private bool _showLogicSettings = true;

        // 验证状态
        private bool _hasValidationError;
        private string _validationMessage;
        
        private void OnEnable()
        {
            _id = serializedObject.FindProperty("id");
            _buffName = serializedObject.FindProperty("buffName");
            _description = serializedObject.FindProperty("description");
            _effectType = serializedObject.FindProperty("effectType");
            _isUnique = serializedObject.FindProperty("isUnique");
            _stackMode = serializedObject.FindProperty("stackMode");
            _maxStack = serializedObject.FindProperty("maxStack");
            _addStackCount = serializedObject.FindProperty("addStackCount");
            _isPermanent = serializedObject.FindProperty("isPermanent");
            _duration = serializedObject.FindProperty("duration");
            _canRefresh = serializedObject.FindProperty("canRefresh");
            _removeMode = serializedObject.FindProperty("removeMode");
            _removeStackCount = serializedObject.FindProperty("removeStackCount");
            _removeInterval = serializedObject.FindProperty("removeInterval");
            _buffLogicInstance = serializedObject.FindProperty("buffLogicInstance");

            ValidateBuffData();
        }

        /// <summary>
        /// 验证Buff数据配置
        /// </summary>
        private void ValidateBuffData()
        {
            var currentData = (BuffDataSO)target;
            if (currentData == null) return;

            // 重置验证状态
            _hasValidationError = false;
            _validationMessage = "";

            // 获取所有Buff数据
            var allBuffs = BuffDatabase.Instance.GetAllBuffData();

            bool hasIdConflict = false;
            bool hasNameConflict = false;
            string conflictMessage = "";

            foreach (var buff in allBuffs)
            {
                if (buff == null) continue;

                // 跳过当前正在编辑的Buff（除非是其他同名/同ID的）
                if (buff == currentData) continue;

                // 检查ID冲突
                if (buff.Id == currentData.Id)
                {
                    hasIdConflict = true;
                    conflictMessage += $"• ID冲突: {buff.Id} 已被 '{buff.Name}' 使用\n";
                }

                // 检查Name冲突
                if (!string.IsNullOrEmpty(buff.Name) &&
                    !string.IsNullOrEmpty(currentData.Name) &&
                    buff.Name == currentData.Name)
                {
                    hasNameConflict = true;
                    conflictMessage += $"• 名称冲突: '{buff.Name}' 已被 ID:{buff.Id} 使用\n";
                }
            }

            // 保存验证状态
            if (hasIdConflict || hasNameConflict)
            {
                _hasValidationError = true;
                _validationMessage = conflictMessage;
                Debug.LogError($"[BuffDataSOEditor] {currentData.Name} (ID:{currentData.Id}) 配置验证失败:\n{conflictMessage}", currentData);
            }
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(10);

            // 标题
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("BUFF配置", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 显示验证错误
            if (_hasValidationError)
            {
                EditorGUILayout.HelpBox(
                    $"配置验证失败:\n{_validationMessage}",
                    MessageType.Error);
                EditorGUILayout.Space(5);
            }
            
            // 基础信息
            _showBasicInfo = EditorGUILayout.Foldout(_showBasicInfo, "基础信息", true, EditorStyles.foldoutHeader);
            if (_showBasicInfo)
            {
                EditorGUI.indentLevel++;

                // ID字段 - 如果有冲突显示错误样式
                var idContent = new GUIContent("ID", "Buff唯一标识符");
                if (_hasValidationError && _validationMessage.Contains($"ID冲突: {_id.intValue}"))
                {
                    GUI.color = Color.red;
                    EditorGUILayout.PropertyField(_id, idContent);
                    GUI.color = Color.white;
                }
                else
                {
                    EditorGUILayout.PropertyField(_id, idContent);
                }

                // Name字段 - 如果有冲突显示错误样式
                var nameContent = new GUIContent("名称", "Buff显示名称");
                var currentName = _buffName.stringValue;
                if (_hasValidationError && _validationMessage.Contains($"名称冲突: '{currentName}'"))
                {
                    GUI.color = Color.red;
                    EditorGUILayout.PropertyField(_buffName, nameContent);
                    GUI.color = Color.white;
                }
                else
                {
                    EditorGUILayout.PropertyField(_buffName, nameContent);
                }

                EditorGUILayout.PropertyField(_description, new GUIContent("描述", "Buff详细描述"));
                EditorGUILayout.PropertyField(_effectType, new GUIContent("效果类型", "Buff的效果分类"));
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            
            // 叠加设置
            _showStackSettings = EditorGUILayout.Foldout(_showStackSettings, "叠加设置", true, EditorStyles.foldoutHeader);
            if (_showStackSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_isUnique, new GUIContent("唯一性", "同类型Buff是否只能存在一个"));
                EditorGUILayout.PropertyField(_stackMode, new GUIContent("叠加模式", "Buff的叠加行为"));
                
                var stackMode = (BuffStackMode)_stackMode.enumValueIndex;
                if (stackMode == BuffStackMode.Stackable)
                {
                    EditorGUILayout.PropertyField(_maxStack, new GUIContent("最大层数", "Buff最高可叠加层数"));
                    EditorGUILayout.PropertyField(_addStackCount, new GUIContent("添加层数", "每次添加的层数"));
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            
            // 持续时间设置
            _showDurationSettings = EditorGUILayout.Foldout(_showDurationSettings, "持续时间设置", true, EditorStyles.foldoutHeader);
            if (_showDurationSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_isPermanent, new GUIContent("永久", "是否永久存在"));
                
                if (!_isPermanent.boolValue)
                {
                    EditorGUILayout.PropertyField(_duration, new GUIContent("持续时间", "Buff持续时间（秒）"));
                    EditorGUILayout.PropertyField(_canRefresh, new GUIContent("可刷新", "重新添加时是否刷新持续时间"));
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            
            // 移除设置
            _showRemoveSettings = EditorGUILayout.Foldout(_showRemoveSettings, "移除设置", true, EditorStyles.foldoutHeader);
            if (_showRemoveSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_removeMode, new GUIContent("移除模式", "Buff过期时的移除方式"));
                
                var removeMode = (BuffRemoveMode)_removeMode.enumValueIndex;
                if (removeMode == BuffRemoveMode.Reduce)
                {
                    EditorGUILayout.PropertyField(_removeStackCount, new GUIContent("移除层数", "每次移除的层数"));
                    EditorGUILayout.PropertyField(_removeInterval, new GUIContent("移除间隔", "逐层移除的时间间隔"));
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            
            // 逻辑设置
            _showLogicSettings = EditorGUILayout.Foldout(_showLogicSettings, "逻辑脚本", true, EditorStyles.foldoutHeader);
            if (_showLogicSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_buffLogicInstance, new GUIContent("逻辑实例", "Buff的具体逻辑实现"));
                
                // 显示帮助信息
                EditorGUILayout.HelpBox(
                    "逻辑脚本需要继承 BuffLogicBase 类，\n" +
                    "并实现需要的生命周期接口。", 
                    MessageType.Info);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // 预览信息
            DrawPreview();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawPreview()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("预览", EditorStyles.boldLabel);
            
            var data = (BuffDataSO)target;
            
            EditorGUILayout.LabelField($"ID: {data.Id}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"名称: {data.Name}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"类型: {data.EffectType}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"叠加: {data.StackMode} (最大{data.MaxStack}层)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"持续: {(data.IsPermanent ? "永久" : $"{data.Duration}秒")}", EditorStyles.miniLabel);
            
            EditorGUILayout.EndVertical();
        }
    }
}
