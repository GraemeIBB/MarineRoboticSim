//First off, I know this could be way more optimized. I am literally turning a byte array into a string then back to ints and byte array
//step one is always proof of concept. I leave the refinement to my lackies. 

//TODO:
//test time to decode as is, see if refactoring is worth it
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class messageDecode : MonoBehaviour
{
    private SimpleWebSocket socket;
    private string previousBits;

    void Start()
    {
        socket = GetComponent<SimpleWebSocket>();
        socket.OnBitsChanged += OnBitsChanged;
        previousBits = socket.bits;

        string example = "01-03-00-00-00-07-53-2F-E0-31-06-0C-18-00-01-00-00-12-00-00-00-48-65-6C-6C-6F-20-57-6F-72-6C-64-3A-20-31-37-39-38-00-00-00";
        Decode(example);
    }

    private void OnBitsChanged(string newBits)
    {
        if (newBits != previousBits)
        {
            Decode(newBits);
            previousBits = newBits;
        }
    }

    public void Decode(string bitstream)
    {
        byte[] data = StringToByteArray(bitstream);
        if (data.Length > 1024*16)
        {
            Debug.LogError("Message is too large to be printed");
            return;
        }

        if (data.Length < 1)
        {
            Debug.LogError("Invalid bitstream length.");
            return;
        }

        byte opcode = data[0];
        switch (opcode)
        {
            case 0x01:
                DecodeMessageData(data);
                break;
            case 0x02:
                DecodeTimeSync(data);
                break;
            case 0x03:
                DecodeServiceCallResponse(data);
                break;
            // Add cases for other opcodes here
            default:
                DecodeDefault(data);
                break;
        }
    }

    private void DecodeMessageData(byte[] data)
    {
        if (data.Length < 13)
        {
            Debug.LogError("Invalid bitstream length for Message Data.");
            return;
        }

        uint subscriptionId = BitConverter.ToUInt32(data, 1);
        ulong receiveTimestamp = BitConverter.ToUInt64(data, 5);
        byte[] datapayload = data.Skip(13).ToArray();
        // byte[] topicBytes = data.Skip(14).Take(3).ToArray();  used to get the topic
        // byte[] messageLengthBytes = data.Skip(17).Take(4).ToArray(); used to get the message length,and make method more efficient
        byte[] messagePayloadBytes = data.Skip(21).ToArray();
        string messagePayloadString = Encoding.ASCII.GetString(messagePayloadBytes);

        Debug.Log($"Opcode: {data[0]}");
        Debug.Log($"Subscription ID: {subscriptionId}");
        Debug.Log($"Receive Timestamp: {receiveTimestamp}");
        Debug.Log($"Payload Length: {datapayload.Length}");
        Debug.Log($"full payload: {BitConverter.ToString(data)}");
        Debug.Log($"Raw Payload Bytes: {BitConverter.ToString(messagePayloadBytes)}");
        Debug.Log($"Message Payload: {messagePayloadString}");
    }
    private void DecodeTimeSync(byte[] data)
    {
        if (data.Length < 9)
        {
            Debug.LogError("Invalid bitstream length for Time Sync.");
            return;
        }

        ulong Timestamp = BitConverter.ToUInt64(data, 1);

        Debug.Log($"Opcode: {data[0]}");
        Debug.Log($"Timestamp: {Timestamp}");

    }
    private void DecodeServiceCallResponse(byte[] data)
    {
        // Implement decoding for Service Call Response
        // 1	opcode	0x03
        // 4	uint32	service id
        // 4	uint32	call id
        // 4	uint32	encoding length
        // encoding length	char[]	encoding, same encoding that was used for the request
        // remaining bytes	uint8[]	response payload
    }
    private void DecodeDefault(byte[] data)
    {
        Debug.Log(Encoding.ASCII.GetString(data));
    }

    private byte[] StringToByteArray(string hex)
    {
        hex = hex.Replace("-", "");
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }
}
