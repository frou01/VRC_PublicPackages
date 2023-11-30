
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Teleporter : UdonSharpBehaviour
{
    public Transform target;
    public Vector3 Offset = new Vector3(0, 1, 0);
    void Start()
    {
        
    }

    public override void Interact()
    {
        Networking.LocalPlayer.TeleportTo(target.position + Offset, target.rotation);
    }
}
