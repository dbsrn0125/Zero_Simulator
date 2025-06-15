using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    [Header("Set Up")]
    public LayerMask groundLayer;
    public float maxDistance = 1.0f;
    public float wheelRadius = 0.093f;

    [Header("Raycast Offset")]
    public Vector3 centerOffset = Vector3.zero;
    public Vector3 forwardOffset = new Vector3(0, 0, 0.1f);
    public Vector3 sideOffset = new Vector3(0.1f, 0, 0);

    // --- 출력 데이터 ---
    public bool IsContacting { get; private set; }
    public float GroundHeight { get; private set; }
    public float GroundPitch { get; private set; }
    public float GroundRoll { get; private set; }

    void FixedUpdate()
    {
        // ======================== 이 부분이 스케일 문제를 해결합니다 ========================
        // TransformPoint 대신, 스케일의 영향을 받지 않도록 직접 월드 위치를 계산합니다.
        // (현재 바퀴의 월드 위치) + (현재 바퀴의 월드 회전 * 로컬 오프셋)
        Vector3 ray_origin_center = transform.position + transform.rotation * centerOffset;
        Vector3 ray_origin_forward = transform.position + transform.rotation * forwardOffset;
        Vector3 ray_origin_side = transform.position + transform.rotation * sideOffset;
        // ==============================================================================

        Vector3 ray_direction = -transform.up;

        // 레이캐스트 실행
        RaycastHit hit_c, hit_f, hit_s;
        bool did_hit_c = Physics.Raycast(ray_origin_center, ray_direction, out hit_c, maxDistance, groundLayer);
        bool did_hit_f = Physics.Raycast(ray_origin_forward, ray_direction, out hit_f, maxDistance, groundLayer);
        bool did_hit_s = Physics.Raycast(ray_origin_side, ray_direction, out hit_s, maxDistance, groundLayer);

        if (did_hit_c && did_hit_f && did_hit_s)
        {
            IsContacting = true;
            GroundHeight = hit_c.point.y;

            Vector3 local_hit_c = transform.InverseTransformPoint(hit_c.point);
            Vector3 local_hit_f = transform.InverseTransformPoint(hit_f.point);
            Vector3 local_hit_s = transform.InverseTransformPoint(hit_s.point);

            Vector3 forward_vec = local_hit_f - local_hit_c;
            Vector3 side_vec = local_hit_s - local_hit_c;

            GroundPitch = Mathf.Atan2(forward_vec.y, forward_vec.z);
            GroundRoll = Mathf.Atan2(side_vec.y, side_vec.x);
        }
        else
        {
            IsContacting = false;
            GroundHeight = transform.position.y - wheelRadius-100;
            GroundPitch = 0f;
            GroundRoll = 0f;
        }
    }

    // 디버깅용 시각화
    void OnDrawGizmosSelected()
    {
        // OnDrawGizmosSelected도 FixedUpdate와 동일한 방식으로 월드 위치를 계산해야
        // 씬 뷰에서 기즈모가 정확한 위치에 그려집니다.
        if (!Application.isPlaying) return;

        Gizmos.color = Color.yellow;
        Vector3 world_offset_c = transform.position + transform.rotation * centerOffset;
        Vector3 world_offset_f = transform.position + transform.rotation * forwardOffset;
        Vector3 world_offset_s = transform.position + transform.rotation * sideOffset;

        Gizmos.DrawSphere(world_offset_c, 0.02f);
        Gizmos.DrawSphere(world_offset_f, 0.02f);
        Gizmos.DrawSphere(world_offset_s, 0.02f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(world_offset_c, world_offset_c - transform.up * maxDistance);
        Gizmos.DrawLine(world_offset_f, world_offset_f - transform.up * maxDistance);
        Gizmos.DrawLine(world_offset_s, world_offset_s - transform.up * maxDistance);
    }
}