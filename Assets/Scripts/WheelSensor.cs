using UnityEngine;

// ������ PID ����� Ŭ����
public class SimplePID
{
    public float Kp = 20f;
    public float Ki = 10f;
    public float Kd = 1f;
    public float MaxOutput = 200f; // ��: �ִ� ���� ��ũ

    private float integral = 0f;
    private float lastError = 0f;

    public float Update(float error, float deltaTime)
    {
        if (deltaTime <= 0f) return 0f;

        integral += error * deltaTime;
        // ��Ƽ ���ε�� (�ɼ�): ���а��� �ʹ� Ŀ���� ���� ����
        if (Ki > 0) // Ki�� 0�� ���� integralMax�� ���Ѵ밡 �� �� �����Ƿ� ����
        {
            float integralMax = MaxOutput / Ki;
            integral = Mathf.Clamp(integral, -integralMax, integralMax);
        }


        float derivative = (error - lastError) / deltaTime;
        lastError = error;

        float output = Kp * error + Ki * integral + Kd * derivative;
        return Mathf.Clamp(output, -MaxOutput, MaxOutput);
    }

    public void Reset()
    {
        integral = 0f;
        lastError = 0f;
    }
}

[RequireComponent(typeof(WheelCollider))]
public class WheelSensor : MonoBehaviour
{
    [Header("WheelCollider Reference")]
    public WheelCollider wheelCollider;

    [Header("Visuals (Optional)")]
    public Transform visualWheel; // ������ �ð��� �� (��ġ ����ȭ��)

    [Header("Inputs (Set by external script from Command/ROS/FMU)")]
    public float TargetSteerAngle = 0f; // ��(Degree) ����
    public float TargetRPM = 0f;        // �д� ȸ���� (RPM)

    [Header("Control Parameters")]
    public float motorKp = 25f;         // RPM ����� PID - P ����
    public float motorKi = 15f;         // RPM ����� PID - I ����
    public float motorKd = 1.5f;        // RPM ����� PID - D ����
    public float maxMotorTorque = 200f; // �ִ� ���� ��ũ (Nm)
    public float maxBrakeTorque = 300f; // �ִ� �극��ũ ��ũ (Nm)
    public float rpmStopThreshold = 2.0f; // �� RPM �����̰� ��ǥ RPM�� 0�̸� �극��ũ ���� ���

    [Header("Outputs (For FMU via an external script)")]
    public Vector3 CalculatedGRF_World;
    public Vector3 ContactPoint_World;
    public bool IsGrounded;

    [Header("Debug Info (Read-only in Inspector)")]
    public Vector3 NormalForceDebug_World;
    public Vector3 FrictionForceDebug_World;
    public float ForwardSlipDebug;
    public float SidewaysSlipDebug;
    public float CurrentRPMDebug;
    public float AppliedMotorTorqueDebug;
    public float AppliedBrakeTorqueDebug;

    private SimplePID rpmPidController;
    private const float RPM_TO_RAD_PER_SEC = Mathf.PI * 2f / 60f;
    private const float RAD_PER_SEC_TO_RPM = 60f / (Mathf.PI * 2f);


    void Awake()
    {
        if (wheelCollider == null)
        {
            wheelCollider = GetComponent<WheelCollider>();
        }

        rpmPidController = new SimplePID
        {
            Kp = motorKp,
            Ki = motorKi,
            Kd = motorKd,
            MaxOutput = maxMotorTorque
        };
    }

    void Update()
    {
        // �ð��� ���� ��ġ ������Ʈ (������� �ݿ�)
        if (visualWheel != null && wheelCollider != null)
        {
            Vector3 position;
            Quaternion rotation; // �� rotation�� WC ������ �����ϹǷ� ���� ��� ����
            wheelCollider.GetWorldPose(out position, out rotation);
            visualWheel.position = position;
            // visualWheel.rotation�� ���Ⱒ(�θ�κ���) + FMU�� ���ɰ����� ���� ó�� �ʿ�
        }

        // ����� ���� ������Ʈ
        if (wheelCollider != null)
        {
            CurrentRPMDebug = wheelCollider.rpm;
            AppliedMotorTorqueDebug = wheelCollider.motorTorque;
            AppliedBrakeTorqueDebug = wheelCollider.brakeTorque;
        }
    }

    void FixedUpdate()
    {
        if (wheelCollider == null) return;

        // --- 1. WheelCollider�� ���� �Է� ���� ---
        wheelCollider.steerAngle = TargetSteerAngle;

        // RPM ���� ����
        float currentRPM = wheelCollider.rpm;

        if (Mathf.Abs(TargetRPM) < rpmStopThreshold) // ��ǥ RPM�� ���� 0�� �� (���� �Ǵ� �ſ� ���� �ӵ�)
        {
            wheelCollider.motorTorque = 0f; // ���� ��ũ 0
            rpmPidController.Reset(); // PID ���� �ʱ�ȭ

            if (Mathf.Abs(currentRPM) > rpmStopThreshold) // ���� ������ ���� �ִٸ� ����
            {
                // ���� ȸ�� ����� �ݴ�� ���� ��ũ�� �ɰų�, �ܼ��� ũ�⸸���� ����
                // wheelCollider.brakeTorque = Mathf.Sign(-currentRPM) * maxBrakeTorque; // �̷��� �ϸ� �ʹ� ���� �� ����
                wheelCollider.brakeTorque = maxBrakeTorque;
            }
            else // �̹� ���� ����ٸ� �극��ũ�� ����
            {
                wheelCollider.brakeTorque = 0f;
            }
        }
        else // ��ǥ RPM�� �־����� ��
        {
            wheelCollider.brakeTorque = 0f; // ���� ���̹Ƿ� �극��ũ ����
            float rpmError = TargetRPM - currentRPM;
            float motorSignal = rpmPidController.Update(rpmError, Time.fixedDeltaTime);
            wheelCollider.motorTorque = motorSignal;
        }

        // --- 2. ���� ���� ���� �������� ---
        IsGrounded = wheelCollider.GetGroundHit(out WheelHit hit);

        if (IsGrounded)
        {
            ContactPoint_World = hit.point;
            ForwardSlipDebug = hit.forwardSlip;
            SidewaysSlipDebug = hit.sidewaysSlip;

            // --- 3. ���� �׷� ��� ---
            NormalForceDebug_World = hit.normal * hit.force;

            // --- 4. ������ ��� ---
            float mu_eff_fwd = CalculateEffectiveMu(hit.forwardSlip, wheelCollider.forwardFriction);
            float mu_eff_side = CalculateEffectiveMu(hit.sidewaysSlip, wheelCollider.sidewaysFriction);

            float Fx_mag = mu_eff_fwd * hit.force;
            float Fy_mag = mu_eff_side * hit.force;

            Vector3 Fx_friction_world = Vector3.zero;
            if (Mathf.Abs(hit.forwardSlip) > 1e-4f)
            {
                Fx_friction_world = -hit.forwardDir * Fx_mag * Mathf.Sign(hit.forwardSlip);
            }

            Vector3 Fy_friction_world = Vector3.zero;
            if (Mathf.Abs(hit.sidewaysSlip) > 1e-4f)
            {
                Fy_friction_world = -hit.sidewaysDir * Fy_mag * Mathf.Sign(hit.sidewaysSlip);
            }

            FrictionForceDebug_World = Fx_friction_world + Fy_friction_world;

            // --- 5. �� ���� �ݷ� ---
            CalculatedGRF_World = NormalForceDebug_World + FrictionForceDebug_World;
        }
        else // ���鿡 ���� �ʾ��� ��
        {
            IsGrounded = false;
            CalculatedGRF_World = Vector3.zero;
            ContactPoint_World = transform.position - (wheelCollider.transform.up * wheelCollider.radius);
            NormalForceDebug_World = Vector3.zero;
            FrictionForceDebug_World = Vector3.zero;
            ForwardSlipDebug = 0;
            SidewaysSlipDebug = 0;
        }
    }

    // WheelFrictionCurve�� �ؼ��Ͽ� ��ȿ ���� ���(mu_eff)�� ��ȯ�ϴ� �Լ�
    float CalculateEffectiveMu(float slipInput, WheelFrictionCurve curve)
    {
        float absSlip = Mathf.Abs(slipInput);
        if (absSlip < curve.extremumSlip)
        {
            if (curve.extremumSlip > 1e-5f) return (absSlip / curve.extremumSlip) * curve.extremumValue;
            return curve.extremumValue;
        }
        else if (absSlip < curve.asymptoteSlip)
        {
            if (Mathf.Abs(curve.asymptoteSlip - curve.extremumSlip) > 1e-5f)
            {
                float t = (absSlip - curve.extremumSlip) / (curve.asymptoteSlip - curve.extremumSlip);
                return Mathf.Lerp(curve.extremumValue, curve.asymptoteValue, t);
            }
            else { return curve.asymptoteValue; }
        }
        else { return curve.asymptoteValue; }
    }

    // �ܺο��� RPM <-> Rad/s ��ȯ�� �ʿ��� ��츦 ���� ���� �Լ� (���� ����)
    public static float ToRPM(float radiansPerSecond) => radiansPerSecond * RAD_PER_SEC_TO_RPM;
    public static float ToRadiansPerSecond(float rpm) => rpm * RPM_TO_RAD_PER_SEC;
}