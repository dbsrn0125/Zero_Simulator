// MissionVideoManager.cs (수정된 버전)

using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System.Collections.Generic;

// TopicAssignment 클래스는 그대로 사용해도 좋습니다.

[System.Serializable]
public class MissionVideoConfiguration
{
    public string missionName;
    public Button triggerButton;
    public List<TopicAssignment> topicAssignments;
}

[System.Serializable]
public class TopicAssignment
{
    public RosVideoSubscriber panel;
    public string topicName;
}
public class MissionVideoManager : MonoBehaviour
{
    public List<MissionVideoConfiguration> missionConfigurations;
    private MissionVideoConfiguration activeMission = null;
    public bool IsStreaming = false;
    // ? 1. 모든 가능한 토픽의 최신 이미지 데이터를 저장할 Dictionary
    private Dictionary<string, CompressedImageMsg> latestImages = new Dictionary<string, CompressedImageMsg>();

    void Start()
    {
        ROSConnection ros = ROSManager.instance.ROSConnection;

        // ? 2. 시작할 때, 설정에 있는 '모든' 토픽을 중복 없이 '한 번만' 구독합니다.
        HashSet<string> allTopics = new HashSet<string>();
        foreach (var config in missionConfigurations)
        {
            foreach (var assignment in config.topicAssignments)
            {
                allTopics.Add(assignment.topicName);
            }
        }

        foreach (string topic in allTopics)
        {
            // 각 토픽에 대한 콜백을 등록. 콜백은 받은 데이터를 Dictionary에 저장만 함.
            ros.Subscribe<CompressedImageMsg>(topic, msg => OnImageReceived(topic, msg));
            Debug.Log($"초기 구독 설정: {topic}");
        }

        // --- 기존 버튼 리스너 연결 로직은 그대로 ---
        foreach (var config in missionConfigurations)
        {
            config.triggerButton.onClick.AddListener(() => ActivateMissionConfiguration(config));
        }

        if (missionConfigurations != null && missionConfigurations.Count > 0)
        {
            ActivateMissionConfiguration(missionConfigurations[0]);
        }
    }

    // ? 3. 모든 토픽 메시지를 수신하는 단일 콜백 함수
    private void OnImageReceived(string topic, CompressedImageMsg msg)
    {
        // 받은 메시지를 토픽 이름을 키로 하여 Dictionary에 저장
        latestImages[topic] = msg;
    }

    // ? 4. 버튼을 눌렀을 때 실행되는 함수 (코루틴 불필요)
    public void ActivateMissionConfiguration(MissionVideoConfiguration config)
    {
        Debug.Log($"'{config.missionName}' 미션 활성화. 실시간 업데이트를 시작합니다.");
        activeMission = config;
    }
    void Update()
    {
        // 활성화된 미션이 없으면 아무것도 하지 않습니다.
        if (activeMission == null) return;

        // 활성화된 미션의 설정에 따라 각 패널의 영상을 업데이트합니다.
        foreach (var assignment in activeMission.topicAssignments)
        {
            if (assignment.panel != null && latestImages.ContainsKey(assignment.topicName) && IsStreaming)
            {
                // 해당 토픽의 최신 이미지를 가져와서 패널에 업데이트하라고 '매 프레임' 명령합니다.
                assignment.panel.UpdateImage(latestImages[assignment.topicName]);
            }
            else
            {
                assignment.panel.ClearDisplay();
            }
        }
    }
}