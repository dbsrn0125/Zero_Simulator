using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System.Collections.Generic;
using System.Collections; // �⺻ Queue�� ���� �߰�

public class RosVideoSubscriber_Coroutine : MonoBehaviour
{
    [Header("ROS Settings")]
    public string rosTopicName = string.Empty;

    [Header("UI Display")]
    public RawImage displayImage;

    // --- Private Variables ---
    private Texture2D receivedTexture;
    private ROSConnection rosConnection;

    // ��Ƽ�������� �ƴϹǷ� �⺻ Queue�� ����ص� �����մϴ�.
    private Queue<byte[]> imageDataQueue = new Queue<byte[]>();

    void Start()
    {
        if (displayImage == null) { enabled = false; return; }
        if (ROSManager.instance == null) { enabled = false; return; }

        rosConnection = ROSManager.instance.ROSConnection;
        receivedTexture = new Texture2D(2, 2);
        displayImage.texture = receivedTexture;

        rosConnection.Subscribe<CompressedImageMsg>(rosTopicName, ReceiveRosMessage);

        // Update() ���, Queue�� �����ϰ� ó���ϴ� �ڷ�ƾ�� �����մϴ�.
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
        // �ֽ� �������� �����ϱ� ���� ť�� �ʹ� ���� ���̸� ������ ���� �����ϴ� (������)
        if (imageDataQueue.Count < 10)
        {
            imageDataQueue.Enqueue(msg.data);
        }
    }

    // �ڡڡ� �ٽ� ����: �ڷ�ƾ�� �̿��� ���� ó�� �ڡڡ�
    private IEnumerator ProcessImageQueue()
    {
        // �� �ڷ�ƾ�� ������ ����Ǵ� ���� ����ؼ� ����˴ϴ�.
        while (true)
        {
            // ť�� ó���� �����Ͱ� �ִٸ�,
            if (imageDataQueue.Count > 0)
            {
                byte[] imageData = imageDataQueue.Dequeue();

                // LoadImage�� ������ ���� �����带 ��� ���߰� ������,
                // �� �۾��� �� ������ ������ �ٸ� Update ������� ��ġ�� �ʽ��ϴ�.
                receivedTexture.LoadImage(imageData);
                receivedTexture.Apply();
            }

            // �۾��� �ֵ� ����, ���� �����ӱ��� ����մϴ�.
            // �̷��� �����ν� Unity�� �ٸ� �۾��� ó���� �ð��� �ݴϴ�.
            yield return null;
        }
    }
}