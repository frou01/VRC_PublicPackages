
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using static VRC.SDKBase.VRCPlayerApi;

public class TestObject : UdonSharpBehaviour
{
    void Start()
    {
        
    }

    private void LateUpdate()
    {

        TrackingData trackingData = Networking.GetOwner(this.gameObject).GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);

        transform.SetPositionAndRotation(trackingData.position, trackingData.rotation);
    }
}
