

#if (UNITY_EDITOR) 
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(terrainPlacer))]
public class terrainPlacerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        terrainPlacer TP = (terrainPlacer)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Place"))
        {
            TP.PlaceObject();
        }
    }
}
#endif
