
#if (UNITY_EDITOR) 
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class terrainDigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Terrain target;

    public CinemachineSmoothPath path;
    public float start;
    public float end;

    public float heightOffset = 0;
    //public float resolution;

    public float range;

    public void DigTerrain()
    {
        TerrainData terrainData = target.terrainData;
        float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        int pickSquarex = 1 + (int)(range / terrainData.size.x * (float)(heights.GetLength(0)-1));
        int pickSquarez = 1 + (int)(range / terrainData.size.z * (float)(heights.GetLength(1)-1));

        Debug.Log(pickSquarex);
        Debug.Log(pickSquarez);
        //int braker = 0;
        for (float step = start; step <= end; step += range/2)
        {
            //onTerrainPos <- floor by terrain HeightMapSquare;
            Vector3 onTerrainPos = path.EvaluatePositionAtUnit(step, CinemachinePathBase.PositionUnits.Distance);
            onTerrainPos -= target.transform.position;
            int onPathX = (int)Mathf.Floor(onTerrainPos.x / terrainData.size.x * (float)(heights.GetLength(0)-1));
            int onPathZ = (int)Mathf.Floor(onTerrainPos.z / terrainData.size.z * (float)(heights.GetLength(1)-1));
            Debug.Log(step);
            EditorUtility.DisplayProgressBar("TerrainDigger", "Digging...", (step - start) / (end - start));
            //braker++;
            for (int xID = onPathX - pickSquarex; (xID <= onPathX + pickSquarex && xID < heights.GetLength(0)); xID++)
            {
                for (int zID = onPathZ - pickSquarez; (zID <= onPathZ + pickSquarex && zID < heights.GetLength(1)); zID++)
                {
                    //braker++;
                    if (xID < 0) xID = 0;
                    if (zID < 0) zID = 0;
                    Vector3 targetPos = new Vector3(
                    target.transform.position.x +
                        xID / (float)(heights.GetLength(0)-1) * terrainData.size.x,
                    0,
                    target.transform.position.z +
                        zID / (float)(heights.GetLength(1)-1) * terrainData.size.z);
                    float closestUnit = path.FindClosestPoint(targetPos, 0, -1, 10);
                    if (path.FromPathNativeUnits(closestUnit, CinemachinePathBase.PositionUnits.Distance) < start
                        ||
                       path.FromPathNativeUnits(closestUnit, CinemachinePathBase.PositionUnits.Distance) > end) continue;
                    Vector3 closestPos = path.EvaluatePosition(closestUnit);
                    if ((targetPos - (closestPos - new Vector3(0, closestPos.y, 0))).sqrMagnitude > range * range) continue;

                    Debug.Log(xID + " , " + zID);
                    heights[zID, xID] = (heightOffset + closestPos.y - target.transform.position.y) / terrainData.size.y;
                    //if (braker > 10000) break;
                }
                //if (braker > 10000) break;
            }
            //if (braker > 10000) break;
            //Pick hight controllPoint & find shortest point & align.
        }
        //for (int x = 0; x < heights.GetLength(1); x++)for (int z = 0; z < heights.GetLength(0); z++)
        //    {
        //        if(x%10 == 0 && z%10 == 0)EditorUtility.DisplayProgressBar("TerrainDigger", "Digging...", (x * terrainData.heightmapResolution + z) / (float)(heights.GetLength(0) * heights.GetLength(1)));
        //        Vector3 targetPos = new Vector3(
        //            target.transform.position.x +
        //                x / (float)heights.GetLength(0) * terrainData.size.x,
        //            0,
        //            target.transform.position.z +
        //                z / (float)heights.GetLength(1) * terrainData.size.z);
        //        float closestUnit = path.FindClosestPoint(targetPos, 0, -1, 10);
        //        if (path.FromPathNativeUnits(closestUnit, CinemachinePathBase.PositionUnits.Distance) < start
        //            ||
        //           path.FromPathNativeUnits(closestUnit, CinemachinePathBase.PositionUnits.Distance) > end) continue;
        //        Vector3 closestPos = path.EvaluatePosition(closestUnit);
        //        if ((targetPos - (closestPos - new Vector3(0, closestPos.y, 0))).sqrMagnitude > range * range) continue;
        //
        //        heights[z,x] = (heightOffset + closestPos.y - target.transform.position.y)/ terrainData.size.y;
        //    }
        terrainData.SetHeights(0, 0, heights);
        target.terrainData = terrainData;
        EditorUtility.ClearProgressBar();

    }
}

#endif