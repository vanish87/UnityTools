using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Networking
{
    public class MessageDataReceiver : MonoBehaviour
    {
        [SerializeField] protected MessageDataSocket socket = new MessageDataSocket();
        [SerializeField] protected Datatex data = new Datatex();
        // Start is called before the first frame update
        void Start()
        {
            this.socket.Bind(data);
            this.socket.StartReceive(12346);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}