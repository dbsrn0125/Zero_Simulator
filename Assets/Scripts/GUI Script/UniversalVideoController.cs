using UnityEngine;
using UnityEngine.UI;
using RosMessageTypes;

public class UniversalVideoController : MonoBehaviour
{
    public enum VideoSourceType { LocalWebcam, ROS2 }
    [Header("Source Selection")]
    [Tooltip("����� ���� �ҽ��� �����մϴ�.")]
    public VideoSourceType currentSource = VideoSourceType.LocalWebcam;

    [Header("UI")]
    [Tooltip("������ ǥ���� UI RawImage ������Ʈ�� ���⿡ �Ҵ��ϼ���.")]
    public RawImage displayImage;

    [Header("Webcam Settings (LocalWebcam ���� ��)")]
    public int requestedWebcamWidth = 640;
    public int requestedWebcamHeight = 480;
    public int requestedWebcamFPS = 30;

    // --- ���� ���� ---
    // ���� ��ķ��
    private WebCamTexture localWebCamTexture;

    // ROS2 �� �Ϲ� ���÷��̿� Texture2D
    // �� Texture2D ��ü�� RawImage�� ���������� ǥ�õ˴ϴ�.
    private Texture2D displayTexture;
    private byte[] receivedRosImageData; // ROS�κ��� ���� ���� �̹��� ������ (�ַ� ����� ����)
    private bool newRosFrameAvailable = false;
    private readonly object rosImageDataLock = new object(); // ROS �ݹ�� Update ���� ������ ����ȭ��

    void Start()
    {
        if (displayImage == null)
        {
            Debug.LogError("UniversalVideoController: 'Display Image' (RawImage)�� �Ҵ���� �ʾҽ��ϴ�! Inspector���� �������ּ���.");
            enabled = false; // ��ũ��Ʈ �۵� ����
            return;
        }

        if (currentSource == VideoSourceType.LocalWebcam)
        {
            InitializeLocalWebcam();
        }
        else if (currentSource == VideoSourceType.ROS2)
        {
            InitializeROS2Video();
        }
    }

