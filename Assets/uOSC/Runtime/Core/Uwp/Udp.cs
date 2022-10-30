#if NETFX_CORE

using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace uOSC.Uwp
{

public class Udp : uOSC.Udp
{
    private const int BufferSize = 8192;

    DatagramSocket socket_;
    HostName sendHost;
    string sendPort;

    object lockObject_ = new object();

    Queue<byte[]> messageQueue_ = new Queue<byte[]>();
    byte[] buffer = new byte[BufferSize];

    public override int messageCount
    {
        get { return messageQueue_.Count; }
    }

    public override bool isRunning
    {
        get { return socket_ != null; }
    }

    public async override void StartServer(int port)
    {
        try 
        {
            socket_ = new DatagramSocket();
            socket_.MessageReceived += OnMessage;
            await socket_.BindServiceNameAsync(port.ToString());
        } 
        catch (Exception e) 
        {
            Debug.LogError(e.ToString());
        }
    }

    async void OnMessage(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
    {
        using (var stream = args.GetDataStream().AsStreamForRead()) 
        {
            var size = await stream.ReadAsync(buffer, 0, BufferSize);
            lock (lockObject_) 
            {
                var data = new byte[size];
                Array.Copy(buffer, data, size);
                messageQueue_.Enqueue(data);
            }
        }
    }

    public override void StartClient(string address, int port)
    {
        socket_ = new DatagramSocket();
        sendHost = new HostName(address);
        sendPort = port.ToString();
    }

    public override void Stop()
    {
        socket_.Dispose();
        socket_ = null;
    }

    public async override void Send(byte[] data, int size)
    {
        var stream = await socket_.GetOutputStreamAsync(sendHost, sendPort);
        var buffer = data.AsBuffer(0, size);
        await stream.WriteAsync(buffer);
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