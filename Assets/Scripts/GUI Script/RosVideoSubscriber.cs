// RosVideoSubscriber.cs (수정된 버전)

using UnityEngine;
using UnityEngine.UI;
using RosMessageTypes.Sensor; // CompressedImageMsg 사용을 위해

public class RosVideoSubscriber : MonoBehaviour
{
    [Header("UI Display")]
    public RawImage displayImage;

    private Texture2D receivedTexture;

    void Start()
    {
        // Start에서는 아무것도 하지 않거나, UI 초기화만 합니다.
        if (displayImage == null)
        {
            Debug.LogError($"'{gameObject.name}'의 'Display Image'가 할당되지 않았습니다!");
            enabled = false;
        }
        receivedTexture = new Texture2D(2, 2);
        ClearDisplay();
    }

    // ? 1. 외부(새로운 Manager)에서 호출할 공개 함수를 만듭니다.
    //    이 함수는 ROS 메시지 데이터를 직접 받아서 화면에 표시합니다.
    public void UpdateImage(CompressedImageMsg msg)
    {
        if (msg == null) return;

        // 받은 이미지 데이터를 텍스처로 변환하여 RawImage에 적용
        receivedTexture.LoadImage(msg.data);
        if (displayImage.texture == null)
        {
            displayImage.texture = receivedTexture;
        }
        displayImage.color = Color.white;
    }

    // ? 2. 화면을 깨끗하게 지우는 함수도 외부에서 호출할 수 있도록 public으로 둡니다.
    public void ClearDisplay()
    {
        displayImage.texture = null;
        displayImage.color = new Color(0, 0, 0, 0);
    }

    // OnDestroy, Callback, Update, ChangeTopic 등 기존의 ROS 관련 함수들은 모두 삭제합니다.
}