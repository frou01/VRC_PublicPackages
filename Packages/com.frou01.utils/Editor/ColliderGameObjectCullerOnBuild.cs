using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ColliderGameObjectCullerOnBuild : IProcessSceneWithReport
{
    public int callbackOrder => 0;


    public List<ColliderGameObjectCuller> target = new List<ColliderGameObjectCuller>();
    public List<ColliderOcclusionPortal> target2 = new List<ColliderOcclusionPortal>();


    public void OnProcessScene(Scene scene, BuildReport report)
    {
        foreach (GameObject obj in scene.GetRootGameObjects())
        {
            Proceed(obj.transform);
        }
        foreach (ColliderGameObjectCuller obj in target)
        {
            if (obj != null && obj.gameObject.activeInHierarchy)
            {
                Debug.Log("SetUp" + obj.name);
                foreach (GameObject go in obj.objects)
                {
                    go.SetActive(false);
                }
                if(obj.isStaticMode) StaticBatchingUtility.Combine(obj.objects,null);
            }
        }
        foreach (ColliderOcclusionPortal obj in target2)
        {
            if (obj != null && obj.gameObject.activeInHierarchy)
            {
                Debug.Log("SetUp" + obj.name);
                obj.GetComponent<OcclusionPortal>().open = false;
            }
        }
    }

    void Proceed(Transform parent)
    {
        if (parent.gameObject.GetComponent<ColliderGameObjectCuller>() != null)
        {
            target.Add(parent.gameObject.GetComponent<ColliderGameObjectCuller>());
        }
        if (parent.gameObject.GetComponent<ColliderOcclusionPortal>() != null)
        {
            target2.Add(parent.gameObject.GetComponent<ColliderOcclusionPortal>());
        }
        foreach (Transform obj in parent)
        {
            Proceed(obj);
        }
    }
}
