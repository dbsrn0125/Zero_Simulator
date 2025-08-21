using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System.Collections.Generic;
using System.Collections; // 기본 Queue를 위해 추가

public class RosVideoSubscriber_Coroutine : MonoBehaviour
{
    [Header("ROS Settings")]
    public string rosTopicName = string.Empty;

    [Header("UI Display")]
    public RawImage displayImage;

    // --- Private Variables ---
    private Texture2D receivedTexture;
    private ROSConnection rosConnection;

    // 멀티스레딩이 아니므로 기본 Queue를 사용해도 안전합니다.
    private Queue<byte[]> imageDataQueue = new Queue<byte[]>();

    void Start()
    {
        if (displayImage == null) { enabled = false; return; }
        if (ROSManager.instance == null) { enabled = false; return; }

        rosConnection = ROSManager.instance.ROSConnection;
        receivedTexture = new Texture2D(2, 2);
        displayImage.texture = receivedTexture;

        rosConnection.Subscribe<CompressedImageMsg>(rosTopicName, ReceiveRosMessage);

        // Update() 대신, Queue를 감시하고 처리하는 코루틴을 시작합니다.
        StartCoroutine(ProcessImageQueue());
    }

    void OnDestroy()
    {
        if (rosConnection != null)
        {
            rosConnection.Unsubscribe(rosTopicName);
        }
    }

    private void ReceiveRosMessage(CompressedImageMsg msg)
    {
        // 최신 프레임을 유지하기 위해 큐가 너무 많이 쌓이면 오래된 것을 버립니다 (선택적)
        if (imageDataQueue.Count < 10)
        {
            imageDataQueue.Enqueue(msg.data);
        }
    }

    // ★★★ 핵심 로직: 코루틴을 이용한 순차 처리 ★★★
    private IEnumerator ProcessImageQueue()
    {
        // 이 코루틴은 게임이 실행되는 동안 계속해서 실행됩니다.
        while (true)
        {
            // 큐에 처리할 데이터가 있다면,
            if (imageDataQueue.Count > 0)
            {
                byte[] imageData = imageDataQueue.Dequeue();

                // LoadImage는 여전히 메인 스레드를 잠시 멈추게 하지만,
                // 이 작업이 한 프레임 동안의 다른 Update 로직들과 겹치지 않습니다.
                receivedTexture.LoadImage(imageData);
                receivedTexture.Apply();
            }

            // 작업이 있든 없든, 다음 프레임까지 대기합니다.
            // 이렇게 함으로써 Unity가 다른 작업을 처리할 시간을 줍니다.
            yield return null;
        }
    }
}