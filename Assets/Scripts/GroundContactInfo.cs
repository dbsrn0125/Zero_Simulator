using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GroundContactInfo
{
    public bool IsContacting;
    public float PenetrationDepth;
    public Vector3 WorldNormal;
    public float FrictionCoefficient;


    public GroundContactInfo(bool isContacting, float depth, Vector3 normal, float friction)
    {
        IsContacting = isContacting;
        PenetrationDepth = depth;
        WorldNormal = normal;
        FrictionCoefficient = friction;

    }

    public static GroundContactInfo NonContact()
    {
        return new GroundContactInfo(false, 0f, Vector3.up, 0.2f);
    }
}
