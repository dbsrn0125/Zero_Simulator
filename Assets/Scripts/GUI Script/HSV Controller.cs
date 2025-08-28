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
    [Tooltip("제어할 ROS 노드 이름들의 목록. 네임스페이스를 포함한 전체 이름을 입력하세요.")]
    public List<string> targetNodeNames;

    [Header("UI Controls")]
    [Tooltip("노드 선택용 TextMeshPro 드롭다운")]
    public TMP_Dropdown nodeSelectorDropdown;
    public Slider minH_Slider, maxH_Slider, minS_Slider, maxS_Slider, minV_Slider, maxV_Slider;
    public TMP_Text minH_Text, maxH_Text, minS_Text, maxS_Text, minV_Text, maxV_Text;

    private ROSConnection ros;
    private string currentTargetNode;
    private Dictionary<string, string> setServiceNames = new Dictionary<string, string>();
    private Dictionary<string, string> getServiceNames = new Dictionary<string, string>();

    // ? 1. 스크립트가 켜질 때, '준비 완료' 신호를 구독 신청합니다.
    void OnEnable()
    {
        SystemEventManager.OnMainNodesReady += Initialize;
    }

    // ? 2. 스크립트가 꺼질 때, 구독을 해제합니다.
    void OnDisable()
    {
        SystemEventManager.OnMainNodesReady -= Initialize;
    }

    void Start()
    {
        if (ROSManager.instance == null)
        {
            Debug.LogError("ROSConnectionManager가 씬에 존재하지 않습니다!");
            enabled = false;
            return;
        }

        // ? 3. 시작 시점에는 모든 UI를 비활성화시켜 사용자의 오작동을 막습니다.
        SetSlidersInteractable(false);
        nodeSelectorDropdown.interactable = false;

        // 드롭다운의 옵션은 미리 채워두어도 좋습니다.
        nodeSelectorDropdown.ClearOptions();
        nodeSelectorDropdown.AddOptions(targetNodeNames);
        currentTargetNode = targetNodeNames[0];
    }

    // ? 4. '준비 완료' 신호가 오면 이 함수가 호출됩니다!
    private void Initialize()
    {
        Debug.Log($"[{gameObject.name}] 'Main Nodes Ready' event received. Initializing HSV Controller.");

        ros = ROSManager.instance.ROSConnection;

        // --- 모든 대상 노드의 서비스를 미리 등록 ---
        foreach (var nodeName in targetNodeNames)
        {
            string setSrvName = $"/{nodeName}/set_parameters";
            string getSrvName = $"/{nodeName}/get_parameters";
            setServiceNames[nodeName] = setSrvName;
            getServiceNames[nodeName] = getSrvName;
            ros.RegisterRosService<SetParametersRequest, SetParametersResponse>(setSrvName);
            ros.RegisterRosService<GetParametersRequest, GetParametersResponse>(getSrvName);
        }

        // --- 슬라이더 이벤트 리스너 연결 ---
        minH_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("h_min", value));
        maxH_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("h_max", value));
        minS_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("s_min", value));
        maxS_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("s_max", value));
        minV_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("v_min", value));
        maxV_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("v_max", value));

        // 드롭다운 리스너 연결
        nodeSelectorDropdown.onValueChanged.AddListener(OnDropdownValueChanged);

        // --- UI 활성화 및 초기화 ---
        SetSlidersInteractable(true);
        nodeSelectorDropdown.interactable = true;

        if (targetNodeNames.Count > 0)
        {
            // 드롭다운의 첫 번째 항목으로 초기 설정 실행
            StartCoroutine(DelayedDropdownInit());
        }
    }

    IEnumerator DelayedDropdownInit()
    {
        yield return new WaitForSeconds(7f); // 1~2프레임 또는 약간 대기
        OnDropdownValueChanged(0);
    }
    // (이하 다른 함수들은 이전 코드와 모두 동일합니다)

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

    // (UpdateText, GetSlider, GetText 함수들은 이전 코드와 모두 동일합니다.)
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