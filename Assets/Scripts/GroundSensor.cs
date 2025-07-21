using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    [Header("Object References")]
    [Tooltip("ȸ������ �ʴ� �ι��� ���� ��ü(����)�� Transform�� �����ϼ���.")]
    public Transform chassisTransform; // ���ø� �������� ��� ���� ����

    [Header("Wheel Settings")]
    [Tooltip("������ ������ (m)")]
    [SerializeField] private float wheelRadius = 0.5f;

    [Header("Raycast Settings")]
    [Tooltip("����ĳ��Ʈ�� �ִ� ���� �Ÿ� (m)")]
    [SerializeField] private float raycastDistance = 1.0f;

    [Tooltip("���� ���� ������ ���� ��/�� ����ĳ��Ʈ�� ������ �Ÿ� (m)")]
    [SerializeField] private float raycastOffset = 0.1f;

    [Tooltip("�������� �ν��� ���̾�")]
    [SerializeField] private LayerMask groundLayer;

    // ���� GetContactInfo�� �Ķ���͸� ���� �ʽ��ϴ�.
    public GroundContactInfo GetContactInfo()
    {
        var info = new GroundContactInfo();

        // ���� Transform�� �Ҵ���� �ʾ����� ���� ����
        if (chassisTransform == null)
        {
            Debug.LogError("Chassis Transform is not assigned in the GroundSensor!", this.gameObject);
            return new GroundContactInfo { IsContacting = false, Gap = 1.0f, GroundNormal = Vector3.up };
        }

        // 1. �� �������� Raycast�� ����
        bool centerHit = Physics.Raycast(transform.position, Vector3.down, out RaycastHit centerHitInfo, raycastDistance, groundLayer);

        // [���� ����] �ι� ��ü(����)�� ��/�� ������ �������� ������ ��ġ�� ���
        Vector3 forwardRayOrigin = transform.position + transform.forward * raycastOffset;
        bool forwardHit = Physics.Raycast(forwardRayOrigin, Vector3.down, out RaycastHit forwardHitInfo, raycastDistance, groundLayer);

        Vector3 sideRayOrigin = transform.position + transform.right * raycastOffset;
        bool sideHit = Physics.Raycast(sideRayOrigin, Vector3.down, out RaycastHit sideHitInfo, raycastDistance, groundLayer);

        // --- ���� ������ ��� ���� ---
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
    // ������� ���� ����� �ð�ȭ �Լ�
    void OnDrawGizmos()
    {
        // ��Ʈ�ѷ����� ���� ������ �޾ƿ��� ���� �� �����Ƿ� null üũ
        // ���� �߿��� ��Ʈ�ѷ��� transform�� ���� �Ҵ����ִ� ���� �� ��Ȯ�մϴ�.
        Transform chassisTransformForGizmo = transform.parent; // �θ� ���÷� ����
        if (chassisTransformForGizmo == null) return;

        // --- ���� ����ĳ��Ʈ�� ��� �������� �ð�ȭ ---
        Color gizmoColor = Color.yellow;

        // 1. �߾� ����
        Vector3 centerRayOrigin = transform.position;
        Gizmos.color = gizmoColor;
        Gizmos.DrawLine(centerRayOrigin, centerRayOrigin + Vector3.down * raycastDistance);

        // 2. ����/���� ������ ������ ��� (���� ������ ����)
        // �� �κ��� ����(transform)�� ȸ���� �״�� ���󰡼� ������ ��
        Vector3 forwardRayOrigin = transform.position + transform.forward * raycastOffset;
        Vector3 sideRayOrigin = transform.position + transform.right * raycastOffset;
        Debug.Log(chassisTransform.forward);
        // �ùٸ� ����: ������ ������ ���󰡾� ��
        // Vector3 correctForwardRayOrigin = transform.position + chassisTransformForGizmo.forward * raycastOffset;
        // Vector3 correctSideRayOrigin = transform.position + chassisTransformForGizmo.right * raycastOffset;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(forwardRayOrigin, 0.05f);
        Gizmos.DrawSphere(sideRayOrigin, 0.05f);
    }
#endif
}

