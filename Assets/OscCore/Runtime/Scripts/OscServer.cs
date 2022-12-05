using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using BlobHandles;
using UnityEngine;
using UnityEngine.Profiling;

[assembly:InternalsVisibleTo("OscCore.Editor")]

namespace OscCore
{
    public sealed unsafe class OscServer : IDisposable
    {
        // used to allow easy removal of single callbacks
        static readonly Dictionary<Action<OscMessageValues>, OscActionPair> k_SingleCallbackToPair = 
            new Dictionary<Action<OscMessageValues>, OscActionPair>();

        readonly OscSocket m_Socket;
        bool m_Disposed;
        bool m_Started;

        readonly byte[] m_ReadBuffer;
        GCHandle m_BufferHandle;
        byte* m_BufferPtr;
        
        Action[] m_MainThreadQueue = new Action[16];
        int m_MainThreadCount;

        readonly Dictionary<int, string> m_ByteLengthToStringBuffer = new Dictionary<int, string>();
        
        readonly HashSet<MonitorCallback> m_MonitorCallbacks = new HashSet<MonitorCallback>();
        
        readonly List<OscActionPair> m_PatternMatchedMethods = new List<OscActionPair>();
        
        internal bool Running { get; set; }

        /// <summary>
        /// Map from port number to the server that handles incoming messages for it
        /// </summary>
        public static readonly Dictionary<int, OscServer> PortToServer = new Dictionary<int, OscServer>();

        public int Port { get; }
        public OscAddressSpace AddressSpace { get; private set; }
        public OscParser Parser { get; }
        
        public OscServer(int port, int bufferSize = 4096)
        {
            if (PortToServer.ContainsKey(port))
            {
                Debug.LogError($"port {port} is already in use, cannot start a new OSC Server on it");
                return;
            }

            k_SingleCallbackToPair.Clear();
            AddressSpace = new OscAddressSpace();
            
            m_ReadBuffer = new byte[bufferSize];
            m_BufferHandle = GCHandle.Alloc(m_ReadBuffer, GCHandleType.Pinned);
            m_BufferPtr = (byte*) m_BufferHandle.AddrOfPinnedObject();
            Parser = new OscParser(m_ReadBuffer);

            Port = port;
            m_Socket = new OscSocket(port) { Server = this };
            Start();
        }

        public void Start()
        {
            // make sure redundant calls don't do anything after the first
            if (m_Started)
            {
                Running = true;
                return;
            }
            
            m_Socket.Start();
            
            m_Disposed = false;
            m_Started = true;
            Running = true;
        }

        /// <summary>
        /// Get an existing OSC server on the given port, or create one if it doesn't exist.
        /// </summary>
        /// <param name="port">The port to listen for incoming message on</param>
        /// <returns></returns>
        public static OscServer GetOrCreate(int port)
        {
            OscServer server;
            if (!PortToServer.TryGetValue(port, out server))
            {
                server = new OscServer(port);
                PortToServer[port] = server;
            }
            return server;
        }
        
        /// <summary>Dispose of an OSC Server</summary>
        /// <param name="port">The port associated with the server</param>
        /// <returns>True if the server was found and disposed of, false otherwise</returns>
        public static bool Remove(int port)
        {
            OscServer server;
            if (PortToServer.TryGetValue(port, out server))
            {
                server.Dispose();
                return PortToServer.Remove(port);
            }
            return false;
        }
        
        /// <summary>
        /// Register a single background thread method for an OSC address
        /// </summary>
        /// <param name="address">The OSC address to handle messages for</param>
        /// <param name="valueReadMethod">
        /// The method to execute immediately on the worker thread that reads values from the message
        /// </param>
        /// <returns>True if the address was valid, false otherwise</returns>
        public bool TryAddMethod(string address, Action<OscMessageValues> valueReadMethod)
        {
            var pair = new OscActionPair(valueReadMethod);
            k_SingleCallbackToPair.Add(valueReadMethod, pair);
            return AddressSpace.TryAddMethod(address, pair);
        }
        
        /// <summary>
        /// Remove a single background thread method from an OSC address
        /// </summary>
        /// <param name="address">The OSC address to handle messages for</param>
        /// <param name="valueReadMethod">
        /// The method to execute immediately on the worker thread that reads values from the message
        /// </param>
        /// <returns>True if the method was removed from this address, false otherwise</returns>
        public bool RemoveMethod(string address, Action<OscMessageValues> valueReadMethod)
        {
            if (k_SingleCallbackToPair.TryGetValue(valueReadMethod, out var pair))
            {
                return AddressSpace.RemoveMethod(address, pair) && 
                        k_SingleCallbackToPair.Remove(valueReadMethod);
            }

            return false;
        }
        
