using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GroundContactInfo
{
    public bool IsContacting;
    public float PenetrationDepth;
    public Vector3 WorldNormal;
    public float FrictionCoefficient;
    public Vector3 ContactPoint_World;

    public GroundContactInfo(bool isContacting, float depth, Vector3 normal, float friction, Vector3 contactPoint)
    {
        IsContacting = isContacting;
        PenetrationDepth = depth;
        WorldNormal = normal;
        FrictionCoefficient = friction;
        ContactPoint_World = contactPoint;
    }

    public static GroundContactInfo NonContact()
    {
        return new GroundContactInfo(false, 0f, Vector3.up, 0.2f, Vector3.zero);
    }
}
