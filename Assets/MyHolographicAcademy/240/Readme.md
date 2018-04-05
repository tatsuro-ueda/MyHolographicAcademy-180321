# My Holographic Academy 240（日本語）



Unity や MRTK のバージョンに依存せずに HoloAcademy 240（というより MRTK の Examples に入っている SharingTest ）を学習できるようにしたものです。
# 準備
## プロジェクトの準備
1. Unityでプロジェクトを新規作成
1. MRTKを導入
1. メニュー＞Mixed Reality Toolkit＞Configure＞Apply Mixed Reality Project Setting
1. デフォルトのチェックボックスの他に「Enable Sharing Services」をチェックする
1. ダウンロードするか聞かれるので「Yes」で Sharing サーバーをダウンロードします
1. Asset / HolographicAcademy フォルダを新規作成
## シェアリングサーバーの準備
1. メニュー＞Mixed Reality Toolkit＞Sharing Service>Launch Sharing Service
2. サーバープログラムがないと、自動的にダウンロードできます
3. しかし、このプログラムが新しすぎることがあります。そのときは下記ページから古いものをダウンロードします（バージョンが合わないときは適宜変更して下さい）。

https://github.com/Microsoft/MixedRealityToolkit-Unity/blob/v1.2017.1.2/External/HoloToolkit/Sharing/Server/SharingService.exe

## シーンの準備
1. Asset / Scenes フォルダを新規作成
1. Sharing シーンを新規作成
1. メニュー＞Mixed Reality Toolkit＞Configure＞Apply Mixed Reality Scene Setting（チェックボックスはデフォルト）
1. Directional Lightを削除
## UWP の機能をオンにする
1. メニュー＞Mixed Reality Toolkit＞Configure＞Apply UWP Capability Setting
1. 「Spatial Mapping」「InternetClientServer」「PrivateNetworkClientServer」をチェック
## デバッグ用 3D Text を作成
1. Asset > HoloToolkit > UX > Prefabs > 3DTextPrefab をヒエラルキービューにドラッグ
1. 新しい GameObject を右クリックして、名前を「Debug Log」に変更します。
1. Debug Log の Transform の Position を (0, 0.1, 2) にします。
1. Debug Log の Text Mesh コンポーネントの Anchor を Lower Center にし、Alignment を Center にします。
5. Debug Log の Text Mesh コンポーネントの Text を以下のようにします：

```
Debug Log
--------------------
```

# Sharing
## アンカーを共有
### ねらい
シェアリングサーバーに接続してアンカーをアップロード・ダウンロードして共有します。
### 手順
1. シェアリングサーバーを起動し、IPアドレスを確認します。
1. Empty Objectを作り Managers と名前を付けます。
2. Input Manager を Managers の子要素に移動します
3. Sharing プレハブを検索し、Managers の子要素に加えます
4. Sharing オブジェクトの Sharing Stage コンポーネントの Server Address を、先ほど確認したサーバーのアドレスにします。
6. Sharing オブジェクトの Sharing World Anchor Manager の Anchor Debug Text の欄の右側の丸いボタンを押し、Debug Log を選択します。
7. Sharing オブジェクトに Auto Join Session And Room スクリプトをアタッチします。
8. 2台の HoloLens にアプリを配置します。
### 動作確認
1. 1台目のHoloLensでアンカーを取得・アップロードできていることを確認します。

![Sharing01](Readme_Data/sharing01.jpg)

1. 2台目のHoloLensでアンカーをダウンロードできていることを確認します。

![Sharing02](Readme_Data/sharing02.jpg)

## HoloLensの位置を共有
### ねらい
とりあえず用意されているスクリプトでシェアリングできることを確かめます。
### 手順
1. Sharing オブジェクトに Custom Messages スクリプトをアタッチします。
2. Sharing オブジェクトに Remote Head Manager スクリプトをアタッチします。
### 動作確認
1. 2台のHoloLensでアンカーのアップロード、ダウンロードに成功すれば、もう片方のHoloLensに Cube が載っているように表示されます。

![Sharing Head](Readme_Data/sharing_head.png)