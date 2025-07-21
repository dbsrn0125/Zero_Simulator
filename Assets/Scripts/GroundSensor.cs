using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    [Header("Object References")]
    [Tooltip("회전하지 않는 로버의 메인 몸체(섀시)의 Transform을 연결하세요.")]
    public Transform chassisTransform; // 섀시를 기준으로 삼기 위한 참조

    [Header("Wheel Settings")]
    [Tooltip("바퀴의 반지름 (m)")]
    [SerializeField] private float wheelRadius = 0.5f;

    [Header("Raycast Settings")]
    [Tooltip("레이캐스트의 최대 감지 거리 (m)")]
    [SerializeField] private float raycastDistance = 1.0f;

    [Tooltip("지면 기울기 감지를 위한 앞/옆 레이캐스트의 오프셋 거리 (m)")]
    [SerializeField] private float raycastOffset = 0.1f;

    [Tooltip("지면으로 인식할 레이어")]
    [SerializeField] private LayerMask groundLayer;

    // 이제 GetContactInfo는 파라미터를 받지 않습니다.
    public GroundContactInfo GetContactInfo()
    {
        var info = new GroundContactInfo();

        // 섀시 Transform이 할당되지 않았으면 오류 방지
        if (chassisTransform == null)
        {
            Debug.LogError("Chassis Transform is not assigned in the GroundSensor!", this.gameObject);
            return new GroundContactInfo { IsContacting = false, Gap = 1.0f, GroundNormal = Vector3.up };
        }

        // 1. 세 방향으로 Raycast를 실행
        bool centerHit = Physics.Raycast(transform.position, Vector3.down, out RaycastHit centerHitInfo, raycastDistance, groundLayer);

        // [최종 수정] 로버 몸체(섀시)의 앞/옆 방향을 기준으로 오프셋 위치를 계산
        Vector3 forwardRayOrigin = transform.position + transform.forward * raycastOffset;
        bool forwardHit = Physics.Raycast(forwardRayOrigin, Vector3.down, out RaycastHit forwardHitInfo, raycastDistance, groundLayer);

        Vector3 sideRayOrigin = transform.position + transform.right * raycastOffset;
        bool sideHit = Physics.Raycast(sideRayOrigin, Vector3.down, out RaycastHit sideHitInfo, raycastDistance, groundLayer);

        // --- 이하 로직은 모두 동일 ---
        if (centerHit && forwardHit && sideHit)
        {
            info.IsContacting = true;
            info.Gap = (transform.position.y - wheelRadius) - centerHitInfo.point.y;

            Vector3 forwardVector = forwardHitInfo.point - centerHitInfo.point;
            Vector3 sideVector = sideHitInfo.point - centerHitInfo.point;
            info.GroundNormal = Vector3.Cross(forwardVector, sideVector).normalized;
        }
        else
        {
            info.IsContacting = false;
            info.Gap = 1.0f;
            info.GroundNormal = Vector3.up;
        }

        return info;
    }

#if UNITY_EDITOR
    // 디버깅을 위한 기즈모 시각화 함수
    void OnDrawGizmos()
    {
        // 컨트롤러에서 섀시 정보를 받아오기 전일 수 있으므로 null 체크
        // 실행 중에는 컨트롤러의 transform을 직접 할당해주는 것이 더 정확합니다.
        Transform chassisTransformForGizmo = transform.parent; // 부모를 섀시로 가정
        if (chassisTransformForGizmo == null) return;

        // --- 현재 레이캐스트가 어떻게 나가는지 시각화 ---
        Color gizmoColor = Color.yellow;

        // 1. 중앙 레이
        Vector3 centerRayOrigin = transform.position;
        Gizmos.color = gizmoColor;
        Gizmos.DrawLine(centerRayOrigin, centerRayOrigin + Vector3.down * raycastDistance);

        // 2. 앞쪽/옆쪽 레이의 기준점 계산 (현재 문제의 원인)
        // 이 부분이 바퀴(transform)의 회전을 그대로 따라가서 문제가 됨
        Vector3 forwardRayOrigin = transform.position + transform.forward * raycastOffset;
        Vector3 sideRayOrigin = transform.position + transform.right * raycastOffset;
        Debug.Log(chassisTransform.forward);
        // 올바른 기준: 섀시의 방향을 따라가야 함
        // Vector3 correctForwardRayOrigin = transform.position + chassisTransformForGizmo.forward * raycastOffset;
        // Vector3 correctSideRayOrigin = transform.position + chassisTransformForGizmo.right * raycastOffset;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(forwardRayOrigin, 0.05f);
        Gizmos.DrawSphere(sideRayOrigin, 0.05f);
    }
#endif
}

