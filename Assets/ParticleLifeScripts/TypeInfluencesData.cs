using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeInfluencesData : MonoBehaviour
{
    public int numParticleTypes = 2;
    
    [HideInInspector] public float[] typeInfluences;
    public float RetrieveTypeInfluence(int myCol, int otherCol)
    {
        return typeInfluences[(myCol - 1)*numParticleTypes + otherCol - 1];
    }
}
