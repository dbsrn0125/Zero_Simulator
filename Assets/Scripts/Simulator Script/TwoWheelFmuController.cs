using UnityEngine;
using UnityEngine.UI; // UI ��Ҹ� ����ϱ� ���� �ʿ�
using FMI2;
using TMPro;
using System.Collections.Generic;

public class TwoWheelFmuController : MonoBehaviour
{
    [Header("FMU Settings")]
    public string fmuName = "robot.fmu"; // ���⿡ ����� FMU ���� �̸��� �Է��ϼ���.

    [Header("Unity Object Links")]
    public GroundSensor leftWheelSensor;
    public GroundSensor rightWheelSensor;
    public Transform leftWheelTransform;  // ������ ���� ȸ���� �ð�ȭ�ϱ� ����
    public Transform rightWheelTransform; // ������ ���� ȸ���� �ð�ȭ�ϱ� ����
    public Vector3 initialPosition = Vector3.zero;

    [Header("Control Inputs")]
    public float targetAngularVelocity_RadPerSec = 5.0f;


    private FMU fmu;
    public float currentTargetVelocity = 0f;
    private Quaternion leftInitialRotation;
    private Quaternion rightInitialRotation;
    void Start()
    {
        if (leftWheelTransform != null)
            leftInitialRotation = leftWheelTransform.localRotation;
        if (rightWheelTransform != null)
            rightInitialRotation = rightWheelTransform.localRotation;
        // FMU �ʱ�ȭ
        fmu = new FMU(fmuName, this.name);
        fmu.Reset();
        fmu.SetupExperiment(Time.fixedTimeAsDouble);
        fmu.EnterInitializationMode();
        fmu.ExitInitializationMode();
    }
    void Update()
    {
        // [����] W, S Ű���� �Է��� �޾� ��ǥ ���ӵ��� �ǽð����� �����մϴ�.
        if (Input.GetKey(KeyCode.W))
        {
            currentTargetVelocity = targetAngularVelocity_RadPerSec;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            currentTargetVelocity = -targetAngularVelocity_RadPerSec;
        }
        else
        {
            currentTargetVelocity = 0f;
        }
    }
    void FixedUpdate()
    {
        // 1. FMU�� �Է� ������ ����
        SetFmuInputs();

        // 2. FMU �ùķ��̼� �� ���� ����
        fmu.DoStep(Time.timeAsDouble, Time.fixedDeltaTime);

        // 3. FMU�κ��� ��� �޾ƿͼ� Unity�� ����
        ApplyFmuOutputs();
    }

    void SetFmuInputs()
    {
        // --- ��ǥ ���� �Է� ---
        //float targetAngleRadians = targetAngleDegrees * Mathf.Deg2Rad; // Radian���� ��ȯ
        fmu.SetReal("LeftWheelWInput", currentTargetVelocity);
        fmu.SetReal("RightWheelWInput", currentTargetVelocity);

        // --- ���� ���� �Է� ---
        // ���� ����
        GroundContactInfo leftInfo = leftWheelSensor.GetContactInfo();
        Vector3 leftNormalForSimscape = new Vector3(-leftInfo.GroundNormal.x, leftInfo.GroundNormal.y, leftInfo.GroundNormal.z);
        fmu.SetReal("Lgap", leftInfo.Gap);
        fmu.SetReal("Lnrm[1]", leftNormalForSimscape.x);
        fmu.SetReal("Lnrm[2]", leftNormalForSimscape.y);
        fmu.SetReal("Lnrm[3]", leftNormalForSimscape.z);

        // ������ ����
        GroundContactInfo rightInfo = rightWheelSensor.GetContactInfo();
        Vector3 rightNormalForSimscape = new Vector3(-rightInfo.GroundNormal.x, rightInfo.GroundNormal.y, rightInfo.GroundNormal.z);
        fmu.SetReal("Rgap", rightInfo.Gap);
        fmu.SetReal("Rnrm[1]", rightNormalForSimscape.x);
        fmu.SetReal("Rnrm[2]", rightNormalForSimscape.y);
        fmu.SetReal("Rnrm[3]", rightNormalForSimscape.z);
    }

    void ApplyFmuOutputs()
    {
        // --- �ι� ��ü ��ġ �� �ڼ� ������Ʈ ---
        double x = fmu.GetReal("Position_x");
        double y = fmu.GetReal("Position_y");
        //double y = 0;
        double z = fmu.GetReal("Position_z");
        Vector3 fmuPosition = new Vector3(-(float)x, (float)y, (float)z);
        transform.position = initialPosition + fmuPosition;

        double qx = fmu.GetReal("Orientation[2,1]");
        double qy = fmu.GetReal("Orientation[3,1]");
        double qz = fmu.GetReal("Orientation[4,1]");
        double qw = fmu.GetReal("Orientation[1,1]");
        Quaternion fmuRotation = new Quaternion(-(float)0, (float)qy, -(float)qz, (float)qw);
        transform.rotation = fmuRotation;

        // --- �� ������ ȸ�� ���� ������Ʈ (�ð�ȭ) ---
        float leftAngleRad = (float)fmu.GetReal("LeftWheelAngleOutput");
        float rightAngleRad = (float)fmu.GetReal("RightWheelAngleOutput");

        // Y-up, X-spin ��ǥ�� ���� localEulerAngles ����
        if (leftWheelTransform != null)
        {
            // 1. FMU���� ���� ȸ�������� 'ȸ�� ��ü'�� ����ϴ� (���ʹϾ�).
            Quaternion spinRotation = Quaternion.Euler(0, -leftAngleRad * Mathf.Rad2Deg, 0);

            // 2. ���� �� �����ص� '�ʱ� 90�� ȸ��'�� '���ο� ȸ��'�� ���ؼ� ���� �ڼ��� ����մϴ�.
            leftWheelTransform.localRotation = leftInitialRotation * spinRotation;
        }

        if (rightWheelTransform != null)
        {
            Quaternion spinRotation = Quaternion.Euler(0, -rightAngleRad * Mathf.Rad2Deg, 0);
            rightWheelTransform.localRotation = rightInitialRotation * spinRotation;
        }
    }

    void OnDestroy()
    {
        if (fmu != null)
        {
            fmu.Dispose();
        }
    }
}