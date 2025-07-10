// GroundSensor.cs

using UnityEngine;
using System.Collections.Generic;

public class GroundSensor : MonoBehaviour
{
    [Header("Sensor Setup")]
    public LayerMask groundLayer;
    [Tooltip("����ĳ��Ʈ�� �� �ִ� �Ÿ�")]
    public float maxDistance = 1.0f;

    [Header("4-Point Raycast Offsets")]
    [Tooltip("�� �����µ��� �����Ͽ� 4���� ����ĳ��Ʈ ��ġ�� �����մϴ�.")]
    public Vector3[] raycastOffsets = new Vector3[4]
    {
        new Vector3( 0.1f, 0,  0.1f), // ��-������
        new Vector3(-0.1f, 0,  0.1f), // ��-����
        new Vector3( 0.1f, 0, -0.1f), // ��-������
        new Vector3(-0.1f, 0, -0.1f)  // ��-����
    };

    // --- ���� ��� ������ ---
    public GroundContactInfo ContactInfo { get; private set; }


    void FixedUpdate()
    {
        UpdateGroundContact();
    }

    /// <summary>
    /// 4�� ����ĳ��Ʈ�� �����ϰ� ���� ������ ������Ʈ�մϴ�.
    /// </summary>
    private void UpdateGroundContact()
    {
        List<RaycastHit> hits = new List<RaycastHit>();
        Vector3 rayDirection = -transform.up; // �׻� ������ �Ʒ� ����

        // 4���� ������ ��ġ���� ���� ����ĳ��Ʈ ����
        foreach (var offset in raycastOffsets)
        {
            // ���� ��ǥ�� ���� ����ĳ��Ʈ ������ ���
            Vector3 rayOrigin = transform.position + transform.rotation * offset;

            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, maxDistance, groundLayer))
            {
                hits.Add(hit);
            }
        }

        // 4���� ���̰� ��� ���鿡 ����� ���� �������� ����
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
    /// 4���� �浹 ����(Hit)���κ��� ��� ��ġ�� ���� ���͸� ����մϴ�.
    /// </summary>
    private GroundContactInfo CalculateContactFromHits(List<RaycastHit> hits)
    {
        // 1. ��� ������ ���
        Vector3 avgContactPoint = Vector3.zero;
        foreach (var hit in hits)
        {
            avgContactPoint += hit.point;
        }
        avgContactPoint /= hits.Count;

        // 2. �� ���� �ﰢ���� ����� ��� ���� ���� ��� (�� �������� ���)
        Vector3 v1 = hits[1].point - hits[0].point; // ��-���� -> ��-������
        Vector3 v2 = hits[2].point - hits[0].point; // ��-������ -> ��-������
        Vector3 normal1 = Vector3.Cross(v1, v2);

        Vector3 v3 = hits[3].point - hits[2].point; // ��-���� -> ��-������
        Vector3 v4 = hits[0].point - hits[2].point; // ��-������ -> ��-������
        Vector3 normal2 = Vector3.Cross(v3, v4);

        Vector3 avgNormal = (normal1 + normal2).normalized;

        // ���� ���Ͱ� �׻� ���� ���ϵ��� ����
        if (Vector3.Dot(avgNormal, transform.up) < 0)
        {
            avgNormal = -avgNormal;
        }

        return new GroundContactInfo(true, avgContactPoint, avgNormal);
    }

    // ������ �ð�ȭ
    void OnDrawGizmosSelected()
    {
        if (raycastOffsets == null || raycastOffsets.Length == 0) return;

        Gizmos.color = IsGrounded() ? Color.green : Color.yellow;
        Vector3 rayDirection = -transform.up;

        foreach (var offset in raycastOffsets)
        {
            // �����Ϳ����� ��Ȯ�� ��ġ�� ���� ���� ���� ��ġ ���
            Vector3 rayOrigin = transform.position + transform.rotation * offset;
            Gizmos.DrawSphere(rayOrigin, 0.02f);
            Gizmos.DrawLine(rayOrigin, rayOrigin + rayDirection * maxDistance);
        }

        // ���� ���� ���� �׸���
        if (IsGrounded())
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(ContactInfo.ContactPoint, ContactInfo.ContactPoint + ContactInfo.ContactNormal * 0.3f);
        }
    }

    // ������ ���� Ȯ�ο� ���� �Լ�
    public bool IsGrounded()
    {
        return ContactInfo.IsContacting;
    }
}