    void InitializeLocalWebcam()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogWarning("UniversalVideoController: ��� ������ ��ķ�� ã�� �� �����ϴ�. ��ǻ�Ϳ� ��ķ�� ����Ǿ� �ִ��� Ȯ�����ּ���.");
            return;
        }
        string deviceName = devices[0].name; // ����� ù ��° ��ķ ���
        localWebCamTexture = new WebCamTexture(deviceName, requestedWebcamWidth, requestedWebcamHeight, requestedWebcamFPS);

        displayImage.texture = localWebCamTexture; // RawImage�� WebCamTexture ���� �Ҵ�
        // WebCamTexture�� ������ �°� RawImage�� �����Ϸ��� 'Aspect Ratio Fitter' ������Ʈ ����� �����մϴ�.
        localWebCamTexture.Play();
        Debug.Log($"UniversalVideoController: ���� ��ķ '{localWebCamTexture.deviceName}' ��� ����.");
    }

    void InitializeROS2Video()
    {
        // ROS2���� ����� Texture2D �غ� (�ʱ� ũ��� �߿����� ����, LoadImage�� �ڵ� ����)
        // displayTexture�� ROS2 ���������� ������Ʈ�� �� ������ �ؽ�ó�Դϴ�.
        if (displayTexture == null)
        {
            displayTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false); // mipChain�� false�� �Ͽ� ���� ��� �õ�
        }
        displayImage.texture = displayTexture; // RawImage�� �� Texture2D �Ҵ�

        Debug.Log("UniversalVideoController: ROS2 ���� �ҽ� �ʱ�ȭ �õ� ��... (���� ���� ���� �ʿ�)");

        // === ���⿡ ������ ROS2-Unity ���̺귯���� ����� ���� ���� ���� �߰� ===
        // 1. ROS2 ��� �ʱ�ȭ (���̺귯�� ��Ŀ� ����)
        // 2. ī�޶� �̹��� ���� ������ ����
        //    - ���� �̸� (��: "/camera/image_compressed")
        //    - �޽��� Ÿ�� (��: sensor_msgs/msg/CompressedImage)
        //    - �ݹ� �Լ�: RosImageCallback ����
        // ���� (���� �ڵ�� ����ϴ� ���̺귯���� ���� ũ�� �޶����ϴ�):
        // YourRos2Manager.Instance.Subscribe<YourCompressedImageMsgType>("/your_camera_topic", RosImageCallback);
        // ====================================================================
    }

    // ROS2 �޽��� ���� �� ȣ��� �ݹ� �Լ� (ROS ���̺귯���� �����忡�� ����� �� ����)
    // �� �Լ��� �Ķ���ʹ� ���� �����ϴ� �޽��� Ÿ�Կ� ����� �մϴ�.
    // ���⼭�� byte[] �����͸� ���� �޴´ٰ� ���� (��: CompressedImage.data)
    public void RosImageCallback(byte[] compressedImageData)
    {
        lock (rosImageDataLock) // ������ ����ȭ
        {
            receivedRosImageData = compressedImageData; // ���ŵ� ������ ����
            newRosFrameAvailable = true;           // �� ������ ���� �÷��� ����
        }
    }

    void Update()
    {
        if (currentSource == VideoSourceType.LocalWebcam)
        {
            // ���� ��ķ�� WebCamTexture�� Play()�� ���� �ڵ����� �ؽ�ó�� ������Ʈ�մϴ�.
            // RawImage�� Aspect Ratio�� WebCamTexture�� ���� �ػ󵵿� �°� �����ϴ� ������ ���⿡ �߰��� �� �ֽ��ϴ�.
            if (localWebCamTexture != null && localWebCamTexture.isPlaying && localWebCamTexture.didUpdateThisFrame)
            {
                // ��: displayImage�� RectTransform ũ�⳪ AspectRatioFitter ���� ������Ʈ
            }
        }
        else if (currentSource == VideoSourceType.ROS2)
        {
            byte[] imageDataToLoadThisFrame = null;

            lock (rosImageDataLock) // ������ ����ȭ
            {
                if (newRosFrameAvailable)
                {
                    imageDataToLoadThisFrame = receivedRosImageData; // ���� �����忡�� ó���� ������ ��������
                    newRosFrameAvailable = false;                    // �÷��� ����
                }
            }

            if (imageDataToLoadThisFrame != null && displayTexture != null)
            {
                // Texture2D.LoadImage�� ���� �����忡�� ȣ���ؾ� �մϴ�.
                // �� �Լ��� JPEG �Ǵ� PNG ���� ����� �̹��� �����͸� ���ڵ��ϰ� �ؽ�ó�� �ε��մϴ�.
                if (displayTexture.LoadImage(imageDataToLoadThisFrame))
                {
                    // LoadImage ���� ��, �ؽ�ó�� GPU�� ���ε�˴ϴ�.
                    // (displayImage.texture = displayTexture; �� Start���� �̹� �ѹ� ���������Ƿ� �ݺ��� �ʿ� ����)
                }
                else
                {
                    Debug.LogError("UniversalVideoController: ROS �̹��� �����͸� Texture2D�� �ε��ϴ� �� �����߽��ϴ�.");
                }
            }
        }
    }

    void OnDestroy() // GameObject�� �ı��� �� ȣ��
    {
        StopSources();
    }

    void OnApplicationQuit() // ���ø����̼��� ����� �� ȣ��
    {
        StopSources();
    }

    void OnDisable() // ��ũ��Ʈ �Ǵ� GameObject�� ��Ȱ��ȭ�� ��
    {
        StopSources();
    }

    private void StopSources()
    {
        // ���� ��ķ ����
        if (localWebCamTexture != null && localWebCamTexture.isPlaying)
        {
            localWebCamTexture.Stop();
            Debug.Log("UniversalVideoController: ���� ��ķ ������.");
        }

        // === ���⿡ ROS2 ���� ���� �� ��� ���� ���� �߰� (�ʿ� ��) ===
        // ��: if (YourRos2Manager.Instance != null) YourRos2Manager.Instance.Unsubscribe(...);
        // ==========================================================
    }
}