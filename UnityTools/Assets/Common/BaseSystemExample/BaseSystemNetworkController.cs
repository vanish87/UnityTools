using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Common.Example
{
    public class BaseSystemNetworkController : Networking.NetworkController, SystemLauncher.ILauncherUser
    {
        public Environment Runtime { get; set; }

        public int Order => (int)SystemLauncher.LauncherOrder.Network;

        public Launcher<SystemLauncher.Data>.LauncherState CurrentState { get; set; }

        public void OnLaunchEvent(SystemLauncher.Data data, Launcher<SystemLauncher.Data>.LaunchEvent levent)
        {
            switch(levent)
            {
                case SystemLauncher.LaunchEvent.Init:
                    {
                        var networkData = this.GetNetworkData(data.pcConfigure.D.pcList);
                        this.NotifyUser(networkData);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
