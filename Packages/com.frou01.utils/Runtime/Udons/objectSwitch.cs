
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class objectSwitch : UdonSharpBehaviour
{
    public GameObject targetObj;
    void Start()
    {

    }
    public override void Interact()
    {
        targetObj.SetActive(!targetObj.activeInHierarchy);
    }
}
