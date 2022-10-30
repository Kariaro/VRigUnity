using UnityEngine;
using uOSC;

[RequireComponent(typeof(uOscClient))]
public class ClientBlobTest : MonoBehaviour
{
    [SerializeField]
    Texture2D texture;

    byte[] byteTexture_;

    void Start()
    {
#if UNITY_2017
        byteTexture_ = ImageConversion.EncodeToPNG(texture);
#else
        byteTexture_ = texture.EncodeToPNG();
#endif
    }

    void Update()
    {
        var client = GetComponent<uOscClient>();
        client.Send("/uOSC/blob", byteTexture_);
    }
}