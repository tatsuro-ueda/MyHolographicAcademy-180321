# My Holographic Academy 210（日本語）

![Gaze Interactable2 02](Readme_Data/gaze-interactable2-02.png)

Unity や MRTK のバージョンに依存せずに HoloAcademy 210 を学習できるようにしたものです。
# 準備
## プロジェクトの準備
1. Unityでプロジェクトを新規作成
1. MRTKを導入
1. メニュー＞Mixed Reality Toolkit＞Configure＞Apply Mixed Reality Project Setting（チェックボックスはデフォルト）
1. Asset / HolographicAcademy / 210 フォルダを新規作成
## シーンの準備
1. 210 フォルダの中で Gazeシーンを新規作成
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
1. Asset / Scripts フォルダを新規作成
1. MyGazeManager スクリプトを新規作成

```csharp
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace EDUCATION.FEELPHYSICS.MY_HOLOGRAPHIC_ACADEMY
{
    public class MyGazeManager : MonoBehaviour
    {
        #region Public Valiables

        /// <summary>
        /// MyGazeManager はユーザーの gaze の場所（当たった場所と法線）を決定する
        /// </summary>
        public static MyGazeManager Instance;

        [Tooltip("gazeが当たるかどうか計算する最大距離")]
        public float MaxGazeDistance = 5.0f;

        [Tooltip("視線がターゲットとするレイヤーを選んで下さい")]
        public LayerMask RaycastLayerMask = Physics.DefaultRaycastLayers;

        /// <summary>
        /// Physics.Raycast がホログラムに当たると true を返す
        /// </summary>
        public bool Hit { get; private set; }

        /// <summary>
        /// HitInfo プロパティを使って
        /// RaycastHit の public なメンバーにアクセスすることができる
        /// </summary>
        public RaycastHit HitInfo { get; private set; }

        /// <summary>
        /// ユーザの gaze の位置
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// RaycastHit の法線方向
        /// </summary>
        public Vector3 Normal { get; private set; }

        #endregion

        #region Private Valuables

        /// <summary>
        /// gaze スタビライザー
        /// </summary>
        private GazeStabilizer gazeStabilizer;

        /// <summary>
        /// gaze の始点
        /// </summary>
        private Vector3 gazeOrigin;

        /// <summary>
        /// gaze の方向
        /// </summary>
        private Vector3 gazeDirection;

        #endregion

        #region MonoBehavior CallBacks

        /// <summary>
        /// 本クラスをシングルトン化する
        /// </summary>
        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// gaze の Raycast の hitInfo を更新し続ける
        /// </summary>
        private void Update()
        {
            // メインカメラの位置をを gazeOrigin 割り当てる
            this.gazeOrigin = Camera.main.transform.position;

            // メインカメラの transform の前方向を gazeDirection に割り当てる
            this.gazeDirection = Camera.main.transform.forward;

            this.UpdateRaycast();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Raycast が当たる場所と法線を計算する
        /// </summary>
        private void UpdateRaycast()
        {
            // RaycastHit 型のhitInfo 変数をつくる
            RaycastHit hitInfo;

            // Unity の Physics Raycast を実行する
            // public なプロパティである Hit の中に返り値を集める
            // 始点を gazeOrigin、方向を gazeDirection として pass する
            // TODO: pass する？
            // hitInfo の情報を集める
            // MaxGazeDistance と RaycastLayerMask を pass する
            this.Hit = Physics.Raycast(
                this.gazeOrigin,
                this.gazeDirection,
                out hitInfo,
                this.MaxGazeDistance,
                this.RaycastLayerMask);

            // 他のクラスがアクセスできるように、hitoInfo 変数を HitInfo の public プロパティに割り当てる
            this.HitInfo = hitInfo;

            if (this.Hit)
            {
                // もし raycast がホログラムに当たったら…

                // プロパティ Position に hitInto の点を割り当てる
                this.Position = hitInfo.point;

                // プロパティ Normal に hitInfo の法線を割り当てる
                this.Normal = hitInfo.normal;
            }
            else
            {
                // もし raycast がホログラムに当たらなかったら…
                // デフォルト値を保存する…

                // プロパティ Position に gazeOrigin ＋ MaxgazeDistance × gazeDirection を割り当てる
                this.Position = this.gazeOrigin + (this.gazeDirection * this.MaxGazeDistance);

                // プロパティ Normal に gazeDirection を割り当てる
                this.Normal = this.gazeDirection;
            }
        }


        #endregion
    }
}
```

https://github.com/weed/MyHolographicAcademy-180321/blob/cd79ad885d1418701d72760ff681d482fc318869/Assets/HolographicAcademy/Scripts/MyGazeManager.cs

1. MyGazeManager スクリプトを managers オブジェクトにアタッチ
### 動作確認
1. [Hierarchy] パネル上部の [Create] メニューをクリックします。
1. [3D Object > Sphere] を選びます。
1. 新しい GameObject を右クリックして、名前を「Test Sphere」に変更します。
1. Debug Log の Transform の Position を (0, 0, 2) に、Scale を (0.1, 0.1, 0.1) にします。
1. DebugLog スクリプトを新規作成

```csharp
using UnityEngine;

namespace EDUCATION.FEELPHYSICS.MY_HOLOGRAPHIC_ACADEMY
{
    /// <summary>
    /// 書いたスクリプトが正常に動作しているか、3D Textに表示して確認する
    /// </summary>
    public class DebugLog : MonoBehaviour
    {

        [Tooltip("3D Text の Text Mesh")]
        public TextMesh MyTextMesh;

        /// <summary>
        /// スクリプトの public 変数を表示し続ける
        /// </summary>
        private void Update()
        {
            this.MyTextMesh.text = 
                "Position: " + MyGazeManager.Instance.Position.ToString() +
                "\nNormal: " + MyGazeManager.Instance.Normal.ToString();
        }
    }
}
```

https://github.com/weed/MyHolographicAcademy-180321/blob/cd79ad885d1418701d72760ff681d482fc318869/Assets/HolographicAcademy/Scripts/DebugLog.cs#L1-L24

1. DebugLog コンポーネントの My Text Mesh 欄に Text Mesh コンポーネントをドラッグします。
1. Playし、右クリックしたままドラッグして gaze の結果が変化するのを確認します。

![Gaze01](Readme_Data/gaze01.png)

![Gaze02](Readme_Data/gaze02.png)

## カーソル
### ねらい
次に、MyCusorManager.cs を編集して、以下のことを行います。
1. アクティブにするカーソルを決めます。
2. カーソルがホログラム上にあるかどうかに応じてカーソルを更新します。
3. ユーザーの視線の先にカーソルを位置付けます。
### MyCursorManager スクリプト

```csharp
using UnityEngine;

namespace EDUCATION.FEELPHYSICS.MY_HOLOGRAPHIC_ACADEMY
{
    /// <summary>
    /// MyCursorManager クラスは Cursor GameObject（複数）を扱います
    /// ホログラム上に表示されるものもありますし、ホログラムを外れたときに表示されるものもあります
    /// ホログラムにヒットしたかどうかで、適切なカーソルを表示します
    /// ヒットした位置に適切なカーソルを設置します
    /// カーソルの法線はヒットした表面の法線に一致します
    /// </summary>
    public class MyCursorManager : MonoBehaviour
    {
        #region Public Valuables

        /// <summary>
        /// シングルトン化するための変数
        /// </summary>
        public static MyCursorManager Instance;

        [Tooltip("ホログラムにヒットしているときに表示するカーソルオブジェクトをドラッグして下さい")]
        public GameObject CursorOnHolograms;

        [Tooltip("ホログラムにヒットしていないときに表示するカーソルオブジェクトをドラッグして下さい")]
        public GameObject CursorOffHolograms;

        #endregion

        #region Private Valuables

        /// <summary>
        /// アクティブなカーソルを入れておく変数（位置と向きを反映させるのに使う）
        /// </summary>
        private GameObject cursor;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// カーソルが指定されていなければスクリプトを終了する
        /// </summary>
        private void Awake()
        {
            if (this.CursorOnHolograms == null || this.CursorOffHolograms == null)
            {
                Debug.LogError("カーソルが指定されていません");
                return;
            }

            // Hide the Cursors to begin with.
            this.CursorOnHolograms.SetActive(false);
            this.CursorOffHolograms.SetActive(false);
        }

        private void Update()
        {
            /* TODO: DEVELOPER CODING EXERCISE 2.b */

            if (MyGazeManager.Instance == null || 
                this.CursorOnHolograms == null || 
                this.CursorOffHolograms == null)
            {
                return;
            }

            if (MyGazeManager.Instance.Hit)
            {
                // ホログラムカーソルをアクティブにして表示する
                this.CursorOnHolograms.SetActive(true);
                // ホログラムカーソルを非アクティブにして非表示にする
                this.CursorOffHolograms.SetActive(false);

                this.cursor = this.CursorOnHolograms;
            }
            else
            {
                // ホログラムなしカーソルをアクティブにして表示する
                this.CursorOffHolograms.SetActive(true);
                // ホログラムなしカーソルを非アクティブにして非表示にする
                this.CursorOnHolograms.SetActive(false);

                this.cursor = this.CursorOffHolograms;
            }

            // GazeManager Position をカーソルオブジェクトの transform.position に割り当てる
            this.cursor.transform.position = MyGazeManager.Instance.Position;

            // GazeManager Normal をカーソルオブジェクトの transform.up に割り当てる
            this.cursor.transform.forward = MyGazeManager.Instance.Normal;
        }

        #endregion
    }
}
```

https://github.com/weed/MyHolographicAcademy-180321/blob/fa47dcbf4651ee2220615cc78029e1755adf2674/Assets/HolographicAcademy/Scripts/MyCursorManager.cs#L1-L95

1. ヒエラルキービューで EmptyObject として manager オブジェクトを作ります
1. 上記のスクリプトを Managers オブジェクトにアタッチ
### 動作確認
1. Asset > HoloToolkit > Input > Prefabs > Cursor > Cursorプレハブをシーンに追加
1. Cursor オブジェクトの Object Cursor コンポーネントをオフにする
1. ヒエラルキービューのCursorを展開してCursorOnHologramsとCursorOffHologramsを表示させる
1. MyCursorManagerコンポーネントのCursor On Holograms欄にCursorOnHologramsをドラッグし、Cursor Off Holograms欄にCursorOffHologramsをドラッグする
1. Playし、カーソルの形が変わるのを確かめる

![Gaze01](Readme_Data/gaze-cursor01.png)

![Gaze02](Readme_Data/gaze-cursor02.png)

## インタラクト
### ねらい
この節は長いです。がんばって下さい。

gaze したオブジェクトに自動的にメッセージを送り、オブジェクトの状態を変えます。
ここでは、gaze すると球の色が変わって音が鳴り、gaze が外れると元の色に戻るスクリプトを作成します。
### オブジェクトの取得
#### ねらい
InteractibleManager.cs は、視線のレイキャストがヒットした位置をフェッチし、ヒットした GameObject を保存します。
#### MyInteractibleManager スクリプト

```csharp
using UnityEngine;

namespace EDUCATION.FEELPHYSICS.MY_HOLOGRAPHIC_ACADEMY
{
    /// <summary>
    /// InteractibleManager は、どの GameObject が現在フォーカスされているかを保持する
    /// </summary>
    public class MyInteractibleManager : MonoBehaviour
    {
        #region Public Valiables

        /// <summary>
        /// このクラスをシングルトンとして使用するための変数
        /// </summary>
        public static MyInteractibleManager Instance;

        public GameObject FocusedGameObject { get; private set; }

        #endregion

        #region Private Valuables

        private GameObject oldFocusedGameObject = null;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// 本クラスをシングルトン化する
        /// </summary>
        private void Awake()
        {
            Instance = this;
        }

        // Use this for initialization
        void Start()
        {
            this.FocusedGameObject = null;
        }

        // Update is called once per frame
        void Update()
        {
            oldFocusedGameObject = FocusedGameObject;

            if (MyGazeManager.Instance.Hit)
            {
                RaycastHit hitInfo = MyGazeManager.Instance.HitInfo;
                if (hitInfo.collider != null)
                {
                    // hitInfo に格納された衝突した GameObject を FocusedGameObject に割り当てる
                    FocusedGameObject = hitInfo.collider.gameObject;
                }
                else
                {
                    FocusedGameObject = null;
                }
            }
            else
            {
                FocusedGameObject = null;
            }
        }

        #endregion
    }
}
```

https://github.com/weed/MyHolographicAcademy-180321/blob/6bde3fb0912eda6a1028a66365f5ebccf65c0c75/Assets/HolographicAcademy/Scripts/MyInteractibleManager.cs#L1-L69

1. スクリプトを Managers オブジェクトにアタッチ
#### 動作確認
#### DebugLog スクリプト
##### Update()

```csharp
        /// <summary>
        /// スクリプトの public 変数を表示し続ける
        /// </summary>
        private void Update()
        {
            var focusedObject = MyInteractibleManager.Instance.FocusedGameObject;
            string focusedName;
            if (focusedObject == null)
            {
                focusedName = "null";
            }
            else
            {
                focusedName = focusedObject.name;
            }

            this.MyTextMesh.text =
                "Position: " + MyGazeManager.Instance.Position.ToString()
                + "\nNormal: " + MyGazeManager.Instance.Normal.ToString()
                + "\nFocusedGameObject: " + focusedName
                ;
        }
```

https://github.com/weed/MyHolographicAcademy-180321/blob/6bde3fb0912eda6a1028a66365f5ebccf65c0c75/Assets/HolographicAcademy/Scripts/DebugLog.cs#L14-L35

1. Playし、ヒットすると対象のオブジェクトの名前が取得できるのを確かめる

![Gaze Interact01](Readme_Data/gaze-interact01.png)

![Gaze Interact02](Readme_Data/gaze-interact02.png)

### メッセージの送信1
#### ねらい
視線の先に操作可能なオブジェクトがある場合は、GazeEntered メッセージを送信します。
操作可能なオブジェクトから視線がそれた場合は、GazeExited メッセージを送信します。
Interactible.cs では、GazeEntered コールバックと GazeExited コールバックを処理します。
#### InteractibleManager スクリプトの更新
##### Update()

```csharp
        private void Update()
        {
            this.oldFocusedGameObject = this.FocusedGameObject;

            if (MyGazeManager.Instance.Hit)
            {
                RaycastHit hitInfo = MyGazeManager.Instance.HitInfo;
                if (hitInfo.collider != null)
                {
                    // hitInfo に格納された衝突した GameObject を FocusedGameObject に割り当てる
                    this.FocusedGameObject = hitInfo.collider.gameObject;
                }
                else
                {
                    this.FocusedGameObject = null;
                }
            }
            else
            {
                this.FocusedGameObject = null;
            }

            // フォーカスした瞬間にフォーカスしたオブジェクトに「GazeEntered」メッセージを送る
            if (this.FocusedGameObject != this.oldFocusedGameObject)
            {
                // フォーカスが外れたオブジェクトに「GazeExited」メッセージを送る
                this.ResetFocusedInteractible();

                if (this.FocusedGameObject != null)
                {
                    if (this.FocusedGameObject.GetComponent<MyInteractible>() != null)
                    {
                        // FocusedGameObject に GazeEntered メッセージを送る
                        this.FocusedGameObject.SendMessage("GazeEntered");
                    }
                }
            }
        }
```

##### region Private Methods

```csharp
        #region Private Methods

        /// <summary>
        /// フォーカスが外れたオブジェクトに「GazeExited」メッセージを送る
        /// </summary>
        private void ResetFocusedInteractible()
        {
            if (this.oldFocusedGameObject != null)
            {
                if (this.oldFocusedGameObject.GetComponent<MyInteractible>() != null)
                {
                    // oldFocusedGameObject に GazeExited メッセージを送る
                    this.oldFocusedGameObject.SendMessage("GazeExited");
                }
            }
        }

        #endregion
```

https://github.com/weed/MyHolographicAcademy-180321/blob/701c43af75a48c09f7ff0b67fe3d5c2ad6ff4f62/Assets/MyHolographicAcademy/Scripts/MyInteractibleManager.cs

#### MyInteractible スクリプト

```csharp
using UnityEngine;

namespace EDUCATION.FEELPHYSICS.MY_HOLOGRAPHIC_ACADEMY
{
    /// <summary>
    /// このクラスは GameObject をインタラクト可能にする。gaze されたとき何が起きるかを決定する。
    /// </summary>
    public class MyInteractible : MonoBehaviour
    {
        #region Public Valuables

        [Tooltip("DebugLog オブジェクトをドラッグして下さい")]
        public GameObject MyDebugLog;

        #endregion

        #region Public Methods

        /// <summary>
        /// gaze された瞬間に呼ばれる
        /// </summary>
        public void GazeEntered()
        {
            DebugLog.Instance.Log += gameObject.name + ": GazeEntered\n";
        }

        /// <summary>
        /// gaze が外れた瞬間に呼ばれる
        /// </summary>
        public void GazeExited()
        {
            DebugLog.Instance.Log += gameObject.name + ": GazeExited\n";
        }

        #endregion
    }
}
```

https://github.com/weed/MyHolographicAcademy-180321/blob/701c43af75a48c09f7ff0b67fe3d5c2ad6ff4f62/Assets/MyHolographicAcademy/Scripts/MyInteractible.cs

#### 動作確認
##### DebugLog スクリプト
###### Update()

```csharp
        /// <summary>
        /// スクリプトの public 変数を表示し続ける
        /// </summary>
        private void Update()
        {
            var focusedObject = MyInteractibleManager.Instance.FocusedGameObject;
            string focusedName;
            if (focusedObject == null)
            {
                focusedName = "null";
            }
            else
            {
                focusedName = focusedObject.name;
            }


            this.MyTextMesh.text = ""
                + "Position: " + MyGazeManager.Instance.Position.ToString()
                + "\nNormal: " + MyGazeManager.Instance.Normal.ToString()
                + "\nFocusedGameObject: " + focusedName
                + "\nLog:\n" + this.Log
                ;
        }
```

https://github.com/weed/MyHolographicAcademy-180321/blob/701c43af75a48c09f7ff0b67fe3d5c2ad6ff4f62/Assets/MyHolographicAcademy/Scripts/DebugLog.cs#L37-L59

1. Playし、ヒットすると対象のオブジェクトがメッセージを受け取っているのを確かめる

![Gaze Interactable01](Readme_Data/gaze-interactable01.png)

![Gaze Interactable02](Readme_Data/gaze-interactable02.png)

### メッセージの送信2
#### ねらい
フォーカスすると色が変わって音が鳴るようにします
#### MyInteractible スクリプト

```csharp
using HoloToolkit.Unity.InputModule;
using UnityEngine;


namespace EDUCATION.FEELPHYSICS.MY_HOLOGRAPHIC_ACADEMY
{
    /// <summary>
    /// このクラスは GameObject をインタラクト可能にする。gaze されたとき何が起きるかを決定する。
    /// </summary>
    public class MyInteractible : MonoBehaviour, IPointerSpecificFocusable
    {
        #region Public Valiables


        [Tooltip("ホログラムにインタラクトしたときに鳴らすオーディオクリップ")]
        public AudioClip TargetFeedbackSound;


        #endregion


        #region Private Valiables


        /// <summary>
        /// フォーカスしたときにオーディオクリップを再生するためのもの
        /// </summary>
        private AudioSource audioSource;


        /// <summary>
        /// フォーカスする対象の GameObject に当たっているマテリアルの配列
        /// </summary>
        private Material[] defaultMaterials;


        #endregion


        #region MonoBehaviour CallBacks


        /// <summary>
        /// もし Collider がなければ追加する
        /// ＋オーディオクリップを再生する準備をする
        /// </summary>
        private void Start()
        {
            this.defaultMaterials = GetComponent<Renderer>().materials;


            // もし Collider がなければ追加する
            Collider collider = GetComponentInChildren<Collider>();
            if (collider == null)
            {
                gameObject.AddComponent<BoxCollider>();
            }


            this.EnableAudioHapticFeedback();
        }


        #endregion


        #region IPointerSpecificFocusable CallBacks
 
        /// <summary>
        /// gaze された瞬間に呼ばれる
        /// </summary>
        public void OnFocusEnter(PointerSpecificEventData eventData)
        {
            for (int i = 0; i < this.defaultMaterials.Length; i++)
            {
                this.SetColorWithEmissionGamma(this.defaultMaterials[i], 0.02f);
            }


            if (this.audioSource != null && !this.audioSource.isPlaying)
            {
                this.audioSource.Play();
            }
        }


        /// <summary>
        /// gaze が外れた瞬間に呼ばれる
        /// </summary>
        public void OnFocusExit(PointerSpecificEventData eventData)
        {
            for (int i = 0; i < this.defaultMaterials.Length; i++)
            {
                this.SetColorWithEmissionGamma(this.defaultMaterials[i], 0f);
            }
        }


        #endregion


        #region Private Methods


        /// <summary>
        /// オーディオクリップを再生するための AudioSource を追加する
        /// </summary>
        private void EnableAudioHapticFeedback()
        {
            if (this.TargetFeedbackSound != null)
            {
                this.audioSource = this.GetComponent<AudioSource>();
                if (this.audioSource == null)
                {
                    this.audioSource = this.gameObject.AddComponent<AudioSource>();
                }


                this.audioSource.clip = this.TargetFeedbackSound;
                this.audioSource.playOnAwake = false;
                this.audioSource.spatialBlend = 1;
                this.audioSource.dopplerLevel = 0;
            }
        }


        /// <summary>
        /// マテリアルを明るくする
        /// </summary>
        /// <param name="material">明るくするマテリアル</param>
        /// <param name="gamma">どれくらい明るくするか</param>
        private void SetColorWithEmissionGamma(Material material, float gamma)
        {
            var baseColor = material.color;
            Color finalColor = baseColor * Mathf.LinearToGammaSpace(gamma);
            material.SetColor("_EmissionColor", finalColor);
        }


        #endregion
    }
}
```

https://github.com/weed/MyHolographicAcademy-180321/blob/8c678fdd40de1ccc265962ee74958ea2cf0803e5/Assets/MyHolographicAcademy/210/Scripts/MyInteractible.cs#L1-L121

1. Test Sphere の My Interactible コンポーネントの Target Feedback Sound 欄に Asset > HoloToolkit > UX > Audio > Interaction > Button_Press.wav をドラッグ
#### マテリアルの編集
1. Test Sphere の Default-Material の Emission のチェックをオンにする
#### 動作確認
1. Playし、ヒットすると対象のオブジェクトが明るくなり、音が鳴る

![Gaze Interactable2 01](Readme_Data/gaze-interactable2-01.png)

![Gaze Interactable2 02](Readme_Data/gaze-interactable2-02.png)

### メッセージの送信3
#### ねらい
なるべくMRTKで用意されているスクリプトを使う
#### スクリプトのオンオフ
1. Managers オブジェクトを削除する（InputManager オブジェクトに GazeManager スクリプトがアタッチされている）
1. Cursor の Object Cursor をオンにする
#### MyInteractible スクリプト
##### region Public Methods
1. 削除する
##### IPointerSpecificFocusable インターフェース
1. これを付けて、実装する
##### region IPointerSpecificFocusable CallBacks

```csharp
        #region IPointerSpecificFocusable CallBacks
 
        /// <summary>
        /// gaze された瞬間に呼ばれる
        /// </summary>
        public void OnFocusEnter(PointerSpecificEventData eventData)
        {
            for (int i = 0; i < this.defaultMaterials.Length; i++)
            {
                this.SetColorWithEmissionGamma(this.defaultMaterials[i], 0.02f);
            }

            if (this.audioSource != null && !this.audioSource.isPlaying)
            {
                this.audioSource.Play();
            }
        }

        /// <summary>
        /// gaze が外れた瞬間に呼ばれる
        /// </summary>
        public void OnFocusExit(PointerSpecificEventData eventData)
        {
            for (int i = 0; i < this.defaultMaterials.Length; i++)
            {
                this.SetColorWithEmissionGamma(this.defaultMaterials[i], 0f);
            }
        }

        #endregion
```

https://github.com/weed/MyHolographicAcademy-180321/blob/57bd31a09ceae7c6632830713f28ef2891c5191d/Assets/MyHolographicAcademy/210/Scripts/MyInteractible.cs#L54-L83

#### 動作確認
##### DebugLog スクリプト
1. Playし、ヒットすると対象のオブジェクトが明るくなり、音が鳴る
## ユーザーの方向を向く
### ねらい
あと少しです。オブジェクトが常にユーザーの方向を向くようにします。
### 作業
1. Test Sphereを選択し、右クリックで Cube を作り、Direction Cube という名前を付ける
1. Direction Cube の Transform を Position(0, 0, -0.5), Scale(0.2, 0.2, 0.2) にする
1. Direction Cube の Mesh Renderer コンポーネントの Materials の Element 0 を DebugNormals にする
1. Test Sphere を選択します
1. [Inspector] パネルで [Add Component] をクリックします。
1. メニューの検索ボックスに「Billboard」と入力します。検索結果を選びます。
1. [Inspector] パネルで [Pivot Axis] を [Y] に設定します。
### 動作確認
1. Play し、右クリックしたまま WSAD キーで前後左右に移動することができます。移動しても Direction Cube が自分の方向を向いていることを確認して下さい。
## 追従
### ねらい
部屋の中でホログラムが自分の動きを追うようにする。
### 作業
1. Test Sphere に Tagalong スクリプトをアタッチする
1. Billboard コンポーネントの [Pivot Axis] を [free] に設定します。
### 動作確認
1. Play し、右クリックしたまま WSAD キーで前後左右に移動することができます。Test Sphere が視界の外に出るように移動しても追従してくることを確認して下さい。