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

        public string Title => GUIMenuGroup.WindowType.Log.ToString();

        public class GUIData
        {
            public bool open = false;
            public Vector2 scrollPosition;
        }
        protected LogToolNetwork.LogToolNetworkSocket logSocket = new LogToolNetwork.LogToolNetworkSocket();
        protected Dictionary<string, GUIData> guis = new Dictionary<string, GUIData>();
        public void OnDrawGUI()
        {
            ConfigureGUI.OnGUISlider(ref this.maxLength, 1, 100);
            ConfigureGUI.OnGUIEnum(ref this.channel, "Log Channel");
            var logs = LogToolNetwork.LogToolNetworkSocket.Get(this.channel);
            foreach(var client in logs)
            {
                var ipString = client.Key;
                if (this.guis.ContainsKey(ipString) == false) this.guis.Add(ipString, new GUIData());

                var gui = this.guis[ipString];
                ConfigureGUI.OnFolder(ref gui.open, ipString);

                if (gui.open)
                {
                    client.Value.Reverse();
                    var guiStr = "";
                    for (var i = 0; i < Mathf.Min(client.Value.Count, this.maxLength); ++i)
                    {
                        var str = client.Value[i];
                        if (string.IsNullOrEmpty(str)) continue;

                        guiStr += str + "\n";
                    }

                    gui.scrollPosition = GUILayout.BeginScrollView(gui.scrollPosition);
                    GUILayout.Label(guiStr);
                    GUILayout.EndScrollView();
                }
            }
        }

        protected void Start()
        {
            this.logSocket.StartRecieve(13210);
        }

        public byte[] OnSerialize()
        {
            throw new System.NotImplementedException();
        }

        public void OnDeseialize(byte[] data)
        {
            throw new System.NotImplementedException();
        }
    }
}
