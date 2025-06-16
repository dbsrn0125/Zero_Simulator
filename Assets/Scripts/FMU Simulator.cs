using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMI2;
using RosMessageTypes.ZeroInterfaces;

public class FMUSimulator : MonoBehaviour
{
    public FMU fmu;
    public string fmuName;
    public List<GroundSensor> groundSensors = new List<GroundSensor>(4);
    public Vector3 initialPosition;
    private Dictionary<WheelLocation, float> targetWheelSpeeds = new Dictionary<WheelLocation, float>();
    private Dictionary<WheelLocation, float> targetSteeringAngles = new Dictionary<WheelLocation, float>();
    private GroundContactInfo[] currentGroundContacts = new GroundContactInfo[4];
    public float targetWheelSpeed_RadPerSec=3f;
    public KeyCode forwardKey = KeyCode.W;      // ���� Ű
    public KeyCode backwardKey = KeyCode.S;     // ���� Ű
    public KeyCode stopKey = KeyCode.X;         // ���� Ű (�ɼ�: ����� ����)
    public enum DriveState { Idle, Forward, Backward };
    public DriveState currentDriveState = DriveState.Idle;
    // Start is called before the first frame update
    void Start()
    {
        fmu = new FMU(fmuName, name);
        Reset();
        foreach (WheelLocation loc in System.Enum.GetValues(typeof(WheelLocation)))
        {
            targetSteeringAngles[loc] = 0.0f;
            targetWheelSpeeds[loc] = 0.0f;
            currentGroundContacts[(int)loc] = GroundContactInfo.NonContact();
        }
    }
    private void Update()
    {
        // 1. Ű���� �Է� ����
        bool forwardPressed = Input.GetKey(forwardKey);
        bool backwardPressed = Input.GetKey(backwardKey);
        bool stopPressed = Input.GetKeyDown(stopKey); // GetKeyDown�� Ű�� ������ ������ true

        // 2. �κ� ���� ���� ����
        if (stopPressed) // ���� Ű�� �켱������ ����
        {
            currentDriveState = DriveState.Idle;
        }
        else if (forwardPressed && !backwardPressed) // ���� Ű�� ������ ��
        {
            currentDriveState = DriveState.Forward;
        }
        else if (backwardPressed && !forwardPressed) // ���� Ű�� ������ ��
        {
            currentDriveState = DriveState.Backward;
        }
        else if (!forwardPressed && !backwardPressed) // �ƹ� �̵� Ű�� �� �������� ���� (�Ǵ� ���� ���� ���� - ���⼭�� ������)
        {
            currentDriveState = DriveState.Idle;
        }
        switch (currentDriveState)
        {
            case DriveState.Forward:
                targetWheelSpeeds[WheelLocation.LF] = targetWheelSpeed_RadPerSec;
                targetWheelSpeeds[WheelLocation.LB] = targetWheelSpeed_RadPerSec;
                targetWheelSpeeds[WheelLocation.RF] = targetWheelSpeed_RadPerSec;
                targetWheelSpeeds[WheelLocation.RB] = targetWheelSpeed_RadPerSec;
                break;
            case DriveState.Backward:
                targetWheelSpeeds[WheelLocation.LF] = -targetWheelSpeed_RadPerSec;
                targetWheelSpeeds[WheelLocation.LB] = -targetWheelSpeed_RadPerSec;
                targetWheelSpeeds[WheelLocation.RF] = -targetWheelSpeed_RadPerSec;
                targetWheelSpeeds[WheelLocation.RB] = -targetWheelSpeed_RadPerSec;
                break;
            case DriveState.Idle:
            default:
                targetWheelSpeeds[WheelLocation.LF] = 0;
                targetWheelSpeeds[WheelLocation.LB] = 0;
                targetWheelSpeeds[WheelLocation.RF] = 0;
                targetWheelSpeeds[WheelLocation.RB] = 0;
                break;
        }
    }
    Quaternion correctionRotation = Quaternion.Euler(180f, 0f, 0f);
    // Update is called once per frame
    void FixedUpdate()
    {
        FMUInput();
        fmu.DoStep(Time.timeAsDouble, Time.fixedDeltaTime);
        zero6dof();
    }
    void zero6dof()
    {
        double x, y, z, qw, qx, qy, qz;
        x = fmu.GetReal("zero_position_x");
        y = fmu.GetReal("zero_position_y");
        z = fmu.GetReal("zero_position_z");
        transform.position= initialPosition + new Vector3((float)x, (float)y, (float)z);

        qx = fmu.GetReal("zero_orientation[2,1]");
        qy = fmu.GetReal("zero_orientation[3,1]");
        qz = fmu.GetReal("zero_orientation[4,1]");
        qw = fmu.GetReal("zero_orientation[1,1]");
        transform.rotation = new Quaternion((float)qx,(float)qy, (float)qz, (float)qw) ;
    }
    public void Reset()
    {
        fmu.Reset();
        fmu.SetupExperiment(Time.fixedTimeAsDouble);
        fmu.EnterInitializationMode();
    }


