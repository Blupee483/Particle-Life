using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TypeInfluencesData))]
public class TypeInfluencesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Reference to the target script
        TypeInfluencesData mapData = (TypeInfluencesData)target;

        // Allow changing the grid size at the top
        int newSize = EditorGUILayout.IntField("Number of Types:", mapData.numParticleTypes);
        if (newSize != mapData.numParticleTypes && newSize > 0)
        {
            mapData.numParticleTypes = newSize;
        }

        int totalCells = mapData.numParticleTypes * mapData.numParticleTypes;

        // Resize the array if it doesn't match the grid dimensions
        if (mapData.typeInfluences == null || mapData.typeInfluences.Length != totalCells)
        {
            System.Array.Resize(ref mapData.typeInfluences, totalCells);
        }

        GUILayout.Space(10);
        GUILayout.Label("Grid Values Table", EditorStyles.boldLabel);

        // Record changes for Unity's Undo system
        Undo.RecordObject(mapData, "Modify Grid");

        // Draw the square table
        for (int y = 0; y < mapData.numParticleTypes; y++)
        {
            // Start a horizontal row
            GUILayout.BeginHorizontal();
            
            for (int x = 0; x < mapData.numParticleTypes; x++)
            {
                // Calculate the 1D index from 2D coordinates
                int index = y * mapData.numParticleTypes + x;

                // Draw an Editor field for each cell (adjust width so they look like squares)
                mapData.typeInfluences[index] = EditorGUILayout.FloatField(mapData.typeInfluences[index], GUILayout.Width(40));
            }

            // End the horizontal row
            GUILayout.EndHorizontal();
        }

        // Save changes if any values were modified
        if (GUI.changed)
        {
            EditorUtility.SetDirty(mapData);
        }
    }
}
