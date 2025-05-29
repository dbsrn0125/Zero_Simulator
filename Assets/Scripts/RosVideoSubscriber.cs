using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class RosVideoSubscriber : MonoBehaviour
{
    [Header("ROS Settings")]
    [Tooltip("ROS Topic Name")]
    public string rosTopicName = "/camera/image_compressed";

    [Header("UI Display")]
    [Tooltip("UI RawImage component")]
    public RawImage displayImage;

    //내부 변수
    private Texture2D receivedTexture;
    private byte[] imageDataBuffer;
    private bool newFrameAvailable = false;
    private readonly object _lock = new object();

    // Start is called before the first frame update
    void Start()
    {
        if (displayImage == null)
        {
            Debug.LogError($"[{gameObject.name}] ROSVideoSubscriber: 'Display Image' (RawImage)가 할당되지 않았습니다!");
            enabled = false;
            return;
        }

        // 초기 텍스처 생성 (크기는 LoadImage 호출 시 자동 조절됨)
        receivedTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        displayImage.texture = receivedTexture;

        // ROSConnection 인스턴스를 통해 토픽 구독
        try
        {
            ROSConnection.GetOrCreateInstance().Subscribe<CompressedImageMsg>(rosTopicName, CompressedImageCallback);
            Debug.Log($"[{gameObject.name}] '{rosTopicName}' Subscribe (CompressedImageMsg)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[{gameObject.name}] 토픽 구독 중 오류 발생: {rosTopicName} - {e.Message}");
        }
    }
    
    void CompressedImageCallback(CompressedImageMsg msg)
    {
        lock (_lock) 
        {
            //msg.data는 byte[] 타입입니다.
            imageDataBuffer = msg.data;
            newFrameAvailable = true;
        }
    }


    void Update()
    {
        byte[] currentFrameData = null;
        bool processThisFrame = false;

        lock (_lock)
        {
            if (newFrameAvailable)
            {
                currentFrameData = imageDataBuffer;
                processThisFrame = true;
                newFrameAvailable = false; // 플래그 리셋!
            }
        }

        if (processThisFrame && currentFrameData != null)
        {
            // 압축된 이미지 데이터(JPEG, PNG 등)를 Texture2D로 로드합니다.
            // LoadImage는 텍스처 크기를 자동으로 조절하고 GPU에 업로드합니다 (메인 스레드에서만 호출).
            if (!receivedTexture.LoadImage(currentFrameData))
            {
                Debug.LogWarning($"[{gameObject.name}] 압축 이미지 데이터를 텍스처로 로드하는 데 실패했습니다. 토픽: {rosTopicName}. 데이터 길이: {currentFrameData.Length}");
            }
        }
    }

    void OnDestroy()
    {
        // ROS-TCP-Connector는 ROSConnection 오브젝트가 파괴될 때 구독을 정리해줄 수 있습니다.
        // 명시적인 구독 해제가 필요하다면 라이브러리 문서를 참조하세요.
        // 예: ROSConnection.GetOrCreateInstance().Unsubscribe(rosTopicName); (API 확인 필요)
        Debug.Log($"[{gameObject.name}] '{rosTopicName}' Subscriber Destroyed.");
    }
}
