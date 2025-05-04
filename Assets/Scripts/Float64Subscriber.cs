using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using System;

//Float64 Ÿ�� �޽����� �����ϰ� UnityEvent�� ���� �ܺο� ������ ����
public class Float64Subscriber : MonoBehaviour
{
    [Tooltip("������ ROS ���� �̸�")]
    public string topicName = "/default_float64_topic";

    //Float64 �޽����� �޾��� �� ȣ��� Unity Event(double Ÿ�� ������ ����)
    //Instpector â�� ����Ǿ� �ٸ� �Լ��� ������ ��ٸ�
    public UnityEvent<double> OnMeesageReceived;
    // Start is called before the first frame update
    void Start()
    {
        ROSConnection.GetOrCreateInstance().Subscribe<Float64Msg>(topicName, Callback);
        Debug.Log($"Started subcription on topic: {topicName}");
    }

    private void Callback(Float64Msg message)
    {
        //��ϵ� ������(�Լ�)�鿡�� ���ŵ� ������(message.data)�� �����ϸ� �̺�Ʈ ȣ��
        OnMeesageReceived.Invoke(message.data);
    }
}
