

#if (UNITY_EDITOR) 
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(terrainDigger))]
public class terrainDiggerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        terrainDigger TD = (terrainDigger)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Digging!"))
        {
            TD.DigTerrain();
        }
    }
}
#endif
