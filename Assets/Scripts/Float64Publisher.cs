using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;//float64Msg ���
using System;

//std_msgs/Float64 Ÿ���� �޼����� ������ �������� �����ϴ� ���� ������Ʈ
public class Float64Publisher : MonoBehaviour
{
    [Tooltip("������ ROS ���� �̸�")]
    public string topicName = "/default_float_topic";

    [Tooltip("���� ��(Hz)")]
    public float publishRateHz = 30.0f;

    [Tooltip("�����͸� ������ ������Ʈ(IFloatDataProvider �������̽� ���� �ʿ�)")]
    public MonoBehaviour dataSourceProvider;

    private ROSConnection ros;
    private IFloatDataProvider dataProvider;
    private float timeElapsed;
    private Float64Msg message;
    private bool isDataSourceValid = false;

    // Start is called before the first frame update
    void Start()
    {
        //Ros Connection �ν��Ͻ� ��������
        ros = ROSConnection.GetOrCreateInstance();
        
        //������ �ҽ� ������Ʈ���� IFloatDataProvider �������̽� �������� �õ�
        if(dataSourceProvider!=null)
        {
            dataProvider = dataSourceProvider as IFloatDataProvider;
            if(dataProvider==null)
            {
                Debug.LogError($"'{dataSourceProvider.gameObject.name}' GameObject�� '{dataSourceProvider.GetType().Name}' ������Ʈ�� IFloatDataProvider �������̽��� �������� �ʾҽ��ϴ�.", this);
                isDataSourceValid = false;
            }
            else
            {
                isDataSourceValid = true;
            }
        }
        else
        {
            Debug.LogError("DataSorceProvider�� �������� �ʾҽ��ϴ�!", this);
            isDataSourceValid = false;
        }

        //Publisher ���
        ros.RegisterPublisher<Float64Msg>(topicName);

        //�޽��� ��ü �ʱ�ȭ
        message = new Float64Msg();

        //���� �ֱ� ���
        if(publishRateHz<=0)
        {
            Debug.LogWarning("Publish Rate Hz�� 0���� Ŀ���մϴ�. �⺻�� 1Hz�� �����˴ϴ�.", this);
            publishRateHz = 1.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //������ �ҽ��� ��ȿ�ϰ�, ROS�� ����Ǿ� ���� ���� ����
        if (!isDataSourceValid)
            return;
        timeElapsed += Time.deltaTime;

        //������ �ֱ⿡ ���� �޽��� ����
        if(timeElapsed>=(1.0f/publishRateHz))
        {
            PublishData();
            timeElapsed = 0f;
        }
    }

    private void PublishData()
    {
        //�������̽��� ���� ������ ��������
        message.data = dataProvider.GetData();

        //ROS �������� �޽��� ����
        ros.Publish(topicName, message);
    }
}
