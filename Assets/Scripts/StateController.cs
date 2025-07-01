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
    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<ChangeStateRequest, ChangeStateResponse>(ChangeStateServiceName);
        launchButton.onClick.AddListener(OnNormalStateChangeClick);
        emergencyButton.onClick.AddListener(OnEmergencyButtonClick);
        statusText.text = "waiting...";
        statusText.color = Color.white;
    }

    private void OnNormalStateChangeClick()
    {
        string selectedState = stateDropdown.options[stateDropdown.value].text;
        RequestStateChange(selectedState);
    }

    private void OnEmergencyButtonClick()
    {
        RequestStateChange("EMERGENCY");
    }
    private void RequestStateChange(string state)
    {
        ChangeStateRequest request = new ChangeStateRequest(state);
        ros.SendServiceMessage<ChangeStateResponse>(ChangeStateServiceName, request, OnServiceResponse);

        statusText.text = "[" + state + "] Requseted!";
        if (state == "EMERGENCY")
        {
            statusText.color = Color.red;
        }
        else
        {
            statusText.color = Color.yellow;
        }
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
