﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using static VRC.SDKBase.VRCPlayerApi;
using VRC.Udon;
using VRC.Udon.Common.Enums;
using VRC.SDK3.Components;
//参考元:SmartPickupSharp by InPlanaria https://inplanaria.booth.pm/items/3640206

public class PlayerLocalPickup : UdonSharpBehaviour
{
    [UdonSynced] Vector3 ObjectPlayerLocalPos;
    [UdonSynced] Quaternion ObjectPlayerLocalRot;
    [UdonSynced] Vector3 ObjectBoneLocalPos;
    [UdonSynced] Quaternion ObjectBoneLocalRot;
    //Vector3 Local_ObjectBoneLocalPos;
    //Quaternion Local_ObjectBoneLocalRot;
    Vector3 Local_ObjectTrackingLocalPos;
    Quaternion Local_ObjectTrackingLocalRot;
    VRCPlayerApi ownerPlayer;
    VRCPlayerApi LocalPlayer;
    [SerializeField]PlayerChaser playerChaser;

    private VRC_Pickup Pickup;
    [UdonSynced] bool picked;
    [SerializeField] bool isLocal = false;
    bool pickedInit;
    bool dropInit;
    [UdonSynced] bool RightHand;

    [UdonSynced] bool carrying = false;
    bool droppedFlag = false;

    [SerializeField] GameObject carryingStateShower;

    bool init = false;
    Transform TransformCache;
    bool postInit = false;
    [SerializeField] VRCObjectPool ObjectPool;
    [SerializeField] VRCObjectPool[] ReturnPool;
    Vector3 First_Pos;
    Quaternion First_Rot;

    bool isVR;
    void Start()
    {
        if (LocalPlayer != null) isVR = LocalPlayer.IsUserInVR();
        Pickup = this.GetComponent<VRC_Pickup>();
        LocalPlayer = Networking.LocalPlayer;
        TransformCache = this.transform;
        init = true;
        dropInit = true;
        droppedFlag = false;

        First_Pos = ObjectPlayerLocalPos = TransformCache.position;
        First_Rot = TransformCache.localRotation;
    }

    VRCPlayerApi prevOwner;
    public override void OnPickup()
    {
        Debug.Log("debug pick");
        isVR = LocalPlayer.IsUserInVR();
        //Networking.SetOwner(LocalPlayer, gameObject);
        picked = true;
        pickedInit = false;
        dropInit = false;
        RightHand = Pickup.currentHand == VRC_Pickup.PickupHand.Right;
        ownerPlayer = LocalPlayer;
        if (carryingStateShower) carryingStateShower.SetActive(carrying);
        droppedFlag = false;
    }
    public override void OnDrop()
    {
        picked = false;
        if (carryingStateShower) carryingStateShower.SetActive(false);
        droppedFlag = false;
    }

    float fromUsed;
    public override void OnPickupUseDown()
    {
        if (fromUsed > 0.2f)
        {
            fromUsed = 0;
            return;
        }
        carrying = !carrying;
        Pickup.UseText = carrying ? "Global Mode" : "Local Mode";
        if (carryingStateShower) carryingStateShower.SetActive(carrying);
        RequestSerialization();
        droppedFlag = false;
    }
    //public void Update()
    //{
    //    if (!postInit) return;
    //    //Owner
    //    //VR:TrackingData OK
    //    //VR:GetBonePosition OK
    //    //Desk:TrackingData OK
    //    //Desk:GetBonePosition OK
    //    //Remote
    //    //VR:TrackingData => BonePosition
    //    //VR:GetBonePosition OK
    //    //Desk:TrackingData => BonePosition
    //    //Desk:GetBonePosition OK
    //    if (picked)
    //    {
    //
    //    }
    //}