        /// <summary>Remove an address and all its methods from the server's address space</summary>
        /// <param name="address">The OSC address to handle messages for</param>
        /// <returns>True if the method address was found and removed, false otherwise</returns>
        public bool RemoveAddress(string address)
        {
            return AddressSpace.RemoveAddressMethod(address);
        }

        /// <summary>
        /// Add a background thread read callback and main thread callback associated with an OSC address.
        /// </summary>
        /// <param name="address">The OSC address to associate a method with</param>
        /// <param name="actionPair">The pair of callbacks to add</param>
        /// <returns>True if the address was valid & methods associated with it, false otherwise</returns>
        public bool TryAddMethodPair(string address, OscActionPair actionPair) => 
            AddressSpace.TryAddMethod(address, actionPair);
        
        /// <summary>
        /// Add a background thread read callback and main thread callback associated with an OSC address.
        /// </summary>
        /// <param name="address">The OSC address to associate a method with</param>
        /// <returns>True if the address was valid & methods associated with it, false otherwise</returns>
        public bool TryAddMethodPair(string address, Action<OscMessageValues> read, Action mainThread) => 
            AddressSpace.TryAddMethod(address, new OscActionPair(read, mainThread));
        
        /// <summary>
        /// Remove a background thread read callback and main thread callback associated with an OSC address.
        /// </summary>
        /// <param name="address">The OSC address to remove methods from</param>
        /// <param name="actionPair">The pair of callbacks to remove</param>
        /// <returns>True if successfully removed, false otherwise</returns>
        public bool RemoveMethodPair(string address, OscActionPair actionPair)
        {
            // if the address space is null, this got called during cleanup / shutdown,
            // and effectively all addresses are removed by setting it to null
            return AddressSpace == null || AddressSpace.RemoveMethod(address, actionPair);
        }

        /// <summary>
        /// Add a method to be invoked every time an OSC message is received. If there are any monitor callbacks added,
        /// memory has to be allocated for every message received, so it's recommended to only do this while editing.
        /// </summary>
        /// <param name="callback">The method to invoke</param>
        public void AddMonitorCallback(MonitorCallback callback)
        {
            m_MonitorCallbacks.Add(callback);
        }
        
        /// <summary>Remove a monitor method</summary>
        /// <param name="callback">The method to remove</param>
        public bool RemoveMonitorCallback(MonitorCallback callback)
        {
            return m_MonitorCallbacks.Remove(callback);
        }

        /// <summary>Must be called on the main thread every frame to handle queued events</summary>
        public void Update()
        {
            for (int i = 0; i < m_MainThreadCount; i++)
            {
                m_MainThreadQueue[i]();
            }
            
            m_MainThreadCount = 0;
        }

        /// <summary>
        /// Parse a single OSC message that's been copied into the start of the buffer.
        /// Bundled messages can contain multiple elements
        /// </summary>
        /// <param name="byteLength">The length of the received message</param>
        public void ParseBuffer(int byteLength)
        {
            var bufferPtr = Parser.BufferPtr;
            var bufferLongPtr = Parser.BufferLongPtr;
            var parser = Parser;
            var addressToMethod = AddressSpace.AddressToMethod;
            
            // determine if the message is a bundle or not 
            if (*bufferLongPtr != Constant.BundlePrefixLong)
            {
                // address length here doesn't include the null terminator and alignment padding.
                // this is so we can look up the address by only its content bytes.
                // var addressLength = parser.FindUnalignedAddressLength();
                var addressLength = parser.Parse();
                if (addressLength < 0)
                    return;    // address didn't start with '/'

                // see if we have a method registered for this address
                if (addressToMethod.TryGetValueFromBytes(bufferPtr, addressLength, out var methodPair))
                {
                    HandleCallbacks(methodPair, parser.MessageValues);
                }
                else if (AddressSpace.PatternCount > 0)
                {
                    TryMatchPatterns(parser, bufferPtr, addressLength);
                }

                Profiler.EndSample();
                if (m_MonitorCallbacks.Count > 0)
                    HandleMonitorCallbacks(bufferPtr, addressLength, parser);

                return;
            }

            // the message is a bundle, so we need to recursively scan the bundle elements
            int MessageOffset = 0;
            bool recurse;
            // the outer do-while loop runs once for every #bundle encountered
            do
            {
                // Timestamp isn't used yet, but it will be eventually
                // var time = parser.MessageValues.ReadTimestampIndex(MessageOffset + 8);
                // '#bundle ' + timestamp = 16 bytes
                MessageOffset += 16;
                recurse = false;

                // the inner while loop runs once per bundle element
                while (MessageOffset < byteLength && !recurse)
                {
                    var messageSize = (int) parser.MessageValues.ReadUIntIndex(MessageOffset);
                    var contentIndex = MessageOffset + 4;

                    if (parser.IsBundleTagAtIndex(contentIndex))
                    {
                        // this bundle element's contents are a bundle, break out to the outer loop to scan it
                        MessageOffset = contentIndex;
                        recurse = true;
                        continue;
                    }

                    // parse the actual contents of this bundle element just like a non-bundled message
                    var bundleAddressLength = parser.Parse(contentIndex);
                    if (bundleAddressLength <= 0)
                    {
                        // if an error occured parsing the content, skip this messagse entirely
                        MessageOffset += messageSize + 4;
                        continue;
                    }

                    var contentPtr = bufferPtr + contentIndex;
                    if (addressToMethod.TryGetValueFromBytes(contentPtr, bundleAddressLength, out var bundleMethodPair))
                    {
                        HandleCallbacks(bundleMethodPair, parser.MessageValues);
                    }
                    // if we have no handler for this exact address, we may have a pattern that matches it
                    else if (AddressSpace.PatternCount > 0)
                    {
                        TryMatchPatterns(parser, bufferPtr, bundleAddressLength);
                    }

                    MessageOffset += messageSize + 4;

                    if (m_MonitorCallbacks.Count > 0) 
                        HandleMonitorCallbacks(contentPtr, bundleAddressLength, parser);
                }
            }
            // restart the outer while loop every time a bundle within a bundle is detected
            while (recurse);
        }

