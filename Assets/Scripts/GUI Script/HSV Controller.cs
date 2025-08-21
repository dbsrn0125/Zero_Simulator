using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.RclInterfaces;
using System.Linq;

public class HSVController : MonoBehaviour
{
    [Header("ROS Settings")]
    [Tooltip("제어할 ROS 노드 이름들의 목록. 네임스페이스를 포함한 전체 이름을 입력하세요.")]
    public List<string> targetNodeNames; // ? 제어할 노드 목록

    [Header("UI Controls")]
    [Tooltip("노드 선택용 TextMeshPro 드롭다운")]
    public TMP_Dropdown nodeSelectorDropdown; // ? 노드 선택 드롭다운
    public Slider minH_Slider, maxH_Slider, minS_Slider, maxS_Slider, minV_Slider, maxV_Slider;
    public TMP_Text minH_Text, maxH_Text, minS_Text, maxS_Text, minV_Text, maxV_Text;

    private ROSConnection ros;
    private string currentTargetNode; // 현재 선택된 노드 이름

    // ? 여러 서비스 클라이언트를 관리할 Dictionary
    private Dictionary<string, string> setServiceNames = new Dictionary<string, string>();
    private Dictionary<string, string> getServiceNames = new Dictionary<string, string>();

    void Start()
    {
        // 1. ROSConnectionManager 싱글턴 인스턴스가 있는지 확인합니다.
        if (ROSManager.instance == null)
        {
            Debug.LogError("ROSConnectionManager가 씬에 존재하지 않습니다! RosVideoSubscriber를 사용할 수 없습니다.");
            enabled = false;
            return;
        }

        // 2. Manager로부터 중앙 관리되는 ROSConnection 객체를 받아옵니다.
        ros = ROSManager.instance.ROSConnection;

        // --- 드롭다운 메뉴 설정 ---
        nodeSelectorDropdown.ClearOptions();
        nodeSelectorDropdown.AddOptions(targetNodeNames);
        nodeSelectorDropdown.onValueChanged.AddListener(OnDropdownValueChanged);

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

        // --- 초기화 ---
        // 드롭다운의 첫 번째 항목으로 초기 설정 실행
        if (targetNodeNames.Count > 0)
        {
            OnDropdownValueChanged(0);
        }
    }

    // ? 드롭다운 값이 변경될 때 호출되는 '컨트롤 타워' 함수
    void OnDropdownValueChanged(int index)
    {
        currentTargetNode = targetNodeNames[index];
        Debug.Log($"Target node changed to: {currentTargetNode}");

        // 새로 선택된 노드의 현재 파라미터 값을 읽어와서 슬라이더에 반영
        FetchAndUpdateSliders();
    }

    // ? 슬라이더 값이 변경될 때 ROS로 파라미터 설정 요청
    void OnSliderValueChanged(string paramName, float paramValue)
    {
        // 현재 타겟 노드가 설정되지 않았으면 무시
        if (string.IsNullOrEmpty(currentTargetNode)) return;

        int intValue = (int)paramValue;

        // UI 텍스트 업데이트
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

    // ? ROS 노드로부터 현재 파라미터 값을 가져와 슬라이더에 반영
    void FetchAndUpdateSliders()
    {
        var paramNames = new List<string> { "h_min", "h_max", "s_min", "s_max", "v_min", "v_max" };
        var request = new GetParametersRequest { names = paramNames.ToArray() };

        ros.SendServiceMessage<GetParametersResponse>(getServiceNames[currentTargetNode], request, response =>
        {
            // 요청한 파라미터 개수와 응답으로 온 값의 개수가 같은지 확인
            if (response.values.Length != paramNames.Count)
            {
                Debug.LogError("Requested parameter count does not match received values count.");
                return;
            }

            Debug.Log($"Received parameters from {currentTargetNode}");

            // ? foreach 대신 for 반복문을 사용하여 이름과 값을 인덱스로 매칭합니다.
            for (int i = 0; i < paramNames.Count; i++)
            {
                string name = paramNames[i];         // 요청했던 이름 목록에서 이름 가져오기
                ParameterValueMsg value = response.values[i]; // 응답 값 목록에서 값 가져오기

                if (value.type == ParameterTypeMsg.PARAMETER_INTEGER)
                {
                    // SetSliderValue의 두 번째 인자는 int 타입이므로 캐스팅합니다.
                    SetSliderValue(name, (int)value.integer_value);
                }
            }
        });
    }

    // --- UI 업데이트 헬퍼 함수들 ---
    void SetSliderValue(string paramName, int value)
    {
        // 슬라이더 값을 변경할 때 onValueChanged 이벤트가 또 발생하는 것을 막기 위해
        // 리스너를 잠시 해제했다가 다시 연결합니다.
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