using UnityEngine;
using UnityEditor;
using BuffSystem.Data;

namespace BuffSystem.Editor
{
    /// <summary>
    /// Buff系统菜单项
    /// </summary>
    public static class BuffSystemMenu
    {
        private const string menuRoot = "BuffSystem/";
        
        #region Create Assets
        
        [MenuItem(menuRoot + "Create/Buff Data", priority = 1)]
        private static void CreateBuffData()
        {
            CreateAsset<BuffDataSO>("NewBuffData");
        }   
        
        [MenuItem(menuRoot + "Create/Buff Data Center", priority = 2)]
        private static void CreateBuffDataCenter()
        {
            CreateAsset<BuffDataCenter>("BuffDataCenter");
        }
        
        [MenuItem(menuRoot + "Create/System Config", priority = 3)]
        private static void CreateBuffSystemConfig()
        {
            CreateAsset<BuffSystemConfig>("BuffSystemConfig");
        }
        
        private static void CreateAsset<T>(string defaultName) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!string.IsNullOrEmpty(System.IO.Path.GetExtension(path)))
            {
                path = System.IO.Path.GetDirectoryName(path);
            }
            
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{defaultName}.asset");
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        
        #endregion
        
        #region Tools
        
        [MenuItem(menuRoot + "Tools/Reload Database", priority = 20)]
        private static void ReloadDatabase()
        {
            BuffDatabase.Instance.Reload();
            EditorUtility.DisplayDialog("BuffSystem", "Buff数据库已重新加载", "确定");
        }
        
        [MenuItem(menuRoot + "Tools/Create Resources Folders", priority = 21)]
        private static void CreateResourcesFolders()
        {
            CreateFolder("Assets/Resources/BuffSystem");
            CreateFolder("Assets/Resources/BuffSystem/BuffData");
            EditorUtility.DisplayDialog("BuffSystem", "Resources文件夹已创建", "确定");
        }
        
        private static void CreateFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path);
                string folder = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
        
        #endregion
        
        #region Documentation

        [MenuItem(menuRoot + "Documentation/Open README", priority = 40)]
        private static void OpenDocumentation()
        {
            // 尝试打开本地README文件
            string readmePath = System.IO.Path.Combine(Application.dataPath, "BuffSystem", "README.md");
            if (System.IO.File.Exists(readmePath))
            {
                Application.OpenURL("file://" + readmePath);
            }
            else
            {
                EditorUtility.DisplayDialog("BuffSystem", "未找到README.md文件", "确定");
            }
        }

        #endregion
    }
}