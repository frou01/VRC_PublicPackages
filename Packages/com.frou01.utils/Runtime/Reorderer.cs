
#if (UNITY_EDITOR) 
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using UnityEngine;

//Creative Commons license 0
[ExecuteInEditMode()]
public class Reorderer : MonoBehaviour , IPreprocessBuildWithReport
{

    public int index;

    public int callbackOrder => 1;

    private void Update()
    {
        UnityEditor.SceneManagement.PrefabStage stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (stage == null)
        {
            if(transform.parent != null && index < transform.parent.childCount) transform.parent.GetChild(index).SetSiblingIndex(transform.GetSiblingIndex());
            transform.SetSiblingIndex(index);
        }
        else
        {
            if (this.transform.parent != null && PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(this.transform.parent.gameObject).Equals(""))
            {
                //Debug.Log("test" + gameObject.name);
                if (transform.GetSiblingIndex() != index)
                {
                    index = transform.GetSiblingIndex();
                    PrefabUtility.RecordPrefabInstancePropertyModifications(this);
                }
            }
        }
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        transform.SetSiblingIndex(index);
    }
}

#endif