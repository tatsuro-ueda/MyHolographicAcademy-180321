# My Holographic Academy 211（日本語）



Unity や MRTK のバージョンに依存せずに HoloAcademy 211 を学習できるようにしたものです。
# 準備
## プロジェクトの準備
1. Unityでプロジェクトを新規作成
1. MRTKを導入
1. メニュー＞Mixed Reality Toolkit＞Configure＞Apply Mixed Reality Project Setting（チェックボックスはデフォルト）
1. Asset / HolographicAcademy / 211 フォルダを新規作成
## シーンの準備
1. 211 フォルダで Gesture シーンを新規作成
1. メニュー＞Mixed Reality Toolkit＞Configure＞Apply Mixed Reality Scene Setting（チェックボックスはデフォルト）
1. Directional Lightを削除
## デバッグ用3D Textを作成
1. Asset > HoloToolkit > UX > Prefabs > 3DTextPrefab をヒエラルキービューにドラッグ
1. 新しい GameObject を右クリックして、名前を「Debug Log」に変更します。
1. Debug Log の Transform の Position を (0.1, 0, 2) にします。
1. Debug Log の Text Mesh コンポーネントの Anchor を Middle left にし、Alignment を Left にします。
# Gaze
## ヒット
### ねらい
視線の先（gaze）がヒットした地点の位置と法線を取得できるようにします
### MyGazeManager スクリプト
