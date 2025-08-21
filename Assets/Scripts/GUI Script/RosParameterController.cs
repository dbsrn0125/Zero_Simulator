using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.RclInterfaces;

// ? 1. ROS �Ķ���� Ÿ���� �����ϱ� ���� enum(������)�� �����մϴ�.
public enum RosParameterType { Double, Integer }

[System.Serializable]
public class ParameterSlider
{
    public string parameterName;
    public Slider slider;
    public TMP_Text valueText;

    // ? 2. �� �Ķ������ Ÿ���� �ν����Ϳ��� ������ �� �ֵ��� �ʵ带 �߰��մϴ�.
    [Tooltip("�� �Ķ������ ROS Ÿ���� �����ϼ��� (Double �Ǵ� Integer)")]
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

    void Start()
    {
        // 1. ROSConnectionManager �̱��� �ν��Ͻ��� �ִ��� Ȯ���մϴ�.
        if (ROSManager.instance == null)
        {
            Debug.LogError("ROSConnectionManager�� ���� �������� �ʽ��ϴ�! RosVideoSubscriber�� ����� �� �����ϴ�.");
            enabled = false;
            return;
        }

        // 2. Manager�κ��� �߾� �����Ǵ� ROSConnection ��ü�� �޾ƿɴϴ�.
        ros = ROSManager.instance.ROSConnection;
        setParametersServiceName = $"/{targetNodeName}/set_parameters";
        ros.RegisterRosService<SetParametersRequest, SetParametersResponse>(setParametersServiceName);

        foreach (var ps in parameterSliders)
        {
            ParameterSlider capturedPs = ps;
            UpdateSliderText(capturedPs);
            ps.slider.onValueChanged.AddListener(value => OnSliderValueChanged(capturedPs, value));
        }
    }

    void UpdateSliderText(ParameterSlider ps)
    {
        if (ps.valueText != null)
        {
            // ? 3. �Ķ���� Ÿ�Կ� ���� �ؽ�Ʈ ǥ�� ����� �ٸ��� �մϴ�.
            if (ps.parameterType == RosParameterType.Double)
            {
                // Double Ÿ���� �Ҽ��� �� �ڸ����� ǥ��
                ps.valueText.text = ps.slider.value.ToString("F1");
            }
            else // Integer Ÿ��
            {
                // Integer Ÿ���� ������ ǥ��
                ps.valueText.text = ((int)ps.slider.value).ToString();
            }
        }
    }

    void OnSliderValueChanged(ParameterSlider ps, float paramValue)
    {
        UpdateSliderText(ps);

        var paramValueMsg = new ParameterValueMsg();

        // ? 4. ������ �Ķ���� Ÿ�Կ� ���� ��û �޽����� �ٸ��� �����մϴ�.
        switch (ps.parameterType)
        {
            case RosParameterType.Double:
                paramValueMsg.type = ParameterTypeMsg.PARAMETER_DOUBLE;
                paramValueMsg.double_value = paramValue;
                break;

            case RosParameterType.Integer:
                paramValueMsg.type = ParameterTypeMsg.PARAMETER_INTEGER;
                paramValueMsg.integer_value = (long)paramValue; // ROS 2�� integer�� 64��Ʈ(long)�Դϴ�.
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