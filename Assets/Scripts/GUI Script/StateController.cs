using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.ZenithInterfaces;
using Unity.VisualScripting;
public class StateController : MonoBehaviour
{
    public TMP_Dropdown stateDropdown;
    public Button launchButton;
    public Button emergencyButton;
    public TMP_Text statusText;
    private ROSConnection ros;
    private const string ChangeStateServiceName = "/change_state";

    void OnEnable()
    {
        ROSManager.OnRosConnectionReady += InitializeRos;
    }

    // 2. 스크립트가 비활성화/파괴될 때 '구독'을 해제 (매우 중요!)
    //    이것을 안하면 메모리 누수나 오류의 원인이 될 수 있습니다.
    void OnDisable()
    {
        ROSManager.OnRosConnectionReady -= InitializeRos;
    }

    // Start is called before the first frame update
    void Start()
    {
        // 1. ROSConnectionManager 싱글턴 인스턴스가 있는지 확인합니다.
        if (ROSManager.instance == null)
        {
            Debug.LogError("ROSConnectionManager가 씬에 존재하지 않습니다! RosVideoSubscriber를 사용할 수 없습니다.");
            enabled = false;
            return;
        }

        // 2. Manager로부터 중앙 관리되는 ROSConnection 객체를 받아옵니다.
        //ros = ROSManager.instance.ROSConnection;
        //ros.RegisterRosService<ChangeStateRequest, ChangeStateResponse>(ChangeStateServiceName);
        launchButton.onClick.AddListener(OnNormalStateChangeClick);
        //emergencyButton.onClick.AddListener(OnEmergencyButtonClick);
        statusText.text = "waiting...";
        statusText.color = Color.white;
    }

    private void InitializeRos()
    {
        Debug.Log("[StateController] ROS 연결 준비 신호를 받아 서비스를 다시 등록합니다.");

        if (ROSManager.instance != null)
        {
            // 말씀하신 두 줄의 코드를 바로 이 함수 안으로 옮기는 것입니다.
            ros = ROSManager.instance.ROSConnection;
            ros.RegisterRosService<ChangeStateRequest, ChangeStateResponse>(ChangeStateServiceName);

            statusText.text = "Ready.";
            statusText.color = Color.green;
        }
        else
        {
            Debug.LogError("[StateController] ROSManager 인스턴스를 찾을 수 없습니다!");
            statusText.text = "ERROR: ROSManager not found";
            statusText.color = Color.red;
        }
    }

    private void OnNormalStateChangeClick()
    {
        string selectedState = stateDropdown.options[stateDropdown.value].text;
        RequestStateChange(selectedState);
    }

    //private void OnEmergencyButtonClick()
    //{
    //    RequestStateChange("EMERGENCY");
    //}
    private void RequestStateChange(string state)
    {
        ChangeStateRequest request = new ChangeStateRequest(state);
        ros.SendServiceMessage<ChangeStateResponse>(ChangeStateServiceName, request, OnServiceResponse);

        statusText.text = "[" + state + "] Requested!";
        statusText.color = (state == "EMERGENCY") ? Color.red : Color.yellow;
    }
    void OnServiceResponse(ChangeStateResponse response)
    {
        Debug.Log("Service response received: " + response.message);
        statusText.text = response.message;
        statusText.color = response.success ? Color.green : Color.red;
    }
}
