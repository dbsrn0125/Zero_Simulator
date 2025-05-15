using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    // Inspector���� ���� ������ "�޽��� ����" GameObject�� Transform�� �Ҵ�
    public Transform wheelMeshTransform; // ��: "0.6_Tirefinal.stp-1" Transform

    public Vector3[] rayOriginsLocal = new Vector3[3]; // ���� "���� �޽��� ���� �߽�" ���� ���� ������
    public LayerMask groundLayer;
    public float maxRayDistance = 2.0f;
    public float normalRayLength = 0.3f;
    public float gizmoRadius = 0.02f;

    public bool isContacting { get; private set; }
    public float[] rayDistances { get; private set; } = new float[3];
    public Vector3 groundNormal_World { get; private set; }
    private Vector3[] hitPoints_World = new Vector3[3];
    private int contactCount = 0;

    private Vector3 actualWheelCenter_World_Cached; // �Ź� ������� �ʵ��� ĳ�� ����
    private Quaternion wheelRotation_World_Cached;

    void Awake()
    {
        if (wheelMeshTransform == null)
        {
            Debug.LogError(gameObject.name + ": 'Wheel Mesh Transform'�� Inspector���� �Ҵ���� �ʾҽ��ϴ�!", this.gameObject);
            this.enabled = false;
            return;
        }
    }

    void FixedUpdate()
    {
        if (wheelMeshTransform == null) return;

        // 1. ���� �޽��� ���� ���� �߽ɰ� ���� ���� ȸ���� �����ɴϴ�.
        // MeshFilter�� ���� �޽��� ���� �ٿ�� �ڽ� �߽��� ������ ���� ��ǥ�� ��ȯ�մϴ�.
        MeshFilter meshFilter = wheelMeshTransform.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError(gameObject.name + ": 'Wheel Mesh Transform'�� MeshFilter �Ǵ� Mesh�� �����ϴ�!", wheelMeshTransform.gameObject);
            actualWheelCenter_World_Cached = wheelMeshTransform.position; //Fallback: Pivot ��ġ ���
        }
        else
        {
            // �޽��� ���� �߽����� �����ͼ�, wheelMeshTransform�� ���� ���� ��ȯ�� ����
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
            // ���� rayOriginsLocal�� "���� �޽��� ���� �߽�"�� (0,0,0)���� ���� ���� ���� �������Դϴ�.
            Vector3 worldOffset = wheelRotation_World_Cached * rayOriginsLocal[i];
            Vector3 rayStartPoint_World = actualWheelCenter_World_Cached + worldOffset;

            Vector3 rayDirection_World = -(wheelRotation_World_Cached * Vector3.up); // ������ ���� '�Ʒ�' ���� (���� ����)

            RaycastHit hitInfo;
            if (Physics.Raycast(rayStartPoint_World, rayDirection_World, out hitInfo, maxRayDistance, groundLayer))
            {
                // ... (������ ������ �浹 ó�� ����) ...
                isContacting = true;
                contactCount++;
                rayDistances[i] = hitInfo.distance;
                accumulatedNormal += hitInfo.normal;
                hitPoints_World[i] = hitInfo.point;
                Debug.DrawRay(rayStartPoint_World, rayDirection_World * hitInfo.distance, Color.green);
            }
            else
            {
                // ... (������ ������ ���浹 ó�� ����) ...
                rayDistances[i] = maxRayDistance;
                hitPoints_World[i] = rayStartPoint_World + rayDirection_World * maxRayDistance;
                Debug.DrawRay(rayStartPoint_World, rayDirection_World * maxRayDistance, Color.red);
            }
        }

        // ... (groundNormal_World ��� �� ���� ���� �׸��� ������ ������ �����ϰ�,
        //      ��, Debug Ray �������� actualWheelCenter_World_Cached �������� �ϰų� representativeContactPoint ���) ...
        if (contactCount > 0)
        {
            groundNormal_World = (accumulatedNormal / contactCount).normalized;
            // ���� ���� �׸��� (��: ���� ���� ���� �߽ɿ���)
            Debug.DrawRay(actualWheelCenter_World_Cached, groundNormal_World * normalRayLength, Color.magenta, Time.fixedDeltaTime);
        }
        else
        {
            groundNormal_World = Vector3.up;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Gizmo�� �׸� ���� ���� ���� �޽��� �߽��� �������� rayOriginsLocal �������� �ð�ȭ�ؾ� �մϴ�.
        if (wheelMeshTransform == null)
        {
            if (!Application.isPlaying && transform.parent != null) wheelMeshTransform = transform.parent; // �����Ϳ� �ӽ�
            else if (!Application.isPlaying) wheelMeshTransform = transform;
            if (wheelMeshTransform == null) return;
        }

        Vector3 effectiveCenter_World;
        Quaternion effectiveRotation_World = wheelMeshTransform.rotation; // ȸ���� Pivot�� ȸ���� �״�� ���

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