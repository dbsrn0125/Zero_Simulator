// RosVideoSubscriber.cs (������ ����)

using UnityEngine;
using UnityEngine.UI;
using RosMessageTypes.Sensor; // CompressedImageMsg ����� ����

public class RosVideoSubscriber : MonoBehaviour
{
    [Header("UI Display")]
    public RawImage displayImage;

    private Texture2D receivedTexture;

    void Start()
    {
        // Start������ �ƹ��͵� ���� �ʰų�, UI �ʱ�ȭ�� �մϴ�.
        if (displayImage == null)
        {
            Debug.LogError($"'{gameObject.name}'�� 'Display Image'�� �Ҵ���� �ʾҽ��ϴ�!");
            enabled = false;
        }
        receivedTexture = new Texture2D(2, 2);
        ClearDisplay();
    }

    // ? 1. �ܺ�(���ο� Manager)���� ȣ���� ���� �Լ��� ����ϴ�.
    //    �� �Լ��� ROS �޽��� �����͸� ���� �޾Ƽ� ȭ�鿡 ǥ���մϴ�.
    public void UpdateImage(CompressedImageMsg msg)
    {
        if (msg == null) return;

        // ���� �̹��� �����͸� �ؽ�ó�� ��ȯ�Ͽ� RawImage�� ����
        receivedTexture.LoadImage(msg.data);
        if (displayImage.texture == null)
        {
            displayImage.texture = receivedTexture;
        }
        displayImage.color = Color.white;
    }

    // ? 2. ȭ���� �����ϰ� ����� �Լ��� �ܺο��� ȣ���� �� �ֵ��� public���� �Ӵϴ�.
    public void ClearDisplay()
    {
        displayImage.texture = null;
        displayImage.color = new Color(0, 0, 0, 0);
    }

    // OnDestroy, Callback, Update, ChangeTopic �� ������ ROS ���� �Լ����� ��� �����մϴ�.
}