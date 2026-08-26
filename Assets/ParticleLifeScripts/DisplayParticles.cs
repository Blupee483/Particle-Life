using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayParticles : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSettings settings;
    [SerializeField] private ParticleData data;
    [SerializeField] private TypeInfluencesData typeData;
    [Header("Mesh and Material")]
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;

    private int[] typeIndices;
    private Matrix4x4[][] matrices;
    private RenderParams[] cachedRenderParams;
    private MaterialPropertyBlock propBlock;

    private bool startDisplay = false;

    void Start()
    {
        //initialize settings script
        settings = gameObject.GetComponent<ParticleSettings>();

        //initialize the render matrices
        matrices = new Matrix4x4[typeData.numParticleTypes][];
        for (int i = 0; i < typeData.numParticleTypes; i++)
        {
            matrices[i] = new Matrix4x4[settings.particleTypeAmounts[i]];
        }

        //cache rps and colors
        cachedRenderParams = new RenderParams[typeData.numParticleTypes];
        for(int i = 0; i < typeData.numParticleTypes; i++)
        {
            float t = typeData.numParticleTypes > 1 ? (float)i / (typeData.numParticleTypes - 1) : 0f;
            float hue = t * 0.85f;
            Color currentColor = Color.HSVToRGB(hue, 1f, 1f);

            // We use a unique property block or handle color per mesh batch
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_Color", currentColor);
            
            cachedRenderParams[i] = new RenderParams(material)
            {
                matProps = block
            };
        }

        //initialize type indices
        typeIndices = new int[typeData.numParticleTypes];

        //start the display's update loop
        startDisplay = true;
    }

    void LateUpdate()
    {
        if(!startDisplay) return;

        // Reset our reused index tracker without re-allocating memory
        System.Array.Clear(typeIndices, 0, typeIndices.Length);
        
        Vector3 scale = new Vector3(settings.particleScale, settings.particleScale, 1f);
        Quaternion identityRot = Quaternion.identity;

        // 1. Build the TRS matrices from your native/managed arrays
        for(int i = 0; i < data.particles.Length; i++)
        {
            int type = data.particles[i].type - 1;
            int index = typeIndices[type];

            // Guard against exceeding the array limits if matrices size is fixed
            if (index >= matrices[type].Length) continue; 

            matrices[type][index].SetTRS((Vector2)data.particles[i].position, identityRot, scale);
            typeIndices[type]++;
        }

        // 2. Render each type in batches of 1023
        for(int i = 0; i < typeData.numParticleTypes; i++)
        {
            int totalParticlesOfType = typeIndices[i];
            int remaining = totalParticlesOfType;
            int offset = 0;

            // Loop to split into chunks of 1023 to satisfy GPU constraints
            while (remaining > 0)
            {
                int countToRender = Mathf.Min(remaining, 1023);
                
                // Render a subset chunk of your matrices array
                Graphics.RenderMeshInstanced(
                    cachedRenderParams[i], 
                    mesh, 
                    0, 
                    matrices[i], 
                    countToRender, 
                    offset
                );

                offset += countToRender;
                remaining -= countToRender;
            }
        }
    }
}
