# My Holographic Academy 240（日本語）



Unity や MRTK のバージョンに依存せずに HoloAcademy 210 を学習できるようにしたものです。また、可能な限り Unity の Play Mode で動かすことができるようにしています。

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
1. DebugLog スクリプトがなければ新規作成
### DebugLog スクリプト
```csharp
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    /// <summary>
    /// アタッチされた GameObject を gaze で動かし、エアタップで固定する
    /// </summary>
    public class MyHologramPlacement : Singleton<MyHologramPlacement>, IInputClickHandler
    {
        #region Public Valuables

        /// <summary>
        /// すでにエアタップで固定されたか否か
        /// </summary>
        public bool GotTransform;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// アタッチされた GameObject にフォーカスし続けるようにする
        /// </summary>
        private void Start()
        {
            InputManager.Instance.OverrideFocusedObject = this.gameObject;
        }

        /// <summary>
        /// GameObject がまだ配置されていなければ、現在位置の視点の先の点の中間に配置する
        /// </summary>
        private void Update()
        {
            if (!this.GotTransform)
            {
                this.transform.position = Vector3.Lerp(
                    this.transform.position, this.ProposeTransformPosition(), 0.2f);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// カメラで見ている方向の 2m 先の地点
        /// </summary>
        /// <returns>カメラで見ている方向の2m先の地点</returns>
        private Vector3 ProposeTransformPosition()
        {
            return Camera.main.transform.position + (Camera.main.transform.forward * 2);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// エアタップすると「配置済み」としてフォーカスを外す
        /// </summary>
        /// <param name="eventData">eventData</param>
        public void OnInputClicked(InputClickedEventData eventData)
        {
            this.GotTransform = true;
            InputManager.Instance.OverrideFocusedObject = null;
        }

        #endregion
    }
}
```
# Sharing
### ねらい
gaze した状態でエアタップするたびに色が切り替わるオブジェクトを用意します
### 準備
1. Cube を作り、Test Cube と名前を付けます
1. Transform を Position(0, 0, 2), Scale(0.1, 0.1, 0.1) にする
1. ColorChanger スクリプトを新規作成
### ColorChanger スクリプト
```csharp

```

1. Assets > HoloToolkit > Sharing > Sharing プレハブ をシーンに追加
1. 