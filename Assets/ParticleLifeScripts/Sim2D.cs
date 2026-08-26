//Lots of credit to Sebastion Lague's fluid sim video
//some features such as the spatial lookup system was derived from his video
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

public class Sim2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleData data;
    [SerializeField] private TypeInfluencesData typeData;
    [SerializeField] private ParticleSettings settings;
    [SerializeField] private SimMaths simMath;
    [SerializeField] private SpatialLookup lookup;

    [Header("Debugging and Stats")]
    public int loopsPerFrame;

    //This script's variables
    public int particleCount = 600;
    private float radius;
    private float fixedDT;
    //This Script's arrays
    NativeArray<Particle> particles;
    NativeArray<SpatialLookupStruct> spatialLookup;
    NativeArray<int> startIndices;
    NativeArray<float> typeInfluences;

    //runs at the very start
    void OnEnable()
    {
        //Initialize common variables
        radius = settings.influenceRadius;
        fixedDT = settings.fixedDT;
        particleCount = settings.numParticles;

        //Set native arrays to be persistent
        spatialLookup = new NativeArray<SpatialLookupStruct>(particleCount, Allocator.Persistent);
        startIndices = new NativeArray<int>(particleCount, Allocator.Persistent);
        typeInfluences = new NativeArray<float>(typeData.typeInfluences, Allocator.Persistent);

        //Initialize particle positions
        particles = new NativeArray<Particle>(settings.InitParticles(), Allocator.Persistent);
        data.particles = new Particle[particleCount];
        particles.CopyTo(data.particles);
    }
    void OnDisable()
    {
        //manually frees memory to prevent memory leaks
        if(particles.IsCreated) particles.Dispose();
        if(spatialLookup.IsCreated) spatialLookup.Dispose();
        if(startIndices.IsCreated) startIndices.Dispose();
        if(typeInfluences.IsCreated) typeInfluences.Dispose();
    }

    //spatial lookup functions and maths
    int FindCellHash(int2 cellCoord)
    {
        const int primeFactorX = 15823;
        const int primeFactorY = 9737333;
        return cellCoord.x*primeFactorX + cellCoord.y*primeFactorY;
    }

    int FindCellKeyFromHash(int cellHash)
    {
        return Mathf.Abs(cellHash%spatialLookup.Length);
    }

    int2 FindCellCoord(float2 point, float cellSize)
    {
        Vector2 offset = point + new float2(cellSize/2f, cellSize/2f);
        int x = Mathf.FloorToInt(offset.x/cellSize);
        int y = Mathf.FloorToInt(offset.y/cellSize);
       return new int2(x, y); 
    }

    //main loop
    void Update()
    {
        //Set common variables
        radius = settings.influenceRadius;
        fixedDT = settings.fixedDT;
        typeInfluences.CopyFrom(typeData.typeInfluences);

        //Update cell keys and sort
        for(int i = 0; i < particles.Length; i++)
        {
            Particle p = particles[i];
            int2 cellCoord = FindCellCoord(p.position, radius);
            int thisCellKey = FindCellKeyFromHash(FindCellHash(cellCoord));
            p.cellKey = thisCellKey;
            particles[i] = p;

            spatialLookup[i] = new SpatialLookupStruct{ particleIndex = i, cellKey = thisCellKey };
        }

        //Native sorter
        spatialLookup.Sort();

        //restart start indices
        for(int i = 0; i < startIndices.Length; i++) startIndices[i] = int.MaxValue;

        //rebuild start indices
        for(int i = 0; i < particles.Length; i++)
        {
            int key = spatialLookup[i].cellKey;
            int keyPrev = (i == 0) ? int.MaxValue : spatialLookup[i - 1].cellKey;
            if (key != keyPrev)
            {
                startIndices[key] = i;
            }
        }


        //Setup and schedule the job
        UpdateMovementAndForcesJob forcesJob = new UpdateMovementAndForcesJob
        {
            particles = particles,
            spatialLookup = spatialLookup,
            startIndices = startIndices,
            typeInfluences = typeInfluences,
            radius = radius,
            repelRadius = settings.repelRadius,
            fixedDT = fixedDT,
            forceScale = settings.forceScale,
            gravity = settings.gravity,
            bounds = settings.bounds,
            damping = settings.damping,
            velocityHalfLife = settings.velocityHalfLife,
            numParticleTypes = typeData.numParticleTypes
        };

        JobHandle jobHandle = forcesJob.Schedule(particleCount, 64);

        jobHandle.Complete();

        particles.CopyTo(data.particles);
    }

    public struct SpatialLookupComparer : System.Collections.Generic.IComparer<SpatialLookupStruct>
    {
        public int Compare(SpatialLookupStruct x, SpatialLookupStruct y)
        {
            return x.cellKey.CompareTo(y.cellKey);
        }
    }
}




