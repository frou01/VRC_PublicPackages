
using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDKBase;
using VRC.Udon;

public class FloorStationController : UdonSharpBehaviour
{
    public VehicleInSideSeatMNG preset_Manager;

    private PlayerChaser loacl_PlayerChaser;
    private Transform loacl_PlayerChaserTransform;
    private CharacterController local_InVehicleController;

    private VRCStation local_StationBody;
    public Transform preset_sittingTransform;
    public Transform preset_exit___Position;

    public Vector3 local_controlMoveInput = new Vector3(0,0,0);
    private Vector3 local_moveVelocity = new Vector3(0,0,0);
    private float local_controlRollInput;
    private GameObject local_VehicleObject;
    private GameObject local_inVehicleCollider;
    private CatchCollider_Vehicle local_catchCollider;
    VRCPlayerApi.TrackingData local_trackingData;
    VRCPlayerApi local_playerApi;
    [System.NonSerialized] [UdonSynced(UdonSyncMode.None)] public bool synced_Using;

    public bool isSDK2Mode;
    public Animator seatedSetter;

    bool tempFlag_resetRolling = false;


    void Start()
    {
        local_InVehicleController = GetComponent<CharacterController>();
        local_StationBody = GetComponent<VRCStation>();
        local_InitialControllerCenter = local_InVehicleController.center;
        loacl_PlayerChaser = preset_Manager.preset_playerChaser;
        loacl_PlayerChaserTransform = loacl_PlayerChaser.transform;
    }
    public void startSeating(int vehicleID)
    {
        GetVehicleData(vehicleID);

        startSeating();
    }

    private void GetVehicleData(int vehicleID)
    {
        if (vehicleID < 0) return;
        local_catchCollider = preset_Manager.preset_CatchColliders[vehicleID];
        local_VehicleObject = local_catchCollider.vehicleObject;
        local_inVehicleCollider = local_catchCollider.inVehicleCollider;
        global_targetVehicleID = vehicleID;

    }
    public void startSeating()
    {


        this.local_playerApi = Networking.LocalPlayer;
        Networking.SetOwner(local_playerApi, gameObject);

        //setUp Hierarchy
        preset_sittingTransform.parent = loacl_PlayerChaserTransform.parent = local_VehicleObject.transform;
        local_InVehicleController.enabled = true;
        local_inVehicleCollider.SetActive(true);

        //setUp Position/Rotation
        loacl_PlayerChaser.PlayerPositionScriptControlMode = true;
        preset_sittingTransform.localPosition = local_InVehicleController.gameObject.transform.localPosition = local_VehicleObject.transform.InverseTransformPoint(local_playerApi.GetPosition());
        Quaternion temp = local_playerApi.GetRotation() * Quaternion.Inverse(local_VehicleObject.transform.rotation);
        tempFlag_resetRolling = true;
        preset_sittingTransform.localRotation = local_InVehicleController.gameObject.transform.localRotation = temp;
        preset_sittingTransform.up = Vector3.up;

        //Reset_Controller Center
        local_InVehicleController.center = local_InitialControllerCenter;

        //Set SyncParam
        position = transform.localPosition;
        rotation = transform.localRotation;
        synced_Using = true;
        local_StationBody.UseStation(local_playerApi);
        RequestSerialization();
    }


    Vector3 local_InitialControllerCenter;
    Vector3 movedByRotation;
    Vector3 ControllerToHead;

    public int synced_targetVehicleID;
    [UdonSynced(UdonSyncMode.None)] public int global_targetVehicleID;
    [UdonSynced(UdonSyncMode.None)] public Vector3 syncedPosition;
    [UdonSynced(UdonSyncMode.None)] public Quaternion syncedRotation;
    [UdonSynced(UdonSyncMode.None)] public int AllocatePlayer = -1;
    [UdonSynced(UdonSyncMode.None)] public bool SDK2Fallback = false;

    public Vector3 prevSyncedPosition;
    public Quaternion prevSyncedRotation;
    public Vector3 position;
    public Quaternion rotation;
    float syncInterval = 0.4f;
    float FromLastExcuteSync;
    bool excuteSync;
    Vector3 local_restorePos;

