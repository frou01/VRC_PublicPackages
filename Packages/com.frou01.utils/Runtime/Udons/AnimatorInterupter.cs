
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AnimatorInterupter : UdonSharpBehaviour
{
    [UdonSynced] bool interrupt = false;
    [SerializeField] UdonBehaviour[] targetUdons;
    void Start()
    {

    }

    public void interrputionEventToggleTrue()
    {
        if(Networking.IsOwner(gameObject) && !interrupt)
        {
            for (int id = 0; id < targetUdons.Length; id++)
            {
                targetUdons[id].SendCustomEvent("Interruption_True");
            }
            interrupt = true;
            RequestSerialization();
        }
    }
    public void interrputionEventToggleFalse()
    {
        if (Networking.IsOwner(gameObject) && interrupt)
        {
            for (int id = 0; id < targetUdons.Length; id++)
            {
                targetUdons[id].SendCustomEvent("Interruption_False");
            }
            interrupt = false;
            RequestSerialization();
        }
    }
    public override void OnDeserialization()
    {
        if (interrupt)
        {
            for (int id = 0; id < targetUdons.Length; id++)
            {
                targetUdons[id].SendCustomEvent("Interruption_True");
            }
        }
        else
        {
            for (int id = 0; id < targetUdons.Length; id++)
            {
                targetUdons[id].SendCustomEvent("Interruption_False");
            }
        }
    }
}
