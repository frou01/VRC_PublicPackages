
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BogieRotationConstraint : UdonSharpBehaviour
{
    Quaternion sourceLocalRotation;
    [SerializeField] Quaternion initialLocalRotation;
    Transform applyTargetParent;
    [SerializeField] Transform applyTarget;
    [SerializeField] Transform sourceTarget;

    void Start()
    {
        if (applyTarget != null) applyTarget = transform;
        applyTargetParent = applyTarget.parent;
        initialLocalRotation = applyTarget.localRotation;
    }
    private void LateUpdate()
    {
        if (applyTargetParent == null) return;
        sourceLocalRotation = Quaternion.Inverse(applyTargetParent.rotation) * sourceTarget.rotation;
        applyTarget.localRotation = sourceLocalRotation;
        if (Quaternion.Angle(initialLocalRotation, sourceLocalRotation) > 90)
        {
            applyTarget.Rotate(Vector3.up, 180);
        }
    }
}
