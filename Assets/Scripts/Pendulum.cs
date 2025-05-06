using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMI2; 

public class Pendulum : MonoBehaviour, ISetpointStateProvider
{
    private FMU fmu;
    public float initialAngle = 45;
    public float dampingCoefficient = 0.00001f;
    public string fmuName;
    public double torque;
    [Tooltip("PID 제어 목표 각도 (Degrees)")]
    public double targetAngleDegrees = 0.0;
    // Start is called before the first frame update
    void Start()
    {
        fmu = new FMU(fmuName, name);
        Reset();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //customTorque();
        fmu.DoStep(Time.timeAsDouble, Time.deltaTime);
        //Debug.Log(fmu.GetReal("Angle"));
        double angleY = GetCurrentAngle();
        transform.localRotation = Quaternion.Euler(0, (float)angleY, 0);
    }
    public void SetDampingCoefficient(float e)
    {
        dampingCoefficient = Mathf.Clamp(e, 0.0001f, 0.001f);
        fmu.SetReal("DampingCoefficient", dampingCoefficient);
        Debug.Log(fmu.GetReal("DampingCoefficient"));
    }

    public void Reset()
    {
        fmu.Reset();
        fmu.SetupExperiment(Time.timeAsDouble);
        fmu.EnterInitializationMode();
        fmu.SetReal("InitialAngle",initialAngle);
        fmu.SetReal("DampingCoefficient", dampingCoefficient);
        
    }
    public void customTorque()
    {
        fmu.SetReal("Torque", torque);
    }
    public void Torque(double torque)
    {
        Debug.Log(torque);
        fmu.SetReal("Torque", torque);
    }
    void OnDestroy()
    {
        // clean up
        fmu.Dispose();
    }
    private static double NormalizeAngleDeg(double angleDeg)
    {
        double remainder = angleDeg % 360.0;
        if (remainder <= -180)
            return remainder + 360.0;
        else if (remainder >= 180)
            return remainder - 360.0;
        else
            return remainder;
    }
    public double GetTargetAngle()
    {
        return targetAngleDegrees;
    }

    public double GetCurrentAngle()
    {
        double currentAngle = fmu.GetReal("Angle");
        //Debug.Log(currentAngle);
        return currentAngle;
    }
}
