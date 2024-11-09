
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
    [UdonSynced] protected Vector3 ObjectLocalPos;
    [UdonSynced] protected Quaternion ObjectLocalRot;
    [UdonSynced] protected Vector3 ObjectBoneLocalPos;
    [UdonSynced] protected Quaternion ObjectBoneLocalRot;
    protected Vector3 Local_ObjectTrackingLocalPos;
    protected Quaternion Local_ObjectTrackingLocalRot;

    [UdonSynced] protected bool RightHand;
    Vector3 HandBonePos;
    Quaternion HandBoneRot;
    TrackingData trackingData;

    protected VRCPlayerApi prevOwner;
    protected VRCPlayerApi ownerPlayer;

    //----------------CONSTANT------------------
    protected VRCPlayerApi LocalPlayer;
    protected VRC_Pickup Pickup;
    protected Transform TransformCache;
    protected Vector3 First_Pos;
    protected Quaternion First_Rot;
    [SerializeField] protected bool isLocal;//For None(Local Object Mode)
                                            //----------------FALGS---------------------
    protected bool startFlag = false;
    protected bool postStartFrag = false;
    [UdonSynced] protected bool pickedFlag = false;
    protected bool pickInitFlag = false;
    protected bool dropInitFlag = false;
    protected bool dropFlag = false;

    protected bool isOwnerTransferredFlag = false;
    protected bool isThefting = false;
    //------------------------------------------


    //OwnerTransfer
    //|
    //V
    //OnPickup


    //PlayerLeft
    //|
    //V
    //OwnerTransfer
    //|
    //V
    //Update
    float fromUsed = 10;

    void Start()
    {
        Pickup = this.GetComponent<VRC_Pickup>();
        LocalPlayer = Networking.LocalPlayer;
        TransformCache = this.transform;
        First_Pos = ObjectLocalPos = TransformCache.localPosition;
        First_Rot = ObjectLocalRot = TransformCache.localRotation;
        startFlag = true;
    }

    public override void PostLateUpdate()
    {
        if (!startFlag)
        {
            return;
        }
        else if (!postStartFrag)
        {
            if (Networking.IsObjectReady(gameObject))
            {
                prevOwner = ownerPlayer = Networking.GetOwner(gameObject);
                postStartFrag = true;
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

        if (isOwnerTransferredFlag)
        {
            Debug.Log("onOwnerTransferred");
            onOwnerTransferred();
        }

        if (pickedFlag)
        {
            Debug.Log("onPicked");
            onPicked();
        }
        else if (dropInitFlag && ownerPlayer == LocalPlayer)
        {
            Debug.Log("onDropInit");
            onDropInit();
        }
        else if (dropFlag)
        {
            Debug.Log("onDropped");
            onDropped();
        }
    }

    protected void onOwnerTransferred()
    {
        if (pickedFlag)
        {
            if (isThefting)
            {
                if (Utilities.IsValid(prevOwner))
                {
                    FetchTrackingData(prevOwner);
                    MoveObjectByBone();
                }
                else
                {
                    pickedFlag = false;
                }
                isThefting = false;
            }
            else
            {
                pickedFlag = pickInitFlag;
            }
        }
        CalculateOffsetOnTransform(TransformCache.parent);
        RequestSerialization();
        isOwnerTransferredFlag = false;
    }

    protected void onPicked()
    {
        FetchTrackingData(ownerPlayer);
        if (ownerPlayer == LocalPlayer)
        {
            if (fromUsed < 10) fromUsed += Time.deltaTime;
            if (pickInitFlag)
            {
                Debug.Log("onPickInit");
                onPickInit();
            }
            MoveObjectByTrackingData();
        }
        else
        {
            MoveObjectByBone();
        }
        CalculateOffsetOnTransform(TransformCache.parent);
    }

    protected void onPickInit()
    {
        MoveObjectByOnTransformOffset(TransformCache.parent);
        CalculateOffsetOnTrackingData();
        CalculateOffsetOnBone();
        RequestSerialization();
        if (!LocalPlayer.IsUserInVR()) SendCustomEventDelayedSeconds(nameof(DeskTopWalkAround), 2);
        pickInitFlag = false;
    }

    protected void onDropInit()
    {
        FetchTrackingData(ownerPlayer);
        MoveObjectByTrackingData();
        CalculateOffsetOnTransform(TransformCache.parent);
        RequestSerialization();
        dropInitFlag = false;
    }
    protected void onDropped()
    {
        MoveObjectByOnTransformOffset(TransformCache.parent);
        dropFlag = false;
    }

    protected void FetchTrackingData()
    {
        FetchTrackingData(LocalPlayer);
    }
    protected void FetchTrackingData(VRCPlayerApi playerApi)
    {
        if (RightHand)
        {
            trackingData = playerApi.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
            HandBonePos = playerApi.GetBonePosition(HumanBodyBones.RightHand);
            HandBoneRot = playerApi.GetBoneRotation(HumanBodyBones.RightHand);
        }
        else
        {
            trackingData = playerApi.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
            HandBonePos = playerApi.GetBonePosition(HumanBodyBones.LeftHand);
            HandBoneRot = playerApi.GetBoneRotation(HumanBodyBones.LeftHand);
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
    protected void MoveObjectByOnTransformOffset(Transform parentTransform)
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
    protected void CalculateOffsetOnTransform(Transform parentTransform)
    {
        if (parentTransform)
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
        isThefting = pickedFlag;
        pickedFlag = true;
        pickInitFlag = true;
        dropInitFlag = false;
        dropFlag = false;
        RightHand = Pickup.currentHand == VRC_Pickup.PickupHand.Right;
    }
    public override void OnDrop()
    {
        pickedFlag = false;
        dropInitFlag = true;
        dropFlag = true;
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
        CalculateOffsetOnTransform(TransformCache.parent);


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
    }
    public override void OnOwnershipTransferred(VRC.SDKBase.VRCPlayerApi player)
    {
        Debug.Log("OwnerTransfer");
        if (isLocal) return;
        Debug.Log("transfered from " + ownerPlayer.playerId + "to " + player.playerId);
        prevOwner = ownerPlayer;
        ownerPlayer = player;
        if(player == LocalPlayer)
        {
            isOwnerTransferredFlag = true;
        }
    }
    public override void OnDeserialization()
    {
        dropFlag = true;//Update Position
    }
    public void DeskTopWalkAround()
    {
        RequestSerialization();
    }

}