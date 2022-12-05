using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OscCore
{
    public sealed class OscAddressSpace
    {
        const int k_DefaultPatternCapacity = 8;
        const int k_DefaultCapacity = 16;

        internal readonly OscAddressMethods AddressToMethod;
        
        // Keep a list of registered address patterns and the methods they're associated with just like addresses
        internal int PatternCount;
        internal Regex[] Patterns = new Regex[k_DefaultPatternCapacity];
        internal OscActionPair[] PatternMethods = new OscActionPair[k_DefaultPatternCapacity];
        
        readonly Queue<int> FreedPatternIndices = new Queue<int>();
        readonly Dictionary<string, int> PatternStringToIndex = new Dictionary<string, int>();

        public int HandlerCount => AddressToMethod.HandleToValue.Count;

        public IEnumerable<string> Addresses => AddressToMethod.SourceToBlob.Keys;

        public OscAddressSpace(int startingCapacity = k_DefaultCapacity)
        {
            AddressToMethod = new OscAddressMethods(startingCapacity);
        }

        public bool TryAddMethod(string address, OscActionPair onReceived)
        {
            if (string.IsNullOrEmpty(address) || onReceived == null) 
                return false;

            switch (OscParser.GetAddressType(address))
            {    
                case AddressType.Address:
                    AddressToMethod.Add(address, onReceived);
                    return true;
                case AddressType.Pattern:
                    int index;
                    // if a method has already been registered for this pattern, add the new delegate
                    if (PatternStringToIndex.TryGetValue(address, out index))
                    {
                        PatternMethods[index] += onReceived;
                        return true;
                    }

                    if (FreedPatternIndices.Count > 0)
                    {
                        index = FreedPatternIndices.Dequeue();
                    }
                    else
                    {
                        index = PatternCount;
                        if (index >= Patterns.Length)
                        {
                            var newSize = Patterns.Length * 2;
                            Array.Resize(ref Patterns, newSize);
                            Array.Resize(ref PatternMethods, newSize);
                        }
                    }

                    Patterns[index] = new Regex(address);
                    PatternMethods[index] = onReceived;
                    PatternStringToIndex[address] = index;
                    PatternCount++;
                    return true;
                default: 
                    return false;
            }
        }

        public bool RemoveAddressMethod(string address)
        {
            if (string.IsNullOrEmpty(address))
                return false;

            switch (OscParser.GetAddressType(address))
            {
                case AddressType.Address:
                    return AddressToMethod.RemoveAddress(address);
                default:
                    return false;
            }
        }

        public bool RemoveMethod(string address, OscActionPair onReceived)
        {
            if (string.IsNullOrEmpty(address) || onReceived == null) 
                return false;

            switch (OscParser.GetAddressType(address))
            {    
                case AddressType.Address:
                    return AddressToMethod.Remove(address, onReceived);
                case AddressType.Pattern:
                    if (!PatternStringToIndex.TryGetValue(address, out var patternIndex))
                        return false;

                    var method = PatternMethods[patternIndex].ValueRead;
                    if (method.GetInvocationList().Length == 1)
                    {
                        Patterns[patternIndex] = null;
                        PatternMethods[patternIndex] = null;
                    }
                    else
                    {
                        PatternMethods[patternIndex] -= onReceived;
                    }

                    PatternCount--;
                    FreedPatternIndices.Enqueue(patternIndex);
                    return PatternStringToIndex.Remove(address);
                default: 
                    return false;
            }
        }

        /// <summary>
        /// Try to match an address against all known address patterns,
        /// and add a handler for the address if a pattern is matched
        /// </summary>
        /// <param name="address">The address to match</param>
        /// <param name="allMatchedMethods"></param>
        /// <returns>True if a match was found, false otherwise</returns>
        public bool TryMatchPatternHandler(string address, List<OscActionPair> allMatchedMethods)
        {
            if (!OscParser.AddressIsValid(address))
                return false;
            
            allMatchedMethods.Clear();

            bool any = false;
            for (var i = 0; i < PatternCount; i++)
            {
                if (Patterns[i].IsMatch(address))
                {
                    var handler = PatternMethods[i];
                    AddressToMethod.Add(address, handler);
                    any = true;
                }
            }

            return any;
        }
    }
}

