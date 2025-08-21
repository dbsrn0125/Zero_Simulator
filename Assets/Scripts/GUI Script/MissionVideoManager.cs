using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TopicAssignment
{
    [Tooltip("RosVideoSubscriber Component")]
    public RosVideoSubscriber panel;
    [Tooltip("Ros Topic Name")]
    public string topicName;
}
[System.Serializable]
public class MissionVideoConfiguration
{
    [Tooltip("Mission Name")]
    public string missionName;
    [Tooltip("Button")]
    public Button triggerButton;
    [Tooltip("Topic List")]
    public List<TopicAssignment> topicAssignments;
}
public class MissionVideoManager : MonoBehaviour
{
    [Header("Configuration List")]
    [Tooltip("Add mission Configurations")]
    public List<MissionVideoConfiguration> missionConfigurations;

    private void Awake()
    {
        foreach (var config in missionConfigurations)
        {
            if(config.triggerButton != null)
            {
                config.triggerButton.onClick.AddListener(()=> ActivateMissionConfiguration(config));
            }
            else
            {
                Debug.LogWarning($"'{config.missionName}' 미션 설정에 버튼이 할당되지 않았습니다.");
            }
        }
    }

    public void ActivateMissionConfiguration(MissionVideoConfiguration config)
    {
        Debug.Log($"'Subscribe {config.missionName}'");
        foreach (var assignment in config.topicAssignments)
        {
            if (assignment.panel != null)
            {
                assignment.panel.ChangeTopic(assignment.topicName);
            }
        }
    }
}
