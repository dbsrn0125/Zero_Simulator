// LaunchController.cs (연결 안정성을 높인 최종 버전)

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.ZenithInterfaces;
using RosMessageTypes.Std;
using System.Collections;
using System.Collections.Generic;

public class LaunchController : MonoBehaviour
{
    [Header("UI Elements")]
    public Button launchButton;
    public TMP_Text statusText;
    public ScrollRect logScrollRect; // Scroll View 자체를 연결
    [Header("Prefabs & Settings")]
    public GameObject logEntryPrefab;

    public int maxLogCount = 200;

    private Queue<GameObject> logQueue = new Queue<GameObject>();

    private ROSConnection ros;

    // ? 1. 스크립트가 활성화될 때 ROSManager의 이벤트에 '구독'을 신청합니다.
    void OnEnable()
    {
        ROSManager.OnRosConnectionReady += Initialize;
    }

    // ? 2. 스크립트가 비활성화될 때 '구독'을 해제합니다. (메모리 누수 방지)
    void OnDisable()
    {
        ROSManager.OnRosConnectionReady -= Initialize;
    }

    void Start()
    {
        // 처음에는 버튼을 비활성화해서, 연결이 되기 전에 누르는 것을 방지합니다.
        launchButton.interactable = false;
        statusText.text = "Waiting for ROS Connection...";
        statusText.color = Color.gray;
    }

    // ? 3. ROSManager가 준비 신호를 보내면 이 함수가 호출됩니다.
    private void Initialize()
    {
        Debug.Log("LaunchController: ROS Connection is ready. Initializing.");

        // 이 시점에는 ROSManager.instance가 항상 준비된 상태입니다.
        ros = ROSManager.instance.ROSConnection;

        // 토픽 구독
        ros.Subscribe<StringMsg>("/zenith/launch_feedback", OnFeedback);
        ros.Subscribe<LaunchResultMsg>("/zenith/launch_result", OnResult);
        ros.RegisterRosService<StartLaunchRequest, StartLaunchResponse>("/zenith/start_launch");
        // 버튼 리스너 연결
        launchButton.onClick.AddListener(OnLaunchButtonClick);

        // 모든 준비가 끝났으니 버튼을 활성화합니다.
        launchButton.interactable = true;
        statusText.text = "Ready to Launch.";
        statusText.color = Color.green;
    }

    private async void OnLaunchButtonClick()
    {
        // 'ros' 변수는 Initialize 함수에서 이미 할당되었으므로 null이 될 수 없습니다.
        if (!ros.HasConnectionThread)
        {
            statusText.text = "Error: Not connected!";
            statusText.color = Color.red;
            return;
        }

        string packageName = "zenith_bringup";
        string launchFileName = "zenith_main.launch.py";

        if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(launchFileName))
        {
            statusText.text = "Error: Package and file name are required!";
            statusText.color = Color.red;
            return;
        }

        StartLaunchRequest request = new StartLaunchRequest(packageName, launchFileName);
        Debug.Log(request);
        statusText.text = $"Sending launch request...";
        statusText.color = Color.yellow;

        try
        {
            StartLaunchResponse response = await ros.SendServiceMessage<StartLaunchResponse>("/zenith/start_launch", request);
            if (response.success)
            {
                statusText.text = "Launch process started...";
                SystemEventManager.TriggerMainNodesReady();
            }
            else
            {
                statusText.text = "Launch command rejected: " + response.message;
                statusText.color = Color.red;
            }
        }
        catch (System.Exception e)
        {
            statusText.text = "Service call failed: " + e.Message;
            statusText.color = Color.red;
        }
    }

    void OnFeedback(StringMsg feedback)
    {
        if (logQueue.Count >= maxLogCount)
        {
            GameObject oldestLog = logQueue.Dequeue();
            Destroy(oldestLog);
        }

        Transform contentTransform = logScrollRect.content;
        GameObject newLogEntry = Instantiate(logEntryPrefab, contentTransform);

        TextMeshProUGUI logTextComponent = newLogEntry.GetComponentInChildren<TextMeshProUGUI>();
        if(logTextComponent != null )
        {
            logTextComponent.text=feedback.data;
        }
        logQueue.Enqueue(newLogEntry);
        StartCoroutine(ScrollToBottom());
    }

    void OnResult(LaunchResultMsg result)
    {
        Debug.Log($"Result: Success={result.success}, Message='{result.message}'");
        statusText.text = result.message;
        statusText.color = result.success ? Color.green : Color.red;
        StartCoroutine(ScrollToBottom());
    }

    IEnumerator ScrollToBottom()
    {
        // 현재 프레임의 모든 UI 계산이 끝날 때까지 기다립니다.
        yield return new WaitForEndOfFrame();

        // 이제 스크롤 위치를 맨 아래(0)로 설정합니다.
        logScrollRect.verticalNormalizedPosition = 0f;
    }
}