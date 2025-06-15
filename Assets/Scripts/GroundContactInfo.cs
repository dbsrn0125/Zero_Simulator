// WheelLocation Enum (아마 이미 있을 겁니다. 없다면 추가해주세요)

// 센서 데이터를 담을 구조체
public struct GroundContactInfo
{
    public bool IsContacting;
    public float GroundHeight; // ray_in
    public float GroundPitch;  // gnd_pitch
    public float GroundRoll;   // gnd_roll

    // 기본 생성자
    public GroundContactInfo(bool isContacting, float height, float pitch, float roll)
    {
        IsContacting = isContacting;
        GroundHeight = height;
        GroundPitch = pitch;
        GroundRoll = roll;
    }

    // 비접촉 상태를 위한 정적 메서드
    public static GroundContactInfo NonContact()
    {
        return new GroundContactInfo(false, -1f, 0f, 0f); // 기본값
    }
}