using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using System;
public class IPAdressController : MonoBehaviour
{
    [Header("UI Connection")]
    public TMP_InputField ipAddressInputField;
    public ROSConnection rosConnectionPrefab;
    public Image statusIndicatorImage;

    [Header("Status Colors")]
    public Color connectedColor = Color.green;
    public Color errorColor = Color.red;
    public Color disconnectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    // Start is called before the first frame update
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
            }
            else
            {
                statusIndicatorImage.color = connectedColor;
            }
        }
        else
        {
            statusIndicatorImage.color = disconnectedColor;
        }
    }
}
