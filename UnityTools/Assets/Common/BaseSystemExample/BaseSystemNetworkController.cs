using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Common.Example
{
    public class BaseSystemNetworkController : Networking.NetworkController, SystemLauncher.ILauncherUser
    {
        public Environment Runtime { get; set; }

        public int Order => (int)SystemLauncher.Order.Network;

        public SystemLauncher.State CurrentState { get; set; }

        public void OnLaunchEvent(SystemLauncher.Data data, SystemLauncher.Event levent)
        {
            switch(levent)
            {
                case SystemLauncher.Event.Init:
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
