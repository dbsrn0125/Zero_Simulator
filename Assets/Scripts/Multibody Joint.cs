using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMI2;
public class MultibodyJoint : MonoBehaviour
{
    public enum JointType
    {
        SingleAxisRotation,
        FullTransform_World,
        FullTransform_Local
    }
    [Header("FMU Connection")]
    public FMUSimulator fmuSimulator; // �߾� FMU ������ ���� (Inspector���� �Ҵ� �Ǵ� Start���� FindObjectOfType)
    public JointType jointType = JointType.SingleAxisRotation; // enum �̸� ����

    [Header("Single Axis Rotation Settings")]
    public string fmuOutput_Angle_Name; // FMU���� �о�� ���� ���� �̸� (���ڿ�)
    public Vector3 localRotationAxis = Vector3.up;
    //public float angleScale = Mathf.Rad2Deg;
    public bool invertRotation = false;
    [Header("Full Transform Settings (World or Local)")]
    // ��ġ FMU ���� �̸��� (�Ǵ� �ϳ��� Vector3 ���� �̸� - ���̺귯��/FMU ��� ��Ŀ� ����)
    public string fmuOutput_PositionX_Name;
    public string fmuOutput_PositionY_Name;
    public string fmuOutput_PositionZ_Name;
    // �ڼ�(���ʹϾ�) FMU ���� �̸���
    public string fmuOutput_RotationQuatW_Name;
    public string fmuOutput_RotationQuatX_Name;
    public string fmuOutput_RotationQuatY_Name;
    public string fmuOutput_RotationQuatZ_Name;

    private Quaternion initialLocalRotation;
    // Start is called before the first frame update
    void Start()
    {
        if(jointType == JointType.SingleAxisRotation)
        {
            initialLocalRotation = transform.localRotation;
        }
    }
    private void LateUpdate()
    {
        switch (jointType) 
        { 
            case JointType.SingleAxisRotation:
                UpdateSingleAxisRotation();
                break;
            case JointType.FullTransform_World:
                UpdateFullTransformWorld();
                break;
            case JointType.FullTransform_Local:
                UpdateFullTransfromLocal();
                break;
        }
    }
    private void UpdateSingleAxisRotation()
    {
        float angle_deg = (float)fmuSimulator.fmu.GetReal(fmuOutput_Angle_Name);
        if (invertRotation) angle_deg *= -1f;
        transform.localRotation = initialLocalRotation * Quaternion.AngleAxis(angle_deg, localRotationAxis.normalized);
        if (fmuOutput_Angle_Name == "LFSteeringAngleOutput") Debug.Log(fmuSimulator.fmu.GetReal("LFSteeringAngleOutput"));
    }

    private void UpdateFullTransformWorld()
    {
        throw new NotImplementedException();
    }
    private void UpdateFullTransfromLocal()
    {
        throw new NotImplementedException();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
