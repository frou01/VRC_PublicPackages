
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using static VRC.SDKBase.VRCPlayerApi;

public class PlayerChaser : UdonSharpBehaviour
{
    VRCPlayerApi playerApi;
    public Transform HandL;
    public Transform HandR;
    [System.NonSerialized]public bool PlayerPositionScriptControlMode;
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
            if (playerApi != null) gameObject.transform.position = playerApi.GetPosition();
        }
    }
    public override void PostLateUpdate()
    {
        if (playerApi != null)
        {
            //if (!PlayerPositionScriptControlMode)
            //{
            //    gameObject.transform.rotation = playerApi.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin).rotation;
            //}
            trackingData = playerApi.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
            HandR.position = trackingData.position;
            HandR.rotation = trackingData.rotation;

            trackingData = playerApi.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
            HandL.position = trackingData.position;
            HandL.rotation = trackingData.rotation;
        }

    }
}
