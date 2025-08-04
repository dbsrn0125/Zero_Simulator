using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class ROSManager : MonoBehaviour
{
    public static ROSManager instance = null;
    public ROSConnection ROSConnection;

    [Header("Topic List")]
    public List<string> videoTopicsToSubscribe;

    private Dictionary<string, Texture2D> receivedTextures = new Dictionary<string, Texture2D>();
    private readonly object _lock = new object();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            ROSConnection = GetComponent<ROSConnection>();
            UnityEngine.Debug.LogException(new System.Exception("범인 발견: 누군가 GetOrCreateInstance()를 호출함!"));
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 리스트에 있는 모든 토픽에 대해 구독을 시작합니다.
        foreach (string topicName in videoTopicsToSubscribe)
        {
            // 각 토픽에 대한 텍스처를 저장할 공간을 미리 만듭니다.
            receivedTextures[topicName] = new Texture2D(2, 2);

            // Subscribe 메서드에 람다식을 사용하여, 콜백 함수가 자신이 어떤 토픽에서 왔는지 알게 합니다.
            ROSConnection.Subscribe<CompressedImageMsg>(topicName, (msg) => CompressedImageCallback(topicName, msg));
            Debug.Log($"[ROSManager] {topicName} 토픽 구독 시작.");
        }
    }

    // 콜백 함수는 이제 어떤 토픽에서 메시지가 왔는지 'topicName' 인자를 통해 알 수 있습니다.
    void CompressedImageCallback(string topicName, CompressedImageMsg msg)
    {
        lock (_lock)
        {
            // 해당 토픽 이름에 맞는 텍스처에 이미지를 로드합니다.
            if (receivedTextures.ContainsKey(topicName))
            {
                receivedTextures[topicName].LoadImage(msg.data);
            }
        }
    }

    // 다른 스크립트(RosVideoSubscriber)가 이 함수를 호출하여 이미지를 가져갑니다.
    public Texture2D GetTexture(string topicName)
    {
        lock (_lock)
        {
            if (receivedTextures.ContainsKey(topicName))
            {
                return receivedTextures[topicName];
            }
            return null;
        }
    }

}
