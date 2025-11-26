using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelGroundSensor : MonoBehaviour
{
    [Header("1. Object References")]
    public Transform chassisTransform;

    [Header("2. Wheel Parameters")]
    public float wheelRadius = 0.35f;

    [Header("3. Raycast settings")]
    public float raycastDistance = 0.5f;
    public LayerMask groundLayer;

    [Header("4. Physics Parameters (Suspension")]
    public float springStiffness = 950000.0f;
    public float springDamper = 9750.0f;

    [SerializeField]
    public float VerticalForce { get; private set; } = 0.0f;
    [SerializeField]
    public bool IsGrounded { get; private set; } = false;
    [SerializeField]
    public float Penetration { get; private set; } = 0.0f;
    private float lastPenetration = 0.0f;

    public void CalculateGroundForces()
    {
        if(chassisTransform == null)
        {
            Debug.LogError("Chassis Transform missing", this.gameObject);
            IsGrounded = false;
            VerticalForce = 0.0f;
            Penetration = 0.0f;
            return;
        }


        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
        {
            Penetration = wheelRadius - hit.distance;

            //    if (Penetration > 0)
            //    {
            //        IsGrounded = true;

            //        //float springForce = Penetration * springStiffness;

            //        //float penetrationVelocity = (Penetration - lastPenetration) / Time.fixedDeltaTime;
            //        //lastPenetration = Penetration;
            //        //float damperForce = penetrationVelocity * springDamper;

            //        //VerticalForce = springForce + damperForce;

            //        //if (VerticalForce < 0)
            //        //{
            //        //    VerticalForce = 0.0f;
            //        //}
            //    }
            //    else
            //    {
            //        IsGrounded = false;
            //        VerticalForce = 0.0f;
            //        Penetration = wheelRadius - raycastDistance; ;
            //        lastPenetration = 0.0f;
            //    }
            //}
            //else
            //{
            //    IsGrounded = false;
            //    VerticalForce = 0.0f;
            //    Penetration = wheelRadius - raycastDistance;
            //    lastPenetration = 0.0f;
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color=IsGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position+Vector3.down*raycastDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position,wheelRadius);
    }
#endif
}
