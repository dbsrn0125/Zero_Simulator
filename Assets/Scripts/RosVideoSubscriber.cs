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

    //���� ����
    private Texture2D receivedTexture;
    private byte[] imageDataBuffer;
    private bool newFrameAvailable = false;
    private readonly object _lock = new object();

    // Start is called before the first frame update
    void Start()
    {
        if (displayImage == null)
        {
            Debug.LogError($"[{gameObject.name}] ROSVideoSubscriber: 'Display Image' (RawImage)�� �Ҵ���� �ʾҽ��ϴ�!");
            enabled = false;
            return;
        }

        // �ʱ� �ؽ�ó ���� (ũ��� LoadImage ȣ�� �� �ڵ� ������)
        receivedTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        displayImage.texture = receivedTexture;

        // ROSConnection �ν��Ͻ��� ���� ���� ����
        try
        {
            ROSConnection.GetOrCreateInstance().Subscribe<CompressedImageMsg>(rosTopicName, CompressedImageCallback);
            Debug.Log($"[{gameObject.name}] '{rosTopicName}' Subscribe (CompressedImageMsg)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[{gameObject.name}] ���� ���� �� ���� �߻�: {rosTopicName} - {e.Message}");
        }
    }
    
    void CompressedImageCallback(CompressedImageMsg msg)
    {
        lock (_lock) 
        {
            //msg.data�� byte[] Ÿ���Դϴ�.
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
                newFrameAvailable = false; // �÷��� ����!
            }
        }

        if (processThisFrame && currentFrameData != null)
        {
            // ����� �̹��� ������(JPEG, PNG ��)�� Texture2D�� �ε��մϴ�.
            // LoadImage�� �ؽ�ó ũ�⸦ �ڵ����� �����ϰ� GPU�� ���ε��մϴ� (���� �����忡���� ȣ��).
            if (!receivedTexture.LoadImage(currentFrameData))
            {
                Debug.LogWarning($"[{gameObject.name}] ���� �̹��� �����͸� �ؽ�ó�� �ε��ϴ� �� �����߽��ϴ�. ����: {rosTopicName}. ������ ����: {currentFrameData.Length}");
            }
        }
    }

    void OnDestroy()
    {
        // ROS-TCP-Connector�� ROSConnection ������Ʈ�� �ı��� �� ������ �������� �� �ֽ��ϴ�.
        // ������� ���� ������ �ʿ��ϴٸ� ���̺귯�� ������ �����ϼ���.
        // ��: ROSConnection.GetOrCreateInstance().Unsubscribe(rosTopicName); (API Ȯ�� �ʿ�)
        Debug.Log($"[{gameObject.name}] '{rosTopicName}' Subscriber Destroyed.");
    }
}
