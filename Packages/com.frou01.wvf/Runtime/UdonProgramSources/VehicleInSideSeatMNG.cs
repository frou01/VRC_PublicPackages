
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class VehicleInSideSeatMNG : UdonSharpBehaviour
{

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
        if (local_AllocatedSeat != null)
        {
            Debug.Log("Allocated Seat is " + local_AllocatedSeat.gameObject.name);
            local_AllocatedSeat.startSeating(VehicleID);
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

    public FloorStationController allocateSeat(VRCPlayerApi playerApi)
    {
        Debug.Log("allocating to ID " + playerApi.playerId);
        FloorStationController movableSeat;
        for (int id = 0; id < preset_inVehicleStations.Length; id++)
        {
            movableSeat = preset_inVehicleStations[id];
            Debug.Log("currentSeatID " + movableSeat.AllocatePlayer);
            if (movableSeat.AllocatePlayer == playerApi.playerId)
            {
                local_AllocatedSeat = movableSeat;
                break;
            }
            else
            if (VRCPlayerApi.GetPlayerById(movableSeat.AllocatePlayer) == null || !VRCPlayerApi.GetPlayerById(movableSeat.AllocatePlayer).IsValid())
            {
                local_AllocatedSeat = movableSeat;
                local_AllocatedSeat.AllocatePlayer = playerApi.playerId;
                break;
            }
        }
        return local_AllocatedSeat;
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
        if (Networking.LocalPlayer == playerApi)
            if (Networking.LocalPlayer.isMaster)
            {
                local_AllocatedSeat = allocateSeat(playerApi);
            }
    }


}
