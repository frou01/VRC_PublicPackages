using System.Collections;
using System.Collections.Generic;
using VRC.SDK3.Components;
using UnityEngine;
using VRC.SDKBase.Network;
using VRC.Udon;
using static VRC.SDKBase.Networking;
#if (UNITY_EDITOR) 

public class NoneSyncexcluder : MonoBehaviour
{
    public VRCSceneDescriptor target;

    public void Detach()
    {
        List<NetworkIDPair> CheckingList = target.NetworkIDCollection;
        List<NetworkIDPair> CheckedList = new List<NetworkIDPair>();
        string udonbe = "VRC.Udon.UdonBehaviour";
        string pickup = "VRC.SDK3.Components.VRCPickup";
        bool OK = false;
        foreach (NetworkIDPair pair in CheckingList)
        {
            OK = false;
            if (pair.gameObject)
            {

                foreach (string anPath in pair.SerializedTypeNames)
                {
                    if (!anPath.Equals(udonbe) && !anPath.Equals(pickup))
                    {
                        OK = true;
                        break;
                    }
                }
                if (OK)
                {
                    pair.ID = 10 + CheckedList.Count;
                    CheckedList.Add(pair);
                    continue;
                }

                UdonBehaviour[] Udons = pair.gameObject.GetComponents<UdonBehaviour>();
                foreach (UdonBehaviour anUdon in Udons)
                {
                    if (anUdon.SyncMethod != SyncType.None)
                    {
                        OK = true;
                    }
                }
                if (OK)
                {
                    pair.ID = 10 + CheckedList.Count;
                    CheckedList.Add(pair);
                    continue;
                }

                if (!OK)
                {
                    Debug.Log("NoneSync, but assigned " + pair.gameObject.name);
                }
            }
        }
        target.NetworkIDCollection = CheckedList;
    }
}

#endif