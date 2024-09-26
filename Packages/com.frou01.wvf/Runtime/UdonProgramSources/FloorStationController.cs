
using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDKBase;
using VRC.Udon;

public class FloorStationController : UdonSharpBehaviour
{
    public VehicleIsideSeatMNG preset_Manager;

    private Transform loacl_PlayerChaserTransform;

    public VRCStation preset_StationBody;
    public UdonParentConstraint preset_Constraint;
    public Transform preset_sittingPosition;
    public Transform preset_exit___Position;
    public CharacterController preset_InVehicleController;

    public Vector3 local_controlMoveInput = new Vector3(0,0,0);
    private Vector3 local_moveVelocity = new Vector3(0,0,0);
    private float local_controlRollInput;
    private GameObject local_vehicleObject;
    private GameObject local_inVehicleCollider;
    private CatchCollider_Vehicle local_catchCollider;
    [System.NonSerialized] [UdonSynced(UdonSyncMode.None)] public bool synced_Using;
    public bool local_StationInBounds;

    public FloorStation preset_floorStation;

    public bool isSDK2Mode;
    public Animator seatedSetter;


    void Start()
    {
        local_InitialControllerCenter = preset_InVehicleController.center;
        loacl_PlayerChaserTransform = preset_Manager.preset_playerChaser.transform;
    }
    public void startSeating(CatchCollider_Vehicle vehicle_catchCollider, int vehicleID)
    {
        local_catchCollider = vehicle_catchCollider;
        global_targetVehicleID = vehicleID;
        synced_Using = true;
        local_vehicleObject = vehicle_catchCollider.vehicleObject;
        this.playerApi = Networking.LocalPlayer;
        Networking.SetOwner(playerApi, gameObject);
        Networking.SetOwner(playerApi, preset_InVehicleController.gameObject);

        preset_InVehicleController.enabled = true;  
        
        preset_Constraint.target = local_vehicleObject.transform;
        preset_Constraint.Activate();
        loacl_PlayerChaserTransform.parent = local_vehicleObject.transform;
        preset_Manager.preset_playerChaser.PlayerPositionScriptControlMode = true;
        preset_InVehicleController.gameObject.transform.localPosition = preset_sittingPosition.localPosition = local_vehicleObject.transform.InverseTransformPoint(playerApi.GetPosition());
        Vector3 PlayerLook = playerApi.GetRotation() * Vector3.forward;
        Quaternion temp = playerApi.GetRotation() * Quaternion.Inverse(local_vehicleObject.transform.rotation);
        resetRolling = true;

        preset_InVehicleController.gameObject.transform.localRotation = preset_sittingPosition.localRotation = temp;
        preset_sittingPosition.up = Vector3.up;
        preset_InVehicleController.center = local_InitialControllerCenter;

        local_inVehicleCollider = preset_Manager.preset_inVehicleCollider[vehicleID];
        local_inVehicleCollider.SetActive(true);
        position = preset_InVehicleController.transform.localPosition;
        rotation = preset_InVehicleController.transform.localRotation;

        isSDK2Mode = preset_floorStation.SDK2Fallback;
        RequestSerialization();
    }
    public void ReStartSeating()
    {
        synced_Using = true;
        this.playerApi = Networking.LocalPlayer;
        Networking.SetOwner(playerApi, gameObject);
        Networking.SetOwner(playerApi, preset_InVehicleController.gameObject);

        preset_InVehicleController.enabled = true;

        preset_Constraint.target = local_vehicleObject.transform;
        preset_Constraint.Activate();
        loacl_PlayerChaserTransform.parent = local_vehicleObject.transform;
        preset_Manager.preset_playerChaser.PlayerPositionScriptControlMode = true;
        preset_InVehicleController.gameObject.transform.localPosition = preset_sittingPosition.localPosition = local_vehicleObject.transform.InverseTransformPoint(playerApi.GetPosition());
        Vector3 PlayerLook = playerApi.GetRotation() * Vector3.forward;
        Quaternion temp = playerApi.GetRotation() * Quaternion.Inverse(local_vehicleObject.transform.rotation);
        resetRolling = true;

        preset_InVehicleController.gameObject.transform.localRotation = preset_sittingPosition.localRotation = temp;
        preset_sittingPosition.up = Vector3.up;
        preset_InVehicleController.center = local_InitialControllerCenter;

        local_inVehicleCollider.SetActive(true);
        position = preset_InVehicleController.transform.localPosition;
        rotation = preset_InVehicleController.transform.localRotation;

        isSDK2Mode = preset_floorStation.SDK2Fallback;
        RequestSerialization();
    }

    bool resetRolling = false;
    VRCPlayerApi.TrackingData trackingData;
    VRCPlayerApi playerApi;

    Vector3 local_InitialControllerCenter;
    Vector3 movedByRotation;
    Vector3 ControllerToHead;

    public int synced_targetVehicleID;
    [UdonSynced(UdonSyncMode.None)] public int global_targetVehicleID;
    [UdonSynced(UdonSyncMode.None)] public Vector3 syncedPosition;
    [UdonSynced(UdonSyncMode.None)] public Quaternion syncedRotation;

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
        if (synced_Using && local_vehicleObject != null)
        {
            if (!Utilities.IsValid(playerApi)) return;
            preset_StationBody.UseStation(playerApi);
            if (Networking.IsOwner(this.gameObject))
            {
                if (!Networking.IsOwner(preset_InVehicleController.gameObject))
                {
                    Networking.SetOwner(playerApi, preset_InVehicleController.gameObject);
                }
                trackingData = playerApi.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                ControllerToHead = local_vehicleObject.transform.InverseTransformPoint(trackingData.position) - preset_InVehicleController.transform.localPosition;
                ControllerToHead = Quaternion.Inverse(preset_InVehicleController.transform.localRotation) * ControllerToHead;
                ControllerToHead.y = 0;
                preset_InVehicleController.center = local_InitialControllerCenter + ControllerToHead;
                if (local_controlRollInput != 0)
                {
                    preset_InVehicleController.transform.RotateAround(local_vehicleObject.transform.InverseTransformPoint(trackingData.position), preset_InVehicleController.transform.up, local_controlRollInput * 180 * Time.deltaTime);
                }
                if (resetRolling) preset_InVehicleController.transform.localRotation = Quaternion.Euler(0, preset_InVehicleController.transform.localRotation.eulerAngles.y, 0);

                resetRolling = false;
                position = preset_InVehicleController.transform.localPosition;
                rotation = preset_InVehicleController.transform.localRotation;
                if(position != syncedPosition || rotation != syncedRotation) excuteSync = true;
                //if (!playerApi.IsPlayerGrounded())
                //{
                //    local_restorePos = playerApi.GetPosition();
                //    playerApi.TeleportTo(new Vector3(0,0,0), playerApi.GetRotation());
                //    this.SendCustomEventDelayedFrames(nameof(restorePosition), 2);
                //}
                loacl_PlayerChaserTransform.localPosition = position;
                loacl_PlayerChaserTransform.localRotation = rotation;
            }
        }
        if (!Networking.IsOwner(this.gameObject))
        {
            FromLastExcuteSync += Time.deltaTime;
            position = Vector3.Lerp(prevSyncedPosition, syncedPosition, FromLastExcuteSync / syncInterval);
            rotation = Quaternion.Slerp(prevSyncedRotation, syncedRotation, FromLastExcuteSync / syncInterval);
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
        preset_sittingPosition.localPosition = position;
        preset_sittingPosition.localRotation = rotation;
    }
    public void FixedUpdate()
    {
        if (synced_Using && local_vehicleObject != null)
        {
            if (!Utilities.IsValid(playerApi)) return;
            if (Networking.IsOwner(this.gameObject))
            {
                if (!Networking.IsOwner(preset_InVehicleController.gameObject))
                {
                    Networking.SetOwner(playerApi, preset_InVehicleController.gameObject);
                }
                Quaternion proxyQuat = (Quaternion.Inverse(local_vehicleObject.transform.rotation) * trackingData.rotation);
                proxyQuat = Quaternion.Euler(0, proxyQuat.eulerAngles.y, 0);
                local_moveVelocity.x = 0;
                local_moveVelocity.z = 0;
                Vector3 applyingControl = proxyQuat *
                    local_controlMoveInput;
                local_moveVelocity += applyingControl;
                local_moveVelocity.Normalize();
                if (!preset_InVehicleController.isGrounded) local_moveVelocity.y += -9.8f * Time.fixedDeltaTime;
                else local_moveVelocity.y = 0;
                local_moveVelocity.y *= 0.9f;
                if (!resetRolling) preset_InVehicleController.Move(movedByRotation + local_moveVelocity * Time.fixedDeltaTime);
            }
        }
    }
    public void restorePosition()
    {
        playerApi.TeleportTo(local_restorePos, playerApi.GetRotation());
    }

    public void PlayerExitBounds_force()
    {
        VRCPlayerApi playerApi = Networking.LocalPlayer;
        if (playerApi.isLocal && synced_Using)
        {
            synced_Using = false;
            //preset_exit___Position.position = playerApi.GetPosition();
            //preset_exit___Position.rotation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            //preset_StationBody.ExitStation(playerApi);
            preset_StationBody.ExitStation(playerApi);

            preset_InVehicleController.enabled = false;
            if(local_inVehicleCollider != null) local_inVehicleCollider.gameObject.SetActive(false);
            loacl_PlayerChaserTransform.parent = null;
            preset_Manager.preset_playerChaser.PlayerPositionScriptControlMode = false;
            preset_Manager.preset_playerChaser.PostLateUpdate();
        }
    }
    public void PlayerExitBounds(int VehicleID)
    {
        if (VehicleID != global_targetVehicleID) return;
        VRCPlayerApi playerApi = Networking.LocalPlayer;
        if (synced_Using && playerApi.isLocal)
        {
            synced_Using = false;
            //preset_exit___Position.position = playerApi.GetPosition();
            //preset_exit___Position.rotation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            preset_StationBody.ExitStation(playerApi);
            
            preset_InVehicleController.enabled = false;
            if (local_inVehicleCollider != null) local_inVehicleCollider.gameObject.SetActive(false);
            loacl_PlayerChaserTransform.parent = null;
            preset_Manager.preset_playerChaser.PlayerPositionScriptControlMode = false;
            preset_Manager.preset_playerChaser.PostLateUpdate();
        }
    }

    public void SyncParmReset()
    {
        synced_Using = false;
        global_targetVehicleID = 0;
        RequestSerialization();
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

    public override void OnPreSerialization()
    {
        syncedPosition = position;
        syncedRotation = rotation;
        synced_targetVehicleID = global_targetVehicleID;
        preset_Constraint.target = preset_Manager.preset_CatchColliders[global_targetVehicleID].transform;
        preset_Constraint.Activate();
        FromLastExcuteSync = 0;
    }

    int prev_targetVehicleID = -1;
    public override void OnDeserialization()
    {
        prevSyncedPosition = position;
        prevSyncedRotation = rotation;
        if(prev_targetVehicleID != global_targetVehicleID)
        {
            preset_Constraint.target = preset_Manager.preset_CatchColliders[global_targetVehicleID].transform;
            preset_Constraint.Activate();
            prev_targetVehicleID = global_targetVehicleID;
        }
        FromLastExcuteSync = 0;
    }

}
