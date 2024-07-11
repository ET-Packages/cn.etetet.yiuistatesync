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
            ChangeETCoreFile();
            ChageSourceGeneratorDll();
            if (SyncYooAssetSetting() && CopyET() && ChangeFile() && SwitchToScene())
            {
                tips = $"成功切换Demo >> {(OpenYIUI ? "YIUI" : "ET")} \n记得编译ET.sln工程!!!";
            }
            else
            {
                tips = $"切换Demo 失败 请检查";
            }

            UnityTipsHelper.Show(tips);
            YIUIAutoTool.CloseWindowRefresh();
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

        [BoxGroup("   ")]
        [Button("打开YooAsset设置", 40)]
        private void SetYooAssetSetting()
        {
            Type      type      = typeof(AssetBundleCollectorSettingData);
            FieldInfo fieldInfo = type.GetField("_setting", BindingFlags.NonPublic | BindingFlags.Static);
            if (fieldInfo != null)
            {
                //根据Demo类型可以打开不同的YooAsset设置
                if (OpenYIUI)
                {
                    var tempSetting = ScriptableObject.CreateInstance<AssetBundleCollectorSetting>();
                    var yiuiSetting = SettingLoader.LoadSettingData<YIUIYooAssetSetting>();
                    if (yiuiSetting == null)
                    {
                        Debug.LogError($"没有找到YIUIYooAssetSetting");
                        return;
                    }

                    tempSetting.Packages = yiuiSetting.Packages;
                    fieldInfo.SetValue(null, tempSetting);
                }
                else
                {
                    fieldInfo.SetValue(null, null);
                }

                YIUIAutoTool.CloseWindowRefresh();
                EditorApplication.ExecuteMenuItem("YooAsset/AssetBundle Collector");
            }
            else
            {
                Debug.LogError($"没有找到AssetBundleCollectorSettingData._setting");
            }
        }

        /// <summary>
        /// 同步YooAsset设置
        /// </summary>
        private bool SyncYooAssetSetting()
        {
            var yooSetting = AssetBundleCollectorSettingData.Setting;
            if (yooSetting == null)
            {
                Debug.LogError($"没有找到yoo设置");
                return false;
            }

            var yiuiSetting = SettingLoader.LoadSettingData<YIUIYooAssetSetting>();
            if (yiuiSetting == null)
            {
                Debug.LogError($"没有找到YIUIYooAssetSetting");
                return false;
            }

            var yiuiDefaultPackage = yiuiSetting.GetPackage("DefaultPackage");
            if (yiuiDefaultPackage == null)
            {
                Debug.LogError($"yiuiSetting 没有找到默认包 DefaultPackage");
                return false;
            }

            var defaultPackage = yooSetting.GetPackage("DefaultPackage");
            if (defaultPackage == null)
            {
                Debug.LogError($"yooSetting 没有找到默认包 DefaultPackage");
                return false;
            }

            yooSetting.UniqueBundleName      = true;
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

            if (yiuiGroup != null)
                defaultPackage.Groups.Remove(yiuiGroup);

            defaultPackage.Groups.AddRange(yiuiDefaultPackage.Groups);

            return true;
        }

        /// <summary>
        /// 场景切换
        /// </summary>
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
            var et                  = "ET.sln";
            var packageName         = OpenYIUI ? YIUIPackageName : ETPackageName;
            var sourceFilePath      = $"{Application.dataPath}/../Packages/cn.etetet.{packageName}/{et}";
            var destinationFilePath = $"{Application.dataPath}/../{et}";

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
            var sourceFilePath = $"{Application.dataPath}/../Packages/cn.etetet.{YIUIPackageName}/Scripts/HotfixView/Client";

            var csFilesInA = Directory.GetFiles(sourceFilePath, "*.cs", SearchOption.AllDirectories);

            foreach (var fileInA in csFilesInA)
            {
                var fileInB = fileInA.Replace("yiuistatesync", "statesync");
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
                var fileName    = Path.GetFileName(path);
                var fileContent = File.ReadAllText(path);

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

        #region 额外设置 等ET改以后就不用改这个了

        #region 改文件

        private void ChangeETCoreFile()
        {
            var filePath = $"{Application.dataPath}/../Packages/cn.etetet.core/Runtime/Fiber/EntitySystem.cs";
            var find     = "public class EntitySystem";
            var replace  = "public partial class EntitySystem";
            ReplaceStringInFile(filePath, find, replace);

            var find2    = "private Queue<EntityRef<Entity>>";
            var replace2 = "public Queue<EntityRef<Entity>>";
            ReplaceStringInFile(filePath, find2, replace2);

            var filePath3 = $"{Application.dataPath}/../Packages/cn.etetet.core/Runtime/World/EventSystem/EventSystem.cs";
            var find3     = "public class EventSystem";
            var replace3  = "public partial class EventSystem";
            ReplaceStringInFile(filePath3, find3, replace3);
        }

        private static void ReplaceStringInFile(string filePath, string find, string replace)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"没找到这个文件 {filePath}");
                return;
            }

            try
            {
                var  lines      = File.ReadAllLines(filePath);
                bool isModified = false;

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(find))
                    {
                        lines[i]   = lines[i].Replace(find, replace);
                        isModified = true;
                        break;
                    }
                }

                if (isModified)
                {
                    File.WriteAllLines(filePath, lines);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"写入文件错误 {ex}");
            }
        }

        #endregion

        #region 改分析器

        private void ChageSourceGeneratorDll()
        {
            var sourceFilePath      = $"{Application.dataPath}/../Packages/cn.etetet.yiuistatesync/Editor/ET.SourceGenerator.yiui";
            var destinationFilePath = $"{Application.dataPath}/../Packages/cn.etetet.sourcegenerator/ET.SourceGenerator.dll";

            if (!File.Exists(sourceFilePath))
            {
                Debug.LogError($"没找到这个文件 {sourceFilePath}");
                return;
            }

            try
            {
                if (File.Exists(destinationFilePath))
                {
                    File.Delete(destinationFilePath);
                }

                File.Copy(sourceFilePath, destinationFilePath, true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"写入文件错误 {ex}");
            }
        }

        #endregion

        #endregion
    }
}
#endif
