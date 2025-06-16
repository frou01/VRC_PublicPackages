using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Components;
using VRC.SDKBase.Editor.BuildPipeline;
using VRC.Udon;

public class InteractActivator_HandSetter : IProcessSceneWithReport
{
    public int callbackOrder => 0;

    public PlayerChaser playerChaser;

    public List<InteractActivator> target = new List<InteractActivator>();

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        playerChaser = null;
        target = new List<InteractActivator>();
        foreach (GameObject obj in scene.GetRootGameObjects())
        {
            Proceed(obj.transform);
        }
        foreach (InteractActivator IA in target)
        {
            Debug.Log(IA);
            IA.handL = playerChaser.HandL;
            IA.handR = playerChaser.HandR;
            foreach(UdonBehaviour udon in IA.gameObject.GetComponents<UdonBehaviour>())
            {
                udon.proximity = 1000;
            }
            foreach (VRCPickup pickup in IA.gameObject.GetComponents<VRCPickup>())
            {
                pickup.proximity = 1000;
            }
        }
    }

    void Proceed(Transform parent)
    {
        if (parent.gameObject.GetComponent<PlayerChaser>() != null)
        {
            playerChaser = parent.gameObject.GetComponent<PlayerChaser>();
        }
        if (parent.gameObject.GetComponent<InteractActivator>() != null)
        {
            target.Add(parent.gameObject.GetComponent<InteractActivator>());
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