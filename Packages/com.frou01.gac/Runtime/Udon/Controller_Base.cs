
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class Controller_Base : UdonSharpBehaviour
{
    [Header("コントローラーの回転表示オブジェクト")]
    public Transform controllerTransform;
    protected Transform ControllerRoot;
    protected Transform cachedTransform;

    [Header("制御対象のアニメーター")]
    public Animator TargetAnimator;
    public Animator[] MultiTargetAnimators;
    [Header("を使用します")]
    [Header("(設定名)_segment")]
    [Header("(設定名)_normpos")]
    [Header("(設定名)_position")]
    [Header("入力先のパラメーター")]
    public string paramaterName;

    [Header("最初と最後は回転角度制限になります　2つは必ず設定してください")]
    [Header("コントローラーの切れ目部分（跨ぐ際に振動が発生します）")]
    public float[] segment_points;
    public float[] snap_points;

    public bool useHaptic;

    public bool autoDisable;
    public bool ForceAutoDisable;
    public float autoDisableTime;
    float fromActiveTime;

    protected int positionParamaterID;
    protected int normalizedPositionParamaterID;
    protected int segmentsParamaterID;
    bool hasPosition;
    bool hasNormalizedPosition;
    bool hasSegments;


    protected bool onPick;
    [UdonSynced] bool isPicked;
    protected VRC_Pickup pickup;

    protected Vector3 originPos;
    protected Quaternion originRot;
    protected float position_OnPick;
    protected Vector3 localHandPosition_OnPick;
    protected Quaternion localHandRotation_OnPick;
    protected Vector3 localHandPosition;
    protected Quaternion localHandRotation;

    protected VRCPlayerApi localPlayer;


    [UdonSynced] public int currentSegment;
    int prevSegment;

    [UdonSynced] float currentNormalizePosition;
    float prevNormalizePosition;

    float SyncedControllerPosition;

    [Header("VRC上でもアニメーターからの変更で回転させることができます")]
    [Header("デバッグ時はアニメーターの(設定名)_rotationを変更することで確認できます")]
    [UdonSynced(UdonSyncMode.Linear)] public float controllerPosition;
    float prevControllerPosition;

    private bool isAnimatorControllPosition;

    protected bool netWork_Updating;
    private float SyncInterval = 10;
    private float SinceLastRequest;

    bool Local_isUpdated;
    bool isowner;
    bool positionUpdated;
    bool hasSegmentArray;
    protected VRCPlayerApi.TrackingData trackingData;

    [System.NonSerialized]public bool locked;

    void Start()
    {
        cachedTransform = transform;
        localPlayer = Networking.LocalPlayer;
        originPos = transform.localPosition;
        originRot = transform.localRotation;
        pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        positionParamaterID = Animator.StringToHash(paramaterName + "_position");
        normalizedPositionParamaterID = Animator.StringToHash(paramaterName + "_normpos");
        segmentsParamaterID = Animator.StringToHash(paramaterName + "_segment");
        hasPosition = HasParameter(positionParamaterID, TargetAnimator);
        hasNormalizedPosition = HasParameter(normalizedPositionParamaterID, TargetAnimator);
        hasSegments = HasParameter(segmentsParamaterID, TargetAnimator);
        if (ControllerRoot == null) ControllerRoot = controllerTransform.parent;
        SyncInterval = Random.Range(0.1f, 0.2f);
        isAnimatorControllPosition = TargetAnimator.IsParameterControlledByCurve(positionParamaterID);
        pickup.InteractionText = InteractionText;



        if (hasPosition && controllerPosition != TargetAnimator.GetFloat(positionParamaterID))
        {
            controllerPosition = TargetAnimator.GetFloat(positionParamaterID);
        }
        hasSegmentArray = segment_points.Length >= 2;
        if (hasSegmentArray)
        {
            float leverPosition_temp = controllerPosition;
            prevNormalizePosition = currentNormalizePosition;
            if (segment_points[currentSegment] > leverPosition_temp)
            {
                if (currentSegment > 0)
                {
                    if (isowner) currentSegment--;
                }
                else
                {
                    leverPosition_temp = segment_points[currentSegment];
                }
            }
            if (segment_points[currentSegment + 1] < leverPosition_temp)
            {
                if (currentSegment + 2 < segment_points.Length)
                {
                    if (isowner) currentSegment++;
                }
                else
                {
                    leverPosition_temp = segment_points[currentSegment + 1];
                }
            }
            foreach(float snap_point in snap_points)
            {
                if(segment_points[currentSegment] <= snap_point && snap_point <= segment_points[currentSegment + 1])
                {
                    leverPosition_temp = snap_point;
                }
            }
            currentNormalizePosition = (leverPosition_temp - segment_points[currentSegment]) / (segment_points[currentSegment + 1] - segment_points[currentSegment]);
            if (hasNormalizedPosition)
            {
                TargetAnimator.SetFloat(normalizedPositionParamaterID, currentNormalizePosition);
                foreach(Animator Ananimator in MultiTargetAnimators) Ananimator.SetFloat(normalizedPositionParamaterID, currentNormalizePosition);
                Local_isUpdated = true;
            }
            if (hasSegments)
            {
                TargetAnimator.SetInteger(segmentsParamaterID, currentSegment);
                foreach (Animator Ananimator in MultiTargetAnimators) Ananimator.SetInteger(segmentsParamaterID, currentSegment);
                Local_isUpdated = true;
            }
            if (isowner) controllerPosition = leverPosition_temp;
        }
        autoDisable &= ForceAutoDisable || !isAnimatorControllPosition;
        if (autoDisable) disableThis();
        prevSegment = currentSegment;
        isowner = Networking.IsOwner(gameObject);
        ApplyToTransform();
    }
    static bool HasParameter(int paramHash, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.nameHash == paramHash)
                return true;
        }
        return false;
    }
    public override void OnPickup()
    {
        if (!locked)
        {
            this.enabled = true;
            onPick = true;
            isPicked = true;
        }
    }
    public override void OnDrop()
    {
        isPicked = false;
        transform.localPosition = originPos;
        transform.localRotation = originRot;
        RequestSerialization();
    }

    public void Update()
    {
        cachedTransform.localPosition = originPos;
    }
    public void LateUpdate()
    {
        //if (localPlayer == null) return;

        if (isowner)
        {
            if (SyncedControllerPosition != controllerPosition)
            {
                netWork_Updating = true;
            }
            if (prevControllerPosition != controllerPosition)
            {
                netWork_Updating = true;
            }
            if (hasPosition && controllerPosition != TargetAnimator.GetFloat(positionParamaterID))
            {
                controllerPosition = TargetAnimator.GetFloat(positionParamaterID);
            }
            if (netWork_Updating) SinceLastRequest += Time.deltaTime;
            if (SinceLastRequest > SyncInterval)
            {
                SinceLastRequest = 0;
                netWork_Updating = false;
                RequestSerialization();
            }
            if (locked)
            {
                isPicked = false;
                onPick = false;
            }
        }
        if (isPicked)
        {
            if (isowner) onPicked();
        }
        else if (!isowner)
        {
            if (hasPosition && controllerPosition != TargetAnimator.GetFloat(positionParamaterID))
            {
                controllerPosition = TargetAnimator.GetFloat(positionParamaterID);
            }
        }
        positionUpdated = prevControllerPosition != controllerPosition;
        if (positionUpdated || currentSegment != prevSegment)
        {
            if (hasSegmentArray)
            {
                float leverPosition_temp = controllerPosition;
                prevNormalizePosition = currentNormalizePosition;
                if (segment_points[currentSegment] > leverPosition_temp)
                {
                    if (currentSegment > 0)
                    {
                        if (isowner) currentSegment--;
                    }
                    else
                    {
                        leverPosition_temp = segment_points[currentSegment];
                    }
                    if (isPicked && useHaptic) localPlayer.PlayHapticEventInHand(pickup.currentHand, 0.1f, 1f, 0.5f);
                }
                if (segment_points[currentSegment + 1] < leverPosition_temp)
                {
                    if (currentSegment + 2 < segment_points.Length)
                    {
                        if (isowner) currentSegment++;
                    }
                    else
                    {
                        leverPosition_temp = segment_points[currentSegment + 1];
                    }
                    if (isPicked && useHaptic) localPlayer.PlayHapticEventInHand(pickup.currentHand, 0.1f, 1f, 0.5f);
                }

                float nearest = 360;
                float currentDist;
                foreach (float snap_point in snap_points)
                {
                    if (segment_points[currentSegment] < snap_point && snap_point < segment_points[currentSegment + 1])
                    {
                        currentDist = Mathf.Abs(wrapAngleTo180(nearest - leverPosition_temp));
                        if (currentDist < nearest)
                        {
                            leverPosition_temp = snap_point;
                            nearest = currentDist;
                        }
                    }
                }
                currentNormalizePosition = (leverPosition_temp - segment_points[currentSegment]) / (segment_points[currentSegment + 1] - segment_points[currentSegment]);
                if (currentNormalizePosition != prevNormalizePosition && hasNormalizedPosition)
                {
                    TargetAnimator.SetFloat(normalizedPositionParamaterID, currentNormalizePosition);
                    foreach (Animator Ananimator in MultiTargetAnimators) Ananimator.SetFloat(normalizedPositionParamaterID, currentNormalizePosition);
                    Local_isUpdated = true;
                }
                if (currentSegment != prevSegment && hasSegments)
                {
                    TargetAnimator.SetInteger(segmentsParamaterID, currentSegment);
                    foreach (Animator Ananimator in MultiTargetAnimators) Ananimator.SetInteger(segmentsParamaterID, currentSegment);
                    Local_isUpdated = true;
                }
                if (isowner) controllerPosition = leverPosition_temp;
            }
            prevSegment = currentSegment;
            ApplyToTransform();
        }
        Local_isUpdated |= positionUpdated = prevControllerPosition != controllerPosition;
        if (!isAnimatorControllPosition && positionUpdated && hasPosition)
        {
            TargetAnimator.SetFloat(positionParamaterID, controllerPosition);
            foreach (Animator Ananimator in MultiTargetAnimators) Ananimator.SetFloat(positionParamaterID, controllerPosition);
        }
        if (Local_isUpdated && !TargetAnimator.enabled)
        {
            Local_isUpdated = false;
            TargetAnimator.enabled = true;
            if (hasNormalizedPosition)
            {
                TargetAnimator.SetFloat(normalizedPositionParamaterID, currentNormalizePosition);
                foreach (Animator Ananimator in MultiTargetAnimators) Ananimator.SetFloat(normalizedPositionParamaterID, currentNormalizePosition);
            }
            if (hasSegments)
            {
                TargetAnimator.SetInteger(segmentsParamaterID, currentSegment);
                foreach (Animator Ananimator in MultiTargetAnimators) Ananimator.SetInteger(segmentsParamaterID, currentSegment);
            }
            if (hasPosition && !isAnimatorControllPosition)
            {
                TargetAnimator.SetFloat(positionParamaterID, controllerPosition);
                foreach (Animator Ananimator in MultiTargetAnimators) Ananimator.SetFloat(positionParamaterID, controllerPosition);
            }
        }
        prevControllerPosition = controllerPosition;
        cachedTransform.localPosition = originPos;
        transform.localRotation = originRot;

        if ((!isPicked || !isowner) && autoDisable) fromActiveTime += Time.deltaTime;
        if (autoDisable && !isPicked && fromActiveTime > autoDisableTime) disableThis();
    }
    private void disableThis()
    {
        this.enabled = false;
        fromActiveTime = 0;
    }

    public void SetPosition(float target)
    {
        if (Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        controllerPosition = target;
        if (hasSegmentArray)
        {
            prevNormalizePosition = currentNormalizePosition;
            while (true)
            {
                if (segment_points[currentSegment] > controllerPosition)
                {
                    if (currentSegment > 0)
                    {
                        if (isowner) currentSegment--;
                    }
                    else
                    {
                        controllerPosition = segment_points[currentSegment];
                        break;
                    }
                }else
                if (segment_points[currentSegment + 1] < controllerPosition)
                {
                    if (currentSegment + 2 < segment_points.Length)
                    {
                        if (isowner) currentSegment++;
                    }
                    else
                    {
                        controllerPosition = segment_points[currentSegment + 1];
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            currentNormalizePosition = (controllerPosition - segment_points[currentSegment]) / (segment_points[currentSegment + 1] - segment_points[currentSegment]);
            if (currentNormalizePosition != prevNormalizePosition && hasNormalizedPosition)
            {
                TargetAnimator.SetFloat(normalizedPositionParamaterID, currentNormalizePosition);
                foreach (Animator Ananimator in MultiTargetAnimators) Ananimator.SetFloat(normalizedPositionParamaterID, currentNormalizePosition);
                Local_isUpdated = true;
            }
            if (currentSegment != prevSegment && hasSegments)
            {
                TargetAnimator.SetInteger(segmentsParamaterID, currentSegment);
                foreach (Animator Ananimator in MultiTargetAnimators) Ananimator.SetInteger(segmentsParamaterID, currentSegment);
                Local_isUpdated = true;
            }
            prevSegment = currentSegment;
            prevControllerPosition = controllerPosition;
        }
        if (!isAnimatorControllPosition && positionUpdated && hasPosition)
        {
            TargetAnimator.SetFloat(positionParamaterID, controllerPosition);
            foreach (Animator Ananimator in MultiTargetAnimators) Ananimator.SetFloat(positionParamaterID, controllerPosition);
        }
        ApplyToTransform();
    }
    protected virtual void onPicked()
    {
    }
    protected virtual void ApplyToTransform()
    {
    }
    protected float wrapAngleTo180(float controllerAngle)
    {
        controllerAngle %= 360;
        controllerAngle = controllerAngle > 180 ? controllerAngle - 360 : controllerAngle;
        controllerAngle = controllerAngle < -180 ? controllerAngle + 360 : controllerAngle;
        return controllerAngle;
    }
    public override void Interact()
    {
    }
    public override void OnPreSerialization()
    {
        SyncedControllerPosition = controllerPosition;
    }
    public override void OnDeserialization()
    {
        //Debug.Log("debug_recieved");
        this.enabled = true;
        TargetAnimator.enabled = true;
        if (hasNormalizedPosition)
        {
            TargetAnimator.SetFloat(normalizedPositionParamaterID, currentNormalizePosition);
            foreach (Animator Ananimator in MultiTargetAnimators) Ananimator.SetFloat(normalizedPositionParamaterID, currentNormalizePosition);
        }
        if (hasSegments)
        {
            TargetAnimator.SetInteger(segmentsParamaterID, currentSegment);
            foreach (Animator Ananimator in MultiTargetAnimators) Ananimator.SetInteger(segmentsParamaterID, currentSegment);
        }
        if (hasPosition && !isAnimatorControllPosition)
        {
            TargetAnimator.SetFloat(positionParamaterID, controllerPosition);
            foreach (Animator Ananimator in MultiTargetAnimators) Ananimator.SetFloat(positionParamaterID, controllerPosition);
        }
    }
    public override void OnOwnershipTransferred(VRC.SDKBase.VRCPlayerApi player)
    {
        isowner = Networking.IsOwner(gameObject);
        this.OnDrop();
    }
}
