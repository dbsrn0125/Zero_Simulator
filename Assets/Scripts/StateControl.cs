using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.ZenithInterfaces;
using Unity.VisualScripting;
public class StateControl : MonoBehaviour
{
    public TMP_Dropdown stateDropdown;
    public Button launchButton;
    public TMP_Text statusText;

    private ROSConnection ros;
    private const string ChangeStateServiceName = "/change_state";
    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<ChangeStateRequest, ChangeStateResponse>(ChangeStateServiceName);
        launchButton.onClick.AddListener(OnButtonClick);
        statusText.text = "waiting...";
        statusText.color = Color.white;
    }


    private void OnButtonClick()
    {
        // 현재 드롭다운에서 선택된 항목의 텍스트를 가져옴
        string selectedState = stateDropdown.options[stateDropdown.value].text;

        // ROS 서비스 요청 메시지 생성
        ChangeStateRequest request = new ChangeStateRequest(selectedState);

        // 서비스 요청을 보내고, 응답이 오면 OnServiceResponse 함수를 실행하도록 등록
        ros.SendServiceMessage<ChangeStateResponse>(ChangeStateServiceName, request, OnServiceResponse);

        // 요청을 보냈다고 UI에 표시
        statusText.text = "[" + selectedState + "] State Change Requested...";
        statusText.color = Color.yellow;
    }
    void OnServiceResponse(ChangeStateResponse response)
    {
        Debug.Log("Service response received: " + response.message);

        // 응답 결과에 따라 UI 텍스트와 색상을 변경
        if (response.success)
        {
            statusText.text = response.message;
            statusText.color = Color.green;
        }
        else
        {
            statusText.text = response.message;
            statusText.color = Color.red;
        }
    }
}
