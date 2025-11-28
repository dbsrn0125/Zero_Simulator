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
    public string fmi_w_In;
    public string fmi_Angle_Out;
    public string fmi_gz;
    public Vector3 fmi_wheelPosition;
    public Quaternion fmi_wheelRotation;

    public float targetSpeed = 0.0f;
    public Quaternion initialRotation;
}