    public void LateUpdate()
    {
        if (synced_targetVehicleID != global_targetVehicleID) excuteSync = true;
        if (synced_Using && local_VehicleObject != null)
        {
            if (Networking.IsOwner(this.gameObject))
            {
                preset_sittingTransform.localPosition = loacl_PlayerChaserTransform.localPosition = position = transform.localPosition;
                local_trackingData = local_playerApi.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                ControllerToHead = local_VehicleObject.transform.InverseTransformPoint(local_trackingData.position) - transform.localPosition;
                ControllerToHead = Quaternion.Inverse(transform.localRotation) * ControllerToHead;
                ControllerToHead.y = 0;
                local_InVehicleController.center = local_InitialControllerCenter + ControllerToHead;

                if (local_controlRollInput != 0)
                {
                    transform.RotateAround(local_VehicleObject.transform.InverseTransformPoint(local_trackingData.position), transform.up, local_controlRollInput * 180 * Time.deltaTime);
                }
                if (tempFlag_resetRolling) transform.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
                tempFlag_resetRolling = false;

                preset_sittingTransform.localRotation = loacl_PlayerChaserTransform.localRotation = rotation = transform.localRotation;

                if (position != syncedPosition || rotation != syncedRotation) excuteSync = true;
            }
        }
        if (!Networking.IsOwner(this.gameObject))
        {
            FromLastExcuteSync += Time.deltaTime;
            position = Vector3.Lerp(prevSyncedPosition, syncedPosition, FromLastExcuteSync / syncInterval);
            rotation = Quaternion.Slerp(prevSyncedRotation, syncedRotation, FromLastExcuteSync / syncInterval);
            preset_sittingTransform.localPosition = transform.localPosition = position;
            preset_sittingTransform.localRotation = transform.localRotation = rotation;
        }
        else
        {
            if (excuteSync) FromLastExcuteSync += Time.deltaTime;
            if (FromLastExcuteSync > syncInterval)
            {
                excuteSync = false;
                FromLastExcuteSync = 0;
                RequestSerialization();
            }
        }
    }
    public void FixedUpdate()
    {
        if (synced_Using && local_VehicleObject != null)
        {
            if (!Utilities.IsValid(local_playerApi)) return;
            if (Networking.IsOwner(this.gameObject))
            {
                if (!Networking.IsOwner(local_InVehicleController.gameObject))
                {
                    Networking.SetOwner(local_playerApi, local_InVehicleController.gameObject);
                }
                Quaternion proxyQuat = (Quaternion.Inverse(local_VehicleObject.transform.rotation) * local_trackingData.rotation);
                proxyQuat = Quaternion.Euler(0, proxyQuat.eulerAngles.y, 0);
                local_moveVelocity.x = 0;
                local_moveVelocity.z = 0;
                Vector3 applyingControl = proxyQuat *
                    local_controlMoveInput;
                local_moveVelocity += applyingControl;
                local_moveVelocity.Normalize();
                if (!local_InVehicleController.isGrounded) local_moveVelocity.y += -9.8f * Time.fixedDeltaTime;
                else local_moveVelocity.y = 0;
                local_moveVelocity.y *= 0.9f;
                if (!tempFlag_resetRolling) local_InVehicleController.Move(movedByRotation + local_moveVelocity * Time.fixedDeltaTime);
            }
        }
    }
    public void restorePosition()
    {
        local_playerApi.TeleportTo(local_restorePos, local_playerApi.GetRotation());
    }

    public void PlayerExitBounds_force()
    {
        VRCPlayerApi playerApi = Networking.LocalPlayer;
        if (playerApi.isLocal && synced_Using)
        {
            synced_Using = false;
            local_StationBody.ExitStation(playerApi);

            local_InVehicleController.enabled = false;
            if(local_inVehicleCollider != null) local_inVehicleCollider.gameObject.SetActive(false);
            loacl_PlayerChaserTransform.parent = null;
            loacl_PlayerChaser.PlayerPositionScriptControlMode = false;
            loacl_PlayerChaser.PostLateUpdate();
        }
    }
    public void PlayerExitBounds(int VehicleID)
    {
        if (VehicleID != global_targetVehicleID) return;
        VRCPlayerApi playerApi = Networking.LocalPlayer;
        if (synced_Using && playerApi.isLocal)
        {
            synced_Using = false;
            local_StationBody.ExitStation(playerApi);
            
            local_InVehicleController.enabled = false;
            if (local_inVehicleCollider != null) local_inVehicleCollider.gameObject.SetActive(false);
            loacl_PlayerChaserTransform.parent = null;
            loacl_PlayerChaser.PlayerPositionScriptControlMode = false;
            loacl_PlayerChaser.PostLateUpdate();
        }
    }

    public void SyncParmReset()
    {
        synced_Using = false;
        global_targetVehicleID = -1;
        RequestSerialization();
    }

    public override void OnPreSerialization()
    {
        syncedPosition = position;
        syncedRotation = rotation;
        synced_targetVehicleID = global_targetVehicleID;
        FromLastExcuteSync = 0;
    }

    int prev_targetVehicleID = -1;
    public override void OnDeserialization()
    {
        Debug.Log("syncedPosition " + syncedPosition);
        Debug.Log("syncedRotation " + syncedRotation);
        prevSyncedPosition = position;
        prevSyncedRotation = rotation;
        if(prev_targetVehicleID != global_targetVehicleID)
        {
            prev_targetVehicleID = global_targetVehicleID;
            GetVehicleData(global_targetVehicleID);
            preset_sittingTransform.parent = local_VehicleObject.transform;
            prevSyncedPosition = syncedPosition;
            prevSyncedRotation = syncedRotation;
        }
        FromLastExcuteSync = 0;
    }

    public override void OnStationExited(VRC.SDKBase.VRCPlayerApi player)
    {
        if (player.isLocal) PlayerExitBounds_force();
    }
    public override void OnStationEntered(VRC.SDKBase.VRCPlayerApi player)
    {
        if (player.isLocal &&
            !synced_Using)
        {
            startSeating();
        }
    }

    public override void InputMoveHorizontal(float value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        local_controlMoveInput.x = value * 2;
    }
    public override void InputMoveVertical(float value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        local_controlMoveInput.z = value * 2;
    }
    public override void InputLookHorizontal(float value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        local_controlRollInput = value;
    }
    public override void InputLookVertical(float value, VRC.Udon.Common.UdonInputEventArgs args)
    {
    }
    public void changeStationFallback()
    {
        SDK2Fallback = !SDK2Fallback;
        seatedSetter.enabled = true;
        if (SDK2Fallback)
        {
            seatedSetter.SetBool("seated", true);
        }
        else
        {
            seatedSetter.SetBool("seated", false);
        }
        RequestSerialization();
    }
}
