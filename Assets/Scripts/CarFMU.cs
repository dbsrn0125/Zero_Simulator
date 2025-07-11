using System.Collections;
using System.Collections.Generic;
using FMI2;
using UnityEngine;

public class CarFMU : MonoBehaviour
{
    public FMU fmu;
    public string fmuName;
    public KeyCode forwardKey = KeyCode.A;      // ÀüÁø Å°
    public KeyCode backwardKey = KeyCode.D;
    // Start is called before the first frame update
    void Start()
    {
        fmu = new FMU(fmuName, name);
        Reset();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        bool forwardPressed = Input.GetKey(forwardKey);
        bool backwardPressed = Input.GetKey(backwardKey);
        double x, y, z, qw, qx, qy, qz;
        x = fmu.GetReal("x");
        y = fmu.GetReal("y");
        z = fmu.GetReal("z");
        Debug.Log(x);
        Debug.Log(y);
        Debug.Log(z);
        fmu.SetReal("qWhl", 1);
        transform.position = new Vector3((float)x, (float)y, (float)z);
    }

    public void Reset()
    {
        fmu.Reset();
        fmu.SetupExperiment(Time.fixedTimeAsDouble);
        fmu.EnterInitializationMode();
    }
}