    public void UpdateControlCommands(CmdVelMsg message)
    {
        targetWheelSpeeds[WheelLocation.LF] = message.fl_vel;
        targetWheelSpeeds[WheelLocation.LB] = message.bl_vel;
        targetWheelSpeeds[WheelLocation.RF] = message.fr_vel;
        targetWheelSpeeds[WheelLocation.RB] = message.br_vel;
        targetSteeringAngles[WheelLocation.LF] = message.fl_ang;
        targetSteeringAngles[WheelLocation.LB] = message.bl_ang;
        targetSteeringAngles[WheelLocation.RF] = message.fr_ang;
        targetSteeringAngles[WheelLocation.RB] = message.br_ang;
    }

    // FMUSimulator.cs �ȿ� �ִ� �� �Լ��� ã�Ƽ� ���빰�� ��ü�ϼ���.
    public void UpdateGroundContactData()
    {
        if (groundSensors == null || groundSensors.Count != 4)
        {
            for (int i = 0; i < currentGroundContacts.Length; ++i)
            {
                currentGroundContacts[i] = GroundContactInfo.NonContact();
            }
            return;
        }

        // [������ ����]
        for (int i = 0; i < groundSensors.Count; i++)
        {
            WheelLocation loc = (WheelLocation)i;
            if (groundSensors[i] != null)
            {
                currentGroundContacts[(int)loc] = new GroundContactInfo(
                    groundSensors[i].IsContacting,
                    groundSensors[i].GroundHeight,
                    groundSensors[i].GroundPitch,
                    groundSensors[i].GroundRoll
                );
            }
            else
            {
                // Debug.LogWarning("GroundSensor at index " + i + " is not assigned.");
                currentGroundContacts[(int)loc] = GroundContactInfo.NonContact();
            }
        }
    }
    public void FMUInput()
    {
        UpdateGroundContactData();
        foreach(WheelLocation loc in System.Enum.GetValues(typeof(WheelLocation))) 
        {
            //Debug.Log(loc.ToString());
            try
            {
                fmu.SetReal($"{loc}TireAngularVelocityInput", targetWheelSpeeds[loc]);
                fmu.SetReal($"{loc}SteeringAngleInput", targetSteeringAngles[loc]);
            }
            catch(System.Exception e)
            {
                Debug.LogError($"FMU SetReal for Control Command (Wheel: {loc}) failed: {e.Message}. Check variable names in modelDescription.xml.");
            }
        }
        for (int i = 0; i < currentGroundContacts.Length; ++i)
        {
            WheelLocation loc = (WheelLocation)i;
            GroundContactInfo contactInfo = currentGroundContacts[i];
            try
            {

                // ���ο� ������ �԰ݿ� ���� FMU �������� �����մϴ�.
                // (�� �������� Simscape ���� �Է� ��Ʈ �̸��� ��Ȯ�� ��ġ�ؾ� �մϴ�)
                fmu.SetReal($"{loc}_is_contacting_in", contactInfo.IsContacting ? 1.0 : 0.0);
                fmu.SetReal($"{loc}_ray_in", contactInfo.GroundHeight);
                fmu.SetReal($"{loc}_gnd_pitch_in", contactInfo.GroundPitch);
                fmu.SetReal($"{loc}_gnd_roll_in", contactInfo.GroundRoll);

                // ����� �α� (������)
                 Debug.Log($"[{loc}] Contact:{contactInfo.IsContacting}, H:{contactInfo.GroundHeight:F3}, P:{contactInfo.GroundPitch:F3}, R:{contactInfo.GroundRoll:F3}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FMU SetReal for Ground Info (Wheel: {loc}) failed: {e.Message}. Check variable names in modelDescription.xml.");
            }
        }
    }
    public void KeyInput()
    {

    }
    void OnDestroy()
    {
        // clean up
        fmu.Dispose();
    }
}