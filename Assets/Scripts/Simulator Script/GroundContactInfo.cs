using UnityEngine;

/// <summary>
/// ������ ���� ������ ������ �������� ������ ��� ����ü.
/// </summary>
public struct GroundContactInfo
{
    public bool IsContacting;      // ���鿡 ��Ҵ°�?
    public float Gap;              // ħ�� ���� (m), ���� ���ϴ� - ���� ����
    public Vector3 GroundNormal;   // ������ ������ ���� ���� (���� ��ǥ��)
}