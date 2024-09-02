
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using static VRC.SDKBase.VRCPlayerApi;

public class LateUpdatePickUpBase : UdonSharpBehaviour
{
    [UdonSynced] Vector3 ObjectLocalPos;
    [UdonSynced] Quaternion ObjectLocalRot;
    [UdonSynced] Vector3 ObjectBoneLocalPos;
    [UdonSynced] Quaternion ObjectBoneLocalRot;
    Vector3 Local_ObjectTrackingLocalPos;
    Quaternion Local_ObjectTrackingLocalRot;

    Transform TransformCache;//transform of this GameObject
    [SerializeField] Transform CurrentParentTransform;
    VRCPlayerApi prevOwner;//refer on transfering Ownership
    VRCPlayerApi ownerPlayer;
    VRCPlayerApi LocalPlayer;

    private VRC_Pickup Pickup;
    [SerializeField] bool isLocal = false;//ignore ownership transfer event
    [UdonSynced] bool RightHand;

    bool pickedInit;//true on loop after of VRC_Pickup.onpickup(). if false, excute initialize bone local position.
    [UdonSynced] bool picked;//true on Picked
    bool dropInit;//true on loop after of VRC_Pickup.ondrop(). if false, excute initialize Object Local position.
    bool droppedFlag = false;//true on Dropped and after dropInit

    bool init = false;//true after initialize
    bool postInit = false;//true after synce logic enabled

    //refer on Initialize. calculate
    [SerializeField] Transform InitialParentTransform;
    Vector3 Initial_Pos;//Local Position on InitialTransform 
    Quaternion Initial_Rot;//Local Position on InitialTransform

    bool isVR;
    TrackingData HandTrackingData;
    public override void PostLateUpdate()
    {
        if (!init)
        {
            return;
        }
        else if (!postInit)
        {
            if (Networking.IsObjectReady(gameObject))
            {
                ownerPlayer = Networking.GetOwner(gameObject);
                postInit = true;
            }
            return;
        }

        //Owner
        //VR:TrackingData OK
        //VR:GetBonePosition OK
        //Desk:TrackingData OK
        //Desk:GetBonePosition OK
        //Remote
        //VR:TrackingData => BonePosition
        //VR:GetBonePosition OK
        //Desk:TrackingData => BonePosition
        //Desk:GetBonePosition OK

        if (picked)
        {
            Vector3 HandPos;
            Quaternion HandRot;
            if (RightHand)
            {
                HandTrackingData = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
                HandPos = ownerPlayer.GetBonePosition(HumanBodyBones.RightHand);
                HandRot = ownerPlayer.GetBoneRotation(HumanBodyBones.RightHand);
            }
            else
            {
                HandTrackingData = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
                HandPos = ownerPlayer.GetBonePosition(HumanBodyBones.LeftHand);
                HandRot = ownerPlayer.GetBoneRotation(HumanBodyBones.LeftHand);
            }
            if (ownerPlayer == LocalPlayer)
            {
                if (!pickedInit)
                {
                    OnPicked(HandPos, HandRot, true);
                }
                InPickup(HandPos, HandRot, true);
                updateLocalPosition();
            }
            else
            {
                InPickup(HandPos, HandRot, false);
            }
        }
        else if (!dropInit && ownerPlayer == LocalPlayer)
        {
            Vector3 HandPos;
            Quaternion HandRot;
            if (RightHand)
            {
                HandTrackingData = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
                HandPos = ownerPlayer.GetBonePosition(HumanBodyBones.RightHand);
                HandRot = ownerPlayer.GetBoneRotation(HumanBodyBones.RightHand);
            }
            else
            {
                HandTrackingData = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
                HandPos = ownerPlayer.GetBonePosition(HumanBodyBones.LeftHand);
                HandRot = ownerPlayer.GetBoneRotation(HumanBodyBones.LeftHand);
            }
            movetoHanding(HandPos, HandRot, true);
            TransformCache.position = HandTrackingData.position + (HandTrackingData.rotation * Local_ObjectTrackingLocalPos);
            TransformCache.rotation = HandTrackingData.rotation * Local_ObjectTrackingLocalRot;

            ObjectLocalPos = TransformCache.localPosition;
            ObjectLocalRot = TransformCache.localRotation;
            dropInit = true;
            RequestSerialization();
        }
        else if (!droppedFlag)
        {
            TransformCache.position = (ObjectLocalPos);
            droppedFlag = true;
        }
        prevOwner = ownerPlayer;
    }

    protected void OnPicked(Vector3 handPos, Quaternion handRot, bool local)
    {
        TransformCache.localRotation = ObjectLocalRot;
        TransformCache.localPosition = ObjectLocalPos;
        //Debug.Log("debug track" + trackingData.position);
        //Debug.Log("debug bonep" + HandPos);
        //PlayerPos = ownerPlayer.GetPosition();
        //PlayerRot = trackingData_Origin.rotation;
        pickedInit = true;

        Local_ObjectTrackingLocalPos = Quaternion.Inverse(HandTrackingData.rotation) * (TransformCache.position - HandTrackingData.position);
        Local_ObjectTrackingLocalRot = Quaternion.Inverse(HandTrackingData.rotation) * TransformCache.rotation;
        RequestSerialization();
        if (!LocalPlayer.IsUserInVR()) SendCustomEventDelayedSeconds(nameof(DeskTopWalkAround), 2);
    }

    protected void InPickup(Vector3 HandPos, Quaternion HandRot, bool local)
    {
        movetoHanding(HandPos, HandRot, local);
    }

    protected void movetoHanding(Vector3 HandPos, Quaternion HandRot, bool local)
    {
        if (local)
        {
            TransformCache.position = HandTrackingData.position + (HandTrackingData.rotation * Local_ObjectTrackingLocalPos);
            TransformCache.rotation = HandTrackingData.rotation * Local_ObjectTrackingLocalRot;
            ObjectBoneLocalPos = Quaternion.Inverse(HandRot) * (TransformCache.position - HandPos);
            ObjectBoneLocalRot = Quaternion.Inverse(HandRot) * TransformCache.rotation;
        }
        else
        {
            TransformCache.position = HandPos + (HandRot * ObjectBoneLocalPos);
            TransformCache.rotation = HandRot * ObjectBoneLocalRot;
        }
    }

    protected void updateLocalPosition()
    {
        ObjectLocalRot = TransformCache.localRotation;
        ObjectLocalPos = TransformCache.localPosition;
    }
    public void DeskTopWalkAround()
    {
        RequestSerialization();
    }
    public override void OnOwnershipTransferred(VRC.SDKBase.VRCPlayerApi player)
    {
        if (isLocal) return;
        Debug.Log("transfered from " + ownerPlayer.playerId + "to " + player.playerId);
        if (picked && player != ownerPlayer && ownerPlayer == LocalPlayer) Pickup.Drop();
        ownerPlayer = player;
        droppedFlag = false;
    }
}
