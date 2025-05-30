using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class ZeroInputController : MonoBehaviour
{
    public enum DriveMode
    {
        Ackermann,
        OmniDirectional
    }
    [Header("Drive Mode")]
    public DriveMode currentDriveMode = DriveMode.Ackermann;
    public TextMeshProUGUI driveModeText;
    private int totalDriveModes;

    [Header("Serial Port Settings")]
    public string comPortName = "COM10";
    public int baudRate = 9600;
    private SerialPort serialPort;

    [Header("Input Actions")]
    public InputActionAsset inputActions;

    [Header("Ackermann Mode Settings")]
    public float maxLinearVelocity = 100.0f;
    public float maxAngularVelocity = 500.0f;

    [Header("Omni-Directional Mode Settings")]
    public float omniMaxLinearVelocity = 100.0f;
    public float omniMaxAngularVelocity = 1000.0f;

    [Header("Input Smoothing")]
    public float linearAcceleration = 2.0f;  // 초당 선형 입력값 증가량 (0에서 1 또는 -1까지 도달하는 속도)
    public float linearDeceleration = 3.0f;  // 초당 선형 입력값 감소량 (1 또는 -1에서 0까지 도달하는 속도)
    public float angularAcceleration = 2.0f; // 초당 각속도 입력값 증가량
    public float angularDeceleration = 3.0f; // 초당 각속도 입력값 감소량

    private InputAction moveAction;
    private InputAction turnAction;
    private InputAction switchModeAction;

    private float currentSmoothedMoveInput = 0f; // 현재 스무딩된 선형 입력 값 (-1 to 1)
    private float currentSmoothedTurnInput = 0f; // 현재 스무딩된 각속도 입력 값 (-1 to 1)


    private float currentV = 0.0f;
    private float currentW = 0.0f;

    private void Awake()
    {
        var manualControlMap = inputActions.FindActionMap("Rover");
        if(manualControlMap == null)
        {
            Debug.Log("Action Map 'Rover' not found!");
            return;
        }

        moveAction = manualControlMap.FindAction("Move");
        turnAction = manualControlMap.FindAction("Turn");
        switchModeAction = manualControlMap.FindAction("SwitchDriveMode");

        if (moveAction == null) { Debug.LogError("'Move' action not found!"); enabled = false; return; }
        if (turnAction == null) { Debug.LogError("'Turn' action not found! Omni rotation might not work."); } // 경고만 표시
        if (switchModeAction == null) { Debug.LogError("'SwitchDriveMode' action not found!"); }

        totalDriveModes = System.Enum.GetValues(typeof(DriveMode)).Length;

    }
    

    // Start is called before the first frame update
    void Start()
    {
        driveModeText.text = currentDriveMode.ToString();
        InitializeSerialPort();
    }

    // Update is called once per frame
    void Update()
    {
        // 1. 원시 입력 값 읽기 (-1, 0, 또는 1)
        float rawMoveInput = moveAction.ReadValue<float>();
        float rawTurnInput = turnAction.ReadValue<float>();

        // 2. 스무딩 적용
        // 선형 속도 입력 스무딩
        float targetMoveInput = rawMoveInput;
        float currentLinearRate = (Mathf.Approximately(targetMoveInput, 0f)) ? linearDeceleration : linearAcceleration;
        currentSmoothedMoveInput = Mathf.MoveTowards(currentSmoothedMoveInput, targetMoveInput, currentLinearRate * Time.deltaTime);

        // 각속도 입력 스무딩
        float targetTurnInput = rawTurnInput;
        float currentAngularRate = (Mathf.Approximately(targetTurnInput, 0f)) ? angularDeceleration : angularAcceleration;
        currentSmoothedTurnInput = Mathf.MoveTowards(currentSmoothedTurnInput, targetTurnInput, currentAngularRate * Time.deltaTime);
        switch(currentDriveMode)
        {
            case DriveMode.Ackermann:
                currentV = currentSmoothedMoveInput * maxLinearVelocity;
                currentW = currentSmoothedTurnInput * maxAngularVelocity;
                SendAckermannCommand(currentV, currentW);
                //Debug.Log($" Mode : {currentDriveMode} v : {currentV}, w : {currentW}");
                break;

            case DriveMode.OmniDirectional:
                currentV = currentSmoothedMoveInput * maxLinearVelocity;
                currentW = currentSmoothedTurnInput * maxAngularVelocity;
                SendOmniCommand(currentV, currentW);
                Debug.Log($" Mode : {currentDriveMode}, v : {currentV}, w : {currentW}");
                break;
        }
        // 3. 최종 v, w 계산
    }

    void InitializeSerialPort()
    {
        try
        {
            serialPort = new SerialPort(comPortName, baudRate)
            {
                ReadTimeout = 100,
                WriteTimeout = 100
            };
            serialPort.Open();
            Debug.Log($"Serial port {comPortName} opened successfully at {baudRate}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error opening serial port{comPortName}: {e.Message}");
            serialPort = null;
        }
    }
    private void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Enable();
        }
        if (switchModeAction != null)
        {
            switchModeAction.performed += onSwitchDriveModePerformed;
        }
    }
    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Disable();
        }
        if (switchModeAction != null)
        {
            switchModeAction.performed -= onSwitchDriveModePerformed;
        }
        CloseSerialPort();
    }

    private void onSwitchDriveModePerformed(InputAction.CallbackContext context)
    {
        currentDriveMode = (DriveMode)(((int)currentDriveMode + 1) % totalDriveModes);
        driveModeText.text = currentDriveMode.ToString();
        Debug.Log("Drive Mode Switched To: " + currentDriveMode.ToString());
        currentSmoothedMoveInput = 0f;
        currentSmoothedTurnInput = 0f;
        currentV = 0f;
        currentW = 0f;
    }
    public void SendAckermannCommand(float v, float w)
    {
        if (serialPort == null || !serialPort.IsOpen) 
        {
            Debug.LogError("serial port is not open.");
            return;
        }
        short v_short = (short)Mathf.Clamp(v * (32767.0f / maxLinearVelocity), short.MinValue, short.MaxValue);
        short w_short = (short)Mathf.Clamp(w * (32767.0f / maxAngularVelocity), short.MinValue, short.MaxValue);

        byte[] v_bytes = BitConverter.GetBytes(v_short);
        byte[] w_bytes = BitConverter.GetBytes(w_short);

        byte[] packet = new byte[6];
        packet[0] = 0x02;
        packet[1] = v_bytes[0];
        packet[2] = v_bytes[1];
        packet[3] = w_bytes[0];
        packet[4] = w_bytes[1];
        packet[5] = 0x03;

        try
        {
            serialPort.Write(packet, 0, packet.Length);
            Debug.Log("Unity Sent Packet: " + BitConverter.ToString(packet));
        }
        catch (TimeoutException)
        {
            Debug.LogWarning("Timeout writing to serialPort");
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }


    public void SendOmniCommand(float v, float theta)
    {

    }
    void CloseSerialPort() // OnDisable, OnApplicationQuit 등에서 호출
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                serialPort.Close();
                Debug.Log($"Serial port {comPortName} closed.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error closing serial port {comPortName}: {e.Message}");
            }
            serialPort = null;
        }
    }
}
