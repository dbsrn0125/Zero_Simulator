using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMI2;

public class FMUManager : MonoBehaviour
{
    [Header("1. FMI Configuration")]
    public string fmuName;
    public Vector3 initialPosition;

    [Header("2. Core Components(Drag-n-Drop")]
    public CoordinateTranslator translator;

    [Header("3. Wheel Controllers(List")]
    public List<WheelController> wheels = new List<WheelController>();

    [Header("4. Keyboard Controls")]
    public float targetWheelSpeed_RadPerSec = 5.0f;
    public float acceleration = 50.0f; // 초당 증가할 속도 (가속도)
    public float maxSpeed = 500.0f;    // 최대 속도 제한
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode stopKey = KeyCode.X;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    

    private FMU fmu;
    private enum DriveState {Idle, Forward, Backward };
    private DriveState currentDriveState = DriveState.Idle;
    // Start is called before the first frame update
    void Start()
    {
        foreach(var wheel in wheels)
        {
            if(wheel.wheelTransform != null)
            {
                wheel.initialRotation = wheel.wheelTransform.rotation;
            }
        }
        if(translator == null)
        {
            Debug.LogError("Translator missing");
            this.enabled = false;
            return;
        }
        if(wheels.Count == 0)
        {
            Debug.LogError("Wheel missing");
            this.enabled = false;
            return;
        }
        Debug.Log($"Loading FMU: {fmuName}");
        
        fmu = new FMU(fmuName, this.name);
        ResetFmu();

    }
    public void ResetFmu()
    {
        if (fmu != null)
        {
            fmu.Reset();
            fmu.SetupExperiment(Time.fixedTimeAsDouble);
            fmu.EnterInitializationMode();
            fmu.ExitInitializationMode();
            Debug.Log("FMU Reset and Initialized.");
        }
    }

    void HandleKeyboardInput()
    {
        // 키보드 상태 감지
        if (Input.GetKeyDown(stopKey)) { currentDriveState = DriveState.Idle; }
        else if (Input.GetKey(forwardKey)) { currentDriveState = DriveState.Forward; }
        else if (Input.GetKey(backwardKey)) { currentDriveState = DriveState.Backward; }
        else if (Input.GetKeyUp(forwardKey) || Input.GetKeyUp(backwardKey)) { currentDriveState = DriveState.Idle; }

        float speed = 0;
        switch (currentDriveState)
        {
            case DriveState.Forward: speed = targetWheelSpeed_RadPerSec; break;
            case DriveState.Backward: speed = -targetWheelSpeed_RadPerSec; break;
            case DriveState.Idle: speed = 0; break;
        }

        // [수정] 목록에 있는 "모든" 바퀴의 목표 속도를 업데이트
        foreach (var wheel in wheels)
        {
            wheel.targetSpeed = speed;
        }
    }
    // Update is called once per frame
    void Update()
    {
        HandleKeyboardInput();
    }

    private void FixedUpdate()
    {
        SetFmuInputs();
        fmu.DoStep(Time.timeAsDouble, Time.deltaTime);
        ApplyFmuOutputs();
    }
    void SetFmuInputs()
    {
        foreach (var wheel in wheels)
        {
            if(wheel.sensor!=null)
                wheel.sensor.CalculateGroundForces();
        }

        foreach(var wheel in wheels)
        {
            try
            {
                fmu.SetReal(wheel.fmi_w_In, (double)wheel.targetSpeed);
                Debug.Log($"Wheel {wheel.wheelId} Target Speed (rad/s): {wheel.targetSpeed}");
                fmu.SetReal(wheel.fmi_gz, (double)wheel.sensor.Penetration);
                //fmu.SetReal(wheel.fmi_gz, 0);
                //Debug.Log($"Wheel {wheel.wheelId} Ground Penetration (m): {wheel.sensor.Penetration}");
            }
            catch(System.Exception e)
            {
                Debug.LogError($"FMI SetReal Error (Wheel: {wheel.wheelId}): {e.Message}. FMI 변수명('{wheel.fmi_w_In}')이 정확한지 확인하세요.");
            }
        }
    }

    void ApplyFmuOutputs()
    {
        double[] simPos = new double[]
        {
            fmu.GetReal("Position_x"),
            fmu.GetReal("Position_y"),
            fmu.GetReal("Position_z"),

        };

        //Debug.Log($"Position_x: {simPos[0]}, Position_y: {simPos[1]}, Position_z: {simPos[2]}");
        double[] simRot = new double[]
        {
            fmu.GetReal("Orientation[1,1]"),
            fmu.GetReal("Orientation[2,1]") ,
            fmu.GetReal("Orientation[3,1]") ,
            fmu.GetReal("Orientation[4,1]"),
        };
        //transform.position = initialPosition + translator.TranslatePositionFromFMI(simPos);
        transform.position = new Vector3((float)simPos[0], (float)simPos[1], (float)simPos[2]);
        //transform.position = initialPosition + new Vector3((float)simPos[0], 0, 0);
        //transform.position = initialPosition + new Vector3((float)simPos[0], (float)simPos[1], (float)simPos[2]);
        //transform.rotation = translator.TranslateRotationFromFMI(simRot);
        transform.rotation = new Quaternion((float)simRot[0], (float)simRot[1], (float)simRot[2], (float)simRot[3]);

        foreach (var wheel in wheels)
        {
            try
            {
                // 각 WheelController에 설정된 FMI 출력 변수명(string)을 사용
                double wheelAngleRad = fmu.GetReal(wheel.fmi_Angle_Out);
                //Debug.Log($"Wheel {wheel.wheelId} Angle (rad): {wheelAngleRad}");

                wheel.fmi_wheelPosition = new Vector3(
                    (float)fmu.GetReal($"{wheel.wheelId}_x"),
                    (float)fmu.GetReal($"{wheel.wheelId}_y"),
                    (float)fmu.GetReal($"{wheel.wheelId}_z")
                );
                double[] wheelrotation = new double[]
                {
                    fmu.GetReal($"{wheel.wheelId}_q[1,1]"),
                    fmu.GetReal($"{wheel.wheelId}_q[2,1]"),
                    fmu.GetReal($"{wheel.wheelId}_q[3,1]"),
                    fmu.GetReal($"{wheel.wheelId}_q[4,1]")
                };

                wheel.fmi_wheelRotation = new Quaternion((float)wheelrotation[0], (float)wheelrotation[1], (float)wheelrotation[2], (float)wheelrotation[3]);

                // 휠 Transform에 로컬 회전값 적용
                Quaternion fmiRotation = Quaternion.Euler(0, (float)wheelAngleRad * Mathf.Rad2Deg, 0);
                Quaternion zRotation = Quaternion.Euler(0, 0, 0);
                wheel.wheelTransform.localRotation = wheel.initialRotation * wheel.fmi_wheelRotation * fmiRotation ;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FMI GetReal Error (Wheel: {wheel.wheelId}): {e.Message}. FMI 변수명('{wheel.fmi_Angle_Out}')이 정확한지 확인하세요.");
            }
        }
    }
    private void OnDestroy()
    {
        if(fmu!= null)
        {
            fmu.Dispose();
        }
    }
}
