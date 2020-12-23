using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Interface;

namespace UnityTools.Networking
{
    public class OscTouchDataSender : OscSenderTool
    {
        [System.Serializable]
        public class RemoteConfigure : ConfigNoneMono<RemoteConfigure.RemoteData>
        {
            [System.Serializable]
            public class RemoteData
            {
                public PCInfo remote;
            }
            [SerializeField] protected RemoteData data;
            public override RemoteData D { get => this.data; set => this.data = value; }

            protected override string filePath => System.IO.Path.Combine(Application.streamingAssetsPath, this.ToString() + ".xml");
        }

        [SerializeField] protected RemoteConfigure configure;

        protected override void OnEnable()
        {
            this.configure = new RemoteConfigure();
            this.configure.LoadAndNotify();
            this.ip = this.configure.D.remote.ipAddress;
            this.port = this.configure.D.remote.ports[0].port;
            base.OnEnable();
        }
        protected void Update()
        {
            if(Input.GetMouseButton(0))
            {
                var pos = Input.mousePosition;
                pos = new Vector3(pos.x / Screen.width, pos.y / Screen.height, 1);
                this.client.Send("/debug/pos", pos.x, pos.y);
            }


            this.configure.Update();
        }

    }
}