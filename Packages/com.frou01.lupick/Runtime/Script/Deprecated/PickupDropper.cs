
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

public class PickupDropper : UdonSharpBehaviour
{
    [SerializeField] public VRCObjectPool ObjectPool;
    void Start()
    {
        
    }

    public override void Interact()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        Networking.SetOwner(Networking.LocalPlayer, ObjectPool.gameObject);
        ObjectPool.TryToSpawn();
    }

    public void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.GetComponent<PlayerLocalPickup>() != null) col.gameObject.GetComponent<PlayerLocalPickup>().returnToPool(this);
    }
}
