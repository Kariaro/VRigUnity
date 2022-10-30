using UnityEngine;
using uOSC;

[RequireComponent(typeof(uOscClient))]
public class ClientTest : MonoBehaviour
{
    void Update()
    {
        var client = GetComponent<uOscClient>();
        client.Send("/uOSC/test", 10, "hoge", "hogehoge", 1.234f, 123f, true, false);
    }

    public void OnClientStarted(string address, int port)
    {
        Debug.Log($"<color=red>Start Client (address: {address}, port: {port})</color>");
    }

    public void OnClientStopped(string address, int port)
    {
        Debug.Log($"<color=red>Stop Client (address: {address}, port: {port})</color>");
    }
}