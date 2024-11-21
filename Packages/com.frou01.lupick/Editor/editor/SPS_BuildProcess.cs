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
    LUP_RC_ColliderManager spsManager = null;

    List<LUP_RC_CatcherCollider> SPSCatchers = new List<LUP_RC_CatcherCollider>();

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        SPSCatchers.Clear();
        foreach (GameObject obj in scene.GetRootGameObjects())
        {
            spsManager = obj.GetComponent<LUP_RC_ColliderManager>();
            if (spsManager != null) break;
        }
        if(spsManager == null)
        {
            GameObject go = new GameObject();
            go.AddComponent<LUP_RC_ColliderManager>();
            spsManager = go.GetComponent<LUP_RC_ColliderManager>();
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
                if (obj.GetComponent<LUPickUpRC_RootChangeable>() != null)
                {
                    LUPickUpRC_RootChangeable LPRC = obj.GetComponent<LUPickUpRC_RootChangeable>();
                    LPRC.spsManager = spsManager;
                }
                if (obj.GetComponent<LUP_RC_CatcherCollider>() != null)
                {
                    obj.GetComponent<LUP_RC_CatcherCollider>().ID = SPSCatchers.Count;
                    SPSCatchers.Add(obj.GetComponent<LUP_RC_CatcherCollider>());
                }
                if(obj.transform.childCount > 0) searchChild(obj.transform);
            }

            spsManager.SPSCatchers = SPSCatchers.ToArray();
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
            if (obj.GetComponent<LUPickUpRC_RootChangeable>() != null)
            {
                LUPickUpRC_RootChangeable LPRC = obj.GetComponent<LUPickUpRC_RootChangeable>();
                LPRC.spsManager = spsManager;
            }
            if (obj.GetComponent<LUP_RC_CatcherCollider>() != null)
            {
                obj.GetComponent<LUP_RC_CatcherCollider>().ID = SPSCatchers.Count;
                SPSCatchers.Add(obj.GetComponent<LUP_RC_CatcherCollider>());
            }
            if (obj.transform.childCount > 0) searchChild(obj.transform);
        }
    }
}
