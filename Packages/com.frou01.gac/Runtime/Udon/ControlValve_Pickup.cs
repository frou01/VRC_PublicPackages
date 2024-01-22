
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace frou01.GrabController
{
    public class ControlValve_Pickup : Controller_Base
    {
        protected override void onPicked()
        {
            netWork_Updating = true;
            if (pickup.currentHand == VRC_Pickup.PickupHand.Left)
            {
                trackingData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
            }
            else
            if (pickup.currentHand == VRC_Pickup.PickupHand.Right)
            {
                trackingData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
            }
            else
            {
                return;
            }
            localHandRotation = Quaternion.Inverse(ControllerRoot.rotation) * trackingData.rotation;
            if (onPick)
            {
                localHandRotation_OnPick = localHandRotation;
                position_OnPick = controllerPosition;
                onPick = false;
            }
            positionSet(localHandRotation_OnPick, localHandRotation);
        }
        private void positionSet(Quaternion a, Quaternion b)
        {
            controllerPosition = wrapAngleTo180(position_OnPick + (b.eulerAngles.y - a.eulerAngles.y));
        }
        protected override void ApplyToTransform()
        {
            controllerPosition = wrapAngleTo180(controllerPosition);
            controllerTransform.localRotation = Quaternion.identity;
            controllerTransform.Rotate(0, controllerPosition, 0);
        }
    }
}
