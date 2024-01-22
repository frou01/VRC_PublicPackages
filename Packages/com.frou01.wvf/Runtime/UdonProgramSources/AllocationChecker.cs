
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AllocationChecker : UdonSharpBehaviour
{
    public VehicleIsideSeatMNG VISMNG;

    //call checkAccloation
    //if checking = false, checking start, checking = true
    //checkOwnerTransfered = false
    //request change owner
    //delay call tryCheckEventSend
    //On Ownership Transferred , checkOwnerTransfered = true
    //on tryCheckEventSend, if checkOwnerTransfered = true, call checkEvent and checking = false , else delay call tryCheckEventSend

    void Start()
    {
        
    }

    bool checking = false;
    bool checkOwnerTransfered = false;
    public void checkAccloation()
    {
        if (!checking)
        {
            //startChecking;
            Debug.Log("startChecking");
            checking = true;
            checkOwnerTransfered = false;
            if (!Networking.IsOwner(this.gameObject)) Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            else checkOwnerTransfered = true;
            SendCustomEventDelayedSeconds(nameof(tryCheckEventSend), 1);
        }
        else if (!Networking.IsOwner(this.gameObject)) {
            Debug.Log("error on transfer owner");
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        }
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        if (checking && player == Networking.LocalPlayer)
        {
            Debug.Log("ownertransfer ok");
            checkOwnerTransfered = true;
        }
    }

    public void tryCheckEventSend()
    {
        Debug.Log("local preparation check");
        if (!checking) return;
        if (checkOwnerTransfered)
        {
            Debug.Log("request allocation check");
            VISMNG.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(VISMNG.requestedCheckOwner));
            checking = false;
            checkOwnerTransfered = false;
        }
        else
        {
            Debug.Log("wait transfer owner");
            SendCustomEventDelayedSeconds(nameof(tryCheckEventSend), 1);
        }
    }
}
