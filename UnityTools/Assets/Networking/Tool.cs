using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace UnityTools.Networking
{
    public class Tool
    {
        public static List<string> GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ret = new List<string>();
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ret.Add(ip.ToString());
                }
            }
            return ret;
        }
    }
}
