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
        // ���� ��Ӵٿ�� ���õ� �׸��� �ؽ�Ʈ�� ������
        string selectedState = stateDropdown.options[stateDropdown.value].text;

        // ROS ���� ��û �޽��� ����
        ChangeStateRequest request = new ChangeStateRequest(selectedState);

        // ���� ��û�� ������, ������ ���� OnServiceResponse �Լ��� �����ϵ��� ���
        ros.SendServiceMessage<ChangeStateResponse>(ChangeStateServiceName, request, OnServiceResponse);

        // ��û�� ���´ٰ� UI�� ǥ��
        statusText.text = "[" + selectedState + "] State Change Requested...";
        statusText.color = Color.yellow;
    }
    void OnServiceResponse(ChangeStateResponse response)
    {
        Debug.Log("Service response received: " + response.message);

        // ���� ����� ���� UI �ؽ�Ʈ�� ������ ����
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
