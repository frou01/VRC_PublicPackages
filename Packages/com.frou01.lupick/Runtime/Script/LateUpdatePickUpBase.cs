
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using static VRC.SDKBase.VRCPlayerApi;

public class LateUpdatePickUpBase : UdonSharpBehaviour
{
//------------------------------------------
//----------------VARIABLE------------------
    [UdonSynced] Vector3 ObjectLocalPos;
    [UdonSynced] Quaternion ObjectLocalRot;
    [UdonSynced] Vector3 ObjectBoneLocalPos;
    [UdonSynced] Quaternion ObjectBoneLocalRot;
    Vector3 Local_ObjectTrackingLocalPos;
    Quaternion Local_ObjectTrackingLocalRot;

    [UdonSynced] bool RightHand;
    Vector3 HandBonePos;
    Quaternion HandBoneRot;
    TrackingData trackingData;

    VRCPlayerApi prevOwner;
    VRCPlayerApi ownerPlayer;

//----------------CONSTANT------------------
    VRCPlayerApi LocalPlayer;
    private VRC_Pickup Pickup;
    Transform TransformCache;
    Vector3 First_Pos;
    Quaternion First_Rot;
    [SerializeField] bool isLocal;//For None(Local Object Mode)
//----------------FALGS---------------------
    bool initFlag = false;
    bool postInitFrag = false;
    [UdonSynced] bool pickedFlag = false;
    bool pickInitFlag = false;
    bool dropInitFlag = true;
    bool droppedFlag = true;
//------------------------------------------


    float fromUsed = 10;

    void Start()
    {
        Pickup = this.GetComponent<VRC_Pickup>();
        LocalPlayer = Networking.LocalPlayer;
        TransformCache = this.transform;
        First_Pos = ObjectLocalPos = TransformCache.localPosition;
        First_Rot = ObjectLocalRot = TransformCache.localRotation;
        initFlag = true;
    }

