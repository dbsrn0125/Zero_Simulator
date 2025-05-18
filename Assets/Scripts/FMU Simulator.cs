using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMI2;
using RosMessageTypes.ZeroInterfaces;

public class FMUSimulator : MonoBehaviour
{
    private FMU fmu;
    public string fmuName;
    public List<GroundSensor> groundSensors = new List<GroundSensor>(4);

    private Dictionary<WheelLocation, float> targetWheelSpeeds = new Dictionary<WheelLocation, float>();
    private Dictionary<WheelLocation, float> targetSteeringAngles = new Dictionary<WheelLocation, float>();
    private GroundContactInfo[] currentGroundContacts = new GroundContactInfo[4];
    private double currentSimTime = 0.0;
    private double fmuStepSize;
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
        fmuStepSize = Time.fixedDeltaTime;
        currentSimTime = Time.fixedTimeAsDouble;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateGroundContactData();
        fmu.DoStep(currentSimTime, fmuStepSize);
    }

    public void Reset()
    {
        fmu.Reset();
        fmu.SetupExperiment(Time.timeAsDouble);
        fmu.EnterInitializationMode();
    }


    public void UpdateControlCommands(CmdVelMsg message)
    {
        targetWheelSpeeds[WheelLocation.FL] = message.fl_vel;
        targetWheelSpeeds[WheelLocation.BL] = message.bl_vel;
        targetWheelSpeeds[WheelLocation.FR] = message.fr_vel;
        targetWheelSpeeds[WheelLocation.BR] = message.br_vel;
        targetSteeringAngles[WheelLocation.FL] = message.fl_ang;
        targetSteeringAngles[WheelLocation.BL] = message.bl_ang;
        targetSteeringAngles[WheelLocation.FR] = message.fr_ang;
        targetSteeringAngles[WheelLocation.BR] = message.br_ang;
    }

    public void UpdateGroundContactData()
    {
        if (groundSensors == null || groundSensors.Count != 4)
        {
            // Debug.LogWarning("GroundSensors List가 제대로 할당되지 않았거나 개수가 맞지 않습니다.");
            // 모든 센서가 할당되지 않았을 경우를 대비해 NonContact으로 초기화
            for (int i = 0; i < currentGroundContacts.Length; ++i)
            {
                currentGroundContacts[i] = GroundContactInfo.NonContact();
            }
            return;
        }

        for (int i = 0; i < groundSensors.Count; i++)
        {
            WheelLocation loc = (WheelLocation)i; // enum 값이 0,1,2,3 순서라고 가정
            if (groundSensors[i] != null)
            {
                currentGroundContacts[(int)loc] = new GroundContactInfo(
                    groundSensors[i].IsContacting,
                    groundSensors[i].CalculatedPenetrationDepth,
                    groundSensors[i].CalculatedWorldNormal,
                    groundSensors[i].CurrentFrictionCoefficient,
                    groundSensors[i].AverageContactPoint_World
                );
            }
            else
            {
                Debug.LogWarning("GroundSensor at index " + i + " is not assigned.");
                currentGroundContacts[(int)loc] = GroundContactInfo.NonContact();
            }
        }
    }
    void OnDestroy()
    {
        // clean up
        fmu.Dispose();
    }
}