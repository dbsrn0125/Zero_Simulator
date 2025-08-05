using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class RosVideoSubscriber : MonoBehaviour
{
    [Header("ROS Settings")]
    [Tooltip("구독할 ROS 토픽 이름")]
    public string rosTopicName = "/marker_lifecycle_node/image_raw/compressed";
    public string rosTopicName2 = string.Empty;
    [Header("UI Display")]
    [Tooltip("UI RawImage 컴포넌트")]
    public RawImage displayImage;
    [Tooltip("초 단위의 타임아웃 시간")]
    public float timeout = 2.0f;

    // --- Private Variables ---
    private Texture2D receivedTexture;
    private byte[] imageDataBuffer;
    private bool newFrameAvailable = false;
    private readonly object _lock = new object();
    private float lastMessageTime;
    private bool isDisplaying = false;

    // 이 스크립트에서 직접 ROSConnection을 생성하지 않고, Manager로부터 받아옵니다.
    private ROSConnection rosConnection;

    void Start()
    {
        if (displayImage == null)
        {
            Debug.LogError($"'{gameObject.name}'의 'Display Image'가 할당되지 않았습니다!");
            enabled = false;
            return;
        }

        // --- ★★★ 핵심 변경 부분 (시작) ★★★ ---
        // 1. ROSConnectionManager 싱글턴 인스턴스가 있는지 확인합니다.
        if (ROSManager.instance == null)
        {
            Debug.LogError("ROSConnectionManager가 씬에 존재하지 않습니다! RosVideoSubscriber를 사용할 수 없습니다.");
            enabled = false;
            return;
        }

        // 2. Manager로부터 중앙 관리되는 ROSConnection 객체를 받아옵니다.
        rosConnection = ROSManager.instance.ROSConnection;
        // --- ★★★ 핵심 변경 부분 (끝) ★★★ ---

        receivedTexture = new Texture2D(2, 2);
        ClearDisplay();

        // 받아온 rosConnection 객체를 사용하여 토픽을 구독합니다.
        rosConnection.Subscribe<CompressedImageMsg>(rosTopicName, CompressedImageCallback);
        Debug.Log($"'{rosTopicName}' 토픽 구독을 시작합니다.");
    }

    void OnDestroy()
    {
        // 이 오브젝트가 파괴될 때, 리소스 낭비를 막기 위해 구독을 명시적으로 해제합니다.
        if (rosConnection != null)
        {
            rosConnection.Unsubscribe(rosTopicName);
            Debug.Log($"'{rosTopicName}' 토픽 구독을 해제합니다.");
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
        // Unsubscribe와 Subscribe를 할 때, Start에서 받아온 rosConnection을 사용합니다.
        if (rosConnection == null) return;

        if (!string.IsNullOrEmpty(rosTopicName))
        {
            rosConnection.Unsubscribe(rosTopicName);
        }

        ClearDisplay();

        if (!string.IsNullOrEmpty(newTopic))
        {
            rosTopicName = newTopic;
            // rosConnection.RefreshTopicsList(); // 토픽 변경 시에는 보통 필요 없습니다.
            rosConnection.Subscribe<CompressedImageMsg>(rosTopicName, CompressedImageCallback);
            Debug.Log($"새로운 토픽 구독: {rosTopicName}");
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
        displayImage.color = new Color(0, 0, 0, 0); // 투명하게 만들어 확실히 비움
        // Debug.Log($"{gameObject.name} Display cleared.");
    }
}