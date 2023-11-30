
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace frou01.GrabController
{
    public class Controller_Screw : UdonSharpBehaviour
    {
        [Header("レバーに使っているオブジェクトと同じ物にアタッチして使います")]
        [Header("レバーはsegment_pointsの最初を-360、最後を360にする等で回転角度を無制限としてください")]
        [Header("コントローラーの回転表示オブジェクト（通常は変更しません）")]
        public Transform controllerTransform;
        [Header("制御対象のアニメーター")]
        public Animator TargetAnimator;
        [Header("入力先のパラメーター")]
        public string ScrewParamaterName;
        [Header("レバーが使用しているパラメーター")]
        [Header("※末尾の_rotationが必要です")]
        public string LeverParamaterName;
        [Header("スクリューの現在の回転量です")]
        [Header("デバッグ時はこちらを変更してください")]
        [Header("アニメーターから制御する際はレバーのパラメータを変更するようにしてください")]
        [UdonSynced(UdonSyncMode.Linear)] [SerializeField] float screwRotation;
        private int ScrewParamaterID;
        private int HandleParamaterID;
        private float prevLeverRotation;
        [Header("最小回転量　-360より小さい値でも設定可能です")]
        [SerializeField] float min;
        [Header("最大回転量　360より大きい値でも設定可能です")]
        [SerializeField] float MAX;
        float range = 1;
        private float SyncInterval = 1;
        private float SinceLastRequest;
        bool dirty;
        void Start()
        {
            range = MAX - min;
            ScrewParamaterID = Animator.StringToHash(ScrewParamaterName);
            HandleParamaterID = Animator.StringToHash(LeverParamaterName);
        }

        public void Update()
        {
            float currentLeverRotation = TargetAnimator.GetFloat(HandleParamaterID);


            if (Networking.IsOwner(gameObject))
            {
                if (dirty) SinceLastRequest += Time.deltaTime;
                if (SinceLastRequest > SyncInterval)
                {
                    SinceLastRequest = 0;
                    dirty = false;
                    RequestSerialization();
                }
                float diff = wrapAngleTo180(currentLeverRotation - prevLeverRotation);
                if (Mathf.Abs(diff) > 0) dirty = true;
                //Debug.Log(diff);

                screwRotation += diff;

                if (screwRotation < min)
                {
                    screwRotation = min;
                }
                else
                if (screwRotation > MAX)
                {
                    screwRotation = MAX;
                }
            }
            TargetAnimator.SetFloat(ScrewParamaterID, (screwRotation - min) / range);
            TargetAnimator.SetFloat(HandleParamaterID, wrapAngleTo180(screwRotation));
            controllerTransform.localRotation = Quaternion.identity;
            controllerTransform.Rotate(0, screwRotation, 0);
            prevLeverRotation = wrapAngleTo180(screwRotation);
        }
        private float wrapAngleTo180(float controllerAngle)
        {
            controllerAngle %= 360;
            controllerAngle = controllerAngle > 180 ? controllerAngle - 360 : controllerAngle;
            controllerAngle = controllerAngle < -180 ? controllerAngle + 360 : controllerAngle;
            return controllerAngle;
        }
    }
}
