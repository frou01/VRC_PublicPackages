
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

[DefaultExecutionOrder(-20)]
internal class walkableVehicleFloorBuildProcess : IProcessSceneWithReport , IVRCSDKBuildRequestedCallback
{
    public int callbackOrder => 0;

    //public int PreprocessOrder => 0;


    public VehicleIsideSeatMNG VISM;
    public List<CatchCollider_Vehicle> target = new List<CatchCollider_Vehicle>();

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        target = new List<CatchCollider_Vehicle>();
        foreach (GameObject obj in scene.GetRootGameObjects())
        {
            Proceed(obj.transform);
        }
        Debug.Log("Processing");

        VISM.transform.localPosition = Vector3.zero;
        VISM.preset_CatchColliders = target.ToArray();
        GameObject[] inVehicleCollider = new GameObject[target.Count];
        int index = 0;
        foreach (CatchCollider_Vehicle CCV in target)
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
    }
    void Proceed(Transform parent)
    {
        if(parent.gameObject != null)
        {
            if (parent.gameObject.GetComponent<CatchCollider_Vehicle>() != null)
            {
                target.Add(parent.gameObject.GetComponent<CatchCollider_Vehicle>());
            }
            if (parent.gameObject.GetComponent<VehicleIsideSeatMNG>() != null)
            {
                VISM = parent.gameObject.GetComponent<VehicleIsideSeatMNG>();
            }
        }
        foreach (Transform obj in parent)
        {
            Proceed(obj);
        }
    }


    public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
    {
        target = new List<CatchCollider_Vehicle>();
        Debug.Log("Processing");
        Scene scene = SceneManager.GetActiveScene();
        foreach (GameObject obj in scene.GetRootGameObjects())
        {
            Proceed(obj.transform);
        }

        VISM.transform.localPosition = Vector3.zero;
        VISM.preset_CatchColliders = target.ToArray();
        Debug.Log("Processing " + VISM.preset_CatchColliders.ToString());
        GameObject[] inVehicleCollider = new GameObject[target.Count];
        int index = 0;
        foreach (CatchCollider_Vehicle CCV in target)
        {
            if (CCV == null)
            {
                Debug.Log("Destroyed CatchCollider_Vehicle ");
                return false;
            }
            inVehicleCollider[index] = CCV.inVehicleCollider;
            if (inVehicleCollider[index] == null)
            {
                Debug.Log("Destroyed inVehicleCollider ");
                return false;
            }
            //inVehicleCollider[index].transform.parent = VISM.transform.parent;
            inVehicleCollider[index].transform.localPosition = Vector3.zero;
            inVehicleCollider[index].transform.localRotation = Quaternion.identity;
            inVehicleCollider[index].SetActive(false);
            index++;
        }
        VISM.preset_inVehicleCollider = inVehicleCollider;
        return true;
    }
}
#endif
