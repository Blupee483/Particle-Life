using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public class SpatialLookup : MonoBehaviour
{
    public SpatialLookupStruct[] spatialLookup;
    public int[] startIndices;

    public (int x, int y) FindCellCoord(float2 point, float cellSize)
    {
        Vector2 offset = point + new float2(cellSize/2f, cellSize/2f);
        int x = Mathf.FloorToInt(offset.x/cellSize);
        int y = Mathf.FloorToInt(offset.y/cellSize);
       return (x, y); 
    }

    public int FindCellHash(int2 cellCoord)
    {
        const int primeFactorX = 15823;
        const int primeFactorY = 9737333;
        return cellCoord.x*primeFactorX + cellCoord.y*primeFactorY;
    }

    public int FindCellKeyFromHash(int cellHash)
    {
        return Mathf.Abs(cellHash%spatialLookup.Length);
    }

//i turned off the gizmos drawing for now with the xxx
    void OnDrawGizmosxxx() //draws a circle at the mouse and the grid it would be on
    {
        const float cellSize = 2f;

        Vector3 mousePos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        mouseWorldPos.z = 0f;

        (int x, int y) cellCoord = FindCellCoord(new Vector2(mouseWorldPos.x, mouseWorldPos.y), cellSize);
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(new Vector2((float)cellCoord.x*cellSize, (float)cellCoord.y*cellSize), new Vector3(cellSize, cellSize, 0f));
        Gizmos.DrawWireSphere(mouseWorldPos, cellSize);
    }
}

public struct SpatialLookupStruct : System.IComparable<SpatialLookupStruct>
{
    public int particleIndex;
    public int cellKey;

    public int CompareTo(SpatialLookupStruct other)
    {
        return cellKey.CompareTo(other.cellKey);
    }
}
