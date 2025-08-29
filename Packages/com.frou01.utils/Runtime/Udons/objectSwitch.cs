
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class objectSwitch : UdonSharpBehaviour
{
    public GameObject targetObj;
    public GameObject[] targetObjs;
    void Start()
    {

    }
    public override void Interact()
    {
        if (targetObj != null) targetObj.SetActive(!targetObj.activeSelf);
        foreach (GameObject obj in targetObjs)
        {
            if (obj != null)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }
    }
}
