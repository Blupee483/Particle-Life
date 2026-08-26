using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

public class ParticleSettings : MonoBehaviour
{
    [Header("Initialize Particles Settings")]
    public int numParticles;
    public float particleScale;
    public float particleSpacing;
    [Header("Physics Settings")]
    public float gravity;
    public float damping;
    public float2 bounds;
    public float repelRadius = 0.3f;
    public float influenceRadius = 1f;
    public float velocityHalfLife = 0.04f;
    public float forceScale = 10f;
    public float fixedDT = 1f/240f;
    [HideInInspector] public int[] particleTypeAmounts;

    [Header("References")]
    [SerializeField] private TypeInfluencesData typeData;


    public Particle[] InitParticles()
    {
        particleTypeAmounts = new int[typeData.numParticleTypes];
        Particle[] particles = new Particle[numParticles];
        int particlesPerRow = (int)Mathf.Sqrt(numParticles);
        int particlesPerColumn = (int)(numParticles-1)/particlesPerRow+1;
        float spacing = particleScale * 2 + particleSpacing;

        for(int i = 0; i < numParticles; i++)
        {
            particles[i] = new Particle();
            Particle particle = particles[i];
            particle.type = UnityEngine.Random.Range(1, typeData.numParticleTypes+1);
            particleTypeAmounts[particle.type-1] += 1;

            float x = (i % particlesPerRow - particlesPerRow / 2f + 0.5f) * spacing;
            float y = (i / particlesPerRow - particlesPerColumn / 2f + 0.5f) * spacing;
            particle.position = new Vector2(x, y);

            particles[i] = particle;
        }
        return particles;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector2.zero, new Vector3(bounds.x, bounds.y, 0f) * 2f);
    }
}
