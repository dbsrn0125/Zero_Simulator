// MissionVideoManager.cs (������ ����)

using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System.Collections.Generic;

// TopicAssignment Ŭ������ �״�� ����ص� �����ϴ�.

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
    // ? 1. ��� ������ ������ �ֽ� �̹��� �����͸� ������ Dictionary
    private Dictionary<string, CompressedImageMsg> latestImages = new Dictionary<string, CompressedImageMsg>();

    void Start()
    {
        ROSConnection ros = ROSManager.instance.ROSConnection;

        // ? 2. ������ ��, ������ �ִ� '���' ������ �ߺ� ���� '�� ����' �����մϴ�.
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
            // �� ���ȿ� ���� �ݹ��� ���. �ݹ��� ���� �����͸� Dictionary�� ���常 ��.
            ros.Subscribe<CompressedImageMsg>(topic, msg => OnImageReceived(topic, msg));
            Debug.Log($"�ʱ� ���� ����: {topic}");
        }

        // --- ���� ��ư ������ ���� ������ �״�� ---
        foreach (var config in missionConfigurations)
        {
            config.triggerButton.onClick.AddListener(() => ActivateMissionConfiguration(config));
        }

        if (missionConfigurations != null && missionConfigurations.Count > 0)
        {
            ActivateMissionConfiguration(missionConfigurations[0]);
        }
    }

    // ? 3. ��� ���� �޽����� �����ϴ� ���� �ݹ� �Լ�
    private void OnImageReceived(string topic, CompressedImageMsg msg)
    {
        // ���� �޽����� ���� �̸��� Ű�� �Ͽ� Dictionary�� ����
        latestImages[topic] = msg;
    }

    // ? 4. ��ư�� ������ �� ����Ǵ� �Լ� (�ڷ�ƾ ���ʿ�)
    public void ActivateMissionConfiguration(MissionVideoConfiguration config)
    {
        Debug.Log($"'{config.missionName}' �̼� Ȱ��ȭ. �ǽð� ������Ʈ�� �����մϴ�.");
        activeMission = config;
    }
    void Update()
    {
        // Ȱ��ȭ�� �̼��� ������ �ƹ��͵� ���� �ʽ��ϴ�.
        if (activeMission == null) return;

        // Ȱ��ȭ�� �̼��� ������ ���� �� �г��� ������ ������Ʈ�մϴ�.
        foreach (var assignment in activeMission.topicAssignments)
        {
            if (assignment.panel != null && latestImages.ContainsKey(assignment.topicName) && IsStreaming)
            {
                // �ش� ������ �ֽ� �̹����� �����ͼ� �гο� ������Ʈ�϶�� '�� ������' ����մϴ�.
                assignment.panel.UpdateImage(latestImages[assignment.topicName]);
            }
            else
            {
                assignment.panel.ClearDisplay();
            }
        }
    }
}