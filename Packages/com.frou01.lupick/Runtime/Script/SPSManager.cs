
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SPSManager : UdonSharpBehaviour
{
    public SPSCatcher[] SPSCatchers;

    public void setCatchers(SPSCatcher[] spsCatchers)
    {
        SPSCatchers = new SPSCatcher[spsCatchers.Length];
        for(int cnt = 0; cnt < SPSCatchers.Length; cnt++)
        {
            SPSCatchers[cnt] = spsCatchers[cnt];
        }
        Debug.Log("" + SPSCatchers.Length);
    }
}
