//By Graeme :)

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SimpleWebSocket : MonoBehaviour
{
    public string uri = "ws://100.74.140.75:8765"; //ubuntu laptop
    [Space]
    private ClientWebSocket clientWebSocket;
    private int x = 0;
    private async void Start()
    {
        clientWebSocket = new ClientWebSocket();
        
        //this was a bitch to figure out
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
        byte[] buffer = new byte[1024*64]; //arbitrary buffer, made it massive so we dont separate anything by accident
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

                
                string bits = BitConverter.ToString(buffer, 0, result.Count);
                Debug.Log($"Received raw data{x++}: {bits}");

                
                string message = ConvertHexToString(bits);
                Debug.Log($"Interpreted as text: {message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while receiving data: {ex.Message}");
                break;
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
            ""encoding"": ""cdr""
            }
        ]
        }";
        var bytes = Encoding.UTF8.GetBytes(subscriptionJson);
        await clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private string ConvertHexToString(string hex)
    {
        
        hex = hex.Replace("-", "").Replace(" ", "");


        // Convert hex string to byte array
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        // Convert byte array to string (UTF-8 encoding)
        string result = Encoding.UTF8.GetString(bytes);
        return result;
    }


    private void OnApplicationQuit()
    { //null conditional operator ?. checks if clientWebSocket is not null
        clientWebSocket?.Dispose();
    }
}