        void HandleCallbacks(OscActionPair pair, OscMessageValues messageValues)
        {
            // call the value read method associated with this OSC address    
            pair.ValueRead(messageValues);
            
            // if there's a main thread method, queue it
            if (pair.MainThreadQueued != null)
            {
                if (m_MainThreadCount >= m_MainThreadQueue.Length)
                    Array.Resize(ref m_MainThreadQueue, m_MainThreadCount + 16);

                m_MainThreadQueue[m_MainThreadCount++] = pair.MainThreadQueued;
            }
        }

        void HandleMonitorCallbacks(byte* bufferPtr, int addressLength, OscParser parser)
        {
            // handle monitor callbacks
            var monitorAddressStr = new BlobString(bufferPtr, addressLength);
            foreach (var callback in m_MonitorCallbacks)
                callback(monitorAddressStr, parser.MessageValues);
        }

        void TryMatchPatterns(OscParser parser, byte* bufferPtr, int addressLength)
        {
            // to support OSC address patterns, we test unmatched addresses against regular expressions
            // To do that, we need it as a regular string.  We may be able to mutate a previous string, 
            // instead of always allocating a new one
            if (!m_ByteLengthToStringBuffer.TryGetValue(addressLength, out var stringBuffer))
            {
                stringBuffer = Encoding.ASCII.GetString(bufferPtr, addressLength);
                m_ByteLengthToStringBuffer[addressLength] = stringBuffer;
            }
            else
            {
                // If we've previously received a message of the same byte length, we can re-use it
                OverwriteAsciiString(stringBuffer, bufferPtr);
            }

            if (AddressSpace.TryMatchPatternHandler(stringBuffer, m_PatternMatchedMethods))
            {
                var bufferCopy = string.Copy(stringBuffer);
                AddressSpace.AddressToMethod.Add(bufferCopy, m_PatternMatchedMethods);
                foreach (var matchedMethod in m_PatternMatchedMethods)
                {
                    matchedMethod.ValueRead(parser.MessageValues);
                    m_MainThreadQueue[m_MainThreadCount++] = matchedMethod.MainThreadQueued;
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void OverwriteAsciiString(string str, byte* bufferPtr)
        {
            fixed (char* addressPtr = str)                   
            {
                for (int i = 0; i < str.Length; i++)
                    addressPtr[i] = (char) bufferPtr[i];
            }
        }
        
        public void Dispose()
        {
            PortToServer.Remove(Port);
            
            if (m_Disposed) return;
            m_Disposed = true;

            if(m_BufferHandle.IsAllocated) m_BufferHandle.Free();
            AddressSpace.AddressToMethod.Dispose();
            AddressSpace = null;
            m_Socket.Dispose();
        }

        ~OscServer()
        {
            Dispose();
        }

        public int CountHandlers()
        {
            return AddressSpace?.AddressToMethod.SourceToBlob.Count ?? 0;
        }
    }
}