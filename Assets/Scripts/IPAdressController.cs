using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using System;
public class IPAdressController : MonoBehaviour
{
    [Header("ROS Settings")]
    [Tooltip("Default ROS IP address")]
    public string defaultIPAddress = "172.30.1.91";
    [Header("UI Connection")]
    public TMP_InputField ipAddressInputField;
    public ROSConnection rosConnectionPrefab;
    public Image statusIndicatorImage;
    public Button launchButton;
    public Button emergencyButton;
    [Header("Status Colors")]
    public Color connectedColor = Color.green;
    public Color errorColor = Color.red;
    public Color disconnectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    // Start is called before the first frame update
    private void Start()
    {
        ipAddressInputField.text = defaultIPAddress;
    }
    public void OnClickIpAdressChange()
    {
        rosConnectionPrefab.RosIPAddress = ipAddressInputField.text;
        UpdateStatusColor();
    }

    private void Update()
    {
        UpdateStatusColor();
    }
    private void UpdateStatusColor()
    {
        if(rosConnectionPrefab.HasConnectionThread)
        {
            if(rosConnectionPrefab.HasConnectionError)
            {
                statusIndicatorImage.color = errorColor;
                launchButton.interactable = false;
                emergencyButton.interactable=false;
            }
            else
            {
                statusIndicatorImage.color = connectedColor;
                launchButton.interactable = true;
                emergencyButton.interactable = true;
            }
        }
        else
        {
            statusIndicatorImage.color = disconnectedColor;
            launchButton.interactable = false;
            emergencyButton.interactable = false;
        }
    }
}
