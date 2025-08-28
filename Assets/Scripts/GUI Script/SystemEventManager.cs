// SystemEventManager.cs

using System; // Action을 사용하기 위해 필요
using UnityEngine;
public static class SystemEventManager
{
    // "메인 노드들이 준비되었다"는 신호를 보낼 이벤트
    public static event Action OnMainNodesReady;

    // 이 함수를 호출하여 '준비 완료' 신호를 전체에 방송합니다.
    public static void TriggerMainNodesReady()
    {
        Debug.Log("System Event: Main nodes are ready! Broadcasting event.");
        OnMainNodesReady?.Invoke();
    }
}