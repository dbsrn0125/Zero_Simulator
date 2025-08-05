using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System;

public class ROSManager : MonoBehaviour
{
    public static ROSManager instance = null;
    public ROSConnection ROSConnection;
    public static event Action OnRosConnectionReady;
    private bool wasConnected = false;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            ROSConnection = GetComponent<ROSConnection>();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        // ROSConnection�� �Ҵ���� �ʾ����� �ƹ��͵� ���� ����
        if (ROSConnection == null) return;

        // ���� ���� ���¸� ������
        bool isConnected = ROSConnection.HasConnectionThread;

        // "�������� ������ �� �ƾ��µ�, ������ ������ ��" ������ ����!
        if (isConnected && !wasConnected)
        {
            Debug.Log("ROS ���� ���� ���� ���� (�����)! OnRosConnectionReady �̺�Ʈ�� ����մϴ�.");

            // ���� �غ� �Ϸ� �̺�Ʈ�� ����մϴ�.
            OnRosConnectionReady?.Invoke();
        }

        // ���� ���¸� ���� �������� ���� �����մϴ�.
        wasConnected = isConnected;
    }
    public void OnReconnectButtonClick()
    {
        if (ROSConnection != null)
        {
            Debug.Log("�翬�� �õ�: ���� ���� ���� �� �� ���� ����");
            ROSConnection.Disconnect(); // Disconnect�ϸ� HasConnectionThread�� false�� ��

            // Disconnect �� ��� ��ٷȴٰ� Connect�� ȣ���ϸ� �� �������Դϴ�.
            // �Ʒ� �ڷ�ƾ�� ����ϰų�, �׳� �ٷ� Connect()�� ȣ���ص� �˴ϴ�.
            StartCoroutine(DelayedConnect(0.5f));
        }
    }
    private IEnumerator DelayedConnect(float delay)
    {
        yield return new WaitForSeconds(delay);
        ROSConnection.Connect(); // Connect�ϸ� HasConnectionThread�� true�� ��
    }

    void OnApplicationQuit()
    {
        ROSConnection.Disconnect();
    }

    void OnDestroy()
    {
        Debug.Log("OnDestroy called. Disconnecting from ROS...");
        ROSConnection.Disconnect();
    }
}
