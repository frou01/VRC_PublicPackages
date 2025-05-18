
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace frou01.GrabController
{
    public class ControllerLever_Pickup : Controller_Base
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

            localHandPosition = ControllerRoot.InverseTransformPoint(trackingData.position);
            localHandPosition.y = 0;
            localHandPosition.Normalize();
            if (onPick)
            {
                localHandPosition_OnPick = localHandPosition;
                localHandRotation_OnPick = localHandRotation;
                position_OnPick = controllerPosition;
                onPick = false;
            }
            positionSet(localHandPosition_OnPick, localHandPosition);
        }
        private void positionSet(Vector3 a, Vector3 b)
        {
            controllerPosition = position_OnPick + Mathf.Atan2(a.z * b.x - a.x * b.z, a.x * b.x + a.z * b.z) * Mathf.Rad2Deg;
        }
        protected override void ApplyToTransform()
        {
            controllerPosition = wrapAngleTo180(controllerPosition);
            controllerTransform.localRotation = Quaternion.identity;
            controllerTransform.Rotate(0, controllerPosition, 0);
        }
        void OnDrawGizmos()
        {
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 1, 1);
            foreach (float segment_point in segment_points)
            {
                Vector3 temp = Quaternion.Euler(0, segment_point, 0) * (new Vector3(0, 0, 1));
                temp.Scale(controllerTransform.parent.lossyScale);
                Gizmos.DrawLine(controllerTransform.position, controllerTransform.position + controllerTransform.parent.rotation * temp);
            }
            Gizmos.color = new Color(1,0,0);
            foreach (float snap_point in snap_points)
            {
                Vector3 temp = Quaternion.Euler(0, snap_point, 0) * (new Vector3(0, 0, 1));
                temp.Scale(controllerTransform.parent.lossyScale);
                Gizmos.DrawLine(controllerTransform.position, controllerTransform.position + controllerTransform.parent.rotation * temp);
            }
        }
    }
}
