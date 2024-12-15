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
        await Task.Delay(5000); // Wait for the websocket to connect
        Debug.Log("Starting capture and send loop");
        while (socket.clientWebSocket.State == System.Net.WebSockets.WebSocketState.Open)
        {
            try
            {
                Debug.Log("Taking screenshot");
                byte[] bits = await CaptureScreenshotAsync();
                Debug.Log("Screenshot taken, length: " + (bits != null ? bits.Length.ToString() : "null"));
                await socket.sendCam(bits);
                Debug.Log("Screenshot sent");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in CaptureAndSendLoop: {ex.Message}");
            }
            await Task.Delay(100); // Adjust the delay as needed
        }
        Debug.Log("Ending capture and send loop");
    }

    private Task<byte[]> CaptureScreenshotAsync()
    {
        var tcs = new TaskCompletionSource<byte[]>();
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                byte[] result = CaptureScreenshot();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }

    byte[] CaptureScreenshot()
    {
        try
        {
            Debug.Log("CaptureScreenshot: Starting capture");

            if (renderTexture == null)
            {
                Debug.LogError("CaptureScreenshot: RenderTexture is null");
                return null;
            }

            if (camera == null)
            {
                Debug.LogError("CaptureScreenshot: Camera is null");
                return null;
            }

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = renderTexture;

            Debug.Log("CaptureScreenshot: RenderTexture set");

            camera.Render();
            Debug.Log("CaptureScreenshot: Camera rendered");

            Texture2D image = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            image.Apply();
            Debug.Log("CaptureScreenshot: Image read and applied");

            RenderTexture.active = currentRT;
            Debug.Log("CaptureScreenshot: RenderTexture restored");

            byte[] bytes = image.GetRawTextureData();
            Debug.Log("CaptureScreenshot: Image data obtained");

            Destroy(image);
            Debug.Log("CaptureScreenshot: Image destroyed");

            return bytes;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in CaptureScreenshot: {ex.Message}");
            return null;
        }
    }
}
