using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticFunctions_Quaternions : MonoBehaviour
{
    public static Quaternion ShortestRotation(Quaternion a, Quaternion b)
    {
        if (Quaternion.Dot(a, b) < 0)
        {
            return a * Quaternion.Inverse(QuatMultiply(b, -1));
        }
        else return a * Quaternion.Inverse(b);
    }

    public static Quaternion QuatMultiply(Quaternion input, float scalar)
    {
        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
    }
}
