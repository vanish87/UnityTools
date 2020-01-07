using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

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

        //WARNING: Do not call this on update
        //It will result memory leak
        public static bool IsReachable(IPEndPoint epRemote)
        {
            try
            {
                using (var pingSender = new Ping())
                {
                    PingReply reply = pingSender.Send(epRemote.Address, 1);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch (SocketException e)
            {

                return false;
            }
        }
    }
}
