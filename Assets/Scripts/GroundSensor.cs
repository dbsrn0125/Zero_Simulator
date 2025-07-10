// GroundSensor.cs

using UnityEngine;
using System.Collections.Generic;

public class GroundSensor : MonoBehaviour
{
    [Header("Sensor Setup")]
    public LayerMask groundLayer;
    [Tooltip("레이캐스트를 쏠 최대 거리")]
    public float maxDistance = 1.0f;

    [Header("4-Point Raycast Offsets")]
    [Tooltip("이 오프셋들을 조정하여 4개의 레이캐스트 위치를 결정합니다.")]
    public Vector3[] raycastOffsets = new Vector3[4]
    {
        new Vector3( 0.1f, 0,  0.1f), // 앞-오른쪽
        new Vector3(-0.1f, 0,  0.1f), // 앞-왼쪽
        new Vector3( 0.1f, 0, -0.1f), // 뒤-오른쪽
        new Vector3(-0.1f, 0, -0.1f)  // 뒤-왼쪽
    };

    // --- 최종 출력 데이터 ---
    public GroundContactInfo ContactInfo { get; private set; }


    void FixedUpdate()
    {
        UpdateGroundContact();
    }

    /// <summary>
    /// 4점 레이캐스트를 실행하고 지면 정보를 업데이트합니다.
    /// </summary>
    private void UpdateGroundContact()
    {
        List<RaycastHit> hits = new List<RaycastHit>();
        Vector3 rayDirection = -transform.up; // 항상 바퀴의 아래 방향

        // 4개의 오프셋 위치에서 각각 레이캐스트 실행
        foreach (var offset in raycastOffsets)
        {
            // 월드 좌표계 기준 레이캐스트 시작점 계산
            Vector3 rayOrigin = transform.position + transform.rotation * offset;

            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, maxDistance, groundLayer))
            {
                hits.Add(hit);
            }
        }

        // 4개의 레이가 모두 지면에 닿았을 때만 접촉으로 인정
        if (hits.Count == 4)
        {
            ContactInfo = CalculateContactFromHits(hits);
        }
        else
        {
            ContactInfo = GroundContactInfo.NonContact();
        }
    }

    /// <summary>
    /// 4개의 충돌 지점(Hit)으로부터 평균 위치와 법선 벡터를 계산합니다.
    /// </summary>
    private GroundContactInfo CalculateContactFromHits(List<RaycastHit> hits)
    {
        // 1. 평균 접촉점 계산
        Vector3 avgContactPoint = Vector3.zero;
        foreach (var hit in hits)
        {
            avgContactPoint += hit.point;
        }
        avgContactPoint /= hits.Count;

        // 2. 두 개의 삼각형을 만들어 평균 법선 벡터 계산 (더 안정적인 방법)
        Vector3 v1 = hits[1].point - hits[0].point; // 앞-왼쪽 -> 앞-오른쪽
        Vector3 v2 = hits[2].point - hits[0].point; // 뒤-오른쪽 -> 앞-오른쪽
        Vector3 normal1 = Vector3.Cross(v1, v2);

        Vector3 v3 = hits[3].point - hits[2].point; // 뒤-왼쪽 -> 뒤-오른쪽
        Vector3 v4 = hits[0].point - hits[2].point; // 앞-오른쪽 -> 뒤-오른쪽
        Vector3 normal2 = Vector3.Cross(v3, v4);

        Vector3 avgNormal = (normal1 + normal2).normalized;

        // 법선 벡터가 항상 위를 향하도록 보정
        if (Vector3.Dot(avgNormal, transform.up) < 0)
        {
            avgNormal = -avgNormal;
        }

        return new GroundContactInfo(true, avgContactPoint, avgNormal);
    }

    // 디버깅용 시각화
    void OnDrawGizmosSelected()
    {
        if (raycastOffsets == null || raycastOffsets.Length == 0) return;

        Gizmos.color = IsGrounded() ? Color.green : Color.yellow;
        Vector3 rayDirection = -transform.up;

        foreach (var offset in raycastOffsets)
        {
            // 에디터에서도 정확한 위치를 보기 위해 월드 위치 계산
            Vector3 rayOrigin = transform.position + transform.rotation * offset;
            Gizmos.DrawSphere(rayOrigin, 0.02f);
            Gizmos.DrawLine(rayOrigin, rayOrigin + rayDirection * maxDistance);
        }

        // 계산된 법선 벡터 그리기
        if (IsGrounded())
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(ContactInfo.ContactPoint, ContactInfo.ContactPoint + ContactInfo.ContactNormal * 0.3f);
        }
    }

    // 간단한 접지 확인용 헬퍼 함수
    public bool IsGrounded()
    {
        return ContactInfo.IsContacting;
    }
}