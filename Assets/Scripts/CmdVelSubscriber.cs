using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.ZeroInterfaces;

public class CmdVelSubscriber : MonoBehaviour
{
    [Tooltip("구독할 ROS 토픽 이름")]
    public string topicName = string.Empty;
    // Start is called before the first frame update
    void Start()
    {
        ROSConnection ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<CmdVelMsg>(topicName, Callback);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Callback(CmdVelMsg message)
    {
        Debug.Log(message);
    }
}
