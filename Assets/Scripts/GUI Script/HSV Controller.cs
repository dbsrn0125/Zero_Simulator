using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.RclInterfaces;
using System.Linq;
using System.Collections;

public class HSVController : MonoBehaviour
{
    [Header("ROS Settings")]
    [Tooltip("������ ROS ��� �̸����� ���. ���ӽ����̽��� ������ ��ü �̸��� �Է��ϼ���.")]
    public List<string> targetNodeNames;

    [Header("UI Controls")]
    [Tooltip("��� ���ÿ� TextMeshPro ��Ӵٿ�")]
    public TMP_Dropdown nodeSelectorDropdown;
    public Slider minH_Slider, maxH_Slider, minS_Slider, maxS_Slider, minV_Slider, maxV_Slider;
    public TMP_Text minH_Text, maxH_Text, minS_Text, maxS_Text, minV_Text, maxV_Text;

    private ROSConnection ros;
    private string currentTargetNode;
    private Dictionary<string, string> setServiceNames = new Dictionary<string, string>();
    private Dictionary<string, string> getServiceNames = new Dictionary<string, string>();

    // ? 1. ��ũ��Ʈ�� ���� ��, '�غ� �Ϸ�' ��ȣ�� ���� ��û�մϴ�.
    void OnEnable()
    {
        SystemEventManager.OnMainNodesReady += Initialize;
    }

    // ? 2. ��ũ��Ʈ�� ���� ��, ������ �����մϴ�.
    void OnDisable()
    {
        SystemEventManager.OnMainNodesReady -= Initialize;
    }

    void Start()
    {
        if (ROSManager.instance == null)
        {
            Debug.LogError("ROSConnectionManager�� ���� �������� �ʽ��ϴ�!");
            enabled = false;
            return;
        }

        // ? 3. ���� �������� ��� UI�� ��Ȱ��ȭ���� ������� ���۵��� �����ϴ�.
        SetSlidersInteractable(false);
        nodeSelectorDropdown.interactable = false;

        // ��Ӵٿ��� �ɼ��� �̸� ä���ξ �����ϴ�.
        nodeSelectorDropdown.ClearOptions();
        nodeSelectorDropdown.AddOptions(targetNodeNames);
        currentTargetNode = targetNodeNames[0];
    }

