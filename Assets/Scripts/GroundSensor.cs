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
    public float CurrentFrictionCoefficient { get; private set; } // �� ���� ��� �������� ���� �߰� �ʿ�
    public Vector3 AverageContactPoint_World { get; private set; }

    // ���� ���� ������ (���̰� rayOriginsLocal.Length�� ���ƾ� ��)
    private float[] rayDistances; // Awake���� �ʱ�ȭ
    private Vector3[] hitPoints_World; // Awake���� �ʱ�ȭ
    private int contactCount = 0;

    void Awake()
    {
        // wheelTransform �Ҵ� ������ ������ ���� (Inspector �Ҵ� ����)
        if (wheelTransform == null)
        {
            if (transform.parent != null)
            {
                wheelTransform = transform.parent;
                Debug.LogWarningFormat("{0}: 'wheelTransform'�� Inspector�� �Ҵ���� �ʾ� �θ� Transform ({1})�� ����մϴ�.", gameObject.name, wheelTransform.name);
            }
            // ... (���� null üũ �� ��Ȱ��ȭ ���� ����) ...
        }

        if (rayOriginsLocal == null || rayOriginsLocal.Length == 0)
        {
            Debug.LogErrorFormat("{0}: 'rayOriginsLocal' �迭�� �������� �ʾҽ��ϴ�! Inspector���� Ray ������ �������� �������ּ���.", gameObject.name);
            this.enabled = false;
            return;
        }

        // �迭 �ʱ�ȭ (rayOriginsLocal.Length ���)
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
            // ���� �浹�� Ray�� �߿��� min_L_ray�� ã�ƾ� ��
            bool actualHitOccurred = false;
            for (int i = 0; i < rayOriginsLocal.Length; i++)
            {
                if (rayDistances[i] < maxRayDistance) // ���� �浹�� �Ͼ Ray
                {
                    if (rayDistances[i] < min_L_ray)
                    {
                        min_L_ray = rayDistances[i];
                    }
                    actualHitOccurred = true;
                }
            }

            // ���� �浹�� �־��� ���� penetration depth ���
            CalculatedPenetrationDepth = actualHitOccurred ? Mathf.Max(0, nominalWheelRadius_R0 - min_L_ray) : 0f;

            // ���� ��� ���� ���� (TODO)
            // ����: RaycastHit�� collider �±׳� material�� ���� ����
            // RaycastHit firstValidHit = new RaycastHit(); // ù��° ��ȿ �浹 ���� ã��
            // for(int i=0; i<rayOriginsLocal.Length; ++i) { if(rayDistances[i] < maxRayDistance) { /* ��� hitInfo�� �������� ��� �ʿ�. Physics.Raycast�� �ٽ� ��ų�, RaycastHit[]�� ���� */ break; }}
            // CurrentFrictionCoefficient = DetermineFrictionFromHit(firstValidHit); 
            CurrentFrictionCoefficient = 0.7f; // �ӽ� ������

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

    // OnDrawGizmosSelected()�� ������ ���� ����, referenceTransform �κи� ��Ȯ��
    void OnDrawGizmosSelected()
    {
        Transform currentWheelTransformForGizmos = wheelTransform; // ���� �Ҵ�� wheelTransform ���
        if (currentWheelTransformForGizmos == null && !Application.isPlaying)
        {
            // �����Ϳ�����, �׸��� wheelTransform�� ���� �Ҵ� �ȵ��� �� �ӽ÷� �θ� �ڽ��� �õ�
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

    // (������) ������� ���� ���� �Լ� ����
    // private float DetermineFrictionFromHit(RaycastHit hit)
    // {
    //     if (hit.collider != null)
    //     {
    //         if (hit.collider.CompareTag("Asphalt")) return 0.8f;
    //         if (hit.collider.CompareTag("Sand")) return 0.3f;
    //         // ... ��Ÿ ���� ���� ...
    //     }
    //     return 0.7f; // �⺻��
    // }
}