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
    public string bits;
    public ClientWebSocket clientWebSocket;
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

                
                bits = BitConverter.ToString(buffer, 0, result.Count); //make public var and connect to next script
                Debug.Log($"Received raw data{x++}: {bits}");

                
                Decode(bits);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while receiving data: {ex.Message}");
                break;
            }
        }
    }
    private void Decode(String bits){ //message is in little endian
        // int subId;
        
        char opcode = bits.Substring(1,2)[0];
        
        if(opcode == '1'){
            
            string[] hexvaluesarr = bits.Substring(2).Split('-'); //all bits after opcode
            List<string> hexvalues = hexvaluesarr.ToList();
            
            List<int> time = new List<int>();
            int subID = 0;
            for(int i = 3; i >= 0; i--){ //iterate through first 4 bytes for subscription id
            subID+=Convert.ToInt32(hexvalues[i], 16); //hopefully that turns it into base 10, throws error if i specify base 10
            
            
            }
            hexvalues.RemoveRange(0,4);
            
            //iterate through next section (8 bytes) of message (time - nanoseconds)
            for(int i = 7; i >= 0; i--){ 
            time.Add(Convert.ToInt32(hexvalues[i], 16));
            }
            hexvalues.RemoveRange(0,8);
            
            //iterate through next section of message (payload)
            Debug.Log(hexvalues.ToString());
            List<byte> byteArray = new List<byte>();
            foreach(string hex in hexvalues)
            {
                byteArray.Add(Convert.ToByte(hex, 16)); // Convert each hex string to a byte
                Debug.Log("fixed");
            }
            string message = Encoding.ASCII.GetString(byteArray.ToArray());
            Debug.Log(subID);
            Debug.Log(time.ToString());
            Debug.Log(message);
            
        }

        // byte[] bytes = Encoding.UTF8.GetBytes(bits);
        //very very very helpful for this: https://github.com/foxglove/ws-protocol/blob/main/docs/spec.md#binary-messages
        

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

    


    private void OnApplicationQuit()
    { //null conditional operator ?. checks if clientWebSocket is not null
        clientWebSocket?.Dispose();
    }
}