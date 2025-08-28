using RosMessageTypes.ZenithInterfaces; // TriggerRequest/Response
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;

public class EmergencyStopController : MonoBehaviour
{
    public Button emergencyStopButton;
    private ROSConnection ros;

    void Start()
    {
        emergencyStopButton.interactable = false;
        ROSManager.OnRosConnectionReady += Initialize;
    }

    void OnDestroy()
    {
        ROSManager.OnRosConnectionReady -= Initialize;
    }

    private void Initialize()
    {
        ros = ROSManager.instance.ROSConnection;
        ros.RegisterRosService<CommandRequest, CommandResponse>("/zero_stop_launch");

        emergencyStopButton.onClick.AddListener(OnEmergencyStopClick);
        emergencyStopButton.interactable = true;
    }

    private async void OnEmergencyStopClick()
    {
        if (ros == null || !ros.HasConnectionThread) return;

        Debug.LogWarning("EMERGENCY STOP! Sending 'EMERGENCY' command...");

        // ? 요청 객체를 CommandRequest로 만들고, command 필드에 값을 넣어줍니다.
        CommandRequest request = new CommandRequest { command = "EMERGENCY" };

        try
        {
            CommandResponse response = await ros.SendServiceMessage<CommandResponse>("/zero_stop_launch", request);
            Debug.Log($"Server response: [{response.success}] {response.message}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to send command: " + e.Message);
        }

        // 현재 씬에 있는 모든 RosVideoSubscriber 컴포넌트를 찾아서 배열에 담습니다.
        RosVideoSubscriber[] allVideoSubscribers = FindObjectsOfType<RosVideoSubscriber>();

        // 배열에 담긴 각 스크립트에 대해 반복합니다.
        foreach (RosVideoSubscriber subscriber in allVideoSubscribers)
        {
            // 각 스크립트의 ClearDisplay() 함수를 호출합니다.
            subscriber.ClearDisplay();
        }
    }
}
