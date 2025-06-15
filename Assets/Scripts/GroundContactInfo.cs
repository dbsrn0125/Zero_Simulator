// WheelLocation Enum (�Ƹ� �̹� ���� �̴ϴ�. ���ٸ� �߰����ּ���)

// ���� �����͸� ���� ����ü
public struct GroundContactInfo
{
    public bool IsContacting;
    public float GroundHeight; // ray_in
    public float GroundPitch;  // gnd_pitch
    public float GroundRoll;   // gnd_roll

    // �⺻ ������
    public GroundContactInfo(bool isContacting, float height, float pitch, float roll)
    {
        IsContacting = isContacting;
        GroundHeight = height;
        GroundPitch = pitch;
        GroundRoll = roll;
    }

    // ������ ���¸� ���� ���� �޼���
    public static GroundContactInfo NonContact()
    {
        return new GroundContactInfo(false, -1f, 0f, 0f); // �⺻��
    }
}