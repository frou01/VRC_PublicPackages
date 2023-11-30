
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ColliderOcclusionPortal : UdonSharpBehaviour
{
    void Start()
    {
        
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerChaser>() != null)
        {
            GetComponent<OcclusionPortal>().open = true;
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerChaser>() != null)
        {
            GetComponent<OcclusionPortal>().open = false;
        }
    }
}
