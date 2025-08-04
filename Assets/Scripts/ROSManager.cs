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
            UnityEngine.Debug.LogException(new System.Exception("���� �߰�: ������ GetOrCreateInstance()�� ȣ����!"));
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // ����Ʈ�� �ִ� ��� ���ȿ� ���� ������ �����մϴ�.
        foreach (string topicName in videoTopicsToSubscribe)
        {
            // �� ���ȿ� ���� �ؽ�ó�� ������ ������ �̸� ����ϴ�.
            receivedTextures[topicName] = new Texture2D(2, 2);

            // Subscribe �޼��忡 ���ٽ��� ����Ͽ�, �ݹ� �Լ��� �ڽ��� � ���ȿ��� �Դ��� �˰� �մϴ�.
            ROSConnection.Subscribe<CompressedImageMsg>(topicName, (msg) => CompressedImageCallback(topicName, msg));
            Debug.Log($"[ROSManager] {topicName} ���� ���� ����.");
        }
    }

    // �ݹ� �Լ��� ���� � ���ȿ��� �޽����� �Դ��� 'topicName' ���ڸ� ���� �� �� �ֽ��ϴ�.
    void CompressedImageCallback(string topicName, CompressedImageMsg msg)
    {
        lock (_lock)
        {
            // �ش� ���� �̸��� �´� �ؽ�ó�� �̹����� �ε��մϴ�.
            if (receivedTextures.ContainsKey(topicName))
            {
                receivedTextures[topicName].LoadImage(msg.data);
            }
        }
    }

    // �ٸ� ��ũ��Ʈ(RosVideoSubscriber)�� �� �Լ��� ȣ���Ͽ� �̹����� �������ϴ�.
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
