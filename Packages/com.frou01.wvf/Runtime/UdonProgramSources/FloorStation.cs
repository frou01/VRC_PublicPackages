
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class FloorStation : UdonSharpBehaviour
{
    public FloorStationController preset_StationMover;
    public Animator seatedSetter;
    [UdonSynced(UdonSyncMode.None)] public bool Allocated = false;
    [UdonSynced(UdonSyncMode.None)] public int AllocatePlayer;
    [UdonSynced(UdonSyncMode.None)] public bool SDK2Fallback = false;

    void Start()
    {
        SendCustomEventDelayedFrames(nameof(checkInit), Random.Range(60, 120));
    }

    bool initSynced = false;

    public void checkInit()
    {
        if (!initSynced || !Networking.IsObjectReady(this.gameObject))
        {
            initSynced |= Networking.IsMaster;
            SendCustomEventDelayedFrames(nameof(checkInit), Random.Range(60, 120));
            RequestSerialization();
        }
    }

    public void AllocateLocal(VRCPlayerApi allcFor)
    {
        if(Networking.IsObjectReady(gameObject))
        {
            Networking.SetOwner(allcFor, this.gameObject);
            Allocate(allcFor);
        }
    }
    public void Allocate( VRCPlayerApi allcFor)
    {
        Allocated = true;
        AllocatePlayer = allcFor.playerId;
        Networking.SetOwner(allcFor, preset_StationMover.gameObject);
        preset_StationMover.SyncParmReset();
        RequestSerialization();
    }
    public void CheckAllocation( VRCPlayerApi[] playerArray)
    {
        for (int id = 0; id < playerArray.Length; id++)
        {
            if(AllocatePlayer == playerArray[id].playerId)return;
        }
        //player allocated is left.
        deAllocate();
    }
    public void deAllocate()
    {
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        Allocated = false;
        AllocatePlayer = -1;
        RequestSerialization();
    }

    public override void OnPreSerialization()
    {
    }
    public override void OnDeserialization()
    {
        //Debug.Log("RCVD Station allocation state");
        initSynced = true;
        seatedSetter.enabled = true;
        if (SDK2Fallback)
        {
            seatedSetter.SetBool("seated", true);
        }
        else
        {
            seatedSetter.SetBool("seated", false);
        }
    }
    public override void OnPostSerialization(VRC.Udon.Common.SerializationResult result)
    {
        if (!result.success)
        {
            SendCustomEventDelayedFrames(nameof(FallbackRequestSerialization), Random.Range(10, 20));
        }
    }
    public void FallbackRequestSerialization()
    {
        RequestSerialization();
    }

    public void changeStationFallback()
    {
        SDK2Fallback = !SDK2Fallback;
        seatedSetter.enabled = true;
        if (SDK2Fallback)
        {
            seatedSetter.SetBool("seated", true);
        }
        else
        {
            seatedSetter.SetBool("seated", false);
        }
        RequestSerialization();
    }
    public void animationEnd()
    {
        seatedSetter.enabled = false;
    }

}
