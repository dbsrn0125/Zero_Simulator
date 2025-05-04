using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using System;

//Float64 타입 메시지를 구독하고 UnityEvent를 통해 외부에 데이터 전달
public class Float64Subscriber : MonoBehaviour
{
    [Tooltip("구독할 ROS 토픽 이름")]
    public string topicName = "/default_float64_topic";

    //Float64 메시지를 받았을 때 호출될 Unity Event(double 타입 데이터 전달)
    //Instpector 창에 노출되어 다른 함수의 연결을 기다림
    public UnityEvent<double> OnMeesageReceived;
    // Start is called before the first frame update
    void Start()
    {
        ROSConnection.GetOrCreateInstance().Subscribe<Float64Msg>(topicName, Callback);
        Debug.Log($"Started subcription on topic: {topicName}");
    }

    private void Callback(Float64Msg message)
    {
        //등록된 리스너(함수)들에게 수신된 데이터(message.data)를 전달하며 이벤트 호출
        OnMeesageReceived.Invoke(message.data);
    }
}
