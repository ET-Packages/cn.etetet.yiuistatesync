using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  Lsy
    /// Date    2024.6.24
    /// Desc
    /// </summary>
    [FriendOf(typeof(LobbyPanelComponent))]
    public static partial class LobbyPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this LobbyPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LobbyPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this LobbyPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始

        private static async ETTask OnEventEnterMapAction(this LobbyPanelComponent self)
        {
            await EnterMapHelper.EnterMapAsync(self.Root());
            await self.UIPanel.CloseAsync();
        }

        #endregion YIUIEvent结束
    }
}