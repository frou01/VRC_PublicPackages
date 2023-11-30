
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ColliderGameObjectCuller : UdonSharpBehaviour
{
    public GameObject[] objects;
    public bool isStaticMode;
    void Start()
    {
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PlayerChaser>() != null)
        {
            foreach (GameObject go in objects)
            {
                go.SetActive(true);
            }
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerChaser>() != null)
        {
            foreach (GameObject go in objects)
            {
                go.SetActive(false);
            }
        }
    }
}
