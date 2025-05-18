
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;

internal class walkableVehicleFloorBuildProcess : IProcessSceneWithReport , IVRCSDKBuildRequestedCallback
{
    public int callbackOrder => 0;

    //public int PreprocessOrder => 0;


    public VehicleInSideSeatMNG VISM;
    public List<CatchCollider_Vehicle> target_CatchCollider_Vehicle = new List<CatchCollider_Vehicle>();

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        target_CatchCollider_Vehicle = new List<CatchCollider_Vehicle>();
        foreach (GameObject obj in scene.GetRootGameObjects())
        {
            Proceed_Search_VehicleInSideSeatMNG(obj.transform);
        }
        Debug.Log("Processing");
        if (VISM == null) return;
        VISM.transform.localPosition = Vector3.zero;
        VISM.preset_CatchColliders = target_CatchCollider_Vehicle.ToArray();
        GameObject[] inVehicleCollider = new GameObject[target_CatchCollider_Vehicle.Count];
        int index = 0;
        foreach (CatchCollider_Vehicle CCV in target_CatchCollider_Vehicle)
        {
            if (CCV == null)
            {
                Debug.Log("Destroyed CatchCollider_Vehicle ");
                continue;
            }
            inVehicleCollider[index] = CCV.inVehicleCollider;
            if (inVehicleCollider[index] == null)
            {
                Debug.Log("Destroyed inVehicleCollider ");
                continue;
            }
            inVehicleCollider[index].transform.parent = VISM.transform.parent;
            inVehicleCollider[index].transform.localPosition = Vector3.zero;
            inVehicleCollider[index].transform.localRotation = Quaternion.identity;
            inVehicleCollider[index].SetActive(false);
            index++;
        }
        VISM.preset_inVehicleCollider = inVehicleCollider;
        Debug.Log("Done");
    }

    void Proceed_Search_VehicleInSideSeatMNG(Transform parent)
    {
        if(parent.gameObject != null)
        {
            if (parent.gameObject.GetComponent<CatchCollider_Vehicle>() != null)
            {
                target_CatchCollider_Vehicle.Add(parent.gameObject.GetComponent<CatchCollider_Vehicle>());
            }
            if (parent.gameObject.GetComponent<VehicleInSideSeatMNG>() != null)
            {
                VISM = parent.gameObject.GetComponent<VehicleInSideSeatMNG>();
            }
        }
        foreach (Transform obj in parent)
        {
            Proceed_Search_VehicleInSideSeatMNG(obj);
        }
    }
    public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
    {
        return true;
    }
}
#endif
