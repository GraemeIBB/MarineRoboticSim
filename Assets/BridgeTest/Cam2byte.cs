using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;

public class Cam2byte : MonoBehaviour
{
    public Camera camera;
    private RenderTexture renderTexture;
    private SimpleWebSocket socket;

    void Start()
    {
        renderTexture = new RenderTexture(640, 480, 24, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB);
        renderTexture.Create();
        camera.targetTexture = renderTexture;

        socket = GetComponent<SimpleWebSocket>();

        // Start the screenshot capture and send process in another thread
        Task.Run(() => CaptureAndSendLoop());
    }

    private async Task CaptureAndSendLoop()
    {
        while (true)
        {
            Debug.Log("Taking screenshot");
            byte[] bits = CaptureScreenshot();
            Debug.Log("Screenshot taken");
            await socket.sendCam(bits);
            await Task.Delay(100); // Adjust the delay as needed
            Debug.Log("Screenshot sent");
        }
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

        // byte[] bytes = image.EncodeToPNG();
        byte[] bytes = image.GetRawTextureData();
        Destroy(image);

        return bytes;
    }
}
