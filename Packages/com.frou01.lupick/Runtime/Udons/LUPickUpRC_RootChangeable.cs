
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class LUPickUpRC_RootChangeable : LUPickUpBase_LateUpdatePickUpBase
{
    [UdonSynced] protected int crntCatcherID = -1;
    [UdonSynced] bool ExitWait_To_PickUp = false;//Issue collider check
    protected Collider[] colliders;
    protected int initialCatcherID;
    [SerializeField] public bool Tags_ExcludeExceptMode;
    [SerializeField] public string[] ExceptCatcherTags = new string[0];
    [SerializeField] public string[] PickupTags = new string[0];
    public override void Start()
    {
        base.Start();
        crntCatcher = GetComponentInParent<LUP_RC_CatcherCollider>();
        SetParentToCollider(crntCatcher);
        initialCatcherID = crntCatcherID;
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

    protected override void onPickInit_OwnerOnly()
    {
        base.onPickInit_OwnerOnly();
        if(ExitWait_To_PickUp)
        {
            ExitWait_To_PickUp = false;
            crntCatcher = null;
            StartExit();//check Collider
        }
    }
    protected override void onDropInit_OwnerOnly()
    {
        base.onDropInit_OwnerOnly();
        if (crntCatcher)
        {
            if (crntCatcher.dropTarget)
            {
                moveToDropPoint();
                CalculateOffsetOnTransform(TransformCache.parent);
                CalculateOffsetOnTrackingData();
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
                if (!CheckTags(catcherCollider)) return;
                if (catcherCollider == crntCatcher)
                {
                    ExitWait_To_PickUp = false;//ColliderCheck OK
                }
                if (catcherCollider.isSyncOwner && Networking.IsOwner(catcherCollider.gameObject))
                {
                    Networking.SetOwner(LocalPlayer, this.gameObject);
                }
                else if (ownerPlayer != LocalPlayer)
                {
                    return;
                }
                if (catcherCollider.isHook ||
                    this.Pickup.IsHeld ||
                    isTransferingColliderFlag)
                {
                    SetParentToCollider(catcherCollider);
                    RequestSerialization();
                }
            }
        }
    }

    protected virtual bool CheckTags(LUP_RC_CatcherCollider catcherCollider)
    {
        bool catchColliderExceptTag_Hit = true;
        foreach(string exceptPickupTag in catcherCollider.ExceptPickupTags)
        {
            foreach (string PickupTags in PickupTags)
            {
                if (exceptPickupTag.Equals(PickupTags)) catchColliderExceptTag_Hit = false;
            }
        }
        bool pickupExceptTag_Hit = true;
        foreach (string exceptCatcherTag in ExceptCatcherTags)
        {
            foreach (string CatcherTags in catcherCollider.CatcherTags)
            {
                if (exceptCatcherTag.Equals(CatcherTags)) pickupExceptTag_Hit = false;
            }
        }
        if (catcherCollider.Tags_ExcludeExceptMode)
        {
            catchColliderExceptTag_Hit = !catchColliderExceptTag_Hit;
        }
        if (Tags_ExcludeExceptMode)
        {
            pickupExceptTag_Hit = !pickupExceptTag_Hit;
        }

        return catchColliderExceptTag_Hit && pickupExceptTag_Hit;
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
            SetParentToNull();
        }
        else
        {
            //Debug.Log(crntCatcherID);
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
            SetParentToNull();
            return;
        }
        //Debug.Log("ReplaceParent");
        crntCatcher = catcherCollider;
        TransformCache.parent = crntCatcher.transform;
        if (crntCatcher.dropTarget)
        {
            moveToDropPoint();
        }
        CalculateOffsetOnTransform(TransformCache.parent);
        if (crntCatcher.isHook)
        {
            //Debug.Log("Try Drop");
            this.Pickup.Drop();
        }
    }

    protected virtual void moveToDropPoint()
    {
        Transform selectedTarget = null;
        if (crntCatcher.transform != crntCatcher.dropTarget && crntCatcher.dropTarget.childCount > 0)
        {
            float minimumScore = -1;
            foreach (Transform snapPoint in crntCatcher.dropTarget)
            {
                float angleToSnap = Quaternion.Angle(snapPoint.rotation, TransformCache.rotation);
                float toDistance = Vector3.Distance(snapPoint.position, TransformCache.position);
                float currentScore = angleToSnap * toDistance;
                if(currentScore < minimumScore || minimumScore == -1)
                {
                    selectedTarget = snapPoint;
                    minimumScore = currentScore;
                }
            }
        }
        else if(crntCatcher.dropTarget != null)
        {
            //Debug.Log("dropTarget " + crntCatcher.dropTarget.name);
            selectedTarget = crntCatcher.dropTarget;
            //Debug.Log("selectedTarget " + selectedTarget.gameObject.name);
        }
        if(selectedTarget != null)
        {
            //Debug.Log(TransformCache.position);
            //Debug.Log(selectedTarget.position);
            TransformCache.position = selectedTarget.position;
            TransformCache.rotation = selectedTarget.rotation;
            //Debug.Log(TransformCache.position);
        }
    }


    public void SetParentToNull()
    {
        this.SetParentToNull(true);
    }

    public void SetParentToNull(bool updateSyncingPos)
    {
        //Debug.Log("ResetParent");
        TransformCache.parent = null;
        crntCatcher = null;
        if (updateSyncingPos) CalculateOffsetOnTransform(TransformCache.parent);
    }

    public override void ResetPosition()
    {
        //Debug.Log(initialCatcherID);
        if (initialCatcherID == -1)
        {
            SetParentToNull();
        }
        else
        {
            SetParentToCollider(RCCManager.RCCatchers[initialCatcherID]);
        }
        base.ResetPosition();
    }
    public override void OnDeserialization()
    {
        if (crntCatcherID == -1)
        {
            SetParentToNull(false);
        }
        else
        {
            //Debug.Log(crntCatcherID);
            SetParentToCollider(RCCManager.RCCatchers[crntCatcherID]);
        }
        base.OnDeserialization();
        //Debug.Log("Recieve");
    }
}
