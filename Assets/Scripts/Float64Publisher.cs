using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;//float64Msg 사용
using System;

//std_msgs/Float64 타입의 메세지를 지정된 토픽으로 발행하는 범용 컴포넌트
public class Float64Publisher : MonoBehaviour
{
    [Tooltip("발행할 ROS 토픽 이름")]
    public string topicName = "/default_float_topic";

    [Tooltip("발행 빈도(Hz)")]
    public float publishRateHz = 30.0f;

    [Tooltip("데이터를 제공할 컴포넌트(IFloatDataProvider 인터페이스 구현 필요)")]
    public MonoBehaviour dataSourceProvider;

    private ROSConnection ros;
    private IFloatDataProvider dataProvider;
    private float timeElapsed;
    private Float64Msg message;
    private bool isDataSourceValid = false;

    // Start is called before the first frame update
    void Start()
    {
        //Ros Connection 인스턴스 가져오기
        ros = ROSConnection.GetOrCreateInstance();
        
        //데이터 소스 컴포넌트에서 IFloatDataProvider 인터페이스 가져오기 시도
        if(dataSourceProvider!=null)
        {
            dataProvider = dataSourceProvider as IFloatDataProvider;
            if(dataProvider==null)
            {
                Debug.LogError($"'{dataSourceProvider.gameObject.name}' GameObject의 '{dataSourceProvider.GetType().Name}' 컴포넌트가 IFloatDataProvider 인터페이스를 구현하지 않았습니다.", this);
                isDataSourceValid = false;
            }
            else
            {
                isDataSourceValid = true;
            }
        }
        else
        {
            Debug.LogError("DataSorceProvider가 설정되지 않았습니다!", this);
            isDataSourceValid = false;
        }

        //Publisher 등록
        ros.RegisterPublisher<Float64Msg>(topicName);

        //메시지 객체 초기화
        message = new Float64Msg();

        //발행 주기 계산
        if(publishRateHz<=0)
        {
            Debug.LogWarning("Publish Rate Hz는 0보다 커야합니다. 기본값 1Hz로 설정됩니다.", this);
            publishRateHz = 1.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //데이터 소스가 유효하고, ROS에 연결되어 있을 때만 실행
        if (!isDataSourceValid)
            return;
        timeElapsed += Time.deltaTime;

        //설정된 주기에 맞춰 메시지 발행
        if(timeElapsed>=(1.0f/publishRateHz))
        {
            PublishData();
            timeElapsed = 0f;
        }
    }

    private void PublishData()
    {
        //인터페이스를 통해 데이터 가져오기
        message.data = dataProvider.GetData();

        //ROS 토픽으로 메시지 발행
        ros.Publish(topicName, message);
    }
}
