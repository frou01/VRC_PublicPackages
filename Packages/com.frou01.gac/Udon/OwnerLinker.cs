
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace frou01.GrabController
{
    public class OwnerLinker : UdonSharpBehaviour
    {
        [Header("アタッチされたオブジェクトのオーナーが移った際、以下に設定されたオブジェクトのオーナーも同時に置き換えるUDONです")]
        public GameObject[] objects;
        //[Header("拒否/許可を応答するUdon")]
        //public UdonBehaviour[] Udons;
        public override void OnOwnershipTransferred(VRC.SDKBase.VRCPlayerApi player)
        {
            if (Networking.LocalPlayer != player) return;

            //for (int i = 0; i < Udons.Length; i++)
            //{
            //    Udons[i].SendCustomEvent("OwnerShipLinkTransfer");
            //    if ((bool)Udons[i].GetProgramVariable("CanTransferOwner"))
            //    {
            //        Udons[i].SetProgramVariable("CanTransferOwner", false);//setToDefault
            //    }
            //    else
            //    {
            //        return;//cancel
            //    }
            //}

                for (int i = 0; i < objects.Length; i++)
            {
                Networking.SetOwner(player, objects[i]);
            }
        }
    }
}