[BurstCompile]
public struct UpdateMovementAndForcesJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction] public NativeArray<Particle> particles;
    [ReadOnly] public NativeArray<SpatialLookupStruct> spatialLookup;
    [ReadOnly] public NativeArray<int> startIndices;
    [ReadOnly] public NativeArray<float> typeInfluences;

    public float radius;
    public float repelRadius;
    public float fixedDT;
    public float forceScale;
    public float gravity;
    public float2 bounds;
    public float damping;
    public float velocityHalfLife;
    public int numParticleTypes;

    int2 FindCellCoord(float2 point, float cellSize)
    {
        Vector2 offset = point + new float2(cellSize/2f, cellSize/2f);
        int x = Mathf.FloorToInt(offset.x/cellSize);
        int y = Mathf.FloorToInt(offset.y/cellSize);
       return new int2(x, y); 
    }
    int FindCellHash(int x, int y)
    {
        const int primeFactorX = 15823;
        const int primeFactorY = 9737333;
        return x*primeFactorX + y*primeFactorY;
    }
    int FindCellKeyFromHash(int cellHash)
    {
        return Mathf.Abs(cellHash%spatialLookup.Length);
    }
    float RetrieveTypeInfluence(int myCol, int otherCol)
    {
        return typeInfluences[(myCol - 1)*numParticleTypes + otherCol - 1];
    }
    float CalcForce(float dist, float repelRadius, float influenceRadius, float typeInfluence)
    {
        dist /= influenceRadius;
        float force;

        if(dist < repelRadius)
        {
            force = dist/repelRadius - 1f;
        }
        else if(dist < 1)
        {
            float forceFactor = 1f - Mathf.Abs(2f*dist - 1 - repelRadius)/(1-repelRadius);
            force = typeInfluence * forceFactor;
        }
        else
        {
            force = 0f;
        }

        return force * influenceRadius;
    }
    float CalcHalfLife(float halfLife)
    {
        return Mathf.Pow(0.5f, fixedDT/halfLife);
    }

    public void Execute(int index)
    {
        //initialize this particle
        Particle myParticle = particles[index];
        float2 myPos = myParticle.position;
        float2 force = float2.zero;

        int2 centre = FindCellCoord(myPos, radius);

        //apply gravity
        myParticle.velocity.y += gravity * fixedDT;
        
        //loops the 3x3 square of grid around the particle
        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                //find the cell key
                int key = FindCellKeyFromHash(FindCellHash(centre.x+offsetX, centre.y+offsetY));

                int cellStartIndex = startIndices[key];
                if (cellStartIndex == int.MaxValue) continue;

                //loop over all particles inside the cell
                for(int i = cellStartIndex; i < spatialLookup.Length; i++)
                {
                    SpatialLookupStruct lookupItem = spatialLookup[i];

                    if(lookupItem.cellKey != key) break;

                    int otherParticleIndex = lookupItem.particleIndex;
                    Particle otherParticle = particles[otherParticleIndex];

                    //find the sqr distance between the two particles
                    float2 delta = otherParticle.position - myPos;
                    float sqrDist = delta.x*delta.x+delta.y*delta.y;
                    if(sqrDist == 0f) continue;

                    //calc the force if the particle is inside the radius
                    if(sqrDist <= radius*radius)
                    {
                        float dist = Mathf.Sqrt(sqrDist);
                        float typeInfluence = RetrieveTypeInfluence(myParticle.type, otherParticle.type);
                        float2 dir = delta / dist;

                        force += dir * CalcForce(dist, repelRadius, radius, typeInfluence);
                    }
                }
            }
        }

        //applies a scale to the force
        force *= forceScale;
        //halfLife applies friction to the particle's velocity
        float halfLifeMultiplier = CalcHalfLife(velocityHalfLife);
        //apply velocity
        myParticle.velocity = halfLifeMultiplier * myParticle.velocity + force * fixedDT;

        //move the position of the particle
        myParticle.position += myParticle.velocity * fixedDT;

        //Resolve collisions
        float x = myParticle.position.x;
        float y = myParticle.position.y;
        float velX = myParticle.velocity.x;
        float velY = myParticle.velocity.y;

        if(Mathf.Abs(x) > bounds.x)
        {
            int sign = (int)(Mathf.Abs(x)/x);
            x = bounds.x * sign;
            velX *= -1f * damping;
        }

        if(Mathf.Abs(y) > bounds.y)
        {
            int sign = (int)(Mathf.Abs(y)/y);
            y = bounds.y * sign;
            velY *= -1f * damping;
        }

        myParticle.position = new float2(x, y);
        myParticle.velocity = new float2(velX, velY);

        //Saves changes back into the particle array
        particles[index] = myParticle;
    }
}
