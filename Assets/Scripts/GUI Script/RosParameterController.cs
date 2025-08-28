using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.RclInterfaces;

// ? 1. ROS 파라미터 타입을 선택하기 위한 enum(열거형)을 정의합니다.
public enum RosParameterType { Double, Integer }

[System.Serializable]
public class ParameterSlider
{
    public string parameterName;
    public Slider slider;
    public TMP_Text valueText;

    // ? 2. 각 파라미터의 타입을 인스펙터에서 설정할 수 있도록 필드를 추가합니다.
    [Tooltip("이 파라미터의 ROS 타입을 선택하세요 (Double 또는 Integer)")]
    public RosParameterType parameterType = RosParameterType.Double;
}

public class RosParameterController : MonoBehaviour
{
    [Header("ROS Settings")]
    public string targetNodeName = "potential_field_node";

    [Header("UI Controls")]
    public List<ParameterSlider> parameterSliders;

    private ROSConnection ros;
    private string setParametersServiceName;
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
        foreach (var ps in parameterSliders)
        {
            ps.slider.interactable = false;
        }
    }

    private void Initialize()
    {
        Debug.Log($"[{gameObject.name}] 'Main Nodes Ready' event received. Initializing ROS parameters.");

        ros = ROSManager.instance.ROSConnection;
        setParametersServiceName = $"/{targetNodeName}/set_parameters";
        ros.RegisterRosService<SetParametersRequest, SetParametersResponse>(setParametersServiceName);

        foreach (var ps in parameterSliders)
        {
            // 리스너를 추가하고, 텍스트를 업데이트하고, 슬라이더를 활성화합니다.
            ParameterSlider capturedPs = ps;
            UpdateSliderText(capturedPs);
            ps.slider.onValueChanged.AddListener(value => OnSliderValueChanged(capturedPs, value));
            ps.slider.interactable = true; // ? 이제 슬라이더를 활성화!
        }
    }
    void UpdateSliderText(ParameterSlider ps)
    {
        if (ps.valueText != null)
        {
            // ? 3. 파라미터 타입에 따라 텍스트 표시 방식을 다르게 합니다.
            if (ps.parameterType == RosParameterType.Double)
            {
                // Double 타입은 소수점 두 자리까지 표시
                ps.valueText.text = ps.slider.value.ToString("F1");
            }
            else // Integer 타입
            {
                // Integer 타입은 정수로 표시
                ps.valueText.text = ((int)ps.slider.value).ToString();
            }
        }
    }

    void OnSliderValueChanged(ParameterSlider ps, float paramValue)
    {
        UpdateSliderText(ps);

        var paramValueMsg = new ParameterValueMsg();

        // ? 4. 설정된 파라미터 타입에 따라 요청 메시지를 다르게 구성합니다.
        switch (ps.parameterType)
        {
            case RosParameterType.Double:
                paramValueMsg.type = ParameterTypeMsg.PARAMETER_DOUBLE;
                paramValueMsg.double_value = paramValue;
                break;

            case RosParameterType.Integer:
                paramValueMsg.type = ParameterTypeMsg.PARAMETER_INTEGER;
                paramValueMsg.integer_value = (long)paramValue; // ROS 2의 integer는 64비트(long)입니다.
                break;
        }

        var request = new SetParametersRequest
        {
            parameters = new ParameterMsg[]
            {
                new ParameterMsg
                {
                    name = ps.parameterName,
                    value = paramValueMsg
                }
            }
        };

        ros.SendServiceMessage<SetParametersResponse>(setParametersServiceName, request, OnParameterChangeResponse);
    }

    void OnParameterChangeResponse(SetParametersResponse response)
    {
        if (response.results.Length > 0 && !response.results[0].successful)
        {
            Debug.LogError($"Failed to set parameter: {response.results[0].reason}");
        }
    }
}