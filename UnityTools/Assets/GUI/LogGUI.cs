using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Debuging;
using UnityTools.Networking;

namespace UnityTools.GUITool
{
    public class LogGUI : MonoBehaviour, GUIMenuGroup.IGUIHandler
    {
        [SerializeField] protected int maxLength = 20;
        [SerializeField] protected LogChannel channel = LogChannel.Everything;
        protected LogToolNetwork.LogToolNetworkSocket logSocket = new LogToolNetwork.LogToolNetworkSocket();

        public string Title => GUIMenuGroup.WindowType.Log.ToString();


        public void OnDrawGUI()
        {
            ConfigureGUI.OnGUISlider(ref this.maxLength, 1, 100);
            ConfigureGUI.OnGUIEnum(ref this.channel, "Log Channel");
            var logs = LogToolNetwork.LogToolNetworkSocket.Get(this.channel);
            foreach(var client in logs)
            {
                client.Value.Reverse();
                for (var i = 0; i < Mathf.Min(client.Value.Count, this.maxLength); ++i)
                {
                    GUILayout.Label(client.Key.endPoint.ToString() + client.Value[i]);
                }
            }
        }

        protected void Start()
        {
            this.logSocket.StartRecieve(13210);
        }
    }
}
