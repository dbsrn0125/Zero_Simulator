using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class RosVideoSubscriber : MonoBehaviour
{
    [Header("ROS Settings")]
    [Tooltip("������ ROS ���� �̸�")]
    public string rosTopicName = "/marker_lifecycle_node/image_raw/compressed";

    [Header("UI Display")]
    [Tooltip("UI RawImage ������Ʈ")]
    public RawImage displayImage;
    [Tooltip("�� ������ Ÿ�Ӿƿ� �ð�")]
    public float timeout = 2.0f;

    private Texture2D receivedTexture;
    private byte[] imageDataBuffer;
    private bool newFrameAvailable = false;
    private readonly object _lock = new object();
    private float lastMessageTime;
    private bool isDisplaying = false; // ���� ������ ǥ�õǰ� �ִ��� ���
    private ROSConnection rosConnection;
    void Start()
    {
        if (displayImage == null)
        {
            Debug.LogError($"'Display Image'�� �Ҵ���� �ʾҽ��ϴ�!");
            enabled = false;
            return;
        }

        receivedTexture = new Texture2D(2, 2);
        ClearDisplay(); // ������ �� ȭ���� �����ϰ� ���
        rosConnection = ROSConnection.GetOrCreateInstance();
        rosConnection.Subscribe<CompressedImageMsg>(rosTopicName, CompressedImageCallback);
        Debug.Log($"'{rosTopicName}' ���� ������ �����մϴ�.");
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
        // 1. �� �������� �����ߴ��� Ȯ��
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

        // 2. �� �������� ������ ȭ�鿡 �׸�
        if (processThisFrame && currentFrameData != null)
        {
            receivedTexture.LoadImage(currentFrameData);
            if (displayImage.texture == null)
            {
                displayImage.texture = receivedTexture;
            }
            displayImage.color = Color.white; // �̹����� ���̵��� ���� ����
            isDisplaying = true; // 'ǥ�� ��' ���·� ����
        }

        // 3. Ÿ�Ӿƿ� Ȯ�� (������ ǥ�� ���� ����)
        if (isDisplaying && (Time.time - lastMessageTime > timeout))
        {
            ClearDisplay(); // ȭ���� �� �� ���� ����
        }
    }
    public void ChangeTopic(string newTopic)
    {
        //// �̹� ���� ������ ���� ���̸� �ƹ��͵� ���� ����
        //if (string.Equals(rosTopicName, newTopic))
        //{
        //    Debug.Log(1);
        //    return;
        //}

        
        // ������ ���� ���� ������ �־��ٸ� ���� ����
        if (!string.IsNullOrEmpty(rosTopicName))
        {
            rosConnection.Unsubscribe(rosTopicName);
        }

        // ȭ���� �����ϰ� ����
        ClearDisplay();

        // ���ο� ���� �̸��� ������� �ʴٸ�, ���� ����
        if (!string.IsNullOrEmpty(newTopic))
        {
            rosTopicName = newTopic;
            rosConnection.RefreshTopicsList();
            rosConnection.Subscribe<CompressedImageMsg>(rosTopicName, CompressedImageCallback);
            Debug.Log($"Subscribed to new topic: {rosTopicName}");
        }
        else
        {
            rosTopicName = "";
        }
    }
    private void ClearDisplay()
    {
        // ? isDisplaying�� false�� �ٲ㼭 �� �Լ��� �ߺ� ȣ��Ǵ� ���� ����
        isDisplaying = false;
        displayImage.texture = null;
        Debug.Log($"{gameObject.name}Display cleared.");
    }
}