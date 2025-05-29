using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ZeroInputController : MonoBehaviour
{
    public enum DriveMode
    {
        Ackermann,
        OmniDirectional
    }
    [Header("Drive Mode")]
    public DriveMode currentDriveMode = DriveMode.Ackermann;
    private int totalDriveModes;

    [Header("Input Actions")]
    public InputActionAsset inputActions;

    [Header("Ackermann Mode Settings")]
    public float maxLinearVelocity = 100.0f;
    public float maxAngularVelocity = 1000.0f;

    [Header("Omni-Directional Mode Settings")]
    public float omniMaxLinearVelocity = 100.0f;
    public float omniMaxAngularVelocity = 1000.0f;

    [Header("Input Smoothing")]
    public float linearAcceleration = 2.0f;  // �ʴ� ���� �Է°� ������ (0���� 1 �Ǵ� -1���� �����ϴ� �ӵ�)
    public float linearDeceleration = 3.0f;  // �ʴ� ���� �Է°� ���ҷ� (1 �Ǵ� -1���� 0���� �����ϴ� �ӵ�)
    public float angularAcceleration = 2.0f; // �ʴ� ���ӵ� �Է°� ������
    public float angularDeceleration = 3.0f; // �ʴ� ���ӵ� �Է°� ���ҷ�

    private InputAction moveAction;
    private InputAction turnAction;
    private InputAction switchModeAction;

    private float currentSmoothedMoveInput = 0f; // ���� �������� ���� �Է� �� (-1 to 1)
    private float currentSmoothedTurnInput = 0f; // ���� �������� ���ӵ� �Է� �� (-1 to 1)


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
        if (turnAction == null) { Debug.LogError("'Turn' action not found! Omni rotation might not work."); } // ��� ǥ��
        if (switchModeAction == null) { Debug.LogError("'SwitchDriveMode' action not found!"); }

        totalDriveModes = System.Enum.GetValues(typeof(DriveMode)).Length;

    }
    private void OnEnable()
    {
        if(inputActions != null)
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
        if(inputActions != null)
        {
            inputActions.Disable();
        }
        if (switchModeAction != null)
        {
            switchModeAction.performed -= onSwitchDriveModePerformed;
        }
    }

    private void onSwitchDriveModePerformed(InputAction.CallbackContext context)
    {
        currentDriveMode = (DriveMode)(((int)currentDriveMode+1)%totalDriveModes);
        Debug.Log("Drive Mode Switched To: " + currentDriveMode.ToString());
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 1. ���� �Է� �� �б� (-1, 0, �Ǵ� 1)
        float rawMoveInput = moveAction.ReadValue<float>();
        float rawTurnInput = turnAction.ReadValue<float>();

        // 2. ������ ����
        // ���� �ӵ� �Է� ������
        float targetMoveInput = rawMoveInput;
        float currentLinearRate = (Mathf.Approximately(targetMoveInput, 0f)) ? linearDeceleration : linearAcceleration;
        currentSmoothedMoveInput = Mathf.MoveTowards(currentSmoothedMoveInput, targetMoveInput, currentLinearRate * Time.deltaTime);

        // ���ӵ� �Է� ������
        float targetTurnInput = rawTurnInput;
        float currentAngularRate = (Mathf.Approximately(targetTurnInput, 0f)) ? angularDeceleration : angularAcceleration;
        currentSmoothedTurnInput = Mathf.MoveTowards(currentSmoothedTurnInput, targetTurnInput, currentAngularRate * Time.deltaTime);
        switch(currentDriveMode)
        {
            case DriveMode.Ackermann:
                currentV = currentSmoothedMoveInput * maxLinearVelocity;
                currentW = currentSmoothedTurnInput * maxAngularVelocity;
                Debug.Log($" Mode : {currentDriveMode} v : {currentV}, w : {currentW}");
                break;

            case DriveMode.OmniDirectional:
                currentV = currentSmoothedMoveInput * maxLinearVelocity;
                currentW = currentSmoothedTurnInput * maxAngularVelocity;
                Debug.Log($" Mode : {currentDriveMode}, v : {currentV}, w : {currentW}");
                break;
        }
        // 3. ���� v, w ���
    }
}
