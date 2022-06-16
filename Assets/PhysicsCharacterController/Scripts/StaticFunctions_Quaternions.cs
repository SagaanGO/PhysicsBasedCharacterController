using UnityEngine;

public static class StaticFunctions_Quaternions
{
    public static Quaternion ShortestRotation(this Quaternion a, Quaternion b)
    {
        if (Quaternion.Dot(a, b) < 0)
        {
            return a * Quaternion.Inverse(b.QuatMultiply(-1));
        }
        else return a * Quaternion.Inverse(b);
    }

    public static Quaternion QuatMultiply(this Quaternion input, float scalar)
    {
        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
    }
}
