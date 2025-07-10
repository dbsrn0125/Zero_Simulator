// GroundContactInfo.cs

using UnityEngine;

/// <summary>
/// Simscape�� ������ ���� ���� ������ ��� ����ü
/// </summary>
public struct GroundContactInfo
{
    public bool IsContacting;      // ���� ���� ����
    public Vector3 ContactPoint;   // 4�� �������� ��� ���� ��ǥ
    public Vector3 ContactNormal;  // 4�� ���������κ��� ���� ��� ���� ����

    // ������
    public GroundContactInfo(bool isContacting, Vector3 point, Vector3 normal)
    {
        IsContacting = isContacting;
        ContactPoint = point;
        ContactNormal = normal;
    }

    // ������ ����� �� ���� �� ����� �⺻��
    public static GroundContactInfo NonContact()
    {
        // ������ ��, ��ġ�� 0, ���� ���ʹ� ����(Y-up)�� �⺻������ ���
        return new GroundContactInfo(false, Vector3.zero, Vector3.up);
    }
}