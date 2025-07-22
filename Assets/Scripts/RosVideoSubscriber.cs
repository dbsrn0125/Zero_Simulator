using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class RosVideoSubscriber : MonoBehaviour
{
    [Header("ROS Settings")]
    [Tooltip("구독할 ROS 토픽 이름")]
    public string rosTopicName = "/marker_lifecycle_node/image_raw/compressed";

    [Header("UI Display")]
    [Tooltip("UI RawImage 컴포넌트")]
    public RawImage displayImage;
    [Tooltip("초 단위의 타임아웃 시간")]
    public float timeout = 2.0f;

    private Texture2D receivedTexture;
    private byte[] imageDataBuffer;
    private bool newFrameAvailable = false;
    private readonly object _lock = new object();
    private float lastMessageTime;
    private bool isDisplaying = false; // 현재 영상이 표시되고 있는지 기억
    private ROSConnection rosConnection;
    void Start()
    {
        if (displayImage == null)
        {
            Debug.LogError($"'Display Image'가 할당되지 않았습니다!");
            enabled = false;
            return;
        }

        receivedTexture = new Texture2D(2, 2);
        ClearDisplay(); // 시작할 때 화면을 깨끗하게 비움
        rosConnection = ROSConnection.GetOrCreateInstance();
        rosConnection.Subscribe<CompressedImageMsg>(rosTopicName, CompressedImageCallback);
        Debug.Log($"'{rosTopicName}' 토픽 구독을 시작합니다.");
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
        // 1. 새 프레임이 도착했는지 확인
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

        // 2. 새 프레임이 있으면 화면에 그림
        if (processThisFrame && currentFrameData != null)
        {
            receivedTexture.LoadImage(currentFrameData);
            if (displayImage.texture == null)
            {
                displayImage.texture = receivedTexture;
            }
            displayImage.color = Color.white; // 이미지가 보이도록 색상 복구
            isDisplaying = true; // '표시 중' 상태로 변경
        }

        // 3. 타임아웃 확인 (영상이 표시 중일 때만)
        if (isDisplaying && (Time.time - lastMessageTime > timeout))
        {
            ClearDisplay(); // 화면을 딱 한 번만 지움
        }
    }
    public void ChangeTopic(string newTopic)
    {
        //// 이미 같은 토픽을 구독 중이면 아무것도 하지 않음
        //if (string.Equals(rosTopicName, newTopic))
        //{
        //    Debug.Log(1);
        //    return;
        //}

        
        // 이전에 구독 중인 토픽이 있었다면 구독 해제
        if (!string.IsNullOrEmpty(rosTopicName))
        {
            rosConnection.Unsubscribe(rosTopicName);
        }

        // 화면을 깨끗하게 정리
        ClearDisplay();

        // 새로운 토픽 이름이 비어있지 않다면, 새로 구독
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
        // ? isDisplaying을 false로 바꿔서 이 함수가 중복 호출되는 것을 방지
        isDisplaying = false;
        displayImage.texture = null;
        Debug.Log($"{gameObject.name}Display cleared.");
    }
}