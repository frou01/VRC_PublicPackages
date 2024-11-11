
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class LUPickUp_RootChangeable : LateUpdatePickUpBase
{
    [UdonSynced] protected int parentTransformID;
    private void OnTriggerEnter(Collider other)
    {
        if (other)
        {
            LUPick_CatcherCollider catcherCollider = other.GetComponent<LUPick_CatcherCollider>();
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
                TransformCache.parent = catcherCollider.transform;
                if (catcherCollider.dropTarget)
                {
                    TransformCache.position = catcherCollider.dropTarget.position;
                    TransformCache.rotation = catcherCollider.dropTarget.rotation;
                }
                if (catcherCollider.isHook)
                {
                    this.Pickup.Drop();
                }
                parentTransformID = catcherCollider.ID;

                RequestSerialization();
            }
        }
    }
}
