/* This exists to create generated json strings
 that aim to subscribe or publish to topics
 
 as of right now, it is not functional
 */
using System.Text;
using UnityEngine;

public class OperationGen : MonoBehaviour
{
    public enum Operation
    {
        Subscribe,
        Publish
    }
    public Operation pickOperation = Operation.Subscribe;
    
    public string id;
    public string channelId;
    public string topic;
    public string encoding = "cdr";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() //can be made to an onclick
    {
        string Json = @"{
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
        Debug.Log(Json);
    }
    private void subscribeToChatter()
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
        
    }

    
}
