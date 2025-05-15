using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    // Inspector에서 실제 바퀴의 "메쉬를 가진" GameObject의 Transform을 할당
    public Transform wheelMeshTransform; // 예: "0.6_Tirefinal.stp-1" Transform

    public Vector3[] rayOriginsLocal = new Vector3[3]; // 이제 "바퀴 메쉬의 실제 중심" 기준 로컬 오프셋
    public LayerMask groundLayer;
    public float maxRayDistance = 2.0f;
    public float normalRayLength = 0.3f;
    public float gizmoRadius = 0.02f;

    public bool isContacting { get; private set; }
    public float[] rayDistances { get; private set; } = new float[3];
    public Vector3 groundNormal_World { get; private set; }
    private Vector3[] hitPoints_World = new Vector3[3];
    private int contactCount = 0;

    private Vector3 actualWheelCenter_World_Cached; // 매번 계산하지 않도록 캐싱 가능
    private Quaternion wheelRotation_World_Cached;

    void Awake()
    {
        if (wheelMeshTransform == null)
        {
            Debug.LogError(gameObject.name + ": 'Wheel Mesh Transform'이 Inspector에서 할당되지 않았습니다!", this.gameObject);
            this.enabled = false;
            return;
        }
    }

    void FixedUpdate()
    {
        if (wheelMeshTransform == null) return;

        // 1. 바퀴 메쉬의 실제 월드 중심과 현재 월드 회전을 가져옵니다.
        // MeshFilter를 통해 메쉬의 로컬 바운딩 박스 중심을 가져와 월드 좌표로 변환합니다.
        MeshFilter meshFilter = wheelMeshTransform.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError(gameObject.name + ": 'Wheel Mesh Transform'에 MeshFilter 또는 Mesh가 없습니다!", wheelMeshTransform.gameObject);
            actualWheelCenter_World_Cached = wheelMeshTransform.position; //Fallback: Pivot 위치 사용
        }
        else
        {
            // 메쉬의 로컬 중심점을 가져와서, wheelMeshTransform의 현재 월드 변환을 적용
            Vector3 meshLocalCenter = meshFilter.sharedMesh.bounds.center;
            actualWheelCenter_World_Cached = wheelMeshTransform.TransformPoint(meshLocalCenter);
        }
        wheelRotation_World_Cached = wheelMeshTransform.rotation;


        // Debug.Log(gameObject.name + " Actual Wheel Center World: " + actualWheelCenter_World_Cached);

        isContacting = false;
        contactCount = 0;
        Vector3 accumulatedNormal = Vector3.zero;

        for (int i = 0; i < rayOriginsLocal.Length; i++)
        {
            // 이제 rayOriginsLocal은 "바퀴 메쉬의 실제 중심"을 (0,0,0)으로 봤을 때의 로컬 오프셋입니다.
            Vector3 worldOffset = wheelRotation_World_Cached * rayOriginsLocal[i];
            Vector3 rayStartPoint_World = actualWheelCenter_World_Cached + worldOffset;

            Vector3 rayDirection_World = -(wheelRotation_World_Cached * Vector3.up); // 바퀴의 로컬 '아래' 방향 (월드 기준)

            RaycastHit hitInfo;
            if (Physics.Raycast(rayStartPoint_World, rayDirection_World, out hitInfo, maxRayDistance, groundLayer))
            {
                // ... (이전과 동일한 충돌 처리 로직) ...
                isContacting = true;
                contactCount++;
                rayDistances[i] = hitInfo.distance;
                accumulatedNormal += hitInfo.normal;
                hitPoints_World[i] = hitInfo.point;
                Debug.DrawRay(rayStartPoint_World, rayDirection_World * hitInfo.distance, Color.green);
            }
            else
            {
                // ... (이전과 동일한 비충돌 처리 로직) ...
                rayDistances[i] = maxRayDistance;
                hitPoints_World[i] = rayStartPoint_World + rayDirection_World * maxRayDistance;
                Debug.DrawRay(rayStartPoint_World, rayDirection_World * maxRayDistance, Color.red);
            }
        }

        // ... (groundNormal_World 계산 및 법선 벡터 그리기 로직은 이전과 유사하게,
        //      단, Debug Ray 시작점을 actualWheelCenter_World_Cached 기준으로 하거나 representativeContactPoint 사용) ...
        if (contactCount > 0)
        {
            groundNormal_World = (accumulatedNormal / contactCount).normalized;
            // 법선 벡터 그리기 (예: 계산된 실제 바퀴 중심에서)
            Debug.DrawRay(actualWheelCenter_World_Cached, groundNormal_World * normalRayLength, Color.magenta, Time.fixedDeltaTime);
        }
        else
        {
            groundNormal_World = Vector3.up;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Gizmo를 그릴 때도 실제 바퀴 메쉬의 중심을 기준으로 rayOriginsLocal 오프셋을 시각화해야 합니다.
        if (wheelMeshTransform == null)
        {
            if (!Application.isPlaying && transform.parent != null) wheelMeshTransform = transform.parent; // 에디터용 임시
            else if (!Application.isPlaying) wheelMeshTransform = transform;
            if (wheelMeshTransform == null) return;
        }

        Vector3 effectiveCenter_World;
        Quaternion effectiveRotation_World = wheelMeshTransform.rotation; // 회전은 Pivot의 회전을 그대로 사용

        MeshFilter mf = wheelMeshTransform.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            effectiveCenter_World = wheelMeshTransform.TransformPoint(mf.sharedMesh.bounds.center);
        }
        else
        {
            effectiveCenter_World = wheelMeshTransform.position; // Fallback
        }

        Gizmos.color = Color.yellow;
        if (rayOriginsLocal != null)
        {
            foreach (Vector3 localOffsetFromTrueCenter in rayOriginsLocal)
            {
                Vector3 worldOffset = effectiveRotation_World * localOffsetFromTrueCenter;
                Vector3 worldPoint = effectiveCenter_World + worldOffset;
                Gizmos.DrawSphere(worldPoint, gizmoRadius);
            }
        }
    }
}