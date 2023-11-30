
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class CatchCollider_Vehicle : UdonSharpBehaviour
{
    [System.NonSerialized]public VehicleIsideSeatMNG local_SeatMNG;

    public GameObject vehicleObject;
    public GameObject inVehicleCollider;

    public bool autoCatch = true;

    [System.NonSerialized]public int local_Id_OnSeatMNG;
    void Start()
    {
        DisableInteractive = true;
    }


    public override void Interact() {
        //Debug.Log("Player " + Networking.LocalPlayer.displayName + " Enter Vehicle " + local_Id_OnSeatMNG);
        local_SeatMNG.InteractedOnVehicle(local_Id_OnSeatMNG);
        DisableInteractive = true;
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerChaser>() != null)
        {
            DisableInteractive = false;
            if (autoCatch)
            {
                //Debug.Log("Player " + Networking.LocalPlayer.displayName + " Enter Vehicle " + local_Id_OnSeatMNG);
                local_SeatMNG.EnterOnVehicle(local_Id_OnSeatMNG);
            }
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerChaser>() != null)
        {
            //Debug.Log("Player " + Networking.LocalPlayer.displayName + " Exit Vehicle " + local_Id_OnSeatMNG);
            local_SeatMNG.Exit(local_Id_OnSeatMNG);
            DisableInteractive = true;
        }
    }
}
