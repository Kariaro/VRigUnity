#if !NETFX_CORE

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace uOSC.DotNet
{

public class Udp : uOSC.Udp
{
    enum State
    {
        Stop,
        Server,
        Client,
    }
    State state_ = State.Stop;

    Queue<byte[]> messageQueue_ = new Queue<byte[]>();
    object lockObject_ = new object();

    UdpClient udpClient_;
    IPEndPoint endPoint_;
    Thread thread_ = new Thread();

    public override int messageCount
    {
        get { return messageQueue_.Count; }
    }

    public override bool isRunning
    {
        get { return state_ != State.Stop; }
    }

    public override void StartServer(int port)
    {
        Stop();
        state_ = State.Server;

        try
        {
            endPoint_ = new IPEndPoint(IPAddress.IPv6Any, port);
            udpClient_ = new UdpClient(AddressFamily.InterNetworkV6);
            udpClient_.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
            udpClient_.Client.Bind(endPoint_);
        }
        catch (System.Exception e)
        {

            UnityEngine.Debug.LogError(e.ToString());
            state_ = State.Stop;
            return;
        }

        thread_.Start(() => 
        {
            while (udpClient_.Available > 0) 
            {
                var buffer = udpClient_.Receive(ref endPoint_);
                lock (lockObject_)
                {
                    messageQueue_.Enqueue(buffer);
                }
            }
        });
    }

    public override void StartClient(string address, int port)
    {
        Stop();
        state_ = State.Client;

        var ip = IPAddress.Parse(address);
        endPoint_ = new IPEndPoint(ip, port);
        udpClient_ = new UdpClient(endPoint_.AddressFamily);
    }

    public override void Stop()
    {
        if (state_ == State.Stop) return;

        thread_.Stop();
        udpClient_.Close();
        state_ = State.Stop;
    }

    public override void Send(byte[] data, int size)
    {
        try
        {
            udpClient_.Send(data, size, endPoint_);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError(e.ToString());
        }
    }

    public override byte[] Receive()
    {
        byte[] buffer;
        lock (lockObject_)
        {
            buffer = messageQueue_.Dequeue();
        }
        return buffer;
    }
}

}

#endif