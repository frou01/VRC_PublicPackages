
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ColliderUdonCuller : UdonSharpBehaviour
{
    public GameObject playerChaser;

    public GameObject[] targetGameObject;
    public UdonBehaviour[] targetUdons;
    public void OnTriggerEnter(Collider other)
    {
        if (playerChaser == other.gameObject)
        {
            foreach (UdonBehaviour targetUdon in targetUdons)
            {
                targetUdon.enabled = true;
            }
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (playerChaser == other.gameObject)
        {
            foreach (UdonBehaviour targetUdon in targetUdons)
            {
                targetUdon.enabled = false;
            }
        }
    }
}
