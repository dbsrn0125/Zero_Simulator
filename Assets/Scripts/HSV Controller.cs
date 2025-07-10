using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.RclInterfaces;

public class HSVController : MonoBehaviour
{
    [Header("ROS Settings")]
    [Tooltip("�Ķ���͸� ������ ����� �̸�")]
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

        // ? 1. ����� ������ ��ü �̸��� ������ �����մϴ�.
        setParametersServiceName = $"/{targetNodeName}/set_parameters";

        // ? 2. �� ��ũ��Ʈ�� set_parameters ���񽺸� ����� ���̶�� ROSConnection�� �̸� ����մϴ�.
        ros.RegisterRosService<SetParametersRequest, SetParametersResponse>(setParametersServiceName);

        // �����̴� �̺�Ʈ�� ������(�Լ�)�� �����մϴ�.
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

        // ��ϵ� ���� �̸����� ��û�� �����ϴ�.
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