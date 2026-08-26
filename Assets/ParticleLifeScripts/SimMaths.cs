using System;
using UnityEngine;

public class SimMaths : MonoBehaviour 
{
    public float CalcForce(float dist, float repelRadius, float influenceRadius, float typeInfluence)
    {
        dist /= influenceRadius;
        float force;

        if(dist < repelRadius)
        {
            force = dist/repelRadius - 1f;
        }
        else if(dist < 1)
        {
            float forceFactor = 1f - Math.Abs(2f*dist - 1 - repelRadius)/(1-repelRadius);
            force = typeInfluence * forceFactor;
        }
        else
        {
            force = 0f;
        }

        return force * influenceRadius;
    }

    public float CalcHalfLife(float halfLife)
    {
        return Mathf.Pow(0.5f, Time.deltaTime/halfLife);
    }
}