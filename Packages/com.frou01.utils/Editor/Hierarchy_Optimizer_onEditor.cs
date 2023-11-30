
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hierarchy_Optimizer_onEditor : IProcessSceneWithReport
{
    public int callbackOrder => 5;

    public List<Hierarchy_Optimizer> target = new List<Hierarchy_Optimizer>();


    public void OnProcessScene(Scene scene, BuildReport report)
    {
        foreach (GameObject obj in scene.GetRootGameObjects())
        {
        //    //Debug.Log("OK!");
            Proceed(obj.transform);
        }
        foreach (Hierarchy_Optimizer obj in target)
        {
            if(obj != null && (obj.gameObject.activeInHierarchy || obj.forceProceed))
            {
        //        Debug.Log("MoveToRoot" + obj.name);
                obj.transform.parent = obj.target;
            }
        }
    }

    void Proceed(Transform parent)
    {
        if (parent.gameObject.GetComponent<Hierarchy_Optimizer>() != null)
        {
            target.Add(parent.gameObject.GetComponent<Hierarchy_Optimizer>());
        }
        foreach (Transform obj in parent)
        {
            Proceed(obj);
        }
    }
}
