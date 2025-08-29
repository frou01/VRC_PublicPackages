
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class SPSResetter : UdonSharpBehaviour
{
    [SerializeField] LUPickUpBase_LateUpdatePickUpBase target;
    [SerializeField] LUPickUpBase_LateUpdatePickUpBase[] targets;
    void Start()
    {
        
    }
    public override void Interact() {
        this.Perform();
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Perform));
    }
    public void Perform()
    {
        //target.gameObject.SetActive(true);
        if(target != null)
        {
            if(Networking.IsOwner(target.gameObject)) target.ResetPosition();
        }

        foreach (LUPickUpBase_LateUpdatePickUpBase anTarget in targets)
        {
            if (Networking.IsOwner(anTarget.gameObject)) anTarget.ResetPosition();
        }
    }
}
