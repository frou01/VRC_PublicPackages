﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class LUPickUpRC_RootChangeable : LUPickUpBase_LateUpdatePickUpBase
{
    [UdonSynced] protected int crntCatcherID = -1;
    [UdonSynced] bool ExitWait_To_PickUp = false;
    protected Collider[] colliders;
    public override void Start()
    {
        base.Start();
        crntCatcher = GetComponentInParent<LUP_RC_CatcherCollider>();
        SetParentToCollider(crntCatcher);
        colliders = GetComponentsInChildren<Collider>();
    }
    private LUP_RC_CatcherCollider m_crntCatcher;
    protected LUP_RC_CatcherCollider crntCatcher
    {
        get
        {
            return m_crntCatcher;
        }
        set
        {
            m_crntCatcher = value;
            if (m_crntCatcher != null)
            {
                crntCatcherID = m_crntCatcher.ID;
            }
            else
            {
                crntCatcherID = -1;
            }
        }
    }
    [HideInInspector] [SerializeField] public LUP_RC_ColliderManager RCCManager;

    protected override void onPickInit()
    {
        base.onPickInit();
        if(ExitWait_To_PickUp)
        {
            ExitWait_To_PickUp = false;
            crntCatcher = null;
            StartExit();
        }
    }
    protected override void onDropInit()
    {
        base.onDropInit();
        if (crntCatcher)
        {
            if (crntCatcher.dropTarget)
            {
                TransformCache.position = crntCatcher.dropTarget.position;
                TransformCache.rotation = crntCatcher.dropTarget.rotation;
                CalculateOffsetOnTransform(TransformCache.parent);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other)
        {
            LUP_RC_CatcherCollider catcherCollider = other.GetComponent<LUP_RC_CatcherCollider>();
            if (catcherCollider)
            {
                if (catcherCollider == crntCatcher)
                {
                    ExitWait_To_PickUp = false;
                }
                if (catcherCollider.isSyncOwner && Networking.IsOwner(catcherCollider.gameObject))
                {
                    Networking.SetOwner(LocalPlayer, this.gameObject);
                }
                else if (ownerPlayer != LocalPlayer)
                {
                    return;
                }
                SetParentToCollider(catcherCollider);
                RequestSerialization();
            }
        }
    }
    protected bool isTransferingColliderFlag = false;
    private void OnTriggerExit(Collider other)
    {
        if (ownerPlayer != LocalPlayer) return;
        if (other)
        {
            LUP_RC_CatcherCollider catcherCollider = other.GetComponent<LUP_RC_CatcherCollider>();
            if (catcherCollider == crntCatcher)
            {
                if (pickedFlag)
                {
                    crntCatcher = null;
                    StartExit();
                }
                else
                {
                    ExitWait_To_PickUp = true;
                }
            }
        }
    }
    
    private void StartExit()
    {
        if (!isTransferingColliderFlag)
        {
            isTransferingColliderFlag = true;
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
            SendCustomEventDelayedFrames(nameof(_reactivateCollider), 0, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
        }
    }

    public virtual void _reactivateCollider()
    {
        foreach(Collider collider in colliders)
        {
            collider.enabled = true;
        }
        SendCustomEventDelayedFrames(nameof(_tryApplyExit), 0, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
    }

    public void _tryApplyExit()
    {
        if (crntCatcher == null)
        {
            SetParentToCollider(null);
            RequestSerialization();
        }
        isTransferingColliderFlag = false;
    }

    public override void SetPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        if (crntCatcherID == -1)
        {
            ResetParent();
        }
        else
        {
            Debug.Log(crntCatcherID);
            SetParentToCollider(RCCManager.RCCatchers[crntCatcherID]);
            RequestSerialization();
        }
        base.SetPositionAndRotation(position, rotation);

        if (Networking.IsOwner(this.gameObject))
        {
            StartExit();
        }
    }
    public override bool OnOwnershipRequest(VRC.SDKBase.VRCPlayerApi requestingPlayer, VRC.SDKBase.VRCPlayerApi requestedOwner)
    {
        return requestingPlayer == LocalPlayer || !isTransferingColliderFlag;
    }

    protected void SetParentToCollider(LUP_RC_CatcherCollider catcherCollider)
    {
        if (catcherCollider == null)
        {
            ResetParent();
            return;
        }
        if (ownerPlayer == LocalPlayer &&
            !catcherCollider.isHook &&
            !this.Pickup.IsHeld && !isTransferingColliderFlag)
        {
            return;
        }
        Debug.Log("ReplaceParent");
        crntCatcher = catcherCollider;
        TransformCache.parent = crntCatcher.transform;
        if (crntCatcher.dropTarget)
        {
            TransformCache.position = crntCatcher.dropTarget.position;
            TransformCache.rotation = crntCatcher.dropTarget.rotation;
        }
        CalculateOffsetOnTransform(TransformCache.parent);
        if (crntCatcher.isHook)
        {
            Debug.Log("Try Drop");
            this.Pickup.Drop();
        }

    }


    public void ResetParent()
    {
        this.ResetParent(true);
    }

    public void ResetParent(bool updateSyncingPos)
    {
        Debug.Log("ResetParent");
        TransformCache.parent = null;
        if(updateSyncingPos) CalculateOffsetOnTransform(TransformCache.parent);
        crntCatcher = null;
    }

    public override void ResetPosition()
    {
        base.ResetPosition();
        ResetParent();
    }
    public override void OnDeserialization()
    {
        if (crntCatcherID == -1)
        {
            ResetParent(false);
        }
        else
        {
            Debug.Log(crntCatcherID);
            SetParentToCollider(RCCManager.RCCatchers[crntCatcherID]);
        }
        base.OnDeserialization();
        Debug.Log("Recieve");
    }
}
