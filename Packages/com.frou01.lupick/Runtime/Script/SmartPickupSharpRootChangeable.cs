//SmartPickupSharp ver1.0

//当プログラムは、NEET ENIGNEER様のUdon Graphコード"Smart Pickup"を
//InPlanariaがUdon Sharp化し、機能追加を行ったものです。
//↑を参考にfrou01がコードの大半を置き換え、VR車両内ピックアップ向けに特化させたものになります。

//利用規約
//有償無償問わず再配布可　改変可　VRChatのprivateまたはpublicワールドへの組み込み可　クレジット表記不要　ただし製作者の詐称を禁止する

//This program is based on the Udon Graph code "Smart Pickup" by NEET ENIGNEER, which was transcribed by InPlanaria into Udon Sharp with additional functionality.
//Terms of Use
//Redistribution is allowed with or without compensation. Modifications are allowed.　Can be incorporated into private or public VRChat worlds. No credit needed, but do not misrepresent the its creator.

//SmartPickup by NEET ENGINEER https://neet-shop.booth.pm/items/2981343
//SmartPickupSharp by InPlanaria https://inplanaria.booth.pm/items/3640206


using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Enums;
using static VRC.SDKBase.VRCPlayerApi;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SmartPickupSharpRootChangeable : UdonSharpBehaviour
{

    
    public override void Interact()
    {
    }
    public LUPick_ColliderManager spsManager;
    [UdonSynced] int syncedID = -1;
    int localID = -1;
    LUPick_CatcherCollider currentCatcher;
    LUPick_CatcherCollider pennedCatcher;
    bool hooking;
    private Transform rootTransform;
    [SerializeField] bool overrideProximity;
    [SerializeField] float proximity;
    bool currentInteractionState;

    public void OnTriggerEnter(Collider other)
    {
        LUPick_CatcherCollider CldCathcer = other.gameObject.GetComponent<LUPick_CatcherCollider>();
        if (CldCathcer != null)
        {
            if (CldCathcer.isSyncOwner)
            {
                Networking.SetOwner(Networking.GetOwner(CldCathcer.gameObject), this.gameObject);
            }
            else if (!Networking.IsOwner(gameObject)) return;

            //Debug.Log("debug Enter " + CldCathcer.name);
            if (currentCatcher != CldCathcer)
            {
                if (picked || CldCathcer.isHook)
                {
                    if (currentCatcher != null && !currentCatcher.isHook) pennedCatcher = currentCatcher;
                    hooking = CldCathcer.isHook;
                    currentCatcher = CldCathcer;
                    localID = syncedID = CldCathcer.ID;
                    rootTransform = currentCatcher.transform;
                    if (hooking && currentCatcher.dropTarget != null)
                    {
                        TransformCache.position = currentCatcher.dropTarget.position;
                        TransformCache.rotation = currentCatcher.dropTarget.rotation;
                        Vector3 RootPos = rootTransform.position;
                        Quaternion RootRot = rootTransform.rotation;
                        ObjectRootLocalPos = Quaternion.Inverse(RootRot) * (TransformCache.position - RootPos);
                        ObjectRootLocalRot = Quaternion.Inverse(RootRot) * TransformCache.rotation;
                    }
                    RequestSerialization();
                }
                else
                {
                    pennedCatcher = CldCathcer;
                }
            }
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (!Networking.IsOwner(gameObject)) return;
        LUPick_CatcherCollider CldCathcer = other.gameObject.GetComponent<LUPick_CatcherCollider>();
        if (pennedCatcher != null && CldCathcer == pennedCatcher)
        {
            pennedCatcher = null;
        }
        if (CldCathcer != null && CldCathcer == currentCatcher && (picked && !hooking))
        {
            hooking = false;
            if (pennedCatcher != null)
            {
                Vector3 RootPos = rootTransform.position;
                Quaternion RootRot = rootTransform.rotation;
                TransformCache.position = RootPos + RootRot * (ObjectRootLocalPos);
                TransformCache.rotation = RootRot * ObjectRootLocalRot;
                currentCatcher = pennedCatcher;
                hooking = currentCatcher.isHook;
                localID = syncedID = currentCatcher.ID;
                rootTransform = currentCatcher.transform;
                if (hooking && currentCatcher.dropTarget != null)
                {
                    TransformCache.position = currentCatcher.dropTarget.position;
                    TransformCache.rotation = currentCatcher.dropTarget.rotation;
                }
                RootPos = rootTransform.position;
                RootRot = rootTransform.rotation;
                ObjectRootLocalPos = Quaternion.Inverse(RootRot) * (TransformCache.position - RootPos);
                ObjectRootLocalRot = Quaternion.Inverse(RootRot) * TransformCache.rotation;
            }
            else
            {
                hooking = false;
                currentCatcher = null;
                localID = syncedID = -1;
                rootTransform = null;
                ObjectRootLocalPos = TransformCache.position;
                ObjectRootLocalRot = TransformCache.rotation;
            }
            RequestSerialization();
        }
    }

    [UdonSynced] Vector3 ObjectRootLocalPos;
    [UdonSynced] Quaternion ObjectRootLocalRot;
    [UdonSynced] Vector3 ObjectBoneLocalPos;
    [UdonSynced] Quaternion ObjectBoneLocalRot;
    //Vector3 Local_ObjectBoneLocalPos;
    //Quaternion Local_ObjectBoneLocalRot;
    Vector3 Local_ObjectTrackingLocalPos;
    Quaternion Local_ObjectTrackingLocalRot;
    VRCPlayerApi ownerPlayer;
    VRCPlayerApi LocalPlayer;

    private VRC_Pickup Pickup;
    private Rigidbody cachedRB;
    [UdonSynced] bool picked;
    bool pickedInit;
    bool dropInit;
    [UdonSynced] bool RightHand;
    [UdonSynced] public bool isEnabled;

    [UdonSynced] bool carrying = false;

    bool init = false;
    Transform TransformCache;
    bool postInit = false;
    [SerializeField] bool hideOnStart = false;


    private Vector3 First_Pos;
    private Quaternion First_Rot;

    private Vector3 fixedPrevPos;
    private Quaternion fixedPrevRot;

    void Start()
    {
        Pickup = this.GetComponent<VRC_Pickup>();
        cachedRB = this.GetComponent<Rigidbody>();
        LocalPlayer = Networking.LocalPlayer;
        TransformCache = this.transform;
        init = true;
        dropInit = true;
        ObjectRootLocalPos = TransformCache.position;
        ObjectRootLocalRot = TransformCache.rotation;
        this.setIsEnable(!hideOnStart);
        First_Pos = TransformCache.localPosition;
        First_Rot = TransformCache.localRotation;

        if (overrideProximity)
        {
            this.DisableInteractive = !false;
            Pickup.pickupable = false;
            currentInteractionState = false;
        }
    }

    public override void OnPickup()
    {
        Debug.Log("debug pick");
        Networking.SetOwner(LocalPlayer, gameObject);
        picked = true;
        pickedInit = false;
        dropInit = false;
        RightHand = Pickup.currentHand == VRC_Pickup.PickupHand.Right;
        ownerPlayer = LocalPlayer;
    }
    public override void OnDrop()
    {
        if(ownerPlayer == LocalPlayer)
        {
            if (hooking)
            {
                dropInit = true;
                hooking = false;
                picked = false;
                RequestSerialization();
            }
            else
            {
                picked = false;
            }
        }
    }
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
        //if(pennedCatcher) Debug.Log("penned" + pennedCatcher.name);
        //if(currentCatcher) Debug.Log("current" + currentCatcher.name);
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
        carrying = rootTransform != null;
        if (picked)
        {
            Vector3 HandPos;
            Quaternion HandRot;
            TrackingData trackingData;
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
            if (ownerPlayer == LocalPlayer)
            {
                if (!pickedInit)
                {
                    if (carrying)
                    {
                        Vector3 RootPos = rootTransform.position;
                        Quaternion RootRot = rootTransform.rotation;
                        TransformCache.position = RootPos + RootRot * ObjectRootLocalPos;
                        TransformCache.rotation = RootRot * ObjectRootLocalRot;
                    }
                    else
                    {
                        TransformCache.position = ObjectRootLocalPos;
                        TransformCache.rotation = ObjectRootLocalRot;
                    }
                    Debug.Log("debug track" + trackingData.position);
                    Debug.Log("debug bonep" + HandPos);
                    pickedInit = true;

                    Local_ObjectTrackingLocalPos = Quaternion.Inverse(trackingData.rotation) * (TransformCache.position - trackingData.position);
                    Local_ObjectTrackingLocalRot = Quaternion.Inverse(trackingData.rotation) * TransformCache.rotation;
                    RequestSerialization();

                    if (!LocalPlayer.IsUserInVR())SendCustomEventDelayedSeconds(nameof(DeskTopWalkAround),2);
                }
                TransformCache.position = trackingData.position + (trackingData.rotation * Local_ObjectTrackingLocalPos);
                TransformCache.rotation = trackingData.rotation * Local_ObjectTrackingLocalRot;
                ObjectBoneLocalPos = /*Local_ObjectBoneLocalPos =*/ Quaternion.Inverse(HandRot) * (TransformCache.position - HandPos);
                ObjectBoneLocalRot = /*Local_ObjectBoneLocalRot =*/ Quaternion.Inverse(HandRot) * TransformCache.rotation;

                if (hooking)
                {
                    if (currentCatcher.dropTarget != null)
                    {
                        TransformCache.position = currentCatcher.dropTarget.position;
                        TransformCache.rotation = currentCatcher.dropTarget.rotation;
                        Vector3 RootPos = rootTransform.position;
                        Quaternion RootRot = rootTransform.rotation;
                        ObjectRootLocalPos = Quaternion.Inverse(RootRot) * (TransformCache.position - RootPos);
                        ObjectRootLocalRot = Quaternion.Inverse(RootRot) * TransformCache.rotation;
                    }
                    Pickup.Drop();
                }
            }
            else
            {
                TransformCache.position = HandPos + (HandRot * ObjectBoneLocalPos);
                TransformCache.rotation = HandRot * ObjectBoneLocalRot;
            }
            if (carrying)
            {
                Vector3 RootPos = rootTransform.position;
                Quaternion RootRot = rootTransform.rotation;
                ObjectRootLocalPos = Quaternion.Inverse(RootRot) * (TransformCache.position - RootPos);
                ObjectRootLocalRot = Quaternion.Inverse(RootRot) * TransformCache.rotation;
            }
            else
            {
                ObjectRootLocalPos = TransformCache.position;
                ObjectRootLocalRot = TransformCache.rotation;
            }
        }
        else if (!dropInit && ownerPlayer == LocalPlayer)
        {
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
                Vector3 RootPos = rootTransform.position;
                Quaternion RootRot = rootTransform.rotation;
                ObjectRootLocalPos = Quaternion.Inverse(RootRot) * (TransformCache.position - RootPos);
                ObjectRootLocalRot = Quaternion.Inverse(RootRot) * TransformCache.rotation;
            }
            else
            {
                ObjectRootLocalPos = TransformCache.position;
                ObjectRootLocalRot = TransformCache.rotation;
            }
            dropInit = true;
            RequestSerialization();
        }
        else if (carrying)
        {
            Vector3 RootPos = rootTransform.position;
            Quaternion RootRot = rootTransform.rotation;
            TransformCache.position = RootPos + RootRot * (ObjectRootLocalPos);
            TransformCache.rotation = RootRot * ObjectRootLocalRot;
        }
        else
        {
            TransformCache.position = (ObjectRootLocalPos);
            TransformCache.rotation = (ObjectRootLocalRot);
        }
        if (overrideProximity)
        {
            TrackingData handL;
            TrackingData handR;
            Vector3 pos = TransformCache.position;
            handL = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
            handR = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
            if (Vector3.Distance(handL.position, pos) < proximity ||
                Vector3.Distance(handR.position, pos) < proximity)
            {
                changeInteractionState(true);
            }
            else
            {
                changeInteractionState(false);
            }
        }
    }
    Vector3 currentPos;
    Quaternion currentRot;
    public void FixedUpdate()
    {
        if (!init)
        {
            return;
        }
        //トリガー貫通問題に対処するお
        //前回Fixed更新時の位置から現在のpositionまで補間ありで移動させるお
        //物理処理はFixedUpdateの直後だからコレで競合せず動くはずだお
        //https://docs.unity3d.com/ja/2022.3/Manual/ExecutionOrder.html

        //キャッチャーでつかめなくなったお…
        currentPos = TransformCache.position;
        currentRot = TransformCache.rotation;
        TransformCache.position = fixedPrevPos;
        TransformCache.rotation = fixedPrevRot;
        cachedRB.Move(currentPos, currentRot);

        fixedPrevPos = currentPos;
        fixedPrevRot = currentRot;
    }
    public void DeskTopWalkAround()
    {
        RequestSerialization();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if(player == ownerPlayer)
        {
            picked = false;
        }
    }
    public override void OnOwnershipTransferred(VRC.SDKBase.VRCPlayerApi player)
    {
        if (picked && player != ownerPlayer && ownerPlayer == LocalPlayer) Pickup.Drop();
        ownerPlayer = player;
    }
    public override void OnDeserialization()
    {
        Debug.Log("RCVD");
        if (syncedID != localID)
        {
            if (syncedID == -1)
            {
                currentCatcher = null;
                rootTransform = null;
            }
            else
            {
                currentCatcher = spsManager.SPSCatchers[syncedID].GetComponent<LUPick_CatcherCollider>();
                rootTransform = currentCatcher.transform;

            }
            localID = syncedID;
        }
        setIsEnable(isEnabled);
    }

    public override void OnPickupUseDown()
    {
        changeColliderState(false);
    }
    public override void OnPickupUseUp()
    {
        if(isEnabled) changeColliderState(true);
    }
    public void setIsEnable(bool state)
    {
        changeColliderState(state);
        this.GetComponent<MeshRenderer>().enabled = state;
        foreach (Component com in this.GetComponentsInChildren<MeshRenderer>())
        {
            ((MeshRenderer)com).enabled = state;
        }
        isEnabled = state;

        if (Networking.IsOwner(this.gameObject)) RequestSerialization();
    }
    void changeColliderState(bool state)
    {
        foreach (Component com in this.GetComponents<Collider>())
        {
            ((Collider)com).enabled = state;
        }
    }

    void changeInteractionState(bool targetState)
    {
        if(currentInteractionState != targetState)
        {
            this.DisableInteractive = !targetState;
            Pickup.pickupable = targetState;
            currentInteractionState = targetState;
        }
    }
    public void ResetPosition()
    {
        gameObject.SetActive(true);
        setIsEnable(true);
        TransformCache.localPosition = First_Pos;
        TransformCache.localRotation = First_Rot;
        ObjectRootLocalPos = TransformCache.position;
        ObjectRootLocalRot = TransformCache.rotation;


        if (Networking.IsOwner(this.gameObject))
        {
            if (Pickup.IsHeld)
            {
                Pickup.Drop();
            }
            picked = false;
            hooking = false;
            //Debug.Log("debug Exit " + CldCathcer.name);
            currentCatcher = null;
            localID = syncedID = -1;
            rootTransform = null;
            RequestSerialization();
        }

    }
    public void teleport(Vector3 pos, Quaternion qua)
    {
        currentCatcher = null;
        localID = syncedID = -1;
        rootTransform = null;
        picked = false;
        hooking = false;
        if (Networking.IsOwner(gameObject)) RequestSerialization();


        TransformCache.position = pos;
        TransformCache.rotation = qua;
        ObjectRootLocalPos = TransformCache.position;
        ObjectRootLocalRot = TransformCache.rotation;
    }
}
