using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    [Header("References & Setup")]
    public Transform wheelTransform;
    public Vector3[] rayOriginsLocal = new Vector3[3];
    public LayerMask groundLayer;
    public float maxRayDistance = 2.0f;
    public float nominalWheelRadius_R0 = 0.093f;

    [Header("Debug Visualization")]
    public float normalRayLength = 0.3f;
    public float gizmoRadius = 0.02f;
    public bool drawDebugRays = true;

    // Output Data
    public bool IsContacting { get; private set; }
    public float CalculatedPenetrationDepth { get; private set; }
    public Vector3 CalculatedWorldNormal { get; private set; }
    public float CurrentFrictionCoefficient { get; private set; } // 이 값을 어떻게 결정할지 로직 추가 필요
    public Vector3 AverageContactPoint_World { get; private set; }

    // 내부 계산용 변수들 (길이가 rayOriginsLocal.Length와 같아야 함)
    private float[] rayDistances; // Awake에서 초기화
    private Vector3[] hitPoints_World; // Awake에서 초기화
    private int contactCount = 0;

    void Awake()
    {
        // wheelTransform 할당 로직은 이전과 동일 (Inspector 할당 권장)
        if (wheelTransform == null)
        {
            if (transform.parent != null)
            {
                wheelTransform = transform.parent;
                Debug.LogWarningFormat("{0}: 'wheelTransform'이 Inspector에 할당되지 않아 부모 Transform ({1})을 사용합니다.", gameObject.name, wheelTransform.name);
            }
            // ... (이하 null 체크 및 비활성화 로직 동일) ...
        }

        if (rayOriginsLocal == null || rayOriginsLocal.Length == 0)
        {
            Debug.LogErrorFormat("{0}: 'rayOriginsLocal' 배열이 설정되지 않았습니다! Inspector에서 Ray 시작점 오프셋을 정의해주세요.", gameObject.name);
            this.enabled = false;
            return;
        }

        // 배열 초기화 (rayOriginsLocal.Length 사용)
        rayDistances = new float[rayOriginsLocal.Length];
        hitPoints_World = new Vector3[rayOriginsLocal.Length];
    }

    void FixedUpdate()
    {
        if (wheelTransform == null) return;

        IsContacting = false;
        contactCount = 0;
        Vector3 accumulatedNormal = Vector3.zero;
        Vector3 accumulatedHitPoint = Vector3.zero;

        for (int i = 0; i < rayOriginsLocal.Length; i++)
        {
            Vector3 worldOffset = wheelTransform.rotation * rayOriginsLocal[i];
            Vector3 rayStartPoint_World = wheelTransform.position + worldOffset;
            Vector3 rayDirection_World = -wheelTransform.up;

            RaycastHit hitInfo;
            if (Physics.Raycast(rayStartPoint_World, rayDirection_World, out hitInfo, maxRayDistance, groundLayer))
            {
                IsContacting = true;
                contactCount++;
                rayDistances[i] = hitInfo.distance;
                accumulatedNormal += hitInfo.normal;
                hitPoints_World[i] = hitInfo.point;
                accumulatedHitPoint += hitInfo.point;

                if (drawDebugRays) Debug.DrawRay(rayStartPoint_World, rayDirection_World * hitInfo.distance, Color.green);
            }
            else
            {
                rayDistances[i] = maxRayDistance;
                hitPoints_World[i] = rayStartPoint_World + rayDirection_World * maxRayDistance;
                if (drawDebugRays) Debug.DrawRay(rayStartPoint_World, rayDirection_World * maxRayDistance, Color.red);
            }
        }

        if (contactCount > 0)
        {
            CalculatedWorldNormal = (accumulatedNormal / contactCount).normalized;
            AverageContactPoint_World = accumulatedHitPoint / contactCount;

            float min_L_ray = maxRayDistance;
            // 실제 충돌한 Ray들 중에서 min_L_ray를 찾아야 함
            bool actualHitOccurred = false;
            for (int i = 0; i < rayOriginsLocal.Length; i++)
            {
                if (rayDistances[i] < maxRayDistance) // 실제 충돌이 일어난 Ray
                {
                    if (rayDistances[i] < min_L_ray)
                    {
                        min_L_ray = rayDistances[i];
                    }
                    actualHitOccurred = true;
                }
            }

            // 실제 충돌이 있었을 때만 penetration depth 계산
            CalculatedPenetrationDepth = actualHitOccurred ? Mathf.Max(0, nominalWheelRadius_R0 - min_L_ray) : 0f;

            // 마찰 계수 결정 로직 (TODO)
            // 예시: RaycastHit의 collider 태그나 material을 보고 결정
            // RaycastHit firstValidHit = new RaycastHit(); // 첫번째 유효 충돌 정보 찾기
            // for(int i=0; i<rayOriginsLocal.Length; ++i) { if(rayDistances[i] < maxRayDistance) { /* 어떻게 hitInfo를 가져올지 고민 필요. Physics.Raycast를 다시 쏘거나, RaycastHit[]을 저장 */ break; }}
            // CurrentFrictionCoefficient = DetermineFrictionFromHit(firstValidHit); 
            CurrentFrictionCoefficient = 0.7f; // 임시 고정값

            if (drawDebugRays) Debug.DrawRay(AverageContactPoint_World, CalculatedWorldNormal * normalRayLength, Color.magenta, Time.fixedDeltaTime);
        }
        else
        {
            CalculatedPenetrationDepth = 0f;
            CalculatedWorldNormal = Vector3.up;
            CurrentFrictionCoefficient = 0.7f;
            AverageContactPoint_World = wheelTransform.position - wheelTransform.up * nominalWheelRadius_R0;
        }
    }

    // OnDrawGizmosSelected()는 이전과 거의 동일, referenceTransform 부분만 명확히
    void OnDrawGizmosSelected()
    {
        Transform currentWheelTransformForGizmos = wheelTransform; // 현재 할당된 wheelTransform 사용
        if (currentWheelTransformForGizmos == null && !Application.isPlaying)
        {
            // 에디터에서만, 그리고 wheelTransform이 아직 할당 안됐을 때 임시로 부모나 자신을 시도
            if (transform.parent != null) currentWheelTransformForGizmos = transform.parent;
            else currentWheelTransformForGizmos = transform;
        }

        if (currentWheelTransformForGizmos == null || rayOriginsLocal == null || rayOriginsLocal.Length == 0) return;

        Gizmos.color = Color.yellow;
        foreach (Vector3 localOffset in rayOriginsLocal)
        {
            Vector3 worldOffset = currentWheelTransformForGizmos.rotation * localOffset;
            Vector3 worldPoint = currentWheelTransformForGizmos.position + worldOffset;
            Gizmos.DrawSphere(worldPoint, gizmoRadius);
        }

        if (Application.isPlaying && IsContacting && drawDebugRays)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(AverageContactPoint_World, CalculatedWorldNormal * normalRayLength);
        }
    }

    // (선택적) 마찰계수 결정 헬퍼 함수 예시
    // private float DetermineFrictionFromHit(RaycastHit hit)
    // {
    //     if (hit.collider != null)
    //     {
    //         if (hit.collider.CompareTag("Asphalt")) return 0.8f;
    //         if (hit.collider.CompareTag("Sand")) return 0.3f;
    //         // ... 기타 지면 종류 ...
    //     }
    //     return 0.7f; // 기본값
    // }
}