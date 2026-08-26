using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class ParticleData : MonoBehaviour
{
    public Particle[] particles;
}

[System.Serializable]
public struct Particle
{
    public float2 position;
    public float2 velocity;
    public int type;
    public int cellKey;
}
