using UnityEngine;
using UnityEngine.UI;
using RosMessageTypes;

public class UniversalVideoController : MonoBehaviour
{
    public enum VideoSourceType { LocalWebcam, ROS2 }
    [Header("Source Selection")]
    [Tooltip("사용할 비디오 소스를 선택합니다.")]
    public VideoSourceType currentSource = VideoSourceType.LocalWebcam;

    [Header("UI")]
    [Tooltip("영상을 표시할 UI RawImage 컴포넌트를 여기에 할당하세요.")]
    public RawImage displayImage;

    [Header("Webcam Settings (LocalWebcam 선택 시)")]
    public int requestedWebcamWidth = 640;
    public int requestedWebcamHeight = 480;
    public int requestedWebcamFPS = 30;

    // --- 내부 변수 ---
    // 로컬 웹캠용
    private WebCamTexture localWebCamTexture;

    // ROS2 및 일반 디스플레이용 Texture2D
    // 이 Texture2D 객체가 RawImage에 최종적으로 표시됩니다.
    private Texture2D displayTexture;
    private byte[] receivedRosImageData; // ROS로부터 받은 원본 이미지 데이터 (주로 압축된 형태)
    private bool newRosFrameAvailable = false;
    private readonly object rosImageDataLock = new object(); // ROS 콜백과 Update 간의 스레드 동기화용

    void Start()
    {
        if (displayImage == null)
        {
            Debug.LogError("UniversalVideoController: 'Display Image' (RawImage)가 할당되지 않았습니다! Inspector에서 설정해주세요.");
            enabled = false; // 스크립트 작동 중지
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
            Debug.LogWarning("UniversalVideoController: 사용 가능한 웹캠을 찾을 수 없습니다. 컴퓨터에 웹캠이 연결되어 있는지 확인해주세요.");
            return;
        }
        string deviceName = devices[0].name; // 목록의 첫 번째 웹캠 사용
        localWebCamTexture = new WebCamTexture(deviceName, requestedWebcamWidth, requestedWebcamHeight, requestedWebcamFPS);

        displayImage.texture = localWebCamTexture; // RawImage에 WebCamTexture 직접 할당
        // WebCamTexture의 비율에 맞게 RawImage를 조절하려면 'Aspect Ratio Fitter' 컴포넌트 사용을 권장합니다.
        localWebCamTexture.Play();
        Debug.Log($"UniversalVideoController: 로컬 웹캠 '{localWebCamTexture.deviceName}' 재생 시작.");
    }

    void InitializeROS2Video()
    {
        // ROS2에서 사용할 Texture2D 준비 (초기 크기는 중요하지 않음, LoadImage가 자동 조절)
        // displayTexture는 ROS2 프레임으로 업데이트될 빈 껍데기 텍스처입니다.
        if (displayTexture == null)
        {
            displayTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false); // mipChain을 false로 하여 성능 향상 시도
        }
        displayImage.texture = displayTexture; // RawImage에 이 Texture2D 할당

        Debug.Log("UniversalVideoController: ROS2 비디오 소스 초기화 시도 중... (구독 로직 구현 필요)");

        // === 여기에 선택한 ROS2-Unity 라이브러리를 사용한 구독 설정 로직 추가 ===
        // 1. ROS2 노드 초기화 (라이브러리 방식에 따라)
        // 2. 카메라 이미지 토픽 구독자 생성
        //    - 토픽 이름 (예: "/camera/image_compressed")
        //    - 메시지 타입 (예: sensor_msgs/msg/CompressedImage)
        //    - 콜백 함수: RosImageCallback 연결
        // 예시 (실제 코드는 사용하는 라이브러리에 따라 크게 달라집니다):
        // YourRos2Manager.Instance.Subscribe<YourCompressedImageMsgType>("/your_camera_topic", RosImageCallback);
        // ====================================================================
    }

    // ROS2 메시지 수신 시 호출될 콜백 함수 (ROS 라이브러리의 스레드에서 실행될 수 있음)
    // 이 함수의 파라미터는 실제 구독하는 메시지 타입에 맞춰야 합니다.
    // 여기서는 byte[] 데이터를 직접 받는다고 가정 (예: CompressedImage.data)
    public void RosImageCallback(byte[] compressedImageData)
    {
        lock (rosImageDataLock) // 스레드 동기화
        {
            receivedRosImageData = compressedImageData; // 수신된 데이터 복사
            newRosFrameAvailable = true;           // 새 프레임 수신 플래그 설정
        }
    }

    void Update()
    {
        if (currentSource == VideoSourceType.LocalWebcam)
        {
            // 로컬 웹캠은 WebCamTexture가 Play()된 이후 자동으로 텍스처를 업데이트합니다.
            // RawImage의 Aspect Ratio를 WebCamTexture의 실제 해상도에 맞게 조절하는 로직을 여기에 추가할 수 있습니다.
            if (localWebCamTexture != null && localWebCamTexture.isPlaying && localWebCamTexture.didUpdateThisFrame)
            {
                // 예: displayImage의 RectTransform 크기나 AspectRatioFitter 값을 업데이트
            }
        }
        else if (currentSource == VideoSourceType.ROS2)
        {
            byte[] imageDataToLoadThisFrame = null;

            lock (rosImageDataLock) // 스레드 동기화
            {
                if (newRosFrameAvailable)
                {
                    imageDataToLoadThisFrame = receivedRosImageData; // 메인 스레드에서 처리할 데이터 가져오기
                    newRosFrameAvailable = false;                    // 플래그 리셋
                }
            }

            if (imageDataToLoadThisFrame != null && displayTexture != null)
            {
                // Texture2D.LoadImage는 메인 스레드에서 호출해야 합니다.
                // 이 함수는 JPEG 또는 PNG 같은 압축된 이미지 데이터를 디코딩하고 텍스처에 로드합니다.
                if (displayTexture.LoadImage(imageDataToLoadThisFrame))
                {
                    // LoadImage 성공 시, 텍스처는 GPU에 업로드됩니다.
                    // (displayImage.texture = displayTexture; 는 Start에서 이미 한번 설정했으므로 반복할 필요 없음)
                }
                else
                {
                    Debug.LogError("UniversalVideoController: ROS 이미지 데이터를 Texture2D로 로드하는 데 실패했습니다.");
                }
            }
        }
    }

    void OnDestroy() // GameObject가 파괴될 때 호출
    {
        StopSources();
    }

    void OnApplicationQuit() // 애플리케이션이 종료될 때 호출
    {
        StopSources();
    }

    void OnDisable() // 스크립트 또는 GameObject가 비활성화될 때
    {
        StopSources();
    }

    private void StopSources()
    {
        // 로컬 웹캠 정지
        if (localWebCamTexture != null && localWebCamTexture.isPlaying)
        {
            localWebCamTexture.Stop();
            Debug.Log("UniversalVideoController: 로컬 웹캠 정지됨.");
        }

        // === 여기에 ROS2 구독 해제 및 노드 종료 로직 추가 (필요 시) ===
        // 예: if (YourRos2Manager.Instance != null) YourRos2Manager.Instance.Unsubscribe(...);
        // ==========================================================
    }
}