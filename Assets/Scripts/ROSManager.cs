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
        // ROSConnection이 할당되지 않았으면 아무것도 하지 않음
        if (ROSConnection == null) return;

        // 현재 연결 상태를 가져옴
        bool isConnected = ROSConnection.HasConnectionThread;

        // "이전에는 연결이 안 됐었는데, 지금은 연결이 된" 순간을 포착!
        if (isConnected && !wasConnected)
        {
            Debug.Log("ROS 연결 상태 변경 감지 (연결됨)! OnRosConnectionReady 이벤트를 방송합니다.");

            // 연결 준비 완료 이벤트를 방송합니다.
            OnRosConnectionReady?.Invoke();
        }

        // 현재 상태를 다음 프레임을 위해 저장합니다.
        wasConnected = isConnected;
    }
    public void OnReconnectButtonClick()
    {
        if (ROSConnection != null)
        {
            Debug.Log("재연결 시도: 기존 연결 해제 후 새 연결 시작");
            ROSConnection.Disconnect(); // Disconnect하면 HasConnectionThread가 false가 됨

            // Disconnect 후 잠시 기다렸다가 Connect를 호출하면 더 안정적입니다.
            // 아래 코루틴을 사용하거나, 그냥 바로 Connect()를 호출해도 됩니다.
            StartCoroutine(DelayedConnect(0.5f));
        }
    }
    private IEnumerator DelayedConnect(float delay)
    {
        yield return new WaitForSeconds(delay);
        ROSConnection.Connect(); // Connect하면 HasConnectionThread가 true가 됨
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
