using RosMessageTypes.ZenithInterfaces; // TriggerRequest/Response
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using System.Collections.Generic;

public class EmergencyStopController : MonoBehaviour
{
    public Button emergencyStopButton;
    private ROSConnection ros;
    public List<RosVideoSubscriber> subscribers;
    public MissionVideoManager missionVideoManager;
    public StateController stateController;
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
        foreach (RosVideoSubscriber subscriber in subscribers)
        {
            subscriber.ClearDisplay();
        }
        // ? 요청 객체를 CommandRequest로 만들고, command 필드에 값을 넣어줍니다.
        CommandRequest request = new CommandRequest { command = "EMERGENCY" };
        //try
        //{
        //    string state = "UNKNOWN";
        //    stateController.OnNormalStateChangeClick(state);
        //}
        //catch (System.Exception e)
        //{
        //    Debug.LogError("Failed to send command: " + e.Message);
        //}

        try
        {
            CommandResponse response = await ros.SendServiceMessage<CommandResponse>("/zero_stop_launch", request);
            Debug.Log($"Server response: [{response.success}] {response.message}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to send command: " + e.Message);
        }


        missionVideoManager.StopStreaming();
    }
}