    // ? 4. '�غ� �Ϸ�' ��ȣ�� ���� �� �Լ��� ȣ��˴ϴ�!
    private void Initialize()
    {
        Debug.Log($"[{gameObject.name}] 'Main Nodes Ready' event received. Initializing HSV Controller.");

        ros = ROSManager.instance.ROSConnection;

        // --- ��� ��� ����� ���񽺸� �̸� ��� ---
        foreach (var nodeName in targetNodeNames)
        {
            string setSrvName = $"/{nodeName}/set_parameters";
            string getSrvName = $"/{nodeName}/get_parameters";
            setServiceNames[nodeName] = setSrvName;
            getServiceNames[nodeName] = getSrvName;
            ros.RegisterRosService<SetParametersRequest, SetParametersResponse>(setSrvName);
            ros.RegisterRosService<GetParametersRequest, GetParametersResponse>(getSrvName);
        }

        // --- �����̴� �̺�Ʈ ������ ���� ---
        minH_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("h_min", value));
        maxH_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("h_max", value));
        minS_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("s_min", value));
        maxS_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("s_max", value));
        minV_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("v_min", value));
        maxV_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("v_max", value));

        // ��Ӵٿ� ������ ����
        nodeSelectorDropdown.onValueChanged.AddListener(OnDropdownValueChanged);

        // --- UI Ȱ��ȭ �� �ʱ�ȭ ---
        SetSlidersInteractable(true);
        nodeSelectorDropdown.interactable = true;

        if (targetNodeNames.Count > 0)
        {
            // ��Ӵٿ��� ù ��° �׸����� �ʱ� ���� ����
            StartCoroutine(DelayedDropdownInit());
        }
    }

    IEnumerator DelayedDropdownInit()
    {
        yield return new WaitForSeconds(7f); // 1~2������ �Ǵ� �ణ ���
        OnDropdownValueChanged(0);
    }
    // (���� �ٸ� �Լ����� ���� �ڵ�� ��� �����մϴ�)

    void OnDropdownValueChanged(int index)
    {
        currentTargetNode = targetNodeNames[index];
        Debug.Log($"Target node changed to: {currentTargetNode}");
        FetchAndUpdateSliders();
    }

    void OnSliderValueChanged(string paramName, float paramValue)
    {
        if (string.IsNullOrEmpty(currentTargetNode)) return;
        int intValue = (int)paramValue;
        UpdateText(paramName, intValue);
        var request = new SetParametersRequest
        {
            parameters = new ParameterMsg[]
            {
                new ParameterMsg
                {
                    name = paramName,
                    value = new ParameterValueMsg
                    {
                        type = ParameterTypeMsg.PARAMETER_INTEGER,
                        integer_value = intValue
                    }
                }
            }
        };
        ros.SendServiceMessage<SetParametersResponse>(setServiceNames[currentTargetNode], request, response =>
        {
            if (response.results.Length > 0 && !response.results[0].successful)
            {
                Debug.LogError($"Failed to set parameter '{paramName}' on node '{currentTargetNode}': {response.results[0].reason}");
            }
        });
    }

    void FetchAndUpdateSliders()
    {
        var paramNames = new List<string> { "h_min", "h_max", "s_min", "s_max", "v_min", "v_max" };
        var request = new GetParametersRequest { names = paramNames.ToArray() };
        ros.SendServiceMessage<GetParametersResponse>(getServiceNames[currentTargetNode], request, response =>
        {
            if (response.values.Length != paramNames.Count)
            {
                Debug.LogError("Requested parameter count does not match received values count.");
                return;
            }
            Debug.Log($"Received parameters from {currentTargetNode}");
            for (int i = 0; i < paramNames.Count; i++)
            {
                string name = paramNames[i];
                ParameterValueMsg value = response.values[i];
                if (value.type == ParameterTypeMsg.PARAMETER_INTEGER)
                {
                    SetSliderValue(name, (int)value.integer_value);
                }
            }
        });
    }

    void SetSlidersInteractable(bool isInteractable)
    {
        minH_Slider.interactable = isInteractable;
        maxH_Slider.interactable = isInteractable;
        minS_Slider.interactable = isInteractable;
        maxS_Slider.interactable = isInteractable;
        minV_Slider.interactable = isInteractable;
        maxV_Slider.interactable = isInteractable;
    }

    // (UpdateText, GetSlider, GetText �Լ����� ���� �ڵ�� ��� �����մϴ�.)
    void SetSliderValue(string paramName, int value)
    {
        Slider targetSlider = GetSlider(paramName);
        if (targetSlider != null)
        {
            targetSlider.onValueChanged.RemoveAllListeners();
            targetSlider.value = value;
            targetSlider.onValueChanged.AddListener(val => OnSliderValueChanged(paramName, val));
            UpdateText(paramName, value);
        }
    }

    void UpdateText(string paramName, int value)
    {
        TMP_Text targetText = GetText(paramName);
        if (targetText != null)
        {
            targetText.text = value.ToString();
        }
    }

    Slider GetSlider(string paramName)
    {
        switch (paramName)
        {
            case "h_min": return minH_Slider;
            case "h_max": return maxH_Slider;
            case "s_min": return minS_Slider;
            case "s_max": return maxS_Slider;
            case "v_min": return minV_Slider;
            case "v_max": return maxV_Slider;
            default: return null;
        }
    }

    TMP_Text GetText(string paramName)
    {
        switch (paramName)
        {
            case "h_min": return minH_Text;
            case "h_max": return maxH_Text;
            case "s_min": return minS_Text;
            case "s_max": return maxS_Text;
            case "v_min": return minV_Text;
            case "v_max": return maxV_Text;
            default: return null;
        }
    }
}