//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YooAsset.Editor;

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

        [BoxGroup(" ")]
        [Button("切换Demo", 50), GUIColor(0.4f, 0.8f, 1)]
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

        [Button]
        private void YooAssetSetting()
        {
            var yooSetting = AssetBundleCollectorSettingData.Setting;
            if (yooSetting == null)
            {
                Debug.LogError($"没有找到yoo设置");
                return;
            }

            yooSetting.UniqueBundleName = true;
            var defaultPackage = yooSetting.GetPackage("DefaultPackage");
            if (defaultPackage == null)
            {
                Debug.LogError($"没有找到默认包 DefaultPackage");
                return;
            }

            defaultPackage.EnableAddressable = true; //YIUI Demo必须开启可寻址

            var                       yiuiGroupName = "YIUI";
            AssetBundleCollectorGroup yiuiGroup     = default;
            foreach (var group in defaultPackage.Groups)
            {
                if (group.GroupName == yiuiGroupName)
                {
                    yiuiGroup = group;
                    break;
                }
            }

            if (yiuiGroup == null)
            {
                yiuiGroup = new AssetBundleCollectorGroup
                            {
                                GroupName = yiuiGroupName
                            };
                defaultPackage.Groups.Add(yiuiGroup);
            }
            
            //TODO 添加需要的设置
            
        }

        [BoxGroup("  ")]
        [Button("设置TMP中文字体", 40)]
        private void SetTMP()
        {
            var tmpSettings = Resources.Load<TMP_Settings>("TMP Settings");
            if (tmpSettings == null)
            {
                Debug.LogError($"没有找到TMP设置 请手动设置字体");
                return;
            }

            TMP_FontAsset newDefaultFontAsset = Resources.Load<TMP_FontAsset>("Fonts/SourceHanSansSC-VF SDF");
            if (newDefaultFontAsset == null)
            {
                Debug.LogError($"没有找到新的默认字体 请检查");
                return;
            }

            Type      settingsType          = typeof(TMP_Settings);
            FieldInfo defaultFontAssetField = settingsType.GetField("m_defaultFontAsset", BindingFlags.NonPublic | BindingFlags.Instance);

            if (defaultFontAssetField != null && newDefaultFontAsset != null)
            {
                defaultFontAssetField.SetValue(tmpSettings, newDefaultFontAsset);
            }
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
