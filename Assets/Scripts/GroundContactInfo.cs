using UnityEngine;

/// <summary>
/// 바퀴의 지면 센서가 측정한 기하학적 정보를 담는 구조체.
/// </summary>
public struct GroundContactInfo
{
    public bool IsContacting;      // 지면에 닿았는가?
    public float Gap;              // 침투 깊이 (m), 바퀴 최하단 - 지면 높이
    public Vector3 GroundNormal;   // 접촉한 지면의 법선 벡터 (월드 좌표계)
}