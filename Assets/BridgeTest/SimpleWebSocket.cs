//By Graeme :)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SimpleWebSocket : MonoBehaviour
{
    public string uri = "ws://100.74.140.75:8765"; //ubuntu laptop
    [Space]
    private string _bits;
    public string bits
    {
        get { return _bits; }
        set
        {
            if (_bits != value)
            {
                _bits = value;
                OnBitsChanged?.Invoke(_bits);
            }
        }
    }

    public event Action<string> OnBitsChanged;
    public ClientWebSocket clientWebSocket;
    private int x = 0;
    
    private async void Start()
    {
        clientWebSocket = new ClientWebSocket();
        
        //this was a b**** to figure out
        clientWebSocket.Options.AddSubProtocol("foxglove.websocket.v1");
 
        try
        {
            Debug.Log($"Connecting to {uri}...");
            await clientWebSocket.ConnectAsync(new Uri(uri), CancellationToken.None);

            if (clientWebSocket.SubProtocol == "foxglove.websocket.v1")
            {
                Debug.Log("Subprotocol negotiated: " + clientWebSocket.SubProtocol);
            }
            else
            {
                Debug.LogError("Subprotocal not negotiated :(");
            }
            _ = Task.Run(() => ReceiveRawData());
            subscribeToChatter();
            advertiseCam();
            advertiseCamCal();
            
        }
        catch (WebSocketException ex)
        {
            Debug.LogError($"WebSocket error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unexpected error: {ex.Message}");
        }
    }
    private async Task ReceiveRawData() //tasks run parallel to other processes
    {
        byte[] buffer = new byte[1024*16]; //arbitrary buffer, made it massive so we dont separate anything by accident
        while (clientWebSocket.State == WebSocketState.Open)
        {
            try
            {
                var result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.CloseStatus.HasValue)
                {
                    Debug.Log("WebSocket closed.");
                    break;
                }

                if (result.Count > 0 && result.Count <= buffer.Length) //safe check for buffer overflow
                {
                    bits = BitConverter.ToString(buffer, 0, result.Count); // Make public var and connect to next script
                    // Debug.Log($"Received raw data{x++}: {bits}"); // dangerous with large data, unity can crash

                    
                }
                else
                {
                    Debug.LogError("Received data count is out of buffer bounds.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while receiving data: {ex.Message}");
            }
        }
    }
    



    private async void subscribeToChatter()
    {
        string subscriptionJson = @"{
        ""op"": ""subscribe"",
        ""subscriptions"": [
            {
            ""id"":3,
            ""channelId"": 3,
            ""topic"": ""/chatter"",
            ""encoding"": ""json""
            }
        ]
        }";
        var bytes = Encoding.UTF8.GetBytes(subscriptionJson);
        await clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
    private async void advertiseCam() //generalize to any topic
    {
        string Json = @"{
            ""op"": ""advertise"",
            ""channels"": [
                {
                    ""id"": 4,
                    ""topic"": ""Unity/camera"",
                    ""encoding"": ""rgb8"",
                    ""schemaName"": ""sensor_msgs/msg/Image""
                }
            ]
        }";

        var bytes = Encoding.UTF8.GetBytes(Json);
        await clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
    private async void advertiseCamCal(){
        string Json = @"{
            ""op"": ""advertise"",
            ""channels"": [
                {
                    ""id"": 5,
                    ""topic"": ""Unity/camera_info"",
                    ""encoding"": ""json"",
                    ""schemaName"": ""sensor_msgs/msg/CameraInfo""
                }
            ]
        }";
        var bytes = Encoding.UTF8.GetBytes(Json);
        await clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
    public async Task sendCam(byte[] bytes)
    {
        DateTime now = DateTime.UtcNow;
        long unixTime = ((DateTimeOffset)now).ToUnixTimeSeconds();
        int nanoseconds = now.Millisecond * 1000000;

        int width = 640;
        int height = 480;
        int step = width * 3; // For rgb8 encoding

        string json = $@"{{
            ""header"": {{
                ""stamp"": {{
                    ""sec"": {unixTime},
                    ""nanosec"": {nanoseconds}
                }},
                ""frame_id"": ""Placeholder""
            }},
            ""height"": {height},
            ""width"": {width},
            ""encoding"": ""rgb8"",
            ""is_bigendian"": 0,
            ""step"": {step},
            ""data"": [{string.Join(", ", bytes)}]
        }}";

        var jsonBytes = Encoding.UTF8.GetBytes(json);
        byte[] sendBytes = new byte[jsonBytes.Length + 5];
        sendBytes[0] = 1; // Opcode for Message Data
        Array.Copy(BitConverter.GetBytes(4), 0, sendBytes, 1, 4); // Channel ID
        Array.Copy(jsonBytes, 0, sendBytes, 5, jsonBytes.Length); // Message payload
        Debug.Log("message packed");
        await clientWebSocket.SendAsync(new ArraySegment<byte>(sendBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Debug.Log("message sent");
    }

    



    private void OnApplicationQuit()
    { //null conditional operator ?. checks if clientWebSocket is not null
        clientWebSocket?.Dispose();
    }
}