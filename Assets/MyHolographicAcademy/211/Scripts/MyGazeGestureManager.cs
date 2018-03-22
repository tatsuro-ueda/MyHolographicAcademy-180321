using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace EDUCATION.FEELPHYSICS.MY_HOLOGRAPHIC_ACADEMY
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
            DebugLog.Instance.Log += "Event fired\n";

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