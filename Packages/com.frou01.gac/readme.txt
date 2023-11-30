配布場所:https://frou01.booth.pm/items/3772530

・使い方
1  :Prefabをシーンに配置し、回転もしくは移動軸がPrefabのY軸になるように、Prefabを回転させます。
2  :各Prefabを配置したら、未子になっているController_Pickupを選択します。
3  :制御対象アニメーター、パラメーター名を設定します。
4  :アニメーターにFloatパラメーター2つ、Integerパラメーター1つを新規作成します。
5.1:Screwを使う場合は追加でもう一つFloatパラメーターが必要になります。
6  :インスペクター上の記載に沿うようにパラメーター名を変更します。(設定名)_segmentsはIntegerパラメーターです。
7  :アニメーションを作ります。motionTimeに(設定名)_positionを使うと良いでしょう。回転・移動軸を合わせることを推奨します。

・デバッグ方法
Unity上でRunした後、Animatorの(設定名)_rotation/(設定名)_sliderを変更することで動きを確認できます。
positionを変更することでも確認できます。作例にあるようにrotationをアニメーターで制御している時に使えます。



・segments設定の注意点
segmentsは複数設定できますが、その設定を行うとpositionが0-1ではなく、

例）segments 0 45 60 90の場合等
rotation:0                  45      60            90
position:0---------------->1  0--->1  0--------- 1

と言った具合に変化するようになります。