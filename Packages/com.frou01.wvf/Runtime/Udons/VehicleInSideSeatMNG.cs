
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class VehicleInSideSeatMNG : UdonSharpBehaviour
{
    [System.NonSerialized]public FloorStationController local_FoundStation;

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
        if (local_FoundStation == null)
        {
            local_FoundStation = findStation(Networking.LocalPlayer);
        }
        if (!local_FoundStation.synced_Using) local_FoundStation.startSeating(VehicleID);//auto enter only on not using
    }
    public void ForcedRidingOnVehicle(int VehicleID)
    {
        if (local_FoundStation == null)
        {
            local_FoundStation = findStation(Networking.LocalPlayer);
        }
        local_FoundStation.PlayerExitBounds_force();
        local_FoundStation.startSeating(VehicleID);
    }
    public void Exit(int VehicleID)
    {
        local_FoundStation.PlayerExitBounds(VehicleID);
    }
    public FloorStationController findStation(VRCPlayerApi playerApi)
    {
        //TODO Find localPlayer PlayerObject.FloorStationController Method
        var objects = Networking.GetPlayerObjects(playerApi);
        for (int i = 0; i < objects.Length; i++)
        {
            if (!Utilities.IsValid(objects[i])) continue;
            FloorStationController foundScript = objects[i].GetComponent<FloorStationController>();
            if (Utilities.IsValid(foundScript)) return foundScript;
        }
        Debug.LogError("Station Not Found");
        return null;
    }

    public void changeStationFallback()
    {
        if (local_FoundStation == null)
        {
            local_FoundStation = findStation(Networking.LocalPlayer);
        }
        local_FoundStation.changeStationFallback();
    }
}
