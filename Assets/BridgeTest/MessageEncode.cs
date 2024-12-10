using UnityEngine;

public class MessageEncode : MonoBehaviour //https://github.com/foxglove/ws-protocol/blob/main/docs/spec.md#client-message-data
{
    SimpleWebSocket socket;
    Cam2byte cam;
    string bits;
    

    void Start()
    {
        socket = GetComponent<SimpleWebSocket>();
        cam = GetComponent<Cam2byte>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
