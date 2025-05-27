using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
public class IPAdressController : MonoBehaviour
{
    [Header("UI Connection")]
    public TMP_InputField ipAddressInputField;
    public ROSConnection rosConnectionPrefab;
    // Start is called before the first frame update
    public void OnClickIpAdressChange()
    {
        rosConnectionPrefab.RosIPAddress = ipAddressInputField.text;
        Debug.Log("1");
    }
}
