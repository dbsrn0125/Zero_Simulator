using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.PendulumInterfaces;
using System;

//SetpointState �޽����� �����ϴ� ������Ʈ
public class SetPointStatePublisher : MonoBehaviour
{
    [Tooltip("������ ROS ���� �̸�")]
    public string topicName = "/inverted_pendulum/setpoint_state";

    [Tooltip("���� �� (Hz)")]
    public float publishRateHz = 30.0f;

    [Tooltip("�����͸� ������ ������Ʈ (ISetpointStateProvider �������̽� ���� �ʿ�)")]
    public MonoBehaviour dataProviderComponent; // Inspector���� ����

    private ROSConnection ros;
    private ISetpointStateProvider dataProvider; //����� �������̽� Ÿ�� ����
    private SetpointStateMsg message; //����� �޽��� Ÿ�� ����!
    private float timeElapsed;
    private bool isProviderValid = false;
    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        // ������ �ҽ� ������Ʈ Ȯ�� (ISetpointStateProvider ���� Ȯ��)
        if (dataProviderComponent != null)
        {
            dataProvider = dataProviderComponent as ISetpointStateProvider; // �������̽� Ÿ�� ����!
            if (dataProvider == null)
            {
                Debug.LogError($"'{dataProviderComponent.gameObject.name}'�� '{dataProviderComponent.GetType().Name}' ������Ʈ�� ISetpointStateProvider�� �������� �ʾҽ��ϴ�.", this);
            }
            else { isProviderValid = true; }
        }
        else { Debug.LogError("DataProviderComponent�� �������� �ʾҽ��ϴ�!", this); }

        if (isProviderValid)
        {
            ros.RegisterPublisher<SetpointStateMsg>(topicName); // �޽��� Ÿ�� ����!
            message = new SetpointStateMsg();                   // �޽��� Ÿ�� ����!
        }
        if (publishRateHz <= 0) publishRateHz = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        //������ �ҽ��� ��ȿ�� ���� ����
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