    Quaternion onDropRot;
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
            TrackingData trackingData;
            //TrackingData trackingData_Origin = ownerPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin);
            if (RightHand)
            {
                trackingData = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
                HandPos = ownerPlayer.GetBonePosition(HumanBodyBones.RightHand);
                HandRot = ownerPlayer.GetBoneRotation(HumanBodyBones.RightHand);
            }
            else
            {
                trackingData = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
                HandPos = ownerPlayer.GetBonePosition(HumanBodyBones.LeftHand);
                HandRot = ownerPlayer.GetBoneRotation(HumanBodyBones.LeftHand);
            }
            //Quaternion PlayerRot = trackingData_Origin.rotation;
            Vector3 PlayerPos = ownerPlayer.GetPosition();
            if (ownerPlayer == LocalPlayer)
            {
                if (fromUsed < 10) fromUsed += Time.deltaTime;
                if (!pickedInit)
                {
                    if (carrying)
                    {
                        //PlayerRot = prevOwner.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin).rotation;
                        PlayerPos = prevOwner.GetPosition();
                        TransformCache.position = PlayerPos + /*PlayerRot **/ (ObjectPlayerLocalPos);
                        TransformCache.rotation = /*PlayerRot **/ ObjectPlayerLocalRot;
                    }
                    else
                    {
                        TransformCache.position = (ObjectPlayerLocalPos);
                    }
                    Debug.Log("debug track" + trackingData.position);
                    Debug.Log("debug bonep" + HandPos);
                    PlayerPos = ownerPlayer.GetPosition();
                    //PlayerRot = trackingData_Origin.rotation;
                    pickedInit = true;

                    Local_ObjectTrackingLocalPos = Quaternion.Inverse(trackingData.rotation) * (TransformCache.position - trackingData.position);
                    Local_ObjectTrackingLocalRot = Quaternion.Inverse(trackingData.rotation) * TransformCache.rotation;
                    RequestSerialization();
                    if (!LocalPlayer.IsUserInVR()) SendCustomEventDelayedSeconds(nameof(DeskTopWalkAround), 2);
                }
                TransformCache.position = trackingData.position + (trackingData.rotation * Local_ObjectTrackingLocalPos);
                TransformCache.rotation = trackingData.rotation * Local_ObjectTrackingLocalRot;
                ObjectBoneLocalPos = /*Local_ObjectBoneLocalPos = */Quaternion.Inverse(HandRot) * (TransformCache.position - HandPos);
                ObjectBoneLocalRot = /*Local_ObjectBoneLocalRot = */Quaternion.Inverse(HandRot) * TransformCache.rotation;
            }
            else
            {
                TransformCache.position = HandPos + (HandRot * ObjectBoneLocalPos);
                TransformCache.rotation = HandRot * ObjectBoneLocalRot;
            }
            if (carrying)
            {
                ObjectPlayerLocalPos = /*Quaternion.Inverse(PlayerRot) **/ (TransformCache.position - PlayerPos);
                ObjectPlayerLocalRot = /*Quaternion.Inverse(PlayerRot) **/ TransformCache.rotation;
            }
            else
            {
                ObjectPlayerLocalPos = TransformCache.position;
            }
        }
        else if (!dropInit && ownerPlayer == LocalPlayer)
        {
            Vector3 PlayerPos = ownerPlayer.GetPosition();
            //Quaternion PlayerRot =  ownerPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin).rotation;
            //onDropRot = PlayerRot;
            TrackingData trackingData;
            if (RightHand)
            {
                trackingData = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
            }
            else
            {
                trackingData = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
            }
            TransformCache.position = trackingData.position + (trackingData.rotation * Local_ObjectTrackingLocalPos);
            TransformCache.rotation = trackingData.rotation * Local_ObjectTrackingLocalRot;
            if (carrying)
            {
                ObjectPlayerLocalPos = /*Quaternion.Inverse(PlayerRot) **/ (TransformCache.position - PlayerPos);
                ObjectPlayerLocalRot = /*Quaternion.Inverse(PlayerRot) **/ TransformCache.rotation;
            }
            else
            {
                ObjectPlayerLocalPos = TransformCache.position;
            }
            dropInit = true;
            RequestSerialization();
        }
        else if (carrying)
        {
            Vector3 PlayerPos = ownerPlayer.GetPosition();
            //Quaternion PlayerRot = ownerPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin).rotation;
            //if (!isVR && ownerPlayer == LocalPlayer)
            //{
            //    PlayerRot = onDropRot;
            //}
            TransformCache.position = PlayerPos + /*PlayerRot **/ (ObjectPlayerLocalPos);
            TransformCache.rotation = /*PlayerRot **/ ObjectPlayerLocalRot;
        }
        else if (!droppedFlag)
        {
            TransformCache.position = (ObjectPlayerLocalPos);
            droppedFlag = true;
        }
        if (returning)
        {
            returning = false;
            foreach (GameObject go in ObjectPool.Pool)
            {
                if (go == gameObject) ObjectPool.Return(gameObject);
            }
        }
        prevOwner = ownerPlayer;
    }
    public void DeskTopWalkAround()
    {
        RequestSerialization();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (player == prevOwner)
        {
            picked = false;
        }
        droppedFlag = false;
    }
    public override void OnOwnershipTransferred(VRC.SDKBase.VRCPlayerApi player)
    {
        if (isLocal) return;
        Debug.Log("transfered from " + ownerPlayer.playerId + "to " + player.playerId);
        if (picked && player != ownerPlayer && ownerPlayer == LocalPlayer) Pickup.Drop();
        ownerPlayer = player;
        droppedFlag = false;
    }

    bool returning;
    public void returnToPool(PickupDropper pickupDropper)
    {
        if (ownerPlayer == LocalPlayer)
        {
            if (pickupDropper.ObjectPool == this.ObjectPool)
            {
                returning = true;
                picked = false;
                carrying = false;
            }
            else
                foreach (VRCObjectPool objectPool in ReturnPool)
                {
                    if (objectPool == pickupDropper.ObjectPool)
                    {
                        returning = true;
                        picked = false;
                        carrying = false;
                        return;
                    }
                }
        }
        droppedFlag = false;
    }

    public override void OnDeserialization()
    {
        if (carryingStateShower) carryingStateShower.SetActive(false);
        droppedFlag = false;
    }
    public void ResetPosition()
    {
        gameObject.SetActive(true);
        carrying = false;
        TransformCache.position = First_Pos;
        ObjectPlayerLocalPos = First_Pos;
        TransformCache.localRotation = First_Rot;


        if (Networking.IsOwner(this.gameObject))
        {
            if (Pickup.IsHeld)
            {
                Pickup.Drop();
            }
            picked = false;
            RequestSerialization();
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "ResetPosition");
        }
        droppedFlag = false;

    }
}
