using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using BlobHandles;

namespace OscCore
{
    static class Utils
    {
        static readonly List<char> k_TempChars = new List<char>();
        static readonly StringBuilder k_Builder = new StringBuilder();

        public static bool ValidateAddress(ref string address)
        {
            if(string.IsNullOrEmpty(address)) 
                address = "/";
            if(address[0] != '/') address = 
                address.Insert(0, "/");
            if(address.EndsWith(" "))
                address = address.TrimEnd(' ');

            address = ReplaceInvalidAddressCharacters(address);
            return true;
        }
        
        internal static string ReplaceInvalidAddressCharacters(string address)
        {
            k_TempChars.Clear();
            k_TempChars.AddRange(address.Where(OscParser.CharacterIsValidInAddress));
            return new string(k_TempChars.ToArray());
        }

        public static unsafe TPtr* PinPtr<TData, TPtr>(TData[] array, out GCHandle handle) 
            where TData: unmanaged
            where TPtr : unmanaged
        {
            handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            return (TPtr*) handle.AddrOfPinnedObject();
        }
        
        internal static string GetLocalIpAddress()
        {
            string localIP = "unknown";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
        
        public static string MonitorMessageToString(BlobString address, OscMessageValues values)
        {
            k_Builder.Clear();
            k_Builder.Append(address.ToString());
            const string divider = "  ,";
            k_Builder.Append(divider);
            values.ForEachElement((i, type) => { k_Builder.Append((char)type); });
            k_Builder.Append("  ");

            var lastIndex = values.ElementCount - 1;
            values.ForEachElement((i, type) =>
            {
                var elementText = values.ReadStringElement(i);
                k_Builder.Append(elementText);
                if(i != lastIndex) k_Builder.Append(' ');
            });

            return k_Builder.ToString();
        }

    }
}