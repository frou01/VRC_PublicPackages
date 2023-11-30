
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InteractActivator : UdonSharpBehaviour
{
    PlayerChaser playerChaser;

    [System.NonSerialized] public Transform handL;
    [System.NonSerialized] public Transform handR;

    [SerializeField]Collider[] colliders;
    [SerializeField] VRC_Pickup[] pickups;
    [SerializeField] UdonBehaviour[] udons;
    [SerializeField] float proximity;
    bool currentState;
    void Start()
    {
        if(colliders.Length <= 0 && pickups.Length <= 0 && udons.Length <= 0)
        {
            colliders = new Collider[1];
            colliders[0] = GetComponent<Collider>();
        }
        if (Networking.LocalPlayer.IsUserInVR())
        {
            currentState = true;
            changeColliderState(false);
        }
        else
        {
            this.enabled = false;
        }
    }
    public void Update()
    {
        Vector3 pos = transform.position;
        if (Vector3.Distance(handL.position, pos) < proximity ||
            Vector3.Distance(handR.position, pos) < proximity)
        {
            changeColliderState(true);
        }
        else
        {
            changeColliderState(false);
        }
    }
    void changeColliderState(bool state)
    {
        if(currentState == state)
        {
            return;
        }
        Debug.Log("ChangeTo " + state);
        currentState = state;
        foreach (Component com in colliders)
        {
            ((Collider)com).enabled = state;
        }
        foreach (Component com in pickups)
        {
            ((VRC_Pickup)com).pickupable = state;
        }
        foreach (Component com in udons)
        {
            ((UdonBehaviour)com).DisableInteractive = !state;
        }
    }
}
