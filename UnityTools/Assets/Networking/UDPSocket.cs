﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityTools.Common;
using UnityTools.Debuging;

namespace UnityTools.Networking
{
    public class SocketData
    {
        public IPEndPoint endPoint;
        public bool reachable = false;
        public Socket socket;

        public static SocketData Make(IPEndPoint end)
        {
            return new SocketData(end);
        }
        public static SocketData Make(int port = 0)
        {
            return new SocketData(port);
        }
        public static SocketData Make(string ip, int port)
        {
            return new SocketData(ip, port);
        }

        public static SocketData Make(IPAddress ip, int port)
        {
            return new SocketData(ip, port);
        }
        protected SocketData(IPEndPoint end)
        {
            this.endPoint = new IPEndPoint(end.Address, end.Port);
            //optional reachable check, it may cause memory leak
            this.reachable = true; // Tool.IsReachable(this.endPoint);
        }
        protected SocketData(int port = 0)
        {
            this.endPoint = new IPEndPoint(IPAddress.Any, port);
            //optional reachable check, it may cause memory leak
            this.reachable = true; // Tool.IsReachable(this.endPoint);
        }
        protected SocketData(IPAddress address, int port = 0)
        {
            this.endPoint = new IPEndPoint(address, port);
            //optional reachable check, it may cause memory leak
            this.reachable = true; // Tool.IsReachable(this.endPoint);
        }
        protected SocketData(string ip, int port)
        {
            IPAddress local = IPAddress.Any;
            if (ip.ToLower() == "localhost" || ip == "127.0.0.1")
            {
                var localIPs = Dns.GetHostAddresses(Dns.GetHostName()).ToList();
                local = localIPs.Find(ips => ips.AddressFamily == AddressFamily.InterNetwork);
            }
            else
            {
                if (IPAddress.TryParse(ip, out local))
                {
                    //nothing to do
                }
                else
                {
                    local = IPAddress.Any;
                    port = 0;
                }
            }

            Assert.IsTrue(IPEndPoint.MinPort <= port && port <= IPEndPoint.MaxPort);
            this.endPoint = new IPEndPoint(local, port);
            //optional reachable check, it may cause memory leak
            this.reachable = true;// Tool.IsReachable(this.endPoint);
        }

        ~SocketData()
        {
            this.endPoint = null;
        }
    }

    public class State
    {
        private const int bufferSize = 64 * 1024; //64K
        public byte[] buffer = new byte[bufferSize];
    }
    public class ReceiveState : State
    {
        public SocketData remote = SocketData.Make();
    }
    public enum ConnectionType
    {
        UDP,
        TCP,
    }

    //code from https://gist.github.com/darkguy2008/413a6fea3a5b4e67e5e0d96f750088a9
    //for testing latency of UDP
    public class UDPSocket<T> : Disposable
    {
        public ReceiveState receiveState = new ReceiveState();
        public bool DebugLog = false;
        private Socket socket;
        public enum Connection
        {
            Incoming,
            Outgoing,
        }

        public enum SocketRole
        {
            Sender,
            Receiver,
            Broadcast,
        }
        protected Dictionary<SocketRole, bool> roleState = new Dictionary<SocketRole, bool>()
        {
            {SocketRole.Sender      , false },
            {SocketRole.Receiver    , false },
            {SocketRole.Broadcast   , false },
        };
        protected Dictionary<Connection, List<SocketData>> connections = new Dictionary<Connection, List<SocketData>>()
        {
            { Connection.Incoming   , new List<SocketData>() },
            { Connection.Outgoing  , new List<SocketData>() },
        };

        protected override void DisposeManaged()
        {
            if (this.socket != null)
            {
                try
                {
                    this.socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e) { if (DebugLog) Debug.LogWarning(e.ToString()); }
                finally
                {
                    this.socket.Close();
                    this.socket.Dispose();
                }
                this.socket = null;
            }
        }

        public void Setup(SocketRole role, SocketData remote)
        {
            if (this.roleState[role]) return;
            try
            {
                switch (role)
                {
                    case SocketRole.Sender:
                        {
                            Assert.IsNotNull(remote);
                            //connect is optional for UDP
                            this.socket.Connect(remote.endPoint);
                        }
                        break;
                    case SocketRole.Receiver:
                        {
                            this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
                            //this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
                            Assert.IsNotNull(remote);
                            this.socket.Bind(remote.endPoint);

                            if (DebugLog) LogTool.Log("Start Receiver on " + remote.endPoint);
                        }
                        break;
                    case SocketRole.Broadcast:
                        {
                            this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                            this.socket.EnableBroadcast = true;
                            //this.socket.Bind(this.socketConfigure.endPoint);
                        }
                        break;
                }
            }
            catch (SocketException e)
            {
                LogTool.Log(remote.endPoint.ToString() + " " + e.ToString());
                return;
            }
            this.roleState[role] = true;
        }
        public UDPSocket(ConnectionType type = ConnectionType.UDP)
        {
            var st = type == ConnectionType.UDP ? SocketType.Dgram : SocketType.Stream;
            var pt = type == ConnectionType.UDP ? ProtocolType.Udp : ProtocolType.Tcp;
            this.socket?.Close();
            this.socket?.Dispose();
            this.socket = new Socket(AddressFamily.InterNetwork, st, pt);
        }

        public virtual void OnConnect(SocketData socket) { }
        public virtual void OnMessage(SocketData socket, T data) { }
        public virtual void OnDisconnect(SocketData socket) { }
        public virtual void OnError(SocketData socket) { }

