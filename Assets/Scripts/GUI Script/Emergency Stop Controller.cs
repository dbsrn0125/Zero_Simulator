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

        // ? ��û ��ü�� CommandRequest�� �����, command �ʵ忡 ���� �־��ݴϴ�.
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

        // ���� ���� �ִ� ��� RosVideoSubscriber ������Ʈ�� ã�Ƽ� �迭�� ����ϴ�.
        RosVideoSubscriber[] allVideoSubscribers = FindObjectsOfType<RosVideoSubscriber>();

        // �迭�� ��� �� ��ũ��Ʈ�� ���� �ݺ��մϴ�.
        foreach (RosVideoSubscriber subscriber in allVideoSubscribers)
        {
            // �� ��ũ��Ʈ�� ClearDisplay() �Լ��� ȣ���մϴ�.
            subscriber.ClearDisplay();
        }
    }
}
