using UnityEngine;
using UnityEditor;
using BuffSystem.Runtime;

namespace BuffSystem.Editor
{
    /// <summary>
    /// BuffOwner自定义编辑器
    /// </summary>
    [CustomEditor(typeof(BuffOwner))]
    public class BuffOwnerEditor : UnityEditor.Editor
    {
        private BuffOwner _owner;
        
        private void OnEnable()
        {
            _owner = (BuffOwner)target;
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.Space(10);
            
            // 运行时信息
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("运行时信息", EditorStyles.boldLabel);
                
                EditorGUILayout.LabelField($"Buff数量: {_owner.BuffCount}");
                
                if (_owner.BuffCount > 0)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("当前Buff列表:", EditorStyles.miniBoldLabel);
                    
                    foreach (var buff in _owner.BuffContainer.AllBuffs)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        string timeText = buff.IsPermanent ? "∞" : $"{buff.RemainingTime:F1}s";
                        EditorGUILayout.LabelField(
                            $"• {buff.Name}", 
                            GUILayout.Width(120));
                        
                        EditorGUILayout.LabelField(
                            $"{buff.CurrentStack}/{buff.MaxStack}层", 
                            GUILayout.Width(60));
                        
                        EditorGUILayout.LabelField(
                            $"[{timeText}]", 
                            GUILayout.Width(60));
                        
                        if (GUILayout.Button("移除", GUILayout.Width(50)))
                        {
                            _owner.RemoveBuff(buff);
                        }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                }
                
                EditorGUILayout.Space(5);
                
                // 快捷操作
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("清空所有Buff"))
                {
                    _owner.ClearBuffs();
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("运行时显示Buff信息", MessageType.Info);
            }
        }
    }
}
