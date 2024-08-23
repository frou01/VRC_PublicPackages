

#if (UNITY_EDITOR) 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using VRC.Udon;

public class Revert_SerializedUdonProgramSource : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


    string path = "serializedProgramAsset";
    public void Revert()
    {
        GameObject[] rootObjects = 
        gameObject.scene.GetRootGameObjects();

        foreach (GameObject anrootObject in rootObjects)
        {
            Proceed(anrootObject.transform);
        }
    }

    void Proceed(Transform parent)
    {
        if (parent.gameObject.GetComponent<UdonBehaviour>() != null)
            if (PrefabUtility.IsPartOfPrefabInstance(parent.gameObject))
            {

                foreach (UdonBehaviour anUdon in parent.gameObject.GetComponents<UdonBehaviour>())
                {
                    if (PrefabUtility.IsPartOfPrefabInstance(anUdon))
                    {
                        var serializedObject = new SerializedObject(anUdon);
                        SerializedProperty anProperty = serializedObject.FindProperty(path);
                        if (anProperty != null) PrefabUtility.RevertPropertyOverride(anProperty, InteractionMode.AutomatedAction);
                    }
                }
            }
        foreach (Transform obj in parent)
        {
            Proceed(obj);
        }
    }
}


#endif