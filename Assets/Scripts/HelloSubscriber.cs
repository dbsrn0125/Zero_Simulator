using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using TMPro;
using System;

public class HelloSubscriber : MonoBehaviour
{
    [Tooltip("������ ROS ���� �̸�")]
    public string topicName = "/hello_unity";

    [Tooltip("���ŵ� �޽����� ǥ���� UI")]
    public TextMeshProUGUI displayText;
    // Start is called before the first frame update
    void Start()
    {
        //ROSConnection �ν��Ͻ��� �����ͼ� ������ ���� ���� ����
        //�޽����� ���ŵǸ� ReceiveMessageCallback �Լ��� ȣ���
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
