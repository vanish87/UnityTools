

//Note Uncomment this to use OSC
//GitHub:https://github.com/vanish87/OscJack
//#define USE_OSC
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace UnityTools.Networking
{
    public class OscSenderTool : MonoBehaviour
    {

        [SerializeField] protected string ip = "127.0.0.1";
        [SerializeField] protected short port = 7777;
        #if USE_OSC
        protected OscJack.OscClient client;
        #endif

        protected virtual void OnEnable()
        {
            this.OnInit();
        }
        protected virtual void OnDisable()
        {
            this.CleanUp();
        }

        protected void OnInit()
        {
            try
            {
                this.CleanUp();
                #if USE_OSC
                this.client = new OscJack.OscClient(this.ip, this.port);
                #endif
            }
            catch (SocketException e)
            {
                Debug.Log(e.ToString());
            }
        }

        protected virtual void CleanUp()
        {
            #if USE_OSC
            if (this.client != null) this.client.Dispose();
            #endif
        }

    }
}