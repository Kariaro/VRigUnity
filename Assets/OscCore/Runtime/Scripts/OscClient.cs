using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace OscCore
{
    public class OscClient
    {
        readonly Socket m_Socket;
        readonly OscWriter m_Writer;

        /// <summary>Serializes outgoing messages</summary>
        public OscWriter Writer => m_Writer;

        /// <summary>Where this client is sending messages to</summary>
        public IPEndPoint Destination { get; }

        public OscClient(string ipAddress, int port)
        {
            m_Writer = new OscWriter();
            
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            if (ipAddress == "255.255.255.255")
                m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            
            Destination = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            m_Socket.Connect(Destination);
        }
        
        /// <summary>Send a message with no elements</summary>
        public void Send(string address)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            m_Writer.Write(",");
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }

        const uint k_Int32TypeTagBytes = 26924;    // ",i  " 
        
        /// <summary>Send a message with a single 32-bit integer element</summary>
        public void Send(string address, int element)
        {
            m_Writer.WriteAddressAndTags(address, k_Int32TypeTagBytes);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        const uint k_Float32TypeTagBytes = 26156;    // ",f  " 
        
        /// <summary>Send a message with a single 32-bit float element</summary>
        public void Send(string address, float element)
        {
            m_Writer.WriteAddressAndTags(address, k_Float32TypeTagBytes);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }

        const uint k_StringTypeTagBytes = 29484;    // ",s  " 
        
        /// <summary>Send a message with a single string element</summary>
        public void Send(string address, string element)
        {
            m_Writer.WriteAddressAndTags(address, k_StringTypeTagBytes);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        const uint k_BlobTypeTagBytes = 25132;    // ",b  " 

        /// <summary>Send a message with a single blob element</summary>
        /// <param name="address">The OSC address</param>
        /// <param name="bytes">The bytes to copy from</param>
        /// <param name="length">The number of bytes in the blob element</param>
        /// <param name="start">The index in the bytes array to start copying from</param>
        public void Send(string address, byte[] bytes, int length, int start = 0)
        {
            m_Writer.WriteAddressAndTags(address, k_BlobTypeTagBytes);
            m_Writer.Write(bytes, length, start);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        /// <summary>Send a message with 2 32-bit float elements</summary>
        public void Send(string address, Vector2 element)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            const string typeTags = ",ff";
            m_Writer.Write(typeTags);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }

        /// <summary>Send a message with 3 32-bit float elements</summary>
        public void Send(string address, Vector3 element)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            const string typeTags = ",fff";
            m_Writer.Write(typeTags);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        const uint k_Int64TypeTagBytes = 25644;    // ",d  " 
        
        /// <summary>Send a message with a single 64-bit float element</summary>
        public void Send(string address, double element)
        {
            m_Writer.WriteAddressAndTags(address, k_Int64TypeTagBytes);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }

        const uint k_Float64TypeTagBytes = 26668;    // ",h  " 

        /// <summary>Send a message with a single 64-bit integer element</summary>
        public void Send(string address, long element)
        {
            m_Writer.WriteAddressAndTags(address, k_Float64TypeTagBytes);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        const uint k_Color32TypeTagBytes = 29228;    // ",r  " 
        
        /// <summary>Send a message with a single 32-bit color element</summary>
        public void Send(string address, Color32 element)
        {
            m_Writer.WriteAddressAndTags(address, k_Color32TypeTagBytes);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        const uint k_MidiTypeTagBytes = 27948;    // ",m  " 
        
        /// <summary>Send a message with a single MIDI message element</summary>
        public void Send(string address, MidiMessage element)
        {
            m_Writer.WriteAddressAndTags(address, k_MidiTypeTagBytes);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        const uint k_CharTypeTagBytes = 25388;    // ",c  " 

        /// <summary>Send a message with a single ascii character element</summary>
        public void Send(string address, char element)
        {
            m_Writer.WriteAddressAndTags(address, k_CharTypeTagBytes);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }

        const uint k_TrueTypeTagBytes = 21548;    // ",T  " 
        const uint k_FalseTypeTagBytes = 17964;    // ",F  " 

        /// <summary>Send a message with a single True or False tag element</summary>
        public void Send(string address, bool element)
        {
            m_Writer.WriteAddressAndTags(address, element ? k_TrueTypeTagBytes : k_FalseTypeTagBytes);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        const uint k_NilTypeTagBytes = 20012;    // ",N  " 

        /// <summary>Send a message with a single Nil ('N') tag element</summary>
        public void SendNil(string address)
        {
            m_Writer.WriteAddressAndTags(address, k_NilTypeTagBytes);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        const uint k_InfinitumTypeTagBytes = 18732;    // ",I  " 

        /// <summary>Send a message with a single Infinitum ('I') tag element</summary>
        public void SendInfinitum(string address)
        {
            m_Writer.WriteAddressAndTags(address, k_InfinitumTypeTagBytes);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        static unsafe uint[] GetAlignedAsciiBytes(string input)
        {
            var count = Encoding.ASCII.GetByteCount(input);
            var alignedCount = (count + 3) & ~3;
            var bytes = new uint[alignedCount / 4];

            fixed (uint* bPtr = bytes)
            {
                fixed (char* strPtr = input)
                {
                    Encoding.ASCII.GetBytes(strPtr, input.Length, (byte*) bPtr, count);
                }
            }

            return bytes;
        }
    }
}