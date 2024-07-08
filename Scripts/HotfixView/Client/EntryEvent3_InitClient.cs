using System;
using System.Collections.Generic;
using System.IO;
using YIUIFramework;

namespace ET.Client
{
    [Event(SceneType.StateSync)]
    public class EntryEvent3_InitClient: AEvent<Scene, EntryEvent3>
    {
        protected override async ETTask Run(Scene root, EntryEvent3 args)
        {
            root.AddComponent<GlobalComponent>();
            root.AddComponent<ResourcesLoaderComponent>();
            root.AddComponent<PlayerComponent>();
            root.AddComponent<CurrentScenesComponent>();

            {
                var result = await root.AddComponent<YIUIMgrComponent>().Initialize();
                if (!result)
                {
                    Log.Error("初始化UI失败");
                    return;
                }
                //TODO 以后这些消息统一到YIUIMGR初始化成功后自行调用
                //根据需求自行处理 在editor下自动打开  也可以根据各种外围配置 或者 GM等级打开
                //if (Define.IsEditor) //这里默认都打开
                {
                    root.AddComponent<GMCommandComponent>();
                }
            }

            await EventSystem.Instance.PublishAsync(root, new AppStartInitFinish());
        }
    }
}