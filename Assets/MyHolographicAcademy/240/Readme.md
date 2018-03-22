# My Holographic Academy 210（日本語）



Unity や MRTK のバージョンに依存せずに HoloAcademy 210 を学習できるようにしたものです。

# 準備
## プロジェクトの準備
1. Unityでプロジェクトを新規作成
1. MRTKを導入
1. メニュー＞Mixed Reality Toolkit＞Configure＞Apply Mixed Reality Project Setting
1. デフォルトのチェックボックスの他に「Enable Sharing Services」をチェックする
1. ダウンロードするか聞かれるので「Yes」で Sharing サーバーをダウンロードします
1. Asset / HolographicAcademy フォルダを新規作成
## シーンの準備
1. Asset / Scenes フォルダを新規作成
1. Sharing シーンを新規作成
1. メニュー＞Mixed Reality Toolkit＞Configure＞Apply Mixed Reality Scene Setting（チェックボックスはデフォルト）
1. Directional Lightを削除
## UWP の機能をオンにする
1. メニュー＞Mixed Reality Toolkit＞Configure＞Apply UWP Capability Setting
1. 「InternetClientServer」「PrivateNetworkClientServer」をチェック
## デバッグ用3D Textを作成
1. Asset > HoloToolkit > UX > Prefabs > 3DTextPrefab をヒエラルキービューにドラッグ
1. 新しい GameObject を右クリックして、名前を「Debug Log」に変更します。
1. Debug Log の Transform の Position を (0.1, 0, 2) にします。
1. Debug Log の Text Mesh コンポーネントの Anchor を Middle left にし、Alignment を Left にします。
# Sharing
1. Assets > HoloToolkit > Sharing > Sharing プレハブ をシーンに追加
1. 