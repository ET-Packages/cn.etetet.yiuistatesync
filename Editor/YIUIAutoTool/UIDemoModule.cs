//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

#if UNITY_EDITOR
using System;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace YIUIFramework.Editor
{
    [YIUIAutoMenu("Demo", 1000000)]
    internal class UIDemoModule : BaseYIUIToolModule
    {
        //YOOasset的设置 这边写一个配置 然后那边重新设置一下

        private const string YIUIPackageName     = "yiuistatesync";
        private const string ETPackageName       = "statesync";
        private const string ETLoaderPackageName = "loader";

        public enum EDemoType
        {
            YIUI,
            ET
        }

        [Title("模式")]
        [HideLabel]
        [EnumToggleButtons]
        public EDemoType DemoType;

        private bool OpenYIUI => DemoType == EDemoType.YIUI;

        public override void Initialize()
        {
        }

        public override void OnDestroy()
        {
        }

        [Button("切换Demo", 50)]
        [GUIColor(0f, 0.5f, 1f)]
        private void Switch()
        {
            var tips = "";
            if (CopyET() && ChangeFile() && SwitchToScene())
            {
                tips = $"成功切换Demo >> {(OpenYIUI ? "YIUI" : "ET")} \n记得编译ET.sln工程!!!";
            }
            else
            {
                tips = $"切换Demo 失败 请检查";
            }

            UnityTipsHelper.Show(tips);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private bool SwitchToScene()
        {
            var scenePath = $"Packages/cn.etetet.{(OpenYIUI ? YIUIPackageName : ETLoaderPackageName)}/Scenes/Init.unity";
            var path      = $"{Application.dataPath}/../{scenePath}";
            if (!File.Exists(path))
            {
                Debug.LogError($"路径不存在场景: {path} ");
                return false;
            }

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            return true;
        }

        /// <summary>
        /// 拷贝ET工程
        /// </summary>
        private bool CopyET()
        {
            var    et                  = "ET.sln";
            var    packageName         = OpenYIUI ? YIUIPackageName : ETPackageName;
            string sourceFilePath      = $"{Application.dataPath}/../Packages/cn.etetet.{packageName}/{et}";
            string destinationFilePath = $"{Application.dataPath}/../{et}";

            try
            {
                if (!File.Exists(sourceFilePath))
                {
                    UnityTipsHelper.Show($"{et} 源文件不存在。{sourceFilePath}");
                    return false;
                }

                File.Copy(sourceFilePath, destinationFilePath, true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"复制过程中发生错误: {ex.Message}");
            }

            return true;
        }

        /// <summary>
        /// 打开关闭 -- 注释文件
        /// </summary>
        private bool ChangeFile()
        {
            string sourceFilePath = $"{Application.dataPath}/../Packages/cn.etetet.{YIUIPackageName}/Scripts/HotfixView/Client";

            string[] csFilesInA = Directory.GetFiles(sourceFilePath, "*.cs", SearchOption.AllDirectories);

            foreach (string fileInA in csFilesInA)
            {
                string fileInB = fileInA.Replace("yiuistatesync", "statesync");
                if (!ChangeFile(fileInA, OpenYIUI))
                {
                    return false;
                }

                if (!ChangeFile(fileInB, !OpenYIUI))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ChangeFile(string path, bool open)
        {
            if (File.Exists(path))
            {
                string fileName    = Path.GetFileName(path);
                string fileContent = File.ReadAllText(path);

                bool startsWithSlashStar = fileContent.StartsWith("/*");
                bool endsWithStarSlash   = fileContent.EndsWith("*/");

                string modifiedContent;

                if (open)
                {
                    modifiedContent = fileContent;

                    if (startsWithSlashStar)
                    {
                        modifiedContent = modifiedContent.Substring(2);
                    }

                    int endLength = endsWithStarSlash ? 2 : 0;
                    if (endLength > 0 && modifiedContent.Length > endLength)
                    {
                        modifiedContent = modifiedContent.Substring(0, modifiedContent.Length - endLength);
                    }
                }
                else
                {
                    if (!startsWithSlashStar)
                    {
                        modifiedContent = "/*" + fileContent;
                    }
                    else
                    {
                        modifiedContent = fileContent;
                    }

                    if (!endsWithStarSlash)
                    {
                        modifiedContent += "*/";
                    }
                }

                try
                {
                    File.WriteAllText(path, modifiedContent);

                    //Debug.Log($"文件 {fileName} 已修改。 {(open ? "打开" : "关闭")}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"文件 {fileName} 写入失败 {e}。");
                    return false;
                }
            }
            else
            {
                Debug.LogError($"文件 {path} 在路径中不存在");
                return false;
            }

            return true;
        }
    }
}
#endif
