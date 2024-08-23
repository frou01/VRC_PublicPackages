
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class VehicleIsideSeatMNG : UdonSharpBehaviour
{

    [System.NonSerialized]public FloorStation local_AllocatedSeat;
    public GameObject[] preset_inVehicleCollider;

    public FloorStation[] preset_inVehicleStations;

    public AllocationChecker preset_allocationChecker;

    public CatchCollider_Vehicle[] preset_CatchColliders;

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
            local_AllocatedSeat = GetSeat(Networking.LocalPlayer);
        }
        if (local_AllocatedSeat != null)
        {
            Debug.Log("Allocated Seat is " + local_AllocatedSeat.gameObject.name);
            FloorStationController floorStationController = local_AllocatedSeat.preset_StationMover;
            if (!local_AllocatedSeat.preset_StationMover.synced_Using)//disable on auto transfer on riding
            {
                preset_CatchColliders[VehicleID].DisableInteractive = true;
                floorStationController.startSeating(preset_CatchColliders[VehicleID], VehicleID);
            }
        }
        else
        {
            Debug.Log("No Allocated!");
            preset_allocationChecker.checkAccloation();
            //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(checkAllPlayerAllocation));
        }
    }
    public void InteractedOnVehicle(int VehicleID)
    {
        if (local_AllocatedSeat == null)
        {
            local_AllocatedSeat = GetSeat(Networking.LocalPlayer);
        }
        //Debug.Log("Allocated Seat is " + local_AllocatedSeat.gameObject.name);
        if (local_AllocatedSeat != null)
        {
            local_AllocatedSeat.preset_StationMover.PlayerExitBounds_force();
            local_AllocatedSeat.preset_StationMover.startSeating(preset_CatchColliders[VehicleID], VehicleID);
        }
        else
        {
            Debug.Log("No Allocated!");
            preset_allocationChecker.checkAccloation();
            //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(checkAllPlayerAllocation));
        }
    }
    public void Exit(int VehicleID)
    {
        if (local_AllocatedSeat != null)
        {
            local_AllocatedSeat.preset_StationMover.PlayerExitBounds(VehicleID);
        }
    }

    public FloorStation GetSeat(VRCPlayerApi playerApi)
    {
        Debug.Log("Searching allocated Seat " + playerApi.displayName + " , " + playerApi.playerId);
        FloorStation movableSeat;
        for (int id = 0; id < preset_inVehicleStations.Length; id++)
        {
            movableSeat = preset_inVehicleStations[id];
            if (movableSeat.AllocatePlayer == playerApi.playerId)
            {
                return movableSeat;
            }
        }
        Debug.Log("no Allocated Seat. something Went Wrong " + playerApi.displayName);
        return null;
    }

    public void changeStationFallback()
    {
        if (local_AllocatedSeat == null)
        {
            local_AllocatedSeat = GetSeat(Networking.LocalPlayer);
        }
        local_AllocatedSeat.changeStationFallback();
    }

    public override void OnPlayerJoined(VRCPlayerApi playerApi)
    {
        if (Networking.LocalPlayer != null)
            if (Networking.LocalPlayer.isMaster)
            {
                Debug.Log("new Player Join. Allocation seat for " + playerApi.displayName + " , " + playerApi.playerId);
                allocateSeat(playerApi);
            }
    }

    public void allocateSeat(VRCPlayerApi playerApi)
    {
        FloorStation movableSeat;
        for (int id = 0; id < preset_inVehicleStations.Length; id++)
        {
            movableSeat = preset_inVehicleStations[id];
            if (!movableSeat.Allocated)
            {
                if (Networking.GetOwner(movableSeat.gameObject) != Networking.LocalPlayer)
                {
                    Networking.SetOwner(Networking.LocalPlayer, movableSeat.gameObject); ;
                }
                Debug.Log("Allocate " + id + " , " + playerApi.playerId);
                movableSeat.Allocate(playerApi);
                return;
            }
        }
        Debug.Log("overFlow!");
    }

    public void requestedCheckOwner()
    {
        if (!Networking.IsOwner(gameObject)) return;
        VRCPlayerApi requestedPlayer = Networking.GetOwner(preset_allocationChecker.gameObject);
        Debug.Log("requested check allocation for " + requestedPlayer.displayName + " , " + requestedPlayer.playerId);

        FloorStation requestedStation = GetSeat(requestedPlayer);
        if(requestedStation != null)
        {
            Debug.Log("found allocated station." + requestedStation.name);
            requestedStation.RequestSerialization();
        }
        else
        {
            Debug.Log("no allocation. reallocating " + requestedPlayer.displayName + " , " + requestedPlayer.playerId);
            allocateSeat(requestedPlayer);
        }

    }
    public void checkAllPlayerAllocation()
    {
        VRCPlayerApi[] allPlayer = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        allPlayer = VRCPlayerApi.GetPlayers(allPlayer);
        bool found = false;
        for (int indexPlayer = 0; indexPlayer < allPlayer.Length; indexPlayer++)
        {
            for (int indexSeat = 0; indexSeat < preset_inVehicleStations.Length; indexSeat++)
            {
                if (preset_inVehicleStations[indexSeat].AllocatePlayer == allPlayer[indexPlayer].playerId)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Debug.Log("Found no allocated Player!");
                this.allocateSeat(allPlayer[indexPlayer]);
            }
            found = false;
        }
    }
    public override void OnPlayerLeft(VRCPlayerApi playerApi)
    {
        if (Networking.LocalPlayer != null)
            if (Networking.LocalPlayer.isMaster)
            {
                Debug.Log("an player Left. prepare allocation seat " + playerApi.displayName + " " + playerApi.playerId);
                FloorStation movableSeat;
                VRCPlayerApi[] allPlayer = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
                allPlayer = VRCPlayerApi.GetPlayers(allPlayer);
                for (int id = 0; id < preset_inVehicleStations.Length; id++)
                {
                    movableSeat = preset_inVehicleStations[id];
                    if(playerApi.playerId == movableSeat.AllocatePlayer)
                    {
                        movableSeat.deAllocate();
                        Debug.Log("deAllocate " + id + " , " + playerApi.playerId);
                    }
                    if (movableSeat.Allocated)
                    {
                        //Debug.Log("CheckAllocation " + id);
                        movableSeat.CheckAllocation(allPlayer);
                    }
                }
            }
    }


}
