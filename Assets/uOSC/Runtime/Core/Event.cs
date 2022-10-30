using UnityEngine.Events;

namespace uOSC
{

[System.Serializable]
public class ServerStartEvent : UnityEvent<int> {};

[System.Serializable]
public class ServerStopEvent : UnityEvent<int> {};

[System.Serializable]
public class ClientStartEvent : UnityEvent<string, int> {};

[System.Serializable]
public class ClientStopEvent : UnityEvent<string, int> {};

[System.Serializable]
public class DataReceiveEvent : UnityEvent<Message> {};

}
