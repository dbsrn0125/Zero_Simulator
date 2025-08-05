using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class RosVideoSubscriber : MonoBehaviour
{
    [Header("ROS Settings")]
    [Tooltip("������ ROS ���� �̸�")]
    public string rosTopicName = "/marker_lifecycle_node/image_raw/compressed";
    public string rosTopicName2 = string.Empty;
    [Header("UI Display")]
    [Tooltip("UI RawImage ������Ʈ")]
    public RawImage displayImage;
    [Tooltip("�� ������ Ÿ�Ӿƿ� �ð�")]
    public float timeout = 2.0f;

    // --- Private Variables ---
    private Texture2D receivedTexture;
    private byte[] imageDataBuffer;
    private bool newFrameAvailable = false;
    private readonly object _lock = new object();
    private float lastMessageTime;
    private bool isDisplaying = false;

    // �� ��ũ��Ʈ���� ���� ROSConnection�� �������� �ʰ�, Manager�κ��� �޾ƿɴϴ�.
    private ROSConnection rosConnection;

    void Start()
    {
        if (displayImage == null)
        {
            Debug.LogError($"'{gameObject.name}'�� 'Display Image'�� �Ҵ���� �ʾҽ��ϴ�!");
            enabled = false;
            return;
        }

        // --- �ڡڡ� �ٽ� ���� �κ� (����) �ڡڡ� ---
        // 1. ROSConnectionManager �̱��� �ν��Ͻ��� �ִ��� Ȯ���մϴ�.
        if (ROSManager.instance == null)
        {
            Debug.LogError("ROSConnectionManager�� ���� �������� �ʽ��ϴ�! RosVideoSubscriber�� ����� �� �����ϴ�.");
            enabled = false;
            return;
        }

        // 2. Manager�κ��� �߾� �����Ǵ� ROSConnection ��ü�� �޾ƿɴϴ�.
        rosConnection = ROSManager.instance.ROSConnection;
        // --- �ڡڡ� �ٽ� ���� �κ� (��) �ڡڡ� ---

        receivedTexture = new Texture2D(2, 2);
        ClearDisplay();

        // �޾ƿ� rosConnection ��ü�� ����Ͽ� ������ �����մϴ�.
        rosConnection.Subscribe<CompressedImageMsg>(rosTopicName, CompressedImageCallback);
        Debug.Log($"'{rosTopicName}' ���� ������ �����մϴ�.");
    }

    void OnDestroy()
    {
        // �� ������Ʈ�� �ı��� ��, ���ҽ� ���� ���� ���� ������ ��������� �����մϴ�.
        if (rosConnection != null)
        {
            rosConnection.Unsubscribe(rosTopicName);
            Debug.Log($"'{rosTopicName}' ���� ������ �����մϴ�.");
        }
    }

    void CompressedImageCallback(CompressedImageMsg msg)
    {
        lock (_lock)
        {
            imageDataBuffer = msg.data;
            newFrameAvailable = true;
            lastMessageTime = Time.time;
        }
    }

    void Update()
    {
        bool processThisFrame = false;
        byte[] currentFrameData = null;
        lock (_lock)
        {
            if (newFrameAvailable)
            {
                processThisFrame = true;
                currentFrameData = imageDataBuffer;
                newFrameAvailable = false;
            }
        }

        if (processThisFrame && currentFrameData != null)
        {
            receivedTexture.LoadImage(currentFrameData);
            if (displayImage.texture == null)
            {
                displayImage.texture = receivedTexture;
            }
            displayImage.color = Color.white;
            isDisplaying = true;
        }

        if (isDisplaying && (Time.time - lastMessageTime > timeout))
        {
            ClearDisplay();
        }
    }

    public void ChangeTopic(string newTopic)
    {
        // Unsubscribe�� Subscribe�� �� ��, Start���� �޾ƿ� rosConnection�� ����մϴ�.
        if (rosConnection == null) return;

        if (!string.IsNullOrEmpty(rosTopicName))
        {
            rosConnection.Unsubscribe(rosTopicName);
        }

        ClearDisplay();

        if (!string.IsNullOrEmpty(newTopic))
        {
            rosTopicName = newTopic;
            // rosConnection.RefreshTopicsList(); // ���� ���� �ÿ��� ���� �ʿ� �����ϴ�.
            rosConnection.Subscribe<CompressedImageMsg>(rosTopicName, CompressedImageCallback);
            Debug.Log($"���ο� ���� ����: {rosTopicName}");
        }
        else
        {
            rosTopicName = "";
        }
    }

    private void ClearDisplay()
    {
        isDisplaying = false;
        displayImage.texture = null;
        displayImage.color = new Color(0, 0, 0, 0); // �����ϰ� ����� Ȯ���� ���
        // Debug.Log($"{gameObject.name} Display cleared.");
    }
}