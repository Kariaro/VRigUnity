using UnityEngine;

namespace uOSC.Samples
{

public class ServerTest : MonoBehaviour
{
    public void OnDataReceived(Message message)
    {
        // address
        var msg = message.address + ": ";

        // timestamp
        msg += "(" + message.timestamp.ToLocalTime() + ") ";

        // values
        foreach (var value in message.values)
        {
            msg += value.GetString() + " ";
        }

        Debug.Log(msg);
    }

    public void OnServerStarted(int port)
    {
        Debug.Log($"<color=blue>Start Server (port: {port})</color>");
    }

    public void OnServerStopped(int port)
    {
        Debug.Log($"<color=blue>Stop Server (port: {port})</color>");
    }
}

}