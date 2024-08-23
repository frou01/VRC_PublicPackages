
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ForceOwnerTransferer : UdonSharpBehaviour
{
    [SerializeField]GameObject targetObject;
    [SerializeField] GameObject[] targetObjects;
    void Start()
    {
        
    }

    public override void Interact()
    {
        Networking.SetOwner(Networking.LocalPlayer, targetObject);
        foreach(GameObject go in targetObjects)
        {
            Networking.SetOwner(Networking.LocalPlayer, go);
        }
    }
}
