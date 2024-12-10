using UnityEngine;
using System.IO;

public class Cam2byte : MonoBehaviour
{
    public Camera camera;
    private RenderTexture renderTexture;
    private SimpleWebSocket socket;

    void Start()
    {
        socket = GetComponent<SimpleWebSocket>();
        renderTexture = new RenderTexture(640, 480, 24, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB);
        renderTexture.Create();
        camera.targetTexture = renderTexture;

        byte[] bits = CaptureScreenshot();
        SavePNG(bits);
    }

    void FixedUpdate()
    {
        // capture screenshot, send bytes, delete screenshot
    }

    byte[] CaptureScreenshot()
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        camera.Render();

        Texture2D image = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        image.Apply();

        RenderTexture.active = currentRT;

        byte[] bytes = image.EncodeToPNG();
        byte[] bytes2 = image.GetRawTextureData(); //raw pixel data
        Destroy(image);

        return bytes;
    }

    void SavePNG(byte[] bytes)
    {
        string path = Path.Combine(Application.dataPath + "/BridgeTest/Photos", "screenshot.png");
        File.WriteAllBytes(path, bytes);
        Debug.Log($"Screenshot saved to: {path}");
    }
}
