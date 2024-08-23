using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Revert_SerializedUdonProgramSource), true)]

public class RevertSerializedUdonProgramSource_cEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Revert_SerializedUdonProgramSource Target = (Revert_SerializedUdonProgramSource)target;
        if (GUILayout.Button("Revert"))
        {
            Debug.Log("Revert");
            Target.Revert();
        }
    }
}
