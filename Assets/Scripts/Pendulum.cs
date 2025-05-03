using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMI2;

public class Pendulum : MonoBehaviour, IFloatDataProvider
{
    private FMU fmu;
    public float initialAngle = 45;
    public float dampingCoefficient = 0.00001f;
    public string fmuName;
    public float angleX = 0f;
    public float angleZ = 0f;
    public float torque = 0f;
    // Start is called before the first frame update
    void Start()
    {
        fmu = new FMU(fmuName, name);
        Reset();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        fmu.DoStep(Time.timeAsDouble, Time.deltaTime);
        //Debug.Log(fmu.GetReal("Angle"));
        float angleY = (float)fmu.GetReal("Angle");
        transform.localRotation = Quaternion.Euler(angleX, angleY, angleZ);
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
    public void Torque()
    {
        fmu.SetReal("Torque", torque);
    }
    void OnDestroy()
    {
        // clean up
        fmu.Dispose();
    }

    public double GetData()
    {
        return fmu.GetReal("Angle");
    }
}
