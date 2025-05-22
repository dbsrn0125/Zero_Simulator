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
                    groundSensors[i].CurrentFrictionCoefficient
                );
            }
            else
            {
                Debug.LogWarning("GroundSensor at index " + i + " is not assigned.");
                currentGroundContacts[(int)loc] = GroundContactInfo.NonContact();
            }
        }
    }
    public void FMUInput()
    {
        UpdateGroundContactData();
        foreach(WheelLocation loc in System.Enum.GetValues(typeof(WheelLocation))) 
        {
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
            WheelLocation loc= (WheelLocation)i;
            GroundContactInfo contactInfo = currentGroundContacts[i];
            try
            {
                fmu.SetReal($"{loc}Is_contacting_in", contactInfo.IsContacting ? 1.0 : 0.0);
                fmu.SetReal($"{loc}Penetration_depth_in",contactInfo.PenetrationDepth);
                fmu.SetReal($"{loc}Ground_normal_world_in_vec[1]", contactInfo.WorldNormal.x);
                fmu.SetReal($"{loc}Ground_normal_world_in_vec[2]", contactInfo.WorldNormal.y);
                fmu.SetReal($"{loc}Ground_normal_world_in_vec[3]", contactInfo.WorldNormal.z);
                fmu.SetReal($"{loc}Friction_coeff_in",contactInfo.FrictionCoefficient);
                Debug.Log($"{loc} + {contactInfo.IsContacting} + {contactInfo.WorldNormal} + {contactInfo.PenetrationDepth} + {contactInfo.FrictionCoefficient}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FMU SetReal for Ground Info (Wheel: {loc}) failed: {e.Message}. Check variable names in modelDescription.xml.");
            }
        }            
    }
    void OnDestroy()
    {
        // clean up
        fmu.Dispose();
    }
}