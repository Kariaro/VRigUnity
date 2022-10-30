using UnityEngine;
using uOSC;

[RequireComponent(typeof(uOscServer)),
 RequireComponent(typeof(Renderer))]
public class ServerBlobTest : MonoBehaviour
{
    Texture2D texture_;

    void Start()
    {
        var server = GetComponent<uOscServer>();
        server.onDataReceived.AddListener(OnDataReceived);

        texture_ = new Texture2D(256, 256, TextureFormat.ARGB32, true);

        var renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = texture_;
    }

    void OnDataReceived(Message message)
    {
        if (message.address == "/uOSC/blob")
        {
            var byteTexture = (byte[])message.values[0];
#if UNITY_2017
            ImageConversion.LoadImage(texture_, byteTexture, true);
#else
            texture_.LoadImage(byteTexture);
#endif
        }
    }
}