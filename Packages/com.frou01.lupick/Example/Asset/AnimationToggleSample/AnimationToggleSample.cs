
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]

public class AnimationToggleSample : UdonSharpBehaviour
{
    public Animator anim;
    [UdonSynced, FieldChangeCallback(nameof(SyncedToggle))]
    private bool _syncedToggle;

    public bool SyncedToggle
    {
        set
        {
            _syncedToggle = value;
            anim.SetBool("toggle",value);
        }
        get => _syncedToggle;
    }

    public void UseDown()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        SyncedToggle = !SyncedToggle;
        RequestSerialization();
    }


}
