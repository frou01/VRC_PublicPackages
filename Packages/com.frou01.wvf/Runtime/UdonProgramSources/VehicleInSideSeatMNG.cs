
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class VehicleInSideSeatMNG : UdonSharpBehaviour
{

    [UdonSynced] bool dummySync;

    [System.NonSerialized]public FloorStationController local_AllocatedSeat;

    public FloorStationController[] preset_inVehicleStations;

    public CatchCollider_Vehicle[] preset_CatchColliders;

    public GameObject[] preset_inVehicleCollider;

    public PlayerChaser preset_playerChaser;

    void Start()
    {
        for(int id = 0; id < preset_CatchColliders.Length; id++)
        {
            preset_CatchColliders[id].local_SeatMNG = this;
            preset_CatchColliders[id].local_Id_OnSeatMNG = id;
        }
    }

    public void EnterOnVehicle(int VehicleID)
    {
        if (local_AllocatedSeat == null)
        {
            local_AllocatedSeat = allocateSeat(Networking.LocalPlayer);
        }
        else if(local_AllocatedSeat.AllocatePlayer != Networking.LocalPlayer.playerId)
        {
            allocateSeatToLocal();
        }
        if (local_AllocatedSeat != null)
        {
            Debug.Log("Allocated Seat is " + local_AllocatedSeat.gameObject.name);
            if(!local_AllocatedSeat.synced_Using) local_AllocatedSeat.startSeating(VehicleID);
        }
        else
        {
            Debug.Log("No Allocated!");
        }
    }
    public void InteractedOnVehicle(int VehicleID)
    {
        if (local_AllocatedSeat == null)
        {
            local_AllocatedSeat = allocateSeat(Networking.LocalPlayer);
        }
        //Debug.Log("Allocated Seat is " + local_AllocatedSeat.gameObject.name);
        if (local_AllocatedSeat != null)
        {
            local_AllocatedSeat.PlayerExitBounds_force();
            local_AllocatedSeat.startSeating(VehicleID);
        }
        else
        {
            //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(checkAllPlayerAllocation));
        }
    }
    public void Exit(int VehicleID)
    {
        if (local_AllocatedSeat != null)
        {
            local_AllocatedSeat.PlayerExitBounds(VehicleID);
        }
    }
    public void allocateSeatToLocal()
    {
        local_AllocatedSeat = allocateSeat(Networking.LocalPlayer);
    }
    public FloorStationController allocateSeat(VRCPlayerApi playerApi)
    {
        Debug.Log("allocating to ID " + playerApi.playerId);
        FloorStationController foundSeat = null;
        bool found = false;
        for (int id = 0; id < preset_inVehicleStations.Length; id++)
        {
            FloorStationController movableSeat = preset_inVehicleStations[id];
            Debug.Log("currentSeat_PlayerID " + movableSeat.AllocatePlayer);
            if (!Networking.IsObjectReady(movableSeat.gameObject))
            {
                SendCustomEventDelayedSeconds(nameof(allocateSeatToLocal), 3);
                return null;
            }
            if (movableSeat.AllocatePlayer == playerApi.playerId)
            {
                if (!found)
                {
                    foundSeat = movableSeat;
                    found = true;
                }
                else
                {
                    movableSeat.AllocatePlayer = -1;
                }
            }
        }
        if (!found && playerApi.isLocal)
        {
            for (int id = 0; id < preset_inVehicleStations.Length; id++)
            {
                FloorStationController movableSeat = preset_inVehicleStations[id];
                Debug.Log("currentSeatID " + movableSeat.AllocatePlayer);
                Debug.Log("Checking_IsNull " + VRCPlayerApi.GetPlayerById(movableSeat.AllocatePlayer));
                if(VRCPlayerApi.GetPlayerById(movableSeat.AllocatePlayer) != null) Debug.Log("Checking_IsValid " + VRCPlayerApi.GetPlayerById(movableSeat.AllocatePlayer).IsValid());
                if (!found && (VRCPlayerApi.GetPlayerById(movableSeat.AllocatePlayer) == null || !VRCPlayerApi.GetPlayerById(movableSeat.AllocatePlayer).IsValid()))
                {
                    foundSeat = movableSeat;
                    Networking.SetOwner(playerApi, foundSeat.gameObject);
                    foundSeat.AllocatePlayer = playerApi.playerId;
                    foundSeat.RequestSerialization();
                    found = true;
                    break;
                }
            }
        }
        return foundSeat;
    }

    public void changeStationFallback()
    {
        if (local_AllocatedSeat == null)
        {
            local_AllocatedSeat = allocateSeat(Networking.LocalPlayer);
        }
        local_AllocatedSeat.changeStationFallback();
    }

    public override void OnPlayerJoined(VRCPlayerApi playerApi)
    {
        if (Networking.LocalPlayer.isMaster)
        {
            SendCustomEventDelayedSeconds(nameof(allocateSeatToLocal), 3);
            RequestSerialization();
        }
    }

    public override void OnDeserialization()
    {
        if(local_AllocatedSeat == null) SendCustomEventDelayedSeconds(nameof(allocateSeatToLocal), 1f);
    }


    public override void OnPlayerLeft(VRCPlayerApi playerApi)
    {
        if (local_AllocatedSeat != null && Networking.IsOwner(local_AllocatedSeat.gameObject))
        {
            local_AllocatedSeat.AllocatePlayer = -1;
            local_AllocatedSeat.RequestSerialization();
        }
    }


}
