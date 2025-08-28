// SystemEventManager.cs

using System; // Action�� ����ϱ� ���� �ʿ�
using UnityEngine;
public static class SystemEventManager
{
    // "���� ������ �غ�Ǿ���"�� ��ȣ�� ���� �̺�Ʈ
    public static event Action OnMainNodesReady;

    // �� �Լ��� ȣ���Ͽ� '�غ� �Ϸ�' ��ȣ�� ��ü�� ����մϴ�.
    public static void TriggerMainNodesReady()
    {
        Debug.Log("System Event: Main nodes are ready! Broadcasting event.");
        OnMainNodesReady?.Invoke();
    }
}