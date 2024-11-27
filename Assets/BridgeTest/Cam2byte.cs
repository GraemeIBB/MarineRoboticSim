using UnityEngine;

public class Cam2byte : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Camera cam;
    public int width = 640;
    public int height = 480;
    public int depthBuffer = 24; //8bits per color
    private RenderTexture rt;
    private Texture2D t2d;

    void Start()
    {
        rt = new RenderTexture(width, height, depthBuffer);
        cam.targetTexture = rt;

        t2d = new Texture2D(width, height, TextureFormat.RGB24, false);
    }

// call every 0.25 seconds to match robot camera framerate
    public byte[] FeedAsByteArray(){
        RenderTexture.active = rt; //ensuring proper rendertexture object
        t2d.ReadPixels(new Rect(0,0,width,height), 0,0);
        t2d.Apply();
        RenderTexture.active = null;


        return t2d.GetRawTextureData();
    }
}
