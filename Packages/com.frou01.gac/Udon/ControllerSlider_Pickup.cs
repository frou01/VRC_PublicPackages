
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace frou01.GrabController
{
    public class ControllerSlider_Pickup : Controller_Base
    {
        protected override void onPicked()
        {
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
            localHandPosition = ControllerRoot.InverseTransformPoint(trackingData.position);

            if (onPick)
            {
                Debug.Log(pickup.currentHand);
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
                localHandPosition_OnPick = localHandPosition;
                position_OnPick = controllerPosition;
                onPick = false;
            }
            positionSet(localHandPosition_OnPick, localHandPosition);
        }
        private void positionSet(Vector3 a, Vector3 b)
        {
            controllerPosition = position_OnPick - a.y + b.y;
        }
        protected override void ApplyToTransform()
        {
            controllerTransform.Translate(0, controllerPosition - controllerTransform.localPosition.y , 0);
        }
    }
}
