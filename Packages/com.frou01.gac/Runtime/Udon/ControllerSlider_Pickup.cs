
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
        void OnDrawGizmos()
        {
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 1, 1);
            if(segment_points.Length > 0)
            {
                float prev_point = segment_points[0];
                foreach (float segment_point in segment_points)
                {
                    if(prev_point != segment_point)
                    {
                        Gizmos.DrawLine(controllerTransform.position + controllerTransform.parent.rotation * new Vector3(0, prev_point, 0),
                            controllerTransform.position + controllerTransform.parent.rotation * new Vector3(0, segment_point, 0));
                        prev_point = segment_point;
                    }
                    Gizmos.DrawLine(controllerTransform.position + controllerTransform.parent.rotation * new Vector3(0, segment_point, -0.1f)
                        , controllerTransform.position + controllerTransform.parent.rotation * new Vector3(0, segment_point, 0.1f));
                }
            }
            Gizmos.color = new Color(1, 0, 0);
            foreach (float snap_point in snap_points)
                Gizmos.DrawLine(controllerTransform.position + controllerTransform.parent.rotation * new Vector3(0, snap_point, -0.1f)
                    , controllerTransform.position + controllerTransform.parent.rotation * new Vector3(0, snap_point,0.1f));
        }
    }
}
