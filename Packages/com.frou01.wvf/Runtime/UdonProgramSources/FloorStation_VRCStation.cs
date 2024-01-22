
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FloorStation_VRCStation : UdonSharpBehaviour
{
    public FloorStationController preset_AttachingStation;
    void Start()
    {
        this.enabled = false;
    }
    public override void OnStationExited(VRC.SDKBase.VRCPlayerApi player)
    {
        if(player.isLocal) preset_AttachingStation.PlayerExitBounds_force();
    }

    //public void Update()
    //{
    //    Debug.Log("seated" + ((VRCStation)GetComponent(typeof(VRCStation))).seated);
    //}
}
