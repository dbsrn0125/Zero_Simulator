// LaunchController.cs (���� �������� ���� ���� ����)

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
    public ScrollRect logScrollRect; // Scroll View ��ü�� ����
    [Header("Prefabs & Settings")]
    public GameObject logEntryPrefab;

    public int maxLogCount = 200;

    private Queue<GameObject> logQueue = new Queue<GameObject>();

    private ROSConnection ros;

    // ? 1. ��ũ��Ʈ�� Ȱ��ȭ�� �� ROSManager�� �̺�Ʈ�� '����'�� ��û�մϴ�.
    void OnEnable()
    {
        ROSManager.OnRosConnectionReady += Initialize;
    }

    // ? 2. ��ũ��Ʈ�� ��Ȱ��ȭ�� �� '����'�� �����մϴ�. (�޸� ���� ����)
    void OnDisable()
    {
        ROSManager.OnRosConnectionReady -= Initialize;
    }

    void Start()
    {
        // ó������ ��ư�� ��Ȱ��ȭ�ؼ�, ������ �Ǳ� ���� ������ ���� �����մϴ�.
        launchButton.interactable = false;
        statusText.text = "Waiting for ROS Connection...";
        statusText.color = Color.gray;
    }

    // ? 3. ROSManager�� �غ� ��ȣ�� ������ �� �Լ��� ȣ��˴ϴ�.
    private void Initialize()
    {
        Debug.Log("LaunchController: ROS Connection is ready. Initializing.");

        // �� �������� ROSManager.instance�� �׻� �غ�� �����Դϴ�.
        ros = ROSManager.instance.ROSConnection;

        // ���� ����
        ros.Subscribe<StringMsg>("/zenith/launch_feedback", OnFeedback);
        ros.Subscribe<LaunchResultMsg>("/zenith/launch_result", OnResult);
        ros.RegisterRosService<StartLaunchRequest, StartLaunchResponse>("/zenith/start_launch");
        // ��ư ������ ����
        launchButton.onClick.AddListener(OnLaunchButtonClick);

        // ��� �غ� �������� ��ư�� Ȱ��ȭ�մϴ�.
        launchButton.interactable = true;
        statusText.text = "Ready to Launch.";
        statusText.color = Color.green;
    }

    private async void OnLaunchButtonClick()
    {
        // 'ros' ������ Initialize �Լ����� �̹� �Ҵ�Ǿ����Ƿ� null�� �� �� �����ϴ�.
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
        // ���� �������� ��� UI ����� ���� ������ ��ٸ��ϴ�.
        yield return new WaitForEndOfFrame();

        // ���� ��ũ�� ��ġ�� �� �Ʒ�(0)�� �����մϴ�.
        logScrollRect.verticalNormalizedPosition = 0f;
    }
}