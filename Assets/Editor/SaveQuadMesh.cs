using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SaveQuadMesh
{
    [MenuItem("Tools/Save Default Quad Mesh")]
    public static void SaveQuad()
    {
        // Create a temporary primitive GameObject
        GameObject tempQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Mesh sharedMesh = tempQuad.GetComponent<MeshFilter>().sharedMesh;

        // Instantiate a copy so it isn't linked to the scene instance
        Mesh meshToSave = Object.Instantiate(sharedMesh);

        // Save it to your Assets folder
        AssetDatabase.CreateAsset(meshToSave, "Assets/DefaultQuad.asset");
        AssetDatabase.SaveAssets();

        // Clean up the temporary object
        Object.DestroyImmediate(tempQuad);

        Debug.Log("Quad mesh successfully saved to Assets/DefaultQuad.asset!");
    }
}