    public override void PostLateUpdate()
    {
        if (!initFlag)
        {
            return;
        }
        else if (!postInitFrag)
        {
            if (Networking.IsObjectReady(gameObject))
            {
                prevOwner = ownerPlayer = Networking.GetOwner(gameObject);
                postInitFrag = true;
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

        if (pickedFlag)
        {
            onPicked();
        }
        else if (!dropInitFlag && ownerPlayer == LocalPlayer)
        {
            onDropInit();
        }
        else if (!droppedFlag)
        {
            onDropped();
        }
    }

    protected void onPicked()
    {
        FetchTrackingData();
        if (ownerPlayer == LocalPlayer)
        {
            if (fromUsed < 10) fromUsed += Time.deltaTime;
            if (!pickInitFlag)
            {
                onPickInit();
            }
            MoveObjectByTrackingData();
        }
        else
        {
            MoveObjectByBone();
        }
        CalculateOffsetOnTransform();
    }

    protected void onPickInit()
    {
        Debug.Log("debug track" + trackingData.position);
        Debug.Log("debug bonep" + HandBonePos);
        MoveObjectByOnTransformOffset();
        CalculateOffsetOnTrackingData();
        CalculateOffsetOnBone();
        RequestSerialization();
        if (!LocalPlayer.IsUserInVR()) SendCustomEventDelayedSeconds(nameof(DeskTopWalkAround), 2);
        pickInitFlag = true;
    }

    protected void onDropInit()
    {
        FetchTrackingData();
        MoveObjectByTrackingData();
        CalculateOffsetOnTransform();

        dropInitFlag = true;
        RequestSerialization();
    }
    protected void onDropped()
    {
        MoveObjectByOnTransformOffset();
        droppedFlag = true;
    }

    protected void FetchTrackingData()
    {
        if (RightHand)
        {
            trackingData = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
            HandBonePos = ownerPlayer.GetBonePosition(HumanBodyBones.RightHand);
            HandBoneRot = ownerPlayer.GetBoneRotation(HumanBodyBones.RightHand);
        }
        else
        {
            trackingData = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
            HandBonePos = ownerPlayer.GetBonePosition(HumanBodyBones.LeftHand);
            HandBoneRot = ownerPlayer.GetBoneRotation(HumanBodyBones.LeftHand);
        }
    }
    protected void MoveObjectByTrackingData()
    {
        TransformCache.position = trackingData.position + (trackingData.rotation * Local_ObjectTrackingLocalPos);
        TransformCache.rotation = trackingData.rotation * Local_ObjectTrackingLocalRot;
    }
    protected void MoveObjectByBone()
    {
        TransformCache.position = HandBoneRot * ObjectBoneLocalPos + HandBonePos;
        TransformCache.rotation = HandBoneRot * ObjectBoneLocalRot;
    }
    protected void MoveObjectByOnTransformOffset(Transform parentTransform = null)
    {
        if (parentTransform)
        {
            TransformCache.localPosition = parentTransform.rotation * ObjectLocalPos + HandBonePos;
            TransformCache.localRotation = parentTransform.rotation * ObjectLocalRot;
        }
        else
        {
            TransformCache.localPosition = ObjectLocalPos;
            TransformCache.localRotation = ObjectLocalRot;
        }
    }

    protected void CalculateOffsetOnTrackingData()
    {
        Local_ObjectTrackingLocalPos = Quaternion.Inverse(trackingData.rotation) * (TransformCache.position - trackingData.position);
        Local_ObjectTrackingLocalRot = Quaternion.Inverse(trackingData.rotation) * TransformCache.rotation;
    }
    protected void CalculateOffsetOnBone()
    {
        ObjectBoneLocalPos = Quaternion.Inverse(HandBoneRot) * (TransformCache.position - HandBonePos);
        ObjectBoneLocalRot = Quaternion.Inverse(HandBoneRot) * TransformCache.rotation;
    }
    protected void CalculateOffsetOnTransform(Transform parentTransform = null)
    {
        if(parentTransform)
        {
            ObjectLocalPos = Quaternion.Inverse(parentTransform.rotation) * (TransformCache.position - parentTransform.position);
            ObjectLocalRot = Quaternion.Inverse(parentTransform.rotation) * TransformCache.rotation;
        }
        else
        {
            ObjectLocalPos = TransformCache.localPosition;
            ObjectLocalRot = TransformCache.localRotation;
        }
    }
    public override void OnPickup()
    {
        pickedFlag = true;
        pickInitFlag = false;
        dropInitFlag = false;
        droppedFlag = false;
        RightHand = Pickup.currentHand == VRC_Pickup.PickupHand.Right;
        ownerPlayer = LocalPlayer;
    }
    public override void OnDrop()
    {
        pickedFlag = false;
        droppedFlag = false;
    }
    public override void OnPickupUseDown()
    {
        if (fromUsed > 0.2f)
        {
            //singleTap
            fromUsed = 0;
            return;
        }
        //doubleTap
    }
    public void ResetPosition()
    {
        gameObject.SetActive(true);
        TransformCache.localPosition = First_Pos;
        TransformCache.localRotation = First_Rot;
        CalculateOffsetOnTransform();


        if (Networking.IsOwner(this.gameObject))
        {
            if (Pickup.IsHeld)
            {
                Pickup.Drop();
            }
            pickedFlag = false;
            RequestSerialization();
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "ResetPosition");
        }

    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (player == prevOwner)
        {
            pickedFlag = false;
        }
        droppedFlag = false;
    }
    public override void OnOwnershipTransferred(VRC.SDKBase.VRCPlayerApi player)
    {
        if (isLocal) return;
        Debug.Log("transfered from " + ownerPlayer.playerId + "to " + player.playerId);
        if (pickedFlag && player != ownerPlayer && ownerPlayer == LocalPlayer) Pickup.Drop();
        prevOwner = ownerPlayer;
        ownerPlayer = player;
        droppedFlag = false;
    }
    public override void OnDeserialization()
    {
        droppedFlag = false;//Update Position
    }
    public void DeskTopWalkAround()
    {
        RequestSerialization();
    }

}