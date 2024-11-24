using UnityEngine;
using UnityEngine.Rendering;

public class DepthCommandBuffer : MonoBehaviour
{
    public Shader depthShader;
    private Camera cam;
    private Material depthMaterial;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (depthShader)
        {
            depthMaterial = new Material(depthShader);
            CommandBuffer buffer = new CommandBuffer();
            buffer.Blit(null, BuiltinRenderTextureType.CameraTarget, depthMaterial);
            cam.AddCommandBuffer(CameraEvent.AfterEverything, buffer);
        }
    }
}
