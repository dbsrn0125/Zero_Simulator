using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WheelController
{
    [Header("1.Setup")]
    public string wheelId;
    public WheelGroundSensor sensor;
    public Transform wheelTransform;

    [Header("2. FMI Variable Names")]
    public string fmi_Fz_In;
    public string fmi_w_In;
    public string fmi_Angle_Out;

    public float targetSpeed = 0.0f;
    public Quaternion initialRotation;
}
