using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase.Editor.BuildPipeline;

public class InteractActivator_HandSetter : IProcessSceneWithReport, IVRCSDKBuildRequestedCallback
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
        Scene scene = SceneManager.GetActiveScene();
        OnProcessScene(scene, null);
        return true;
    }
}