
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class LUP_RC_CatcherCollider : UdonSharpBehaviour
{
    public int ID;
    public bool isHook;//Drop on entering
    public bool isSyncOwner;
    public Transform dropTarget;
    void Start()
    {
        
    }
}
