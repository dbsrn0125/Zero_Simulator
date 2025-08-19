using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System.Threading;
using System.Collections.Concurrent;

public class RosVideoSubscriberOptimized : MonoBehaviour
{
    [Header("ROS Settings")]
    [Tooltip("ROS Topic Name")]
    public string rosTopicName = string.Empty;

    [Header("UI Display")]
    [Tooltip("UI RawImage Component")]
    public RawImage displayImage;

    private Texture2D receivedTexture;
    private ROSConnection rosConnection;
    private Thread imageProcessingThread;

    private ConcurrentQueue<byte[]> imageDataQueue = new ConcurrentQueue<byte[]>();

    private struct DecodedImageData
    {
        public Color32[] pixels;
        public int width;
        public int height;
    }

    private ConcurrentQueue<DecodedImageData> decodedImageQueue = new ConcurrentQueue<DecodedImageData>();
    // Start is called before the first frame update
    void Start()
    {
        if (displayImage == null)
        {
            Debug.LogError($"'{gameObject.name}'의 'Display Image'가 할당되지 않았습니다!");
            enabled = false;
            return;
        }

        if (ROSManager.instance == null)
        {
            Debug.LogError("ROSConnectionManager가 씬에 존재하지 않습니다!");
            enabled = false;
            return;
        }
        rosConnection = ROSManager.instance.ROSConnection;

        receivedTexture = new Texture2D(2, 2);
        displayImage.texture = receivedTexture;

        rosConnection.Subscribe<CompressedImageMsg>(rosTopicName, ReceiveRosMessage);
        Debug.Log($"Start subscribing topic'{rosTopicName}'");

        imageProcessingThread = new Thread(ImageProcessingLoop);
        imageProcessingThread.IsBackground = true;
        imageProcessingThread.Start();
        Debug.Log("Image Processing Thread has stared");
    }
    private void OnDestroy()
    {
        // 4. 앱 종료 시 스레드를 안전하게 중단
        if (imageProcessingThread != null && imageProcessingThread.IsAlive)
        {
            imageProcessingThread.Abort();
        }

        if (rosConnection != null)
        {
            rosConnection.Unsubscribe(rosTopicName);
        }
    }

    private void ReceiveRosMessage(CompressedImageMsg msg)
    {
        imageDataQueue.Enqueue(msg.data);
    }

    private void ImageProcessingLoop()
    {
        Texture2D tempTexture = new Texture2D(2,2);
        while (true)
        {
            if(imageDataQueue.TryDequeue(out byte[] imageData))
            {
                if(tempTexture.LoadImage(imageData))
                {
                    if (tempTexture.width > 1 && tempTexture.height > 1)
                    {
                        DecodedImageData decodedData = new DecodedImageData
                        {
                            pixels = tempTexture.GetPixels32(),
                            width = tempTexture.width,
                            height = tempTexture.height
                        };
                        decodedImageQueue.Enqueue(decodedData);
                    }
                }
            }
            else
            {
                Thread.Sleep(10);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (decodedImageQueue.TryDequeue(out DecodedImageData imageData)) 
        {
            while(decodedImageQueue.TryDequeue(out DecodedImageData newerImageData))
            {
                imageData = newerImageData;
            }
        }

        if (receivedTexture.width != imageData.width || receivedTexture.height != imageData.height)
        {
            receivedTexture.Reinitialize(imageData.width, imageData.height);
        }

        receivedTexture.SetPixels32(imageData.pixels);

        receivedTexture.Apply();
    }
}