        public virtual byte[] OnSerialize(T data)
        {
            return Serialization.ObjectToByteArray(data);
        }

        public virtual T OnDeserialize(byte[] data, int length)
        {
            //Note data may has different size than length
            return Serialization.ByteArrayToObject<T>(data);
        }

        public virtual void Send(SocketData socket, T data)
        {
            this.Setup(SocketRole.Sender, socket);

            var byteData = this.OnSerialize(data);
            this.SendByte(socket, byteData);
        }
        public virtual void Broadcast(T data, int port)
        {
            this.Setup(SocketRole.Broadcast, null);

            var epTo = SocketData.Make();
            epTo.endPoint.Address = IPAddress.Broadcast;
            epTo.endPoint.Port = port;

            var byteData = this.OnSerialize(data);
            this.SendByte(epTo, byteData);
        }
        public virtual void StartReceive(int port = 0)
        {
            this.receiveState.remote = SocketData.Make(port);
            this.receiveState.remote.socket = this.socket;

            this.Setup(SocketRole.Receiver, this.receiveState.remote);

            EndPoint epFrom = this.receiveState.remote.endPoint;

            try
            {
                this.socket.BeginReceiveFrom(this.receiveState.buffer, 0, this.receiveState.buffer.Length, SocketFlags.None, ref epFrom, this.ReceiveCallback, this.receiveState);
            }
            catch (Exception e)
            {
                //check flag
                Debug.Log(e.ToString());
            }
        }


        protected void SendByte(SocketData epTo, byte[] byteData)
        {
            epTo.socket = this.socket;

            Assert.IsNotNull(epTo);
            Assert.IsNotNull(byteData);
            Assert.IsTrue(byteData.Length > 0);
            Assert.IsTrue(byteData.Length < 64 * 1024, "Data length " + byteData.Length + " exceeds max 64k");
            Assert.IsNotNull(epTo.socket);

            if (byteData.Length > 64 * 1024)
            {
                LogTool.Log("Data exceeded 64K", LogLevel.Warning);
                return;
            }

            if (epTo.reachable)
            {
                try
                {
                    this.socket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epTo.endPoint, this.SendCallback, epTo);
                }
                catch (Exception e)
                {
                    LogTool.Log(epTo.endPoint.ToString());
                    LogTool.Log(e.ToString());
                }
                finally
                {
                    byteData = null;
                }
            }
            else
            {
                LogTool.Log(epTo.endPoint.ToString() + " is unreachable", LogLevel.Warning);
            }
        }
        protected void SendCallback(IAsyncResult ar)
        {
            try
            {
                if (ar.IsCompleted)
                {
                    var socketData = ar.AsyncState as SocketData;

                    int bytes = socketData.socket.EndSendTo(ar);

                    if (this.connections[Connection.Outgoing].Exists(c => c.endPoint.Address.Equals(socketData.endPoint.Address)) == false
                        && socketData.endPoint.Address != IPAddress.Broadcast)
                    {
                        this.connections[Connection.Outgoing].Add(socketData);
                        if (DebugLog) LogTool.LogFormat("Add out connection: {0}", LogLevel.Warning, LogChannel.Network, socketData.endPoint.ToString());
                    }

                    if (DebugLog) LogTool.LogFormat("SEND: {0} bytes To {1}", LogLevel.Dev, LogChannel.Network, bytes, socketData.endPoint.ToString());

                    if (DebugLog)
                    {
                        foreach (var c in this.connections)
                        {
                            LogTool.Log("connections count " + c.Key.ToString() + " " + c.Value.Count);
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                LogTool.Log("UDP Send Exception");
                LogTool.Log(e.ToString());
                LogTool.Log(e.ErrorCode.ToString());
                LogTool.Log(e.Message);
            }
        }
        protected void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var stateFrom = ar.AsyncState as ReceiveState;
                Assert.IsTrue(stateFrom == this.receiveState);

                EndPoint epFrom = stateFrom.remote.endPoint;
                int bytesReceived = stateFrom.remote.socket.EndReceiveFrom(ar, ref epFrom);

                var ipFrom = epFrom as IPEndPoint;
                stateFrom.remote.endPoint = ipFrom;

                if (bytesReceived > 0)
                {
                    if (DebugLog) Debug.LogFormat("RECV: {0}: {1}", epFrom.ToString(), bytesReceived);
                    T data = this.OnDeserialize(stateFrom.buffer, bytesReceived);
                    this.OnMessage(stateFrom.remote, data);
                }

                if (this.connections[Connection.Incoming].Exists(c => c.endPoint.Address.Equals(ipFrom.Address)) == false)
                {
                    this.connections[Connection.Incoming].Add(SocketData.Make(ipFrom));
                    if (DebugLog) LogTool.LogFormat("Add in connection: {0}", LogLevel.Verbose, LogChannel.Network, ipFrom.ToString());
                }

                epFrom = this.receiveState.remote.endPoint;
                stateFrom.remote.socket.BeginReceiveFrom(this.receiveState.buffer, 0, this.receiveState.buffer.Length, SocketFlags.None, ref epFrom, this.ReceiveCallback, this.receiveState);
            }
            catch (SocketException e)
            {
                // LogTool.Log(e.ToString());
                // if(!(e is ObjectDisposedException))
                {
                    LogTool.Log("UDP Receive Exception");
                    LogTool.Log(e.ToString());
                    var se = e as SocketException;
                    if (se != null)
                    {
                        LogTool.Log(se.ErrorCode.ToString());
                        LogTool.Log(se.Message);
                    }
                }
            }
        }
    }


}
