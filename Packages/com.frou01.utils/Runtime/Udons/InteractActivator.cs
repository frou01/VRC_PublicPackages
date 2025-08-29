
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InteractActivator : UdonSharpBehaviour
{
    public Transform Head;
    public Transform handL;
    public Transform handR;

    [SerializeField] Collider[] colliders;
    [SerializeField] public VRC_Pickup[] pickups;
    [SerializeField] public UdonBehaviour[] udons;
    [SerializeField] float proximity;
    [SerializeField] Transform[] BaseTransforms = new Transform[0];
    public bool currentState;
    bool isVR;
    void Start()
    {
        if(colliders.Length <= 0 && pickups.Length <= 0 && udons.Length <= 0)
        {
            colliders = new Collider[1];
            colliders[0] = GetComponent<Collider>();
        }
        isVR = Networking.LocalPlayer.IsUserInVR();
        currentState = true;
        changeColliderState(false);
        if (!isVR)
        {
            foreach (Component com in colliders)
            {
                if (com.GetType() ==  typeof(SphereCollider)) ((SphereCollider)com).radius = proximity;
                if (com.GetType() == typeof(CapsuleCollider)) ((CapsuleCollider)com).radius = proximity;
            }
            foreach (Component com in pickups)
            {
                if (com.GetComponent<SphereCollider>()) com.GetComponent<SphereCollider>().radius = proximity;
                if (com.GetComponent<CapsuleCollider>()) com.GetComponent<CapsuleCollider>().radius = proximity;
            }
            foreach (Component com in udons)
            {
                if (com.GetComponent<SphereCollider>()) com.GetComponent<SphereCollider>().radius = proximity;
                if (com.GetComponent<CapsuleCollider>()) com.GetComponent<CapsuleCollider>().radius = proximity;
            }
            proximity = proximity + 2;
        }
        if (BaseTransforms.Length == 0)
        {
            BaseTransforms = new Transform[] {this.transform};
        }
    }
    Vector3 pos;
    bool nextState = false;
    public override void PostLateUpdate()
    {
        nextState = false;
        foreach (Transform baseTransform in BaseTransforms)
        {
            pos = baseTransform.position;
            if (!isVR)
            {
                if (Vector3.Distance(Head.position, pos) < proximity + 2)
                {
                    nextState = true;
                }
            }
            else
            {
                if (Vector3.Distance(handL.position, pos) < proximity ||
                    Vector3.Distance(handR.position, pos) < proximity)
                {
                    nextState = true;
                }
            }
        }
        changeColliderState(nextState);
    }
    public void changeColliderState(bool state)
    {
        if(currentState == state)
        {
            return;
        }
        //Debug.Log("ChangeTo " + state);
        currentState = state;
        foreach (Component com in colliders)
        {
            ((Collider)com).enabled = state;
        }
        foreach (Component com in pickups)
        {
            ((VRC_Pickup)com).pickupable = state;
        }
        foreach (Component com in udons)
        {
            ((UdonBehaviour)com).DisableInteractive = !state;
        }
    }

    public override void Interact()
    {
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (BaseTransforms.Length == 0)
        {
            pos = this.transform.position;
            Gizmos.DrawWireSphere(pos, proximity);
            if (colliders.Length <= 0 && pickups.Length <= 0 && udons.Length <= 0)
            {
                Gizmos.color = new Color(1, 0, 0, 0.4f);

                Gizmos.DrawSphere(pos, proximity);
            }
            foreach (Component com in colliders)
            {
                Gizmos.DrawLine(pos, com.transform.position);
            }
            foreach (Component com in pickups)
            {
                Gizmos.DrawLine(pos, com.transform.position);
            }
            foreach (Component com in udons)
            {
                Gizmos.DrawLine(pos, com.transform.position);
            }
        }
        else
        {
            foreach (Transform baseTransform in BaseTransforms)
            {
                pos = baseTransform.position;
                Gizmos.DrawWireSphere(pos, proximity);
                if (colliders.Length <= 0 && pickups.Length <= 0 && udons.Length <= 0)
                {
                    Gizmos.color = new Color(1, 0, 0, 0.4f);

                    Gizmos.DrawSphere(pos, proximity);
                }
                foreach (Component com in colliders)
                {
                    Gizmos.DrawLine(pos, com.transform.position);
                }
                foreach (Component com in pickups)
                {
                    Gizmos.DrawLine(pos, com.transform.position);
                }
                foreach (Component com in udons)
                {
                    Gizmos.DrawLine(pos, com.transform.position);
                }
            }
        }
    }
}
