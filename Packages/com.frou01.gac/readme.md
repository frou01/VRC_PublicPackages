# Grab Animator Controller
## 概要
GrabAnimatorControllerはPickupを掴むことでAnimatorを制御するためのU#スクリプト及びPrefabです。

# 目次
- [Runtime(VRC)](#--runtimevrc)

### - Runtime(VRC)
- 共通
    
    Udonの付いたPickUpが持たれて移動されると、移動に応じてAnimatorの値を変化させます。
    
    <details>
    <summary>設定値/仕様</summary>
    
    |設定値|概要|
    |---:|:---|
    controllerTransform|移動/回転に応じて動くオブジェクトです。位置・回転はUdonが制御するためエディタ上での変更は反映できません。位置を移動する場合、一つ上の階層にオブジェクトを挟んでそちらを移動してください。
    TargetAnimator|制御の対象になるAnimatorです。
    MultiTargetAnimators|追加の制御対象Animatorです。TargetAnimatorは別途入れないと使用できません。
    paramaterName|パラメーター名を指定します。<br>(設定名)_segment[integer]、(設定名)_normpos[float]、(設定名)_position[float]の三種類を用います。
    segment_points|パラメーターの区切り値です。デジタルな制御を行う場合に便利です。<br>２値は必須で、最大と最小が操作の制限値になります。
    snap_points|スナップ値です。segment_pointsで区間を複数設けた場合、同じ区間内のみスナップします。
    useHaptic|segment_pointsで設定した区切り値をまたぐ際にVRコントローラーを振動させるかを設定します。
    autoDisable|自動でスクリプト無効化(Update処理停止)を行うかどうかを設定します。
    ForceAutoDisable|

    Gizmoについて：
    
    前後レールの設定がある場合、Gizmoは \\/ (レール部分) \\/ のような形になります。

    前後レールの設定が無い場合、Gizmoは◯--◯ (レール部分) ◯\/のような形になります。

    設定したレール間が離れていると、巨大な球が描画されます。
    </details>