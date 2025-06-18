using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public int baudRate = 115200;
    private SerialPort serialPort;
    private CancellationTokenSource cancellationTokenSource;
    private readonly object serialLock = new object();

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
        if (turnAction == null) { Debug.LogWarning("'Turn' action not found!"); }
        if (switchModeAction == null) { Debug.LogError("'SwitchDriveMode' action not found!"); }

        totalDriveModes = Enum.GetValues(typeof(DriveMode)).Length;
    }

    void Start()
    {
        driveModeText.text = currentDriveMode.ToString();
        InitializeSerialPort();

        cancellationTokenSource = new CancellationTokenSource();
        StartSerialWriterLoop(cancellationTokenSource.Token);
    }

    private void InitializeSerialPort()
    {
        try
        {
            serialPort = new SerialPort(comPortName, baudRate)
            {
                WriteTimeout = 500,
                ReadTimeout = 500,
                Handshake = Handshake.None
            };
            serialPort.Open();
            Debug.Log($"Serial port {comPortName} opened successfully at {baudRate}");
        }
        catch (Exception e)
        {
            Debug.LogError($" Failed to open serial port: {e.Message}");
            serialPort = null;
        }
    }

    private async void StartSerialWriterLoop(CancellationToken token)
    {
        await Task.Delay(1000); // MCU 준비 시간 확보

        Debug.Log("SerialWriterLoop started");

        while (!token.IsCancellationRequested)
        {
            if (serialPort != null && serialPort.IsOpen && serialPort.BytesToWrite == 0)
            {
                byte[] packet = MakeCommandPacket(currentDriveMode, currentV, currentW);
                //Debug.Log("Sending: " + BitConverter.ToString(packet));

                try
                {
                    lock (serialLock)
                    {
                        serialPort.Write(packet, 0, packet.Length);
                        Debug.Log("Sent: " + BitConverter.ToString(packet));
                        
                    }
                }
                catch (TimeoutException te)
                {
                    Debug.LogWarning("Timeout while writing: " + te.Message);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Serial write error: " + ex.Message);
                }
            }

            await Task.Delay(20, token);
        }

        Debug.Log("SerialWriterLoop cancelled.");
    }

    private byte[] MakeCommandPacket(DriveMode mode, float v, float w)
    {
        short v_short = (short)Mathf.Clamp(v * (32767.0f / (mode == DriveMode.Ackermann ? maxLinearVelocity : omniMaxLinearVelocity)), short.MinValue, short.MaxValue);
        short w_short = (short)Mathf.Clamp(w * (32767.0f / (mode == DriveMode.Ackermann ? maxAngularVelocity : omniMaxAngularVelocity)), short.MinValue, short.MaxValue);

        byte[] v_bytes = BitConverter.GetBytes(v_short);
        byte[] w_bytes = BitConverter.GetBytes(w_short);

        byte[] packet = new byte[7];
        packet[0] = 0x02;
        packet[1] = (byte)mode;
        packet[2] = v_bytes[0];
        packet[3] = v_bytes[1];
        packet[4] = w_bytes[0];
        packet[5] = w_bytes[1];
        packet[6] = 0x03;

        return packet;
    }

    private void Update()
    {
        float rawMoveInput = moveAction.ReadValue<float>();
        float rawTurnInput = turnAction.ReadValue<float>();

        float currentLinearRate = Mathf.Approximately(rawMoveInput, 0f) ? linearDeceleration : linearAcceleration;
        currentSmoothedMoveInput = Mathf.MoveTowards(currentSmoothedMoveInput, rawMoveInput, currentLinearRate * Time.deltaTime);

        float currentAngularRate = Mathf.Approximately(rawTurnInput, 0f) ? angularDeceleration : angularAcceleration;
        currentSmoothedTurnInput = Mathf.MoveTowards(currentSmoothedTurnInput, rawTurnInput, currentAngularRate * Time.deltaTime);

        switch (currentDriveMode)
        {
            case DriveMode.Ackermann:
                currentV = currentSmoothedMoveInput * maxLinearVelocity;
                currentW = currentSmoothedTurnInput * maxAngularVelocity;
                break;
            case DriveMode.OmniDirectional:
                currentV = currentSmoothedMoveInput * omniMaxLinearVelocity;
                currentW = currentSmoothedTurnInput * omniMaxAngularVelocity;
                break;
        }
    }

    private void OnEnable()
    {
        if (inputActions != null) inputActions.Enable();
        if (switchModeAction != null) switchModeAction.performed += onSwitchDriveModePerformed;
    }

    private void OnDisable()
    {
        if (inputActions != null) inputActions.Disable();
        if (switchModeAction != null) switchModeAction.performed -= onSwitchDriveModePerformed;
        cancellationTokenSource?.Cancel();
        CloseSerialPort();
    }

    private void onSwitchDriveModePerformed(InputAction.CallbackContext context)
    {
        currentDriveMode = (DriveMode)(((int)currentDriveMode + 1) % totalDriveModes);
        driveModeText.text = currentDriveMode.ToString();
        currentSmoothedMoveInput = 0f;
        currentSmoothedTurnInput = 0f;
        currentV = 0f;
        currentW = 0f;
    }

    private void CloseSerialPort()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                serialPort.Close();
                Debug.Log($"Serial port {comPortName} closed.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error while closing port: " + ex.Message);
            }
        }
        serialPort = null;
    }
}
