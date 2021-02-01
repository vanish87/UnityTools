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
        public class RemoteConfigure : ConfigureNoneMono<RemoteConfigure.RemoteData>
        {
            [System.Serializable]
            public class RemoteData
            {
                public PCInfo remote;
            }
        }

        [SerializeField] protected RemoteConfigure configure;

        protected override void OnEnable()
        {
            this.configure = new RemoteConfigure();
            this.configure.Load();
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