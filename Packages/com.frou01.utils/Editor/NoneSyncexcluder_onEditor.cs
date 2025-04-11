
#if (UNITY_EDITOR) 
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Components;
using VRC.SDKBase.Editor.BuildPipeline;
using VRC.SDKBase.Network;
using VRC.Udon;
using static VRC.SDKBase.Networking;

public class NoneSyncexcluder_onEditor : IProcessSceneWithReport , IVRCSDKBuildRequestedCallback
{
    public int callbackOrder => 0;

    public VRCSceneDescriptor target;


    public void OnProcessScene(Scene scene, BuildReport report)
    {
        foreach (GameObject obj in scene.GetRootGameObjects())
        {
        //    //Debug.Log("OK!");
            Proceed(obj.transform);
        }

        List<NetworkIDPair> CheckingList = target.NetworkIDCollection;
        List<NetworkIDPair> CheckedList = new List<NetworkIDPair>();
        string udonbe = "VRC.Udon.UdonBehaviour";
        string pickup = "VRC.SDK3.Components.VRCPickup";
        bool OK = false;
        foreach (NetworkIDPair pair in CheckingList)
        {
            OK = false;
            if (pair.gameObject)
            {

                foreach (string anPath in pair.SerializedTypeNames)
                {
                    if (!anPath.Equals(udonbe) && !anPath.Equals(pickup))
                    {
                        OK = true;
                        break;
                    }
                }
                if (OK)
                {
                    pair.ID = 10 + CheckedList.Count;
                    CheckedList.Add(pair);
                    continue;
                }

                UdonBehaviour[] Udons = pair.gameObject.GetComponents<UdonBehaviour>();
                foreach (UdonBehaviour anUdon in Udons)
                {
                    if (anUdon.SyncMethod != SyncType.None)
                    {
                        Debug.Log(pair.gameObject.name);
                        OK = true;
                    }
                }
                if (OK)
                {
                    pair.ID = 10 + CheckedList.Count;
                    CheckedList.Add(pair);
                }
            }
        }
        target.NetworkIDCollection = CheckedList;

        target = null;
    }

    void Proceed(Transform parent)
    {
        if (parent.gameObject.GetComponent<VRCSceneDescriptor>() != null)
        {
            target = parent.gameObject.GetComponent<VRCSceneDescriptor>();
        }
        foreach (Transform obj in parent)
        {
            Proceed(obj);
        }
    }

    public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
    {
        return true;
    }
}

#endif