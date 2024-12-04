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
    SimpleWebSocket socket;
    string bits;
    string bits2;

    void Start()
    {
        socket = GetComponent<SimpleWebSocket>();
        bits = socket.bits;
        // string example = "01-03-00-00-00-07-53-2F-E0-31-06-0C-18-00-01-00-00-12-00-00-00-48-65-6C-6C-6F-20-57-6F-72-6C-64-3A-20-31-37-39-38-00-00-00";
        try
        {
            // _ = Task.Run(() => Decode());
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unexpected error: {ex.Message}");
        }
    }
    

    private void Decode(){ //message is in little endian
        // int subId;
        Debug.Log("called");
        char opcode = bits.Substring(1,2)[0];
        
        if(opcode == '1'){
            string[] hexvaluesarr = bits.Substring(2).Split('-'); //all bits after opcode
        List<string> hexvalues = hexvaluesarr.ToList();
        
        List<int> time = new List<int>();
        int subID = 0;
        for(int i = 3; i > 0; i--){ //iterate through first 4 bytes for subscription id
        subID+=Convert.ToInt32(Math.Pow(Convert.ToInt32(hexvalues[i], 16),i)); //hopefully that turns it into base 10, throws error if i specify base 10
        
        hexvalues.RemoveAt(i);
        }

        //iterate through next section (8 bytes) of message (time - nanoseconds)
        for(int i = 7; i > 0; i--){ 
        time.Add(Convert.ToInt32(hexvalues[i], 16));
        hexvalues.RemoveAt(i);
        }
        //iterate through next section of message (payload)
        byte[] byteArray = new byte[hexvalues.Count];
        for (int i = 0; i < hexvalues.Count; i++)
        {
            byteArray[i] = Convert.ToByte(hexvalues[i], 16); // Convert each hex string to a byte
        }
        string message = Encoding.ASCII.GetString(byteArray);
        Debug.Log(subID);
        Debug.Log(time.ToString());
        Debug.Log(message);
            
        }

        // byte[] bytes = Encoding.UTF8.GetBytes(bits);
        //very very very helpful for this: https://github.com/foxglove/ws-protocol/blob/main/docs/spec.md#binary-messages
        

    }
    
}
