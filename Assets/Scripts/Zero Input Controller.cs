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
    public float sendInterval = 0.1f;
    private float timeSinceLastSend = 0f;

    [Header("Input Actions")]
    public InputActionAsset inputActions;

    [Header("Ackermann Mode Settings")]
    public float maxLinearVelocity = 100.0f;
    public float maxAngularVelocity = 500.0f;

    [Header("Omni-Directional Mode Settings")]
    public float omniMaxLinearVelocity = 100.0f;
    public float omniMaxAngularVelocity = 1000.0f;

    [Header("Input Smoothing")]
    public float linearAcceleration = 2.0f;
    public float linearDeceleration = 3.0f;
    public float angularAcceleration = 2.0f;
    public float angularDeceleration = 3.0f;

    private InputAction moveAction;
    private InputAction turnAction;
    private InputAction switchModeAction;

    private float currentSmoothedMoveInput = 0f;
    private float currentSmoothedTurnInput = 0f;

    private float currentV = 0.0f;
    private float currentW = 0.0f;

    private void Awake()
    {
        var manualControlMap = inputActions.FindActionMap("Rover");
        if (manualControlMap == null) { Debug.LogError("Action Map 'Rover' not found!"); return; }

        moveAction = manualControlMap.FindAction("Move");
        turnAction = manualControlMap.FindAction("Turn");
        switchModeAction = manualControlMap.FindAction("SwitchDriveMode");

        if (moveAction == null) { Debug.LogError("'Move' action not found!"); enabled = false; return; }
        if (turnAction == null) { Debug.LogWarning("'Turn' action not found! Rotation might not work."); }
        if (switchModeAction == null) { Debug.LogError("'SwitchDriveMode' action not found!"); }

        totalDriveModes = System.Enum.GetValues(typeof(DriveMode)).Length;
    }

    void Start()
    {
        driveModeText.text = currentDriveMode.ToString();
        InitializeSerialPort();
    }

    void Update()
    {
        float rawMoveInput = moveAction.ReadValue<float>();
        float rawTurnInput = turnAction.ReadValue<float>();

        float targetMoveInput = rawMoveInput;
        float currentLinearRate = (Mathf.Approximately(targetMoveInput, 0f)) ? linearDeceleration : linearAcceleration;
        currentSmoothedMoveInput = Mathf.MoveTowards(currentSmoothedMoveInput, targetMoveInput, currentLinearRate * Time.deltaTime);

        float targetTurnInput = rawTurnInput;
        float currentAngularRate = (Mathf.Approximately(targetTurnInput, 0f)) ? angularDeceleration : angularAcceleration;
        currentSmoothedTurnInput = Mathf.MoveTowards(currentSmoothedTurnInput, targetTurnInput, currentAngularRate * Time.deltaTime);

        switch (currentDriveMode)
        {
            case DriveMode.Ackermann:
                currentV = currentSmoothedMoveInput * maxLinearVelocity;
                currentW = currentSmoothedTurnInput * maxAngularVelocity;
                Debug.Log($"currentV : {currentV} + currentW : {currentW}");
                break;

            case DriveMode.OmniDirectional:
                // 지금은 Omni도 v, w를 보내도록 설정
                currentV = currentSmoothedMoveInput * omniMaxLinearVelocity;
                currentW = currentSmoothedTurnInput * omniMaxAngularVelocity;
                break;
        }

        timeSinceLastSend += Time.deltaTime;
        if (timeSinceLastSend >= sendInterval)
        {
            if (currentDriveMode == DriveMode.Ackermann)
            {
                SendAckermannCommand(currentV, currentW);
            }
            else if (currentDriveMode == DriveMode.OmniDirectional)
            {
                SendOmniCommand(currentV, currentW);
            }
            timeSinceLastSend = 0f;
        }
    }

    // --- (InitializeSerialPort, OnEnable/Disable, onSwitchDriveModePerformed, CloseSerialPort는 기존과 동일) ---
    void InitializeSerialPort()
    {
        try
        {
            serialPort = new SerialPort(comPortName, baudRate)
            {
                ReadTimeout = 500,
                WriteTimeout = 500
            };
            serialPort.Handshake = Handshake.None;
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
        if (inputActions != null) { inputActions.Enable(); }
        if (switchModeAction != null) { switchModeAction.performed += onSwitchDriveModePerformed; }
    }
    private void OnDisable()
    {
        if (inputActions != null) { inputActions.Disable(); }
        if (switchModeAction != null) { switchModeAction.performed -= onSwitchDriveModePerformed; }
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
    void CloseSerialPort()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                serialPort.Close();
                Debug.Log($"Serial port {comPortName} closed.");
            }
            catch (Exception e) { Debug.LogError($"Error closing serial port {comPortName}: {e.Message}"); }
            serialPort = null;
        }
    }
    // --- (여기까지는 거의 동일) ---


    /// <summary>
    /// Ackermann 모드(v, w) 데이터를 전송합니다.
    /// </summary>
    public void SendAckermannCommand(float v, float w)
    {
        if (serialPort == null || !serialPort.IsOpen) return;

        short v_short = (short)Mathf.Clamp(v * (32767.0f / maxLinearVelocity), short.MinValue, short.MaxValue);
        short w_short = (short)Mathf.Clamp(w * (32767.0f / maxAngularVelocity), short.MinValue, short.MaxValue);

        byte[] v_bytes = BitConverter.GetBytes(v_short);
        byte[] w_bytes = BitConverter.GetBytes(w_short);

        // 7바이트 패킷: [STX][Mode][v_LSB][v_MSB][w_LSB][w_MSB][ETX]
        byte[] packet = new byte[7];
        packet[0] = 0x02;                           // STX (Start of Text)
        packet[1] = (byte)DriveMode.Ackermann;      // Mode (0)
        packet[2] = v_bytes[0];                     // v Low Byte
        packet[3] = v_bytes[1];                     // v High Byte
        packet[4] = w_bytes[0];                     // w Low Byte
        packet[5] = w_bytes[1];                     // w High Byte
        packet[6] = 0x03;                           // ETX (End of Text)

        try
        {
            serialPort.Write(packet, 0, packet.Length);
            // Debug.Log("Sent Ackermann Packet: " + BitConverter.ToString(packet));
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    /// <summary>
    /// Omni-directional 모드(v, w) 데이터를 전송합니다.
    /// </summary>
    private void SendOmniCommand(float v, float w)
    {
        if (serialPort == null || !serialPort.IsOpen) return;

        short v_short = (short)Mathf.Clamp(v * (32767.0f / omniMaxLinearVelocity), short.MinValue, short.MaxValue);
        short w_short = (short)Mathf.Clamp(w * (32767.0f / omniMaxAngularVelocity), short.MinValue, short.MaxValue);

        byte[] v_bytes = BitConverter.GetBytes(v_short);
        byte[] w_bytes = BitConverter.GetBytes(w_short);

        // 7바이트 패킷: [STX][Mode][v_LSB][v_MSB][w_LSB][w_MSB][ETX]
        byte[] packet = new byte[7];
        packet[0] = 0x02;                               // STX
        packet[1] = (byte)DriveMode.OmniDirectional;    // Mode (1)
        packet[2] = v_bytes[0];
        packet[3] = v_bytes[1];
        packet[4] = w_bytes[0];
        packet[5] = w_bytes[1];
        packet[6] = 0x03;                               // ETX

        try
        {
            serialPort.Write(packet, 0, packet.Length);
            // Debug.Log("Sent Omni Packet: " + BitConverter.ToString(packet));
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}