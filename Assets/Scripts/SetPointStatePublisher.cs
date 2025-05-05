using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.PendulumInterfaces;
using System;

//SetpointState 메시지를 발행하는 컴포넌트
public class SetPointStatePublisher : MonoBehaviour
{
    [Tooltip("발행할 ROS 토픽 이름")]
    public string topicName = "/inverted_pendulum/setpoint_state";

    [Tooltip("발행 빈도 (Hz)")]
    public float publishRateHz = 30.0f;

    [Tooltip("데이터를 제공할 컴포넌트 (ISetpointStateProvider 인터페이스 구현 필요)")]
    public MonoBehaviour dataProviderComponent; // Inspector에서 연결

    private ROSConnection ros;
    private ISetpointStateProvider dataProvider; //사용할 인터페이스 타입 변경
    private SetpointStateMsg message; //사용할 메시지 타입 변경!
    private float timeElapsed;
    private bool isProviderValid = false;
    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        // 데이터 소스 컴포넌트 확인 (ISetpointStateProvider 구현 확인)
        if (dataProviderComponent != null)
        {
            dataProvider = dataProviderComponent as ISetpointStateProvider; // 인터페이스 타입 변경!
            if (dataProvider == null)
            {
                Debug.LogError($"'{dataProviderComponent.gameObject.name}'의 '{dataProviderComponent.GetType().Name}' 컴포넌트가 ISetpointStateProvider를 구현하지 않았습니다.", this);
            }
            else { isProviderValid = true; }
        }
        else { Debug.LogError("DataProviderComponent가 설정되지 않았습니다!", this); }

        if (isProviderValid)
        {
            ros.RegisterPublisher<SetpointStateMsg>(topicName); // 메시지 타입 변경!
            message = new SetpointStateMsg();                   // 메시지 타입 변경!
        }
        if (publishRateHz <= 0) publishRateHz = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        //데이터 소스가 유효할 때만 실행
        if (!isProviderValid) return;

        timeElapsed += Time.deltaTime;
        if(timeElapsed>=1.0f/publishRateHz)
        {
            PublishState();
            timeElapsed = 0f;
        }
    }

    private void PublishState()
    {
        message.target_angle = dataProvider.GetTargetAngle();
        message.current_angle = dataProvider.GetCurrentAngle();

        ros.Publish(topicName, message);

    }
}
