
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UdonParentConstraint : UdonSharpBehaviour
{
    private Transform defaultParent;

    public Transform target;
    
    void Start()
    {
        defaultParent = transform.parent;
        this.enabled = false;
    }

    public void Activate()
    {
        this.transform.parent = target;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}
