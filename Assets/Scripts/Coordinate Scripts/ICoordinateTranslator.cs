using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICoordinateTranslator
{
    Vector3 TranslatePositionFromFMI(double[] fmiPosition);
    Quaternion TranslateRotationFromFMI(double[] fmiRotation);
}
