using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase.Editor.BuildPipeline;

public class SPS_BuildProcess : IProcessSceneWithReport
{
    public int callbackOrder => 0;
    LUP_RC_ColliderManager RCCManager = null;

    List<LUP_RC_CatcherCollider> RCCatchers = new List<LUP_RC_CatcherCollider>();

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        RCCatchers.Clear();
        RCCManager = null;
        foreach (GameObject obj in scene.GetRootGameObjects())
        {
            RCCManager = obj.GetComponent<LUP_RC_ColliderManager>();
            if (RCCManager != null) break;
        }
        if(RCCManager == null)
        {
            GameObject go = new GameObject();
            go.AddComponent<LUP_RC_ColliderManager>();
            RCCManager = go.GetComponent<LUP_RC_ColliderManager>();
        }
        if (RCCManager != null)
        {
            foreach (GameObject obj in scene.GetRootGameObjects())
            {
                if (obj.GetComponent<SmartPickupSharpRootChangeable>() != null)
                {
                    SmartPickupSharpRootChangeable SPS = obj.GetComponent<SmartPickupSharpRootChangeable>();
                    SPS.spsManager = RCCManager;
                }
                if (obj.GetComponent<LUPickUpRC_RootChangeable>() != null)
                {
                    LUPickUpRC_RootChangeable LPRC = obj.GetComponent<LUPickUpRC_RootChangeable>();
                    LPRC.RCCManager = RCCManager;
                    //Debug.Log(LPRC);
                }
                if (obj.GetComponent<LUP_RC_CatcherCollider>() != null)
                {
                    obj.GetComponent<LUP_RC_CatcherCollider>().ID = RCCatchers.Count;
                    RCCatchers.Add(obj.GetComponent<LUP_RC_CatcherCollider>());
                }
                if(obj.transform.childCount > 0) searchChild(obj.transform);
            }

            RCCManager.RCCatchers = RCCatchers.ToArray();
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
                SPS.spsManager = RCCManager;
            }
            if (obj.GetComponent<LUPickUpRC_RootChangeable>() != null)
            {
                LUPickUpRC_RootChangeable LPRC = obj.GetComponent<LUPickUpRC_RootChangeable>();
                LPRC.RCCManager = RCCManager;
            }
            if (obj.GetComponent<LUP_RC_CatcherCollider>() != null)
            {
                obj.GetComponent<LUP_RC_CatcherCollider>().ID = RCCatchers.Count;
                RCCatchers.Add(obj.GetComponent<LUP_RC_CatcherCollider>());
            }
            if (obj.transform.childCount > 0) searchChild(obj.transform);
        }
    }

    public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
    {
        return true;
    }
}
