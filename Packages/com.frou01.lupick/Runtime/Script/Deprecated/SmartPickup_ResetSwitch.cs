
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]

public class SmartPickup_ResetSwitch : UdonSharpBehaviour
{
    [Header("SmartPickupSharpのあるオブジェクトを以下に登録してください")]
    [Header("Add objects with SmartPickupSharp to the following")]
    public GameObject[] SmartPickupObj;
    void Start()
    {
        
    }

    public override void Interact()
    {
        RESETALL();
    }

    public void RESETALL(){
        if(SmartPickupObj != null){
            for(int i=0; i < SmartPickupObj.Length; i++){
                UdonBehaviour udb = null;
                if(SmartPickupObj[i] != null) udb= (UdonBehaviour)SmartPickupObj[i].GetComponent(typeof(UdonBehaviour));
                if(udb != null)udb.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,"ResetPosition");
            }
        }
        
    }
}
