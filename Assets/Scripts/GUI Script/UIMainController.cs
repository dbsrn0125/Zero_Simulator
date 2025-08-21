using UnityEngine;

public class UIMainController : MonoBehaviour
{
    [Header("Video Panels")]
    [Tooltip("���� �Ʒ� ���� �г��� RosVideoSubscriber")]
    public RosVideoSubscriber missionPanel_Left;
    [Tooltip("������ �Ʒ� ���� �г��� RosVideoSubscriber")]
    public RosVideoSubscriber missionPanel_Right;

    [Header("Main Control Topic")]
    public string main_Left_Topic = "/mission1/video_feed";
    public string main_Right_Topic = "/mission1/debug_feed";

    [Header("AUTO 1 Mission Topic")]
    public string auto1_Left_Topic = "/mission1/video_feed";
    public string auto1_Right_Topic = "/mission1/debug_feed";

    [Header("AUTO 2 Mission Topic")]
    public string auto2_Left_Topic = "/mission2/main_feed";
    public string auto2_Right_Topic = "/mission2/overhead_map";

    [Header("AUTO 3 Mission Topic")]
    public string auto3_Left_Topic = "/marker_lifecycle_node/image_raw/compressed";
    public string auto3_Right_Topic = ""; // ����θ� ���� ȭ������ ǥ��

    [Header("AUTO 4 Mission Topic")]
    public string auto4_Left_Topic = "/marker_lifecycle_node/image_raw/compressed";
    public string auto4_Right_Topic = ""; // ����θ� ���� ȭ������ ǥ��

    [Header("AUTO 5 Mission Topic")]
    public string auto5_Left_Topic = "/marker_lifecycle_node/image_raw/compressed";
    public string auto5_Right_Topic = ""; // ����θ� ���� ȭ������ ǥ��

    // �� ��ư�� ȣ���� ���� �Լ���
    public void OnMainButtonClick()
    {
        Debug.Log("Main �̼� �������� ��ȯ�մϴ�.");
        missionPanel_Left.ChangeTopic(main_Left_Topic);
        missionPanel_Right.ChangeTopic(main_Right_Topic);
    }

    public void OnAuto1ButtonClick()
    {
        Debug.Log("AUTO 1 �̼� �������� ��ȯ�մϴ�.");
        missionPanel_Left.ChangeTopic(auto1_Left_Topic);
        missionPanel_Right.ChangeTopic(auto1_Right_Topic);
    }

    public void OnAuto2ButtonClick()
    {
        Debug.Log("AUTO 2 �̼� �������� ��ȯ�մϴ�.");
        missionPanel_Left.ChangeTopic(auto2_Left_Topic);
        missionPanel_Right.ChangeTopic(auto2_Right_Topic);
    }

    public void OnAuto3ButtonClick()
    {
        Debug.Log("AUTO 3 �̼� �������� ��ȯ�մϴ�.");
        missionPanel_Left.ChangeTopic(auto3_Left_Topic);
        missionPanel_Right.ChangeTopic(auto3_Right_Topic);
    }

    public void OnAuto4ButtonClick()
    {
        Debug.Log("AUTO 4 �̼� �������� ��ȯ�մϴ�.");
        missionPanel_Left.ChangeTopic(auto4_Left_Topic);
        missionPanel_Right.ChangeTopic(auto4_Right_Topic);
    }

    public void OnAuto5ButtonClick()
    {
        Debug.Log("AUTO 5 �̼� �������� ��ȯ�մϴ�.");
        missionPanel_Left.ChangeTopic(auto5_Left_Topic);
        missionPanel_Right.ChangeTopic(auto5_Right_Topic);
    }
}