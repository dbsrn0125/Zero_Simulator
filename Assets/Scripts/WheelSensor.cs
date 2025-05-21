using UnityEngine;

// 간단한 PID 제어기 클래스
public class SimplePID
{
    public float Kp = 20f;
    public float Ki = 10f;
    public float Kd = 1f;
    public float MaxOutput = 200f; // 예: 최대 모터 토크

    private float integral = 0f;
    private float lastError = 0f;

    public float Update(float error, float deltaTime)
    {
        if (deltaTime <= 0f) return 0f;

        integral += error * deltaTime;
        // 안티 와인드업 (옵션): 적분값이 너무 커지는 것을 방지
        if (Ki > 0) // Ki가 0일 때는 integralMax가 무한대가 될 수 있으므로 방지
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
    public Transform visualWheel; // 바퀴의 시각적 모델 (위치 동기화용)

    [Header("Inputs (Set by external script from Command/ROS/FMU)")]
    public float TargetSteerAngle = 0f; // 도(Degree) 단위
    public float TargetRPM = 0f;        // 분당 회전수 (RPM)

    [Header("Control Parameters")]
    public float motorKp = 25f;         // RPM 제어용 PID - P 게인
    public float motorKi = 15f;         // RPM 제어용 PID - I 게인
    public float motorKd = 1.5f;        // RPM 제어용 PID - D 게인
    public float maxMotorTorque = 200f; // 최대 모터 토크 (Nm)
    public float maxBrakeTorque = 300f; // 최대 브레이크 토크 (Nm)
    public float rpmStopThreshold = 2.0f; // 이 RPM 이하이고 목표 RPM이 0이면 브레이크 해제 고려

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
        // 시각적 바퀴 위치 업데이트 (서스펜션 반영)
        if (visualWheel != null && wheelCollider != null)
        {
            Vector3 position;
            Quaternion rotation; // 이 rotation은 WC 스핀을 포함하므로 직접 사용 않음
            wheelCollider.GetWorldPose(out position, out rotation);
            visualWheel.position = position;
            // visualWheel.rotation은 조향각(부모로부터) + FMU의 스핀각으로 별도 처리 필요
        }

        // 디버깅 정보 업데이트
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

        // --- 1. WheelCollider에 제어 입력 적용 ---
        wheelCollider.steerAngle = TargetSteerAngle;

        // RPM 제어 로직
        float currentRPM = wheelCollider.rpm;

        if (Mathf.Abs(TargetRPM) < rpmStopThreshold) // 목표 RPM이 거의 0일 때 (정지 또는 매우 느린 속도)
        {
            wheelCollider.motorTorque = 0f; // 모터 토크 0
            rpmPidController.Reset(); // PID 상태 초기화

            if (Mathf.Abs(currentRPM) > rpmStopThreshold) // 아직 바퀴가 돌고 있다면 제동
            {
                // 현재 회전 방향과 반대로 제동 토크를 걸거나, 단순히 크기만으로 제동
                // wheelCollider.brakeTorque = Mathf.Sign(-currentRPM) * maxBrakeTorque; // 이렇게 하면 너무 강할 수 있음
                wheelCollider.brakeTorque = maxBrakeTorque;
            }
            else // 이미 거의 멈췄다면 브레이크도 해제
            {
                wheelCollider.brakeTorque = 0f;
            }
        }
        else // 목표 RPM이 주어졌을 때
        {
            wheelCollider.brakeTorque = 0f; // 주행 중이므로 브레이크 해제
            float rpmError = TargetRPM - currentRPM;
            float motorSignal = rpmPidController.Update(rpmError, Time.fixedDeltaTime);
            wheelCollider.motorTorque = motorSignal;
        }

        // --- 2. 지면 접촉 정보 가져오기 ---
        IsGrounded = wheelCollider.GetGroundHit(out WheelHit hit);

        if (IsGrounded)
        {
            ContactPoint_World = hit.point;
            ForwardSlipDebug = hit.forwardSlip;
            SidewaysSlipDebug = hit.sidewaysSlip;

            // --- 3. 수직 항력 계산 ---
            NormalForceDebug_World = hit.normal * hit.force;

            // --- 4. 마찰력 계산 ---
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

            // --- 5. 총 지면 반력 ---
            CalculatedGRF_World = NormalForceDebug_World + FrictionForceDebug_World;
        }
        else // 지면에 닿지 않았을 때
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

    // WheelFrictionCurve를 해석하여 유효 마찰 계수(mu_eff)를 반환하는 함수
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

    // 외부에서 RPM <-> Rad/s 변환이 필요할 경우를 위한 헬퍼 함수 (선택 사항)
    public static float ToRPM(float radiansPerSecond) => radiansPerSecond * RAD_PER_SEC_TO_RPM;
    public static float ToRadiansPerSecond(float rpm) => rpm * RPM_TO_RAD_PER_SEC;
}