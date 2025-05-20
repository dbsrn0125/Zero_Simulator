using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using TMPro;
using System;

public class HelloSubscriber : MonoBehaviour
{
    [Tooltip("구독할 ROS 토픽 이름")]
    public string topicName = "/hello_unity";

    [Tooltip("수신된 메시지를 표시할 UI")]
    public TextMeshProUGUI displayText;
    // Start is called before the first frame update
    void Start()
    {
        //ROSConnection 인스턴스를 가져와서 지정된 토픽 구독 시작
        //메시지가 수신되면 ReceiveMessageCallback 함수가 호출됨
        ROSConnection.GetOrCreateInstance().Subscribe<StringMsg>(topicName, ReceiveMessageCallback);

        Debug.Log($"Started subscription on topic: {topicName}");
    }

    private void ReceiveMessageCallback(StringMsg message)
    {
        Debug.Log($"received from ROS: {message.data}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
