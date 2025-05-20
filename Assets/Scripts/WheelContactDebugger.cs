using UnityEngine;

public class WheelSensorTest : MonoBehaviour
{
    public WheelCollider wheelCollider;

    void Start()
    {
        if (wheelCollider == null)
        {
            wheelCollider = GetComponent<WheelCollider>();
        }
        // 초기 WheelCollider 설정 (이전 이미지 참고하여 설정)
        // 예: wheelCollider.radius = 0.5f;
        //     wheelCollider.suspensionDistance = 0.2f; // Suspension Distance 0이면 안됨!
        //     wheelCollider.suspensionSpring = new JointSpring { spring = 35000, damper = 4500, targetPosition = 0.5f };
    }

    void FixedUpdate()
    {
        WheelHit hit;
        if (wheelCollider.GetGroundHit(out hit))
        {
            // 지면과 접촉했을 때!
            Debug.Log("======= Ground Hit! =======");
            Debug.Log($"Contact Point: {hit.point}"); // 접촉점 월드 좌표
            Debug.Log($"Ground Normal: {hit.normal}"); // 지면의 법선 벡터
            Debug.Log($"Suspension Force Magnitude: {hit.force}"); // 서스펜션이 현재 미는 힘의 크기
            Debug.Log($"Forward Slip: {hit.forwardSlip}"); // 종방향 슬립률
            Debug.Log($"Sideways Slip: {hit.sidewaysSlip}"); // 횡방향 슬립률
            Debug.Log($"Collider: {hit.collider.name}"); // 부딪힌 콜라이더 이름
            Debug.Log($"Suspension Force Magnitude: {hit.force}");
            // Scene 뷰에 시각적으로 표시 (Gizmos 대용)
            Debug.DrawRay(hit.point, hit.normal * hit.force, Color.blue); // 법선 방향
            Debug.DrawRay(hit.point, wheelCollider.transform.up * -1f * wheelCollider.suspensionDistance, Color.yellow); // 서스펜션 전체 길이
        }
        else
        {
            // 지면과 접촉하지 않았을 때
            // Debug.Log("Not Grounded");
        }

        // 테스트를 위해 바퀴를 약간 아래로 눌러보는 코드 (선택 사항)
        // Rigidbody rb = GetComponent<Rigidbody>();
        // if (rb != null)
        // {
        //    rb.AddForce(Vector3.down * 10f);
        // }
    }

    // (선택 사항) Scene 뷰에 더 잘 보이게 Gizmos 사용
    void OnDrawGizmos()
    {
        if (wheelCollider == null) return;

        WheelHit hit;
        if (wheelCollider.GetGroundHit(out hit))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.point, 0.05f); // 접촉점

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(hit.point, hit.point + hit.normal * 0.5f); // 법선

            // 서스펜션 현재 상태 시각화
            // Gizmos.color = Color.green;
            // Vector3 suspensionStart = wheelCollider.transform.TransformPoint(wheelCollider.center);
            // Vector3 suspensionEnd = hit.point; // 실제 접촉점까지
            // Gizmos.DrawLine(suspensionStart, suspensionEnd);
        }
    }
}