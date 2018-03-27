# My Holographic Academy 211（日本語）



Unity や MRTK のバージョンに依存せずに HoloAcademy 211 を学習できるようにしたものです。
なおこの章の内容は手の動きを直接取るため、Unity の Play モードでは**動きません**。
Remoting Player は手の動きを取ることが**できる**ため、これを使うのが良いでしょう。
# 準備
## プロジェクトの準備
1. Unityでプロジェクトを新規作成
1. MRTKを導入
1. メニュー＞Mixed Reality Toolkit＞Configure＞Apply Mixed Reality Project Setting（チェックボックスはデフォルト）
1. Assets / MyHolographicAcademy / 211 フォルダを新規作成
## シーンの準備
1. 211 フォルダで Gesture シーンを新規作成
1. メニュー＞Mixed Reality Toolkit＞Configure＞Apply Mixed Reality Scene Setting（チェックボックスはデフォルト）
1. Directional Lightを削除
1. Asset > HoloToolkit > Input > Prefabs > Cursor > Cursorプレハブをシーンに追加
## デバッグ用3D Textを作成
1. Asset > HoloToolkit > UX > Prefabs > 3DTextPrefab をヒエラルキービューにドラッグ
1. 新しい GameObject を右クリックして、名前を「Debug Log」に変更します。
1. Debug Log の Transform の Position を (0.1, 0, 2) にします。
1. Debug Log の Text Mesh コンポーネントの Anchor を Middle left にし、Alignment を Left にします。
1. DebugLog スクリプトがなければ新規作成
## DebugLog スクリプト
```csharp
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using HoloToolkit.Unity;

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

# Gesture
## エアタップ
### ねらい
オブジェクトを gaze した状態でエアタップし、オブジェクトの色を変えます
### 準備
1. Cube を作り、Test Cube と名前を付けます
1. Transform を Position(0, 0, 2), Scale(0.1, 0.1, 0.1) にする
### MyGazeGestureManager スクリプト
1. Assets / MyHolographicAcademy / 211 / Scripts フォルダを新規作成
2. 新規スクリプトを作成

```csharp
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    /// <summary>
    /// ジェスチャーのエアタップを監視して、エアタップされたら、
    /// そのとき gaze でフォーカスしていたオブジェクトに OnSelect メッセージを送る
    /// </summary>
    public class MyGazeGestureManager : Singleton<MyGazeGestureManager>
    {
        #region Public Valuables

        /// <summary>
        /// gaze されているホログラムを格納する
        /// </summary>
        private GameObject FocusedObject;

        /// <summary>
        /// GestureRecognizer を保持するための変数
        /// </summary>
        private GestureRecognizer recognizer;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// エアタップを監視するイベントを登録する
        /// </summary>
        private void Start()
        {
            // Select ジェスチャーを感知するために GestureRecognizer を準備する
            this.recognizer = new GestureRecognizer();
            this.recognizer.TappedEvent += this.Recognizer_TappedEvent;
            this.recognizer.StartCapturingGestures();
        }

        /// <summary>
        /// エアタップされたらフォーカスされているオブジェクトに OnSelect メッセージを送る
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="tapCount">tapCount</param>
        /// <param name="headRay">headRay</param>
        private void Recognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
        {
            // フォーカスされたオブジェクトとその親に OnSelect メッセージを送る
            if (this.FocusedObject != null)
            {
                this.FocusedObject.SendMessageUpwards("OnSelect");
            }
        }

        /// <summary>
        /// gaze してフォーカスしているオブジェクトを取得し続ける
        /// </summary>
        private void Update()
        {
            // どのホログラムがこのフレームでフォーカスされているか明らかにする
            GameObject oldFocusedObject = this.FocusedObject;

            this.FocusedObject = GazeManager.Instance.HitObject;

            // もしこのフレームでフォーカスされているオブジェクトが変わった場合は、
            // 新しいジェスチャーを探し始める
            if (this.FocusedObject != oldFocusedObject)
            {
                this.recognizer.CancelGestures();
                this.recognizer.StartCapturingGestures();
            }
        }

        #endregion
    }
}
```
1. ヒエラルキービューで EmptyObject として manager オブジェクトを作ります
1. 上記のスクリプトを manager オブジェクトにアタッチ
### CubeCommand スクリプト
1. 新規スクリプトを作成
```csharp
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    /// <summary>
    /// OnSelect メッセージを受け取ると色を切り替える
    /// </summary>
    public class CubeCommands : MonoBehaviour
    {
        #region Private Valuables

        /// <summary>
        /// GameObject のマテリアル
        /// </summary>
        private Material material;

        /// <summary>
        /// 表示している色が青か否か
        /// </summary>
        private bool isBlue;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// 表示色を青にする
        /// </summary>
        private void Awake()
        {
            this.material = this.gameObject.GetComponent<Renderer>().material;
            this.material.SetColor("_Color", Color.blue);
            this.isBlue = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// ユーザーが Select ジェスチャーを行ったときに MyGazeGestureManager から呼ばれる
        /// </summary>
        public void OnSelect()
        {
            DebugLog.Instance.Log += "OnSelect\n";
            if (this.isBlue)
            {
                this.material.SetColor("_Color", Color.red);
            }
            else
            {
                this.material.SetColor("_Color", Color.blue);
            }

            this.isBlue = !this.isBlue;
        }

        #endregion
    }
}
```
1. スクリプトを Test Cube にアタッチする
### 動作確認
2. タップするごとに色が変わります

![Gesture Gesture Recognizer01](Readme_Data/Gesture_GestureRecognizer01.png)

![Gesture Gesture Recognizer02](Readme_Data/Gesture_GestureRecognizer02.png)

## 手の検出
1. 新規スクリプトを作成
### MyHandManager スクリプト
```csharp
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    /// <summary>
    /// HandsManager は手を感知する
    /// </summary>
    public class MyHandsManager : Singleton<MyHandsManager>
    {
        #region Public Valuables

        [Tooltip("Audio clip to play when Finger Pressed.")]
        public AudioClip FingerPressedSound;

        /// <summary>
        /// 手が感知されたか否か
        /// </summary>
        public bool HandDetected
        {
            get;
            private set;
        }

        /// <summary>
        /// 手がインタラクトしている GameObject を保持する
        /// </summary>
        public GameObject FocusedGameObject { get; private set; }

        #endregion

        #region Private Valuables

        /// <summary>
        /// エアタップしたときに音を鳴らすための AudioSource
        /// </summary>
        private AudioSource audioSource;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// オーディオクリップを使用可能にし、手の動きを検知するイベントを登録する
        /// </summary>
        protected override void Awake()
        {
            this.EnableAudioHapticFeedback();

            InteractionManager.InteractionSourceDetected += this.InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourceLost     += this.InteractionManager_InteractionSourceLost;
            InteractionManager.InteractionSourcePressed  += this.InteractionManager_InteractionSourcePressed;
            InteractionManager.InteractionSourceReleased += this.InteractionManager_InteractionSourceReleased;

            // FocusedGameObject を null として初期化する
            this.FocusedGameObject = null;
        }

        /// <summary>
        /// 手の動きを検知するイベントを解除する
        /// </summary>
        protected override void OnDestroy()
        {
            InteractionManager.InteractionSourceDetected -= this.InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourceLost     -= this.InteractionManager_InteractionSourceLost;
            InteractionManager.InteractionSourceReleased -= this.InteractionManager_InteractionSourceReleased;
            InteractionManager.InteractionSourcePressed  -= this.InteractionManager_InteractionSourcePressed;
        }

        #endregion

        #region InteractionSource CallBacks

        /// <summary>
        /// 手を感知した
        /// </summary>
        /// <param name="obj">obj</param>
        private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs obj)
        {
            DebugLog.Instance.Log += "Hand Detected\n";
            this.HandDetected = true;
        }

        /// <summary>
        /// 手を見失った
        /// </summary>
        /// <param name="obj">obj</param>
        private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs obj)
        {
            DebugLog.Instance.Log += "Hand Lost\n";
            this.HandDetected = false;

            // FocusedGameObject をリセットする
            this.ResetFocusedGameObject();
        }

        /// <summary>
        /// 指が倒された
        /// </summary>
        /// <param name="hand">hand</param>
        private void InteractionManager_InteractionSourcePressed(InteractionSourcePressedEventArgs hand)
        {
            DebugLog.Instance.Log += "Pressed\n";
            if (GazeManager.Instance.HitObject != null)
            {
                // オーディオソースがあり、再生中でなければ、音を鳴らす
                if (this.audioSource != null && !this.audioSource.isPlaying)
                {
                    this.audioSource.Play();
                }

                this.FocusedGameObject = GazeManager.Instance.HitObject;
            }
        }

        /// <summary>
        /// 指が元の位置に戻った
        /// </summary>
        /// <param name="hand">hand</param>
        private void InteractionManager_InteractionSourceReleased(InteractionSourceReleasedEventArgs hand)
        {
            DebugLog.Instance.Log += "Rleased\n";
            
            // FocusedGameObject をリセットする
            this.ResetFocusedGameObject();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// オーディオクリップがセットされていれば、そのための AudioSource を追加する
        /// </summary>
        private void EnableAudioHapticFeedback()
        {
            if (this.FingerPressedSound != null)
            {
                this.audioSource = this.gameObject.GetComponent<AudioSource>();
                if (this.audioSource == null)
                {
                    this.audioSource = this.gameObject.AddComponent<AudioSource>();
                }

                this.audioSource.clip = this.FingerPressedSound;
                this.audioSource.playOnAwake = false;
                this.audioSource.spatialBlend = 1;
                this.audioSource.dopplerLevel = 0;
            }
        }

        /// <summary>
        /// FocusedGameObject をリセットする
        /// </summary>
        private void ResetFocusedGameObject()
        {
            // FocusedGameObject を null にする
            this.FocusedGameObject = null;

            // 2.a: On GestureManager call ResetGestureRecognizers
            // to complete any currently active gestures.
            // GestureManager.Instance.ResetGestureRecognizers();
        }

        #endregion
    }
}
```
1. 上記のスクリプトを manager オブジェクトにアタッチ
### 動作確認
2. 視野内に手が入ると「Detected」と表示されます。

![Gesture Hand Manager](Readme_Data/Gesture_HandManager.png)

## ナビゲーション
### ねらい
ナビゲーションの値がどのように変化するかを確認する
### MyHandsManager
1. manager オブジェクトに新規スクリプトを追加します

```csharp
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    /// <summary>
    /// HandsManager は手を感知する
    /// </summary>
    public class MyHandsManager : Singleton<MyHandsManager>
    {
        #region Public Valuables

        [Tooltip("Audio clip to play when Finger Pressed.")]
        public AudioClip FingerPressedSound;

        /// <summary>
        /// 手が感知されたか否か
        /// </summary>
        public bool HandDetected
        {
            get;
            private set;
        }

        /// <summary>
        /// 手がインタラクトしている GameObject を保持する
        /// </summary>
        public GameObject FocusedGameObject { get; private set; }

        #endregion

        #region Private Valuables

        /// <summary>
        /// エアタップしたときに音を鳴らすための AudioSource
        /// </summary>
        private AudioSource audioSource;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// オーディオクリップを使用可能にし、手の動きを検知するイベントを登録する
        /// </summary>
        protected override void Awake()
        {
            this.EnableAudioHapticFeedback();

            InteractionManager.InteractionSourceDetected += this.InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourceLost     += this.InteractionManager_InteractionSourceLost;
            InteractionManager.InteractionSourcePressed  += this.InteractionManager_InteractionSourcePressed;
            InteractionManager.InteractionSourceReleased += this.InteractionManager_InteractionSourceReleased;

            // FocusedGameObject を null として初期化する
            this.FocusedGameObject = null;
        }

        /// <summary>
        /// 手の動きを検知するイベントを解除する
        /// </summary>
        protected override void OnDestroy()
        {
            InteractionManager.InteractionSourceDetected -= this.InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourceLost     -= this.InteractionManager_InteractionSourceLost;
            InteractionManager.InteractionSourceReleased -= this.InteractionManager_InteractionSourceReleased;
            InteractionManager.InteractionSourcePressed  -= this.InteractionManager_InteractionSourcePressed;
        }

        #endregion

        #region InteractionSource CallBacks

        /// <summary>
        /// 手を感知した
        /// </summary>
        /// <param name="obj">obj</param>
        private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs obj)
        {
            DebugLog.Instance.Log += "Hand Detected\n";
            this.HandDetected = true;
        }

        /// <summary>
        /// 手を見失った
        /// </summary>
        /// <param name="obj">obj</param>
        private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs obj)
        {
            DebugLog.Instance.Log += "Hand Lost\n";
            this.HandDetected = false;

            // FocusedGameObject をリセットする
            this.ResetFocusedGameObject();
        }

        /// <summary>
        /// 指が倒された
        /// </summary>
        /// <param name="hand">hand</param>
        private void InteractionManager_InteractionSourcePressed(InteractionSourcePressedEventArgs hand)
        {
            DebugLog.Instance.Log += "Pressed\n";
            if (GazeManager.Instance.HitObject != null)
            {
                // オーディオソースがあり、再生中でなければ、音を鳴らす
                if (this.audioSource != null && !this.audioSource.isPlaying)
                {
                    this.audioSource.Play();
                }

                this.FocusedGameObject = GazeManager.Instance.HitObject;
            }
        }

        /// <summary>
        /// 指が元の位置に戻った
        /// </summary>
        /// <param name="hand">hand</param>
        private void InteractionManager_InteractionSourceReleased(InteractionSourceReleasedEventArgs hand)
        {
            DebugLog.Instance.Log += "Rleased\n";
            
            // FocusedGameObject をリセットする
            this.ResetFocusedGameObject();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// オーディオクリップがセットされていれば、そのための AudioSource を追加する
        /// </summary>
        private void EnableAudioHapticFeedback()
        {
            if (this.FingerPressedSound != null)
            {
                this.audioSource = this.gameObject.GetComponent<AudioSource>();
                if (this.audioSource == null)
                {
                    this.audioSource = this.gameObject.AddComponent<AudioSource>();
                }

                this.audioSource.clip = this.FingerPressedSound;
                this.audioSource.playOnAwake = false;
                this.audioSource.spatialBlend = 1;
                this.audioSource.dopplerLevel = 0;
            }
        }

        /// <summary>
        /// FocusedGameObject をリセットする
        /// </summary>
        private void ResetFocusedGameObject()
        {
            // FocusedGameObject を null にする
            this.FocusedGameObject = null;

            // 2.a: On GestureManager call ResetGestureRecognizers
            // to complete any currently active gestures.
            // GestureManager.Instance.ResetGestureRecognizers();
        }

        #endregion
    }
}
```

### MyGestureAction スクリプト
1. Test Cube に新規スクリプトを追加する

```csharp
using UnityEngine;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    /// <summary>
    /// GestureAction performs custom actions based on
    /// which gesture is being performed.
    /// </summary>
    public class MyGestureAction : MonoBehaviour
    {
        #region Public Valuables

        [Tooltip("ナビゲーションの繊細性")]
        public float RotationSensitivity = 0.002f;

        private Vector3 manipulationPreviousPosition;

        /// <summary>
        /// 回転速度
        /// </summary>
        private float rotationFactor;

        #endregion

        #region MonoBehaviour Lifecycle

        /// <summary>
        /// することは、回転だけ
        /// </summary>
        private void Update()
        {
            this.PerformRotation();
        }

        /// <summary>
        /// NavigationX の値から回転速度を決め、GameObject を回転させる
        /// </summary>
        private void PerformRotation()
        {
            if (MyGestureManager.Instance.IsNavigating &&
            MyHandsManager.Instance.FocusedGameObject == this.gameObject)
        {
                // MyGestureManager の NavigationPosition.X に RotationSensitivity をかけて、
                // rotationFactor（回転速度）を計算する
                this.rotationFactor = MyGestureManager.Instance.NavigationPosition.x * this.RotationSensitivity;

                // rotationFactor を使って Y 軸に対して transform.Rotate する
                transform.Rotate(new Vector3(0, -1 * rotationFactor, 0));
            }
        }

        #endregion

        #region Not Used

        void PerformManipulationStart(Vector3 position)
        {
            manipulationPreviousPosition = position;
        }

        void PerformManipulationUpdate(Vector3 position)
        {
            if (MyGestureManager.Instance.IsManipulating)
            {
                /* TODO: DEVELOPER CODING EXERCISE 4.a */

                Vector3 moveVector = Vector3.zero;

                // 4.a: Calculate the moveVector as position - manipulationPreviousPosition.

                // 4.a: Update the manipulationPreviousPosition with the current position.

                // 4.a: Increment this transform&#39;s position by the moveVector.
            }
        }

        #endregion
    }
}
```

### 動作確認
1. NavigationPosition の値が -1.0 から 1.0 までの値を取ります

![Gesture Navigation](Readme_Data/Gesture_Navigation.png)