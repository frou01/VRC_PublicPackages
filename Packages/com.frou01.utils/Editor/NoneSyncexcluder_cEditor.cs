using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(NoneSyncexcluder), true)]

public class NoneSyncexcluder_cEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        NoneSyncexcluder Target = (NoneSyncexcluder)target;
        if (GUILayout.Button("Detach"))
        {
            Debug.Log("Detach");
            Target.Detach();
        }
    }
}
