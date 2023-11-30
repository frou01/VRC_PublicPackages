
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WakableFloorSDK2FallBack : UdonSharpBehaviour
{
    public VehicleIsideSeatMNG preset_SeatMNG;
    void Start()
    {
        
    }

    public override void Interact()
    {
        preset_SeatMNG.changeStationFallback();
        InteractionText = preset_SeatMNG.local_AllocatedSeat.SDK2Fallback ? "Change To SDK3Mode": "Change To SDK2Mode";
    }
}
