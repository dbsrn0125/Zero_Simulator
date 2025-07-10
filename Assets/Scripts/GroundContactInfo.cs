// GroundContactInfo.cs

using UnityEngine;

/// <summary>
/// Simscape에 전달할 지면 접촉 정보를 담는 구조체
/// </summary>
public struct GroundContactInfo
{
    public bool IsContacting;      // 지면 접촉 여부
    public Vector3 ContactPoint;   // 4개 접촉점의 평균 월드 좌표
    public Vector3 ContactNormal;  // 4개 접촉점으로부터 계산된 평균 법선 벡터

    // 생성자
    public GroundContactInfo(bool isContacting, Vector3 point, Vector3 normal)
    {
        IsContacting = isContacting;
        ContactPoint = point;
        ContactNormal = normal;
    }

    // 바퀴가 허공에 떠 있을 때 사용할 기본값
    public static GroundContactInfo NonContact()
    {
        // 비접촉 시, 위치는 0, 법선 벡터는 위쪽(Y-up)을 기본값으로 사용
        return new GroundContactInfo(false, Vector3.zero, Vector3.up);
    }
}