
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DynamicBatching_onEditor : IProcessSceneWithReport
{
    public int callbackOrder => 1;

    public List<Transform> target = new List<Transform>();


    public void OnProcessScene(Scene scene, BuildReport report)
    {
        foreach (GameObject obj in scene.GetRootGameObjects())
        {
            Proceed(obj.transform);
        }
        foreach (Transform obj in target)
        {
            if (obj != null)
            {
                GameObject[] gos = obj.GetComponent<DynamicBatching>().batchingObjects;
                Debug.Log("Bathcing!" + GetHierarchyPath(obj.gameObject));
                StaticBatchingUtility.Combine(gos, obj.gameObject);
            }
        }
    }

    void Proceed(Transform parent)
    {
        if (parent.gameObject.GetComponent<DynamicBatching>() != null)
        {
            target.Add(parent);
        }
        foreach (Transform obj in parent)
        {
            Proceed(obj);
        }
    }
    private static string GetHierarchyPath(GameObject targetObj)
    {
        List<GameObject> objPath = new List<GameObject>();
        objPath.Add(targetObj);
        for (int i = 0; objPath[i].transform.parent != null; i++)
            objPath.Add(objPath[i].transform.parent.gameObject);
        string path = objPath[objPath.Count - 1].gameObject.name;
        for (int i = objPath.Count - 2; i >= 0; i--)
            path += "/" + objPath[i].gameObject.name;

        return path;
    }
}
