using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateTranslator : MonoBehaviour, ICoordinateTranslator
{
    public enum SourceAxis {X,Y,Z}
    public enum SourceDirection {Plus = 1, Minus = -1 }
    [Header("Position Mapping")]
    public SourceAxis unityX_From = SourceAxis.X;
    public SourceDirection unityX_Direction = SourceDirection.Plus;

    public SourceAxis unityY_From = SourceAxis.Y;
    public SourceDirection unityY_Direction = SourceDirection.Plus;
    
    public SourceAxis unityZ_From = SourceAxis.Z;
    public SourceDirection unityZ_Direction = SourceDirection.Plus;

    [Header("Rotation Mapping (Quaternion)")]
    public Vector3 rotationFixEuler = Vector3.zero;

    public Quaternion fixRotation;

    void Awake()
    {
        fixRotation = Quaternion.Euler(rotationFixEuler);
    }

    public Vector3 TranslatePositionFromFMI(double[] fmiPosition)
    {
        return new Vector3(
            (float)fmiPosition[(int)unityX_From],
            (float)fmiPosition[(int)unityY_From],
            (float)fmiPosition[(int)unityZ_From]
            //(float)fmiPosition[(int)unityX_From] * (int)unityX_Direction,
            //(float)fmiPosition[(int)unityY_From] * (int)unityY_Direction,
            //(float)fmiPosition[(int)unityZ_From] * (int)unityZ_Direction
        );
    }

    public Quaternion TranslateRotationFromFMI(double[] fmiRotation)
    {
        Quaternion sim_q = new Quaternion(
            (float)fmiRotation[1], (float)fmiRotation[2], (float)fmiRotation[3], (float)fmiRotation[0]
        );
        return sim_q*fixRotation;
    }
}
