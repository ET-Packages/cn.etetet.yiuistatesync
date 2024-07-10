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
using UnityEngine;

namespace YIUIFramework.Editor
{
    [YIUIAutoMenu("Demo", 1000000)]
    internal class UIDemoModule : BaseYIUIToolModule
    {
        //当前所有脚本跟原始demo对比 然后吧他的都给注释了 用我的
        
        //YOOasset的设置 这边写一个配置 然后那边重新设置一下
        
        
        
        public override void Initialize()
        {

        }

        public override void OnDestroy()
        {

        }
    }
}
#endif