
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace frou01.RigidBodyTrain
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ObjectTrigger_cull : UdonSharpBehaviour
    {
        [SerializeField] GameObject targetObject;
        [SerializeField] GameObject[] targetObjects = new GameObject[0];
        void Start()
        {
            if(targetObject != null) targetObject.SetActive(false);
            foreach (GameObject go in targetObjects)
            {
                go.SetActive(false);
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerChaser>() != null)
            {
                if (targetObject != null) targetObject.SetActive(true);
                foreach (GameObject go in targetObjects)
                {
                    go.SetActive(true);
                }
            }
        }


        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<PlayerChaser>() != null)
            {
                if (targetObject != null) targetObject.SetActive(false);
                foreach (GameObject go in targetObjects)
                {
                    go.SetActive(false);
                }
            }
        }
    }
}
