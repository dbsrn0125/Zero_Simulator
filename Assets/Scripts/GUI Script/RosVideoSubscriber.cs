// RosVideoSubscriber.cs (���� �ϼ� ����)

using UnityEngine;
using UnityEngine.UI;
using RosMessageTypes.Sensor;

public class RosVideoSubscriber : MonoBehaviour
{
    [Header("UI Display")]
    public RawImage displayImage;

    // ? 1. ��� �ν��Ͻ��� ������ '����(static)' ������ �����մϴ�.
    private static Texture2D blankTexture;

    private Texture2D receivedTexture;

    void Awake()
    {
        if (displayImage == null)
            displayImage = GetComponent<RawImage>();

        // ? 2. blankTexture�� ���� ��������� �ʾҴٸ�(null), �� �� ���� �����մϴ�.
        if (blankTexture == null)
        {
            blankTexture = new Texture2D(1, 1);
            blankTexture.SetPixel(0, 0, Color.black);
            blankTexture.Apply();
        }

        // receivedTexture�� �� �ν��Ͻ����� ���������� �����մϴ�.
        receivedTexture = new Texture2D(2, 2);
        ClearDisplay();
    }

    // �ܺ�(MissionVideoManager)���� ȣ���ϴ� �̹��� ������Ʈ �Լ�
    public void UpdateImage(CompressedImageMsg msg)
    {
        if (msg == null) return;

        receivedTexture.LoadImage(msg.data);

        // ? 3. RawImage�� ���� �ؽ�ó�� ������ �ٽ� �����մϴ�.
        displayImage.texture = receivedTexture;
        displayImage.color = Color.white;
    }

    // �ܺ�(EmergencyStopController)���� ȣ���ϴ� ȭ�� Ŭ���� �Լ�
    public void ClearDisplay()
    {
        // ? 4. RawImage�� �̸� ������ '�� �ؽ�ó'�� ������ �����մϴ�.
        displayImage.texture = blankTexture;
        displayImage.color = new Color(0, 0, 0, 0.5f); // �ؽ�ó�� �� ���̵��� ������ �������
    }
}