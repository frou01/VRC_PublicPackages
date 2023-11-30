
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using static VRC.SDKBase.VRCPlayerApi;

public class VRRootChaser : UdonSharpBehaviour
{
    VRCPlayerApi playerApi;
    [System.NonSerialized] public bool PlayerPositionScriptControlMode;
    void Start()
    {
        playerApi = Networking.LocalPlayer;
    }

    TrackingData trackingData;

    public void LateUpdate()
    {
        if (!PlayerPositionScriptControlMode)
        {
            if (playerApi != null && !playerApi.IsValid()) playerApi = null;
        }
    }
    public override void PostLateUpdate()
    {
        if (playerApi != null)
        {
            if (!PlayerPositionScriptControlMode)
            {
                trackingData = playerApi.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin);
                gameObject.transform.rotation = trackingData.rotation;
                gameObject.transform.position = trackingData.position;
            }
        }

    }
}
