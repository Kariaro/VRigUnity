using System;

namespace OscCore
{
    /// <summary>
    /// A pair of methods associated with an OSC address
    /// </summary>
    public class OscActionPair
    {
        /// <summary>
        /// A method executed immediately when the a message is received at the associated OSC address, on the server background thread.
        /// All message values must be read during this callback, as the data it points to may be overwritten afterwards.
        /// </summary>
        public readonly Action<OscMessageValues> ValueRead;
        
        /// <summary>
        /// An optional method, which will be queued for execution on the main thread in the next frame after the message was received.
        /// This is useful for UnityEvents and anything that needs a main thread only Unity api.
        /// </summary>
        public readonly Action MainThreadQueued;

        public OscActionPair(Action<OscMessageValues> valueRead, Action mainThreadQueued = null)
        {
            const string nullWarning = "Value read callbacks required!";
            ValueRead = valueRead ?? throw new ArgumentNullException(nameof(valueRead), nullWarning);
            MainThreadQueued = mainThreadQueued;
        }
        
        public static OscActionPair operator + (OscActionPair l, OscActionPair r)
        {
            var mainThread = l.MainThreadQueued == null ? r.MainThreadQueued : l.MainThreadQueued + r.MainThreadQueued;
            var valueRead = l.ValueRead + r.ValueRead;
            return new OscActionPair(valueRead, mainThread);
        }
        
        public static OscActionPair operator - (OscActionPair l, OscActionPair r)
        {
            var mainThread = l.MainThreadQueued == null ? r.MainThreadQueued : l.MainThreadQueued - r.MainThreadQueued;
            var valueRead = l.ValueRead - r.ValueRead;
            return new OscActionPair(valueRead, mainThread);
        }
    }
}