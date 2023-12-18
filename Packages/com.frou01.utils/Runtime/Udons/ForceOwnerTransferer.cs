
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ForceOwnerTransferer : UdonSharpBehaviour
{
    GameObject targetObject;
    void Start()
    {
        
    }

    public override void Interact()
    {
        Networking.SetOwner(Networking.LocalPlayer, targetObject);
    }
}
