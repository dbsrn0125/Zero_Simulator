using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.RclInterfaces;

public class HSVController : MonoBehaviour
{
    [Header("ROS Settings")]
    [Tooltip("파라미터를 변경할 노드의 이름")]
    public string targetNodeName = "marker_lifecycle_node";

    [Header("UI Sliders")]
    public Slider minH_Slider;
    public Slider maxH_Slider;
    public Slider minS_Slider;
    public Slider maxS_Slider;
    public Slider minV_Slider;
    public Slider maxV_Slider;

    [Header("UI Value Texts")]
    public TMP_Text minH_Text;
    public TMP_Text maxH_Text;
    public TMP_Text minS_Text;
    public TMP_Text maxS_Text;
    public TMP_Text minV_Text;
    public TMP_Text maxV_Text;


    private ROSConnection ros;
    private string setParametersServiceName;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        // ? 1. 사용할 서비스의 전체 이름을 변수에 저장합니다.
        setParametersServiceName = $"/{targetNodeName}/set_parameters";

        // ? 2. 이 스크립트가 set_parameters 서비스를 사용할 것이라고 ROSConnection에 미리 등록합니다.
        ros.RegisterRosService<SetParametersRequest, SetParametersResponse>(setParametersServiceName);

        // 슬라이더 이벤트에 리스너(함수)를 연결합니다.
        minH_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("minH", (int)value, minH_Text));
        maxH_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("maxH", (int)value, maxH_Text));
        minS_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("minS", (int)value, minS_Text));
        maxS_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("maxS", (int)value, maxS_Text));
        minV_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("minV", (int)value, minV_Text));
        maxV_Slider.onValueChanged.AddListener(value => OnSliderValueChanged("maxV", (int)value, maxV_Text));

        UpdateAllTextValues();
    }

    void UpdateAllTextValues()
    {
        minH_Text.text = ((int)minH_Slider.value).ToString();
        maxH_Text.text = ((int)maxH_Slider.value).ToString();
        minS_Text.text = ((int)minS_Slider.value).ToString();
        maxS_Text.text = ((int)maxS_Slider.value).ToString();
        minV_Text.text = ((int)minV_Slider.value).ToString();
        maxV_Text.text = ((int)maxV_Slider.value).ToString();
    }

    void OnSliderValueChanged(string paramName, int paramValue, TMP_Text valueText)
    {
        valueText.text = paramValue.ToString();

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
                        integer_value = paramValue
                    }
                }
            }
        };

        // 등록된 서비스 이름으로 요청을 보냅니다.
        ros.SendServiceMessage<SetParametersResponse>(setParametersServiceName, request, OnParameterChangeResponse);
    }

    void OnParameterChangeResponse(SetParametersResponse response)
    {
        if (!response.results[0].successful)
        {
            Debug.LogError($"Failed to set parameter: {response.results[0].reason}");
        }
    }
}