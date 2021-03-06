﻿using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityTools.Networking
{
    public class MessageDataSender : MonoBehaviour
    {
        [SerializeField] protected MessageDataSocket socket = new MessageDataSocket();
        [SerializeField] protected Datatex data = new Datatex();
        [SerializeField] protected SocketData server = SocketData.Make("127.0.0.1", 12346);
        // Start is called before the first frame update
        void Start()
        {

            this.socket.Bind(this.data, typeof(MessageDataSender).ToString() + "." + nameof(data));
        }

        // Update is called once per frame
        void Update()
        {            
            this.socket.SendData(this.server);
        }
    }
}