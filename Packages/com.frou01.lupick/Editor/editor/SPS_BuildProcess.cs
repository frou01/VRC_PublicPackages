using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SPS_BuildProcess : IProcessSceneWithReport
{
    public int callbackOrder => 0;
    SPSManager spsManager = null;

    List<SPSCatcher> SPSCatchers = new List<SPSCatcher>();

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        SPSCatchers.Clear();
        foreach (GameObject obj in scene.GetRootGameObjects())
        {
            spsManager = obj.GetComponent<SPSManager>();
            if (spsManager != null) break;
        }
        if (spsManager != null)
        {
            foreach (GameObject obj in scene.GetRootGameObjects())
            {
                if (obj.GetComponent<SmartPickupSharpRootChangeable>() != null)
                {
                    SmartPickupSharpRootChangeable SPS = obj.GetComponent<SmartPickupSharpRootChangeable>();
                    SPS.spsManager = spsManager;
                }
                if (obj.GetComponent<SPSCatcher>() != null)
                {
                    obj.GetComponent<SPSCatcher>().ID = SPSCatchers.Count;
                    SPSCatchers.Add(obj.GetComponent<SPSCatcher>());
                }
                if(obj.transform.childCount > 0) searchChild(obj.transform);
            }

            spsManager.setCatchers(SPSCatchers.ToArray());
            //Debug.Log(SPSCatchers.ToArray().Length);
            //Debug.Log(SPSCatchers[0].name);
        }


    }

    void searchChild(Transform transform)
    {
        for (int id = 0;id< transform.childCount;id++)
        {
            GameObject obj = transform.GetChild(id).gameObject;
            if (obj.GetComponent<SmartPickupSharpRootChangeable>() != null)
            {
                SmartPickupSharpRootChangeable SPS = obj.GetComponent<SmartPickupSharpRootChangeable>();
                SPS.spsManager = spsManager;
            }
            if (obj.GetComponent<SPSCatcher>() != null)
            {
                obj.GetComponent<SPSCatcher>().ID = SPSCatchers.Count;
                SPSCatchers.Add(obj.GetComponent<SPSCatcher>());
            }
            if (obj.transform.childCount > 0) searchChild(obj.transform);
        }
    }
}
