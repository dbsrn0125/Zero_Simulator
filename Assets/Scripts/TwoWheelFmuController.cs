using UnityEngine;
using UnityEngine.UI; // UI 요소를 사용하기 위해 필요
using FMI2;
using TMPro;
using System.Collections.Generic;

public class TwoWheelFmuController : MonoBehaviour
{
    [Header("FMU Settings")]
    public string fmuName = "robot.fmu"; // 여기에 사용할 FMU 파일 이름을 입력하세요.

    [Header("Unity Object Links")]
    public GroundSensor leftWheelSensor;
    public GroundSensor rightWheelSensor;
    public Transform leftWheelTransform;  // 바퀴의 실제 회전을 시각화하기 위함
    public Transform rightWheelTransform; // 바퀴의 실제 회전을 시각화하기 위함
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
        // FMU 초기화
        fmu = new FMU(fmuName, this.name);
        fmu.Reset();
        fmu.SetupExperiment(Time.fixedTimeAsDouble);
        fmu.EnterInitializationMode();
        fmu.ExitInitializationMode();
    }
    void Update()
    {
        // [수정] W, S 키보드 입력을 받아 목표 각속도를 실시간으로 결정합니다.
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
        // 1. FMU에 입력 데이터 전송
        SetFmuInputs();

        // 2. FMU 시뮬레이션 한 스텝 실행
        fmu.DoStep(Time.timeAsDouble, Time.fixedDeltaTime);

        // 3. FMU로부터 결과 받아와서 Unity에 적용
        ApplyFmuOutputs();
    }

    void SetFmuInputs()
    {
        // --- 목표 각도 입력 ---
        //float targetAngleRadians = targetAngleDegrees * Mathf.Deg2Rad; // Radian으로 변환
        fmu.SetReal("LeftWheelWInput", currentTargetVelocity);
        fmu.SetReal("RightWheelWInput", currentTargetVelocity);

        // --- 지면 정보 입력 ---
        // 왼쪽 바퀴
        GroundContactInfo leftInfo = leftWheelSensor.GetContactInfo();
        Vector3 leftNormalForSimscape = new Vector3(-leftInfo.GroundNormal.x, leftInfo.GroundNormal.y, leftInfo.GroundNormal.z);
        fmu.SetReal("Lgap", leftInfo.Gap);
        fmu.SetReal("Lnrm[1]", leftNormalForSimscape.x);
        fmu.SetReal("Lnrm[2]", leftNormalForSimscape.y);
        fmu.SetReal("Lnrm[3]", leftNormalForSimscape.z);

        // 오른쪽 바퀴
        GroundContactInfo rightInfo = rightWheelSensor.GetContactInfo();
        Vector3 rightNormalForSimscape = new Vector3(-rightInfo.GroundNormal.x, rightInfo.GroundNormal.y, rightInfo.GroundNormal.z);
        fmu.SetReal("Rgap", rightInfo.Gap);
        fmu.SetReal("Rnrm[1]", rightNormalForSimscape.x);
        fmu.SetReal("Rnrm[2]", rightNormalForSimscape.y);
        fmu.SetReal("Rnrm[3]", rightNormalForSimscape.z);
    }

    void ApplyFmuOutputs()
    {
        // --- 로버 몸체 위치 및 자세 업데이트 ---
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

        // --- 각 바퀴의 회전 각도 업데이트 (시각화) ---
        float leftAngleRad = (float)fmu.GetReal("LeftWheelAngleOutput");
        float rightAngleRad = (float)fmu.GetReal("RightWheelAngleOutput");

        // Y-up, X-spin 좌표계 기준 localEulerAngles 설정
        if (leftWheelTransform != null)
        {
            // 1. FMU에서 받은 회전각으로 '회전 자체'를 만듭니다 (쿼터니언).
            Quaternion spinRotation = Quaternion.Euler(0, -leftAngleRad * Mathf.Rad2Deg, 0);

            // 2. 시작 시 저장해둔 '초기 90도 회전'에 '새로운 회전'을 곱해서 최종 자세를 계산합니다.
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