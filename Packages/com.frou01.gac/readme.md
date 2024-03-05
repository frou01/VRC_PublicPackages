# Grab Animator Controller
## 概要
GrabAnimatorControllerはPickupを掴むことでAnimatorを制御するためのU#スクリプト及びPrefabです。

# 目次
- [Runtime(VRC)](#--runtimevrc)

### - Runtime(VRC)
- 共通
    
    Udonの付いたPickUpが持たれて移動されると、移動に応じてAnimatorの値を変化させます。
    
    
    <summary>設定値/仕様</summary>
    
    |設定値|概要|
    |---:|:---|
    controllerTransform|移動/回転に応じて動く、PickUpの基部となるオブジェクトです。位置・回転はUdonが制御するためエディタ上での変更は反映できません。位置を移動する場合、一つ上の階層にオブジェクトを挟んでそちらを移動してください。
    TargetAnimator|制御の対象になるAnimatorです。
    MultiTargetAnimators|追加の制御対象Animatorです。TargetAnimatorは別途入れないと使用できません。
    paramaterName|パラメーター名を指定します。<br>(設定名)_segment[integer]、(設定名)_normpos[float]、(設定名)_position[float]の三種類を用います。
    segment_points|パラメーターの区切り値です。デジタルな制御を行う場合に便利です。<br>２値は必須で、最大と最小が操作の制限値になります。
    snap_points|スナップ値です。segment_pointsで区間を複数設けた場合、同じ区間内のみスナップします。
    useHaptic|segment_pointsで設定した区切り値をまたぐ際にVRコントローラーを振動させるかを設定します。
    autoDisable|自動でスクリプト無効化(Update処理停止)を行うかどうかを設定します。
    ForceAutoDisable|後述するAnimatorによる制御を受けている場合も、スクリプト無効化するかを設定します。
    autoDisableTime|PickUpを降ろしてから、スクリプトが無効化されるまでの時間です[s]
    currentSegment|初期設定の区間です
    controllerPosition|初期設定の回転/移動量です。controllerTransform.localPositon/localRotationがそれぞれ0の状態からの相対値です。

    仕様：
    - controllerTransformはY軸で回転/移動します。
    - (設定名)_positionのパラメーターをAnimatorから制御すると、Udon側の値を変更できます。戻しバネや特定条件での自動制御等に使用できます。
- 各種のコントローラー

    |name|制御元|制御反映|
    |---:|:---|:---|
    ControlLever_Prefab|PickUp位置|Y軸回転
    ControlValve_Prefab|PickUp回転(捻り)|Y軸回転
    ControlValever_Prefab|PickUp位置+PickUp回転(捻り)|Y軸回転
    ControlSlider_Prefab|PickUp位置|Y軸移動

- Controller_Screw
    