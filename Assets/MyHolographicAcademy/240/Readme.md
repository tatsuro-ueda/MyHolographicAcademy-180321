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
    /// 書いたスクリプトが正常に動作しているか、3D Textに表示して確認する
    /// </summary>
    public class DebugLog : Singleton<DebugLog>
    {
        #region Public Valiables

        [Tooltip("3D Text の Text Mesh")]
        public TextMesh MyTextMesh;

        /// <summary>
        /// 表示するメッセージを受け取るための public 変数
        /// </summary>
        public string Log = "";

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// スクリプトの public 変数を表示し続ける
        /// </summary>
        private void Update()
        {
            this.MyTextMesh.text = ""
                // 以下の2行がフォーカスが、外れたときにうまくはたらかない
                + "Position: " + GazeManager.Instance.HitPosition.ToString()
                + "\nNormal: " + GazeManager.Instance.HitNormal.ToString()
                + "\nFocused: " + this.FocusedGameObjectName()
                + "\nLog:\n" + this.Log
                ;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// フォーカスされている GameObject の名前を取得する
        /// </summary>
        /// <returns>フォーカスされている GameObject の名前</returns>
        private string FocusedGameObjectName()
        {
            string focusedName;

            if (GazeManager.Instance.HitObject == null)
            {
                focusedName = "null";
            }
            else
            {
                focusedName = GazeManager.Instance.HitInfo.collider.gameObject.name;
            }

            return focusedName;
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