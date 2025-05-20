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
        // �ʱ� WheelCollider ���� (���� �̹��� �����Ͽ� ����)
        // ��: wheelCollider.radius = 0.5f;
        //     wheelCollider.suspensionDistance = 0.2f; // Suspension Distance 0�̸� �ȵ�!
        //     wheelCollider.suspensionSpring = new JointSpring { spring = 35000, damper = 4500, targetPosition = 0.5f };
    }

    void FixedUpdate()
    {
        WheelHit hit;
        if (wheelCollider.GetGroundHit(out hit))
        {
            // ����� �������� ��!
            Debug.Log("======= Ground Hit! =======");
            Debug.Log($"Contact Point: {hit.point}"); // ������ ���� ��ǥ
            Debug.Log($"Ground Normal: {hit.normal}"); // ������ ���� ����
            Debug.Log($"Suspension Force Magnitude: {hit.force}"); // ��������� ���� �̴� ���� ũ��
            Debug.Log($"Forward Slip: {hit.forwardSlip}"); // ������ ������
            Debug.Log($"Sideways Slip: {hit.sidewaysSlip}"); // Ⱦ���� ������
            Debug.Log($"Collider: {hit.collider.name}"); // �ε��� �ݶ��̴� �̸�
            Debug.Log($"Suspension Force Magnitude: {hit.force}");
            // Scene �信 �ð������� ǥ�� (Gizmos ���)
            Debug.DrawRay(hit.point, hit.normal * hit.force, Color.blue); // ���� ����
            Debug.DrawRay(hit.point, wheelCollider.transform.up * -1f * wheelCollider.suspensionDistance, Color.yellow); // ������� ��ü ����
        }
        else
        {
            // ����� �������� �ʾ��� ��
            // Debug.Log("Not Grounded");
        }

        // �׽�Ʈ�� ���� ������ �ణ �Ʒ��� �������� �ڵ� (���� ����)
        // Rigidbody rb = GetComponent<Rigidbody>();
        // if (rb != null)
        // {
        //    rb.AddForce(Vector3.down * 10f);
        // }
    }

    // (���� ����) Scene �信 �� �� ���̰� Gizmos ���
    void OnDrawGizmos()
    {
        if (wheelCollider == null) return;

        WheelHit hit;
        if (wheelCollider.GetGroundHit(out hit))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.point, 0.05f); // ������

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(hit.point, hit.point + hit.normal * 0.5f); // ����

            // ������� ���� ���� �ð�ȭ
            // Gizmos.color = Color.green;
            // Vector3 suspensionStart = wheelCollider.transform.TransformPoint(wheelCollider.center);
            // Vector3 suspensionEnd = hit.point; // ���� ����������
            // Gizmos.DrawLine(suspensionStart, suspensionEnd);
        }
    }
}