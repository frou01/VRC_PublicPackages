
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class SPSResetter : UdonSharpBehaviour
{
    [SerializeField]UdonBehaviour target;
    [SerializeField]UdonBehaviour[] targets;
    void Start()
    {
        
    }
    public override void Interact() {
        this.Perform();
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Perform));
    }
    public void Perform()
    {
        //target.gameObject.SetActive(true);
        if(target != null)
        {
            if (target.gameObject.GetComponent<SmartPickupSharpRootChangeable>() != null)
                target.gameObject.GetComponent<SmartPickupSharpRootChangeable>().setIsEnable(true);
            target.SendCustomEvent(nameof(SmartPickupSharpRootChangeable.ResetPosition));
        }

        foreach (UdonBehaviour anTarget in targets)
        {
            if (anTarget.gameObject.GetComponent<SmartPickupSharpRootChangeable>() != null)
                anTarget.gameObject.GetComponent<SmartPickupSharpRootChangeable>().setIsEnable(true);
            anTarget.SendCustomEvent(nameof(SmartPickupSharpRootChangeable.ResetPosition));
        }
    }
}
