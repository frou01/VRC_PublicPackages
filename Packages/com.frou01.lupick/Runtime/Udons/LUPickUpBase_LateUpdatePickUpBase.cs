
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using static VRC.SDKBase.VRCPlayerApi;

public class LUPickUpBase_LateUpdatePickUpBase : UdonSharpBehaviour
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
    protected Vector3 HandBonePos;
    protected Quaternion HandBoneRot;
    protected TrackingData trackingData;

    protected VRCPlayerApi prevOwner;
    protected VRCPlayerApi ownerPlayer;

    //----------------CONSTANT------------------
    protected VRCPlayerApi LocalPlayer;
    protected VRC_Pickup Pickup;
    protected Rigidbody PickupRigidBody;
    protected Transform TransformCache;
    protected Vector3 First_Pos;
    protected Quaternion First_Rot;
    [SerializeField] protected bool isLocal;//For None(Local Object Mode)
                                            //----------------FALGS---------------------
    /**************/protected bool startFlag = false;
    /**************/protected bool postStartFrag = false;
    /**************/protected bool pickInitFlag = false;
    [UdonSynced]    protected bool pickedFlag = false;
    /**************/protected bool dropInitFlag = false;
    /**************/protected bool dropFlag = false;

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

    public virtual void Start()
    {
        Pickup = this.GetComponent<VRC_Pickup>();
        PickupRigidBody = this.GetComponent<Rigidbody>();
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
            //Debug.Log("onOwnerTransferred");
            onOwnerTransferred();
        }

        if (pickedFlag)
        {
            //Debug.Log("onPicked");
            onPicked();
        }
        else if (dropInitFlag && ownerPlayer == LocalPlayer)
        {
            //Debug.Log("onDropInit");
            onDropInit();
        }
        else if (dropFlag)
        {
            //Debug.Log("onDropped");
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
                pickedFlag = this.Pickup.IsHeld;
            }
        }
        CalculateOffsetOnTransform(TransformCache.parent);
        RequestSerialization();
        isOwnerTransferredFlag = false;
    }

    protected virtual void onPicked()
    {
        FetchTrackingData(ownerPlayer);
        if (ownerPlayer == LocalPlayer)
        {
            if (fromUsed < 10) fromUsed += Time.deltaTime;
            if (pickInitFlag)
            {
                //Debug.Log("onPickInit");
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

    protected virtual void onPickInit()
    {
        MoveObjectByOnTransformOffset(TransformCache.parent);
        CalculateOffsetOnTrackingData();
        CalculateOffsetOnBone();
        RequestSerialization();
        if (!LocalPlayer.IsUserInVR()) SendCustomEventDelayedSeconds(nameof(DeskTopWalkAround), 2);
        pickInitFlag = false;
    }

    protected virtual void onDropInit()
    {
        FetchTrackingData(ownerPlayer);
        MoveObjectByTrackingData();
        CalculateOffsetOnTransform(TransformCache.parent);
        RequestSerialization();
        dropInitFlag = false;
    }
    protected virtual void onDropped()
    {
        MoveObjectByOnTransformOffset(TransformCache.parent);
        dropFlag = false;
    }

    protected virtual void FetchTrackingData()
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
            Debug.Log("Debug MOT InTransformBlock");
            TransformCache.position = parentTransform.rotation * ObjectLocalPos + parentTransform.position;
            TransformCache.rotation = parentTransform.rotation * ObjectLocalRot;
        }
        else
        {
            Debug.Log("Debug MOT NullBlock");
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
            Debug.Log("Debug COT InTransformBlock");
            ObjectLocalPos = Quaternion.Inverse(parentTransform.rotation) * (TransformCache.position - parentTransform.position);
            ObjectLocalRot = Quaternion.Inverse(parentTransform.rotation) * TransformCache.rotation;
        }
        else
        {
            Debug.Log("Debug COT NullBlock");
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
    public virtual void ResetPosition()
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
    public virtual void SetPositionAndRotation(Vector3 position,Quaternion rotation)
    {
        if (Networking.IsOwner(this.gameObject))
        {
            gameObject.SetActive(true);
            TransformCache.position = position;
            TransformCache.rotation = rotation;
            CalculateOffsetOnTransform(TransformCache.parent);
            if (Pickup.IsHeld)
            {
                Pickup.Drop();
            }
            pickedFlag = false;
            RequestSerialization();
        }
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
        MoveObjectByOnTransformOffset(TransformCache.parent);//Update Position
    }
    public void DeskTopWalkAround()
    {
        RequestSerialization();
    }

}