// RosVideoSubscriber.cs (최종 완성 버전)

using UnityEngine;
using UnityEngine.UI;
using RosMessageTypes.Sensor;

public class RosVideoSubscriber : MonoBehaviour
{
    [Header("UI Display")]
    public RawImage displayImage;

    // ? 1. 모든 인스턴스가 공유할 '정적(static)' 변수로 선언합니다.
    private static Texture2D blankTexture;

    private Texture2D receivedTexture;

    void Awake()
    {
        if (displayImage == null)
            displayImage = GetComponent<RawImage>();

        // ? 2. blankTexture가 아직 만들어지지 않았다면(null), 딱 한 번만 생성합니다.
        if (blankTexture == null)
        {
            blankTexture = new Texture2D(1, 1);
            blankTexture.SetPixel(0, 0, Color.black);
            blankTexture.Apply();
        }

        // receivedTexture는 각 인스턴스마다 개별적으로 생성합니다.
        receivedTexture = new Texture2D(2, 2);
        ClearDisplay();
    }

    // 외부(MissionVideoManager)에서 호출하는 이미지 업데이트 함수
    public void UpdateImage(CompressedImageMsg msg)
    {
        if (msg == null) return;

        receivedTexture.LoadImage(msg.data);

        // ? 3. RawImage가 비디오 텍스처를 보도록 다시 지정합니다.
        displayImage.texture = receivedTexture;
        displayImage.color = Color.white;
    }

    // 외부(EmergencyStopController)에서 호출하는 화면 클리어 함수
    public void ClearDisplay()
    {
        // ? 4. RawImage가 미리 만들어둔 '빈 텍스처'를 보도록 지정합니다.
        displayImage.texture = blankTexture;
        displayImage.color = new Color(0, 0, 0, 0.5f); // 텍스처가 잘 보이도록 색상은 흰색으로
    }
}