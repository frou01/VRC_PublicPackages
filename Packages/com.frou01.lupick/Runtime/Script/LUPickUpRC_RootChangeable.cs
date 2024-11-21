
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class LUPickUpRC_RootChangeable : LUPickUpBase_LateUpdatePickUpBase
{
    [UdonSynced] protected int crntCatcherID;
    [UdonSynced] protected int pendCatcherID;
    protected LUP_RC_CatcherCollider crntCatcher;
    protected LUP_RC_CatcherCollider pendCatcher;
    [HideInInspector] [SerializeField] public LUP_RC_ColliderManager spsManager;

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
                if (catcherCollider.isSyncOwner && Networking.IsOwner(catcherCollider.gameObject))
                {
                    Networking.SetOwner(LocalPlayer, this.gameObject);
                }
                else if (ownerPlayer != LocalPlayer)
                {
                    return;
                }
                SetParentToCollider(catcherCollider);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other)
        {
            LUP_RC_CatcherCollider catcherCollider = other.GetComponent<LUP_RC_CatcherCollider>();
            if (catcherCollider == crntCatcher)
            {
                crntCatcher = pendCatcher;
                pendCatcher = null;
                SetParentToCollider(crntCatcher);
            }
            else if(catcherCollider == pendCatcher)
            {
                pendCatcher = null;
            }
        }
    }

    protected void SetParentToCollider(LUP_RC_CatcherCollider catcherCollider)
    {
        if (catcherCollider == null)
        {
            ResetParent();
            return;
        }
        Debug.Log("ReplacePairent");
        if (crntCatcher != catcherCollider)
        {
            pendCatcher = crntCatcher;
            if(pendCatcher) pendCatcherID = pendCatcher.ID;
        }
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

        RequestSerialization();
    }

    protected void ResetParent()
    {
        Debug.Log("ResetParent");
        TransformCache.parent = null;
        if (dropFlag) CalculateOffsetOnTransform(TransformCache.parent);
        pendCatcherID = -1;
        crntCatcher = null;
        RequestSerialization();
    }
    public override void OnDeserialization()
    {
        base.OnDeserialization();
        if (crntCatcherID == -1)
        {
            ResetParent();
        }
        else
        {
            SetParentToCollider(spsManager.SPSCatchers[crntCatcherID]);
        }
        if(pendCatcherID == -1)
        {
            pendCatcher = spsManager.SPSCatchers[pendCatcherID];
        }
        else
        {
            pendCatcher = null;
        }
    }
}
