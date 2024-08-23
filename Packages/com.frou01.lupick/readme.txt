SmartPickupSharp
SmartPickup by NEET ENGINEER https://neet-shop.booth.pm/items/2981343
SmartPickupSharp by InPlanaria https://inplanaria.booth.pm/items/3640206

当プログラムは、NEET ENIGNEER様のUdon Graphコード"Smart Pickup"を
InPlanariaがUdon Sharp化し、機能追加を行ったものです。

【利用規約】
当配布アセット内のデータはすべて
●有償無償問わず再配布可
●改変可
●VRChatのprivateまたはpublicワールドへの組み込み可
●クレジット表記不要
●ただし製作者の詐称を禁止する


【説明】
通常のObject Syncは、オブジェクトの位置を常に同期します。
Smart Pickupは「オブジェクトを拾ったときの、Handボーンとオブジェクトの相対位置」を同期し、
ローカル処理によって手に追従して動くように見せています。
そのため同期回数が少なく、手の動きにしっかり追従しているように見えます。


【注意】
●Is Kinematicオンでしか使えません(重力等、Pickup以外の要因で座標や回転が変わることは想定していません)
●元来VRChatでは、アバターのボーン位置はすべてのプレイヤーの視点から一致して見えているわけではありません。Block,Hide,Fallback,Questが関係なくてもです。
　そのため、ピックアップは各プレイヤーによって見えている位置が違います。おそらくペンには向いていません。
●Genericアバターで持ったり、デスクトップモードで[U][O][I][K][J][L][マウスホイール]を使うと、他の人の視点ではなめらかに動きません。


【使い方】
①SmartPickupSharp.prefabをHierarchyに配置してください。
②SmartPickupSharp/Modelの子にモデルを配置してください。
③SmartPickupSharpのBoxColliderの位置とサイズを調整してください。
④(任意)SmartPickup_ResetSwitch.prefabををHierarchyに配置。SmartPickup_ResetSwitchのUdon BehaviourのSmart Pickup Objに、配置したSmartPickupオブジェクトを登録してください。


【サンプルについて】
SmartPickupSharp/Example/ExampleSceneが設定サンプルです。
サンプルデータの利用規約はスクリプト本体と同様です。
●SmartPickupSharp
　通常使用の設定サンプルです。
　　
●SmartPickupSharp_UdonCustomSample
　別のUdonBehaviourと連携して色々やるサンプルです。

●SmartPickupSharp_AnimationToggleSample
　2状態をアニメーションで変化させるシンプルな実装サンプルです。
　
●SmartPickup_ResetSwitch
　位置リセットスイッチの設定サンプルです。

●GlowStick (VRC Object Sync)
　動作比較用。
