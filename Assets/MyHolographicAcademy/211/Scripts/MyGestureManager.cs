using HoloToolkit.Unity;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    /// <summary>
    /// ナビゲーションとマニピュレーションのジェスチャーを認識し、フォーカスしているオブジェクトにメッセージを渡す
    /// </summary>
    public class MyGestureManager : Singleton<MyGestureManager>
    {
        #region Public Valuables

        /// <summary>
        /// タップとナビゲーションのジェスチャー認識器
        /// </summary>
        public GestureRecognizer NavigationRecognizer { get; private set; }

        /// <summary>
        /// マニピュレーションのジェスチャー認識器
        /// </summary>
        public GestureRecognizer ManipulationRecognizer { get; private set; }

        /// <summary>
        /// 現在アクティブなジェスチャー認識器
        /// </summary>
        public GestureRecognizer ActiveRecognizer { get; private set; }

        /// <summary>
        /// 現在ナビゲーションしているか否か
        /// </summary>
        public bool IsNavigating { get; private set; }

        /// <summary>
        /// ナビゲーションの位置
        /// </summary>
        public Vector3 NavigationPosition { get; private set; }

        /// <summary>
        /// 現在マニピュレーションしているか否か
        /// </summary>
        public bool IsManipulating { get; private set; }

        /// <summary>
        /// マニピュレーションの位置
        /// </summary>
        public Vector3 ManipulationPosition { get; private set; }

        #endregion

        #region MonoBehaviour Lifecycle

        /// <summary>
        /// ナビゲーションとマニピュレーションのイベントを登録する
        /// </summary>
        protected override void Awake()
        {
            // NavigationRecognizer をインスタンス化する
            this.NavigationRecognizer = new GestureRecognizer();

            // Tap と NaviagationX を NavigationRecognizer の RecognizableGestures に追加する
            this.NavigationRecognizer.SetRecognizableGestures(
                GestureSettings.Tap |
                GestureSettings.NavigationX);

            // 2.b: Register for the TappedEvent with the NavigationRecognizer_TappedEvent function.
            //NavigationRecognizer.TappedEvent += NavigationRecognizer_TappedEvent;

            // ナビゲーションイベントを ManipulationRecognizer に登録する
            this.NavigationRecognizer.NavigationStartedEvent += this.NavigationRecognizer_NavigationStartedEvent;
            this.NavigationRecognizer.NavigationUpdatedEvent += this.NavigationRecognizer_NavigationUpdatedEvent;
            this.NavigationRecognizer.NavigationCompletedEvent += this.NavigationRecognizer_NavigationCompletedEvent;
            this.NavigationRecognizer.NavigationCanceledEvent += this.NavigationRecognizer_NavigationCanceledEvent;

            // ManipulationRecognizer をインスタンス化する
            this.ManipulationRecognizer = new GestureRecognizer();

            // ManipulationTranslate を ManipulationRecognizer の RecognizableGestures に追加する
            this.ManipulationRecognizer.SetRecognizableGestures(
                GestureSettings.ManipulationTranslate);

            // マニピュレーションイベントを ManipulationRecognizer に登録する
            this.ManipulationRecognizer.ManipulationStartedEvent += this.ManipulationRecognizer_ManipulationStartedEvent;
            this.ManipulationRecognizer.ManipulationUpdatedEvent += this.ManipulationRecognizer_ManipulationUpdatedEvent;
            this.ManipulationRecognizer.ManipulationCompletedEvent += this.ManipulationRecognizer_ManipulationCompletedEvent;
            this.ManipulationRecognizer.ManipulationCanceledEvent += this.ManipulationRecognizer_ManipulationCanceledEvent;

            this.ResetGestureRecognizers();
        }

        /// <summary>
        /// イベントをすべて解除する
        /// </summary>
        protected override void OnDestroy()
        {
            // 2.b: Unregister the Tapped and Navigation events on the NavigationRecognizer.
            //NavigationRecognizer.TappedEvent -= NavigationRecognizer_TappedEvent;

            // ナビゲーションイベントを ManipulationRecognizer から解除する
            this.NavigationRecognizer.NavigationStartedEvent -= this.NavigationRecognizer_NavigationStartedEvent;
            this.NavigationRecognizer.NavigationUpdatedEvent -= this.NavigationRecognizer_NavigationUpdatedEvent;
            this.NavigationRecognizer.NavigationCompletedEvent -= this.NavigationRecognizer_NavigationCompletedEvent;
            this.NavigationRecognizer.NavigationCanceledEvent -= this.NavigationRecognizer_NavigationCanceledEvent;

            // マニピュレーションイベントを ManipulationRecognizer から解除する
            this.ManipulationRecognizer.ManipulationStartedEvent -= this.ManipulationRecognizer_ManipulationStartedEvent;
            this.ManipulationRecognizer.ManipulationUpdatedEvent -= this.ManipulationRecognizer_ManipulationUpdatedEvent;
            this.ManipulationRecognizer.ManipulationCompletedEvent -= this.ManipulationRecognizer_ManipulationCompletedEvent;
            this.ManipulationRecognizer.ManipulationCanceledEvent -= this.ManipulationRecognizer_ManipulationCanceledEvent;
        }

        /// <summary>
        /// デフォルトの GestureRecognizer に戻る
        /// </summary>
        public void ResetGestureRecognizers()
        {
            // ナビゲーションのジェスチャー認識に戻る
            this.Transition(this.NavigationRecognizer);
        }

        /// <summary>
        /// 新しい GestureRecognizer に移る
        /// </summary>
        /// <param name="newRecognizer">移る先の GestureRecognizer</param>
        public void Transition(GestureRecognizer newRecognizer)
        {
            if (newRecognizer == null)
            {
                return;
            }

            if (this.ActiveRecognizer != null)
            {
                if (this.ActiveRecognizer == newRecognizer)
                {
                    return;
                }

                this.ActiveRecognizer.CancelGestures();
                this.ActiveRecognizer.StopCapturingGestures();
            }

            newRecognizer.StartCapturingGestures();
            this.ActiveRecognizer = newRecognizer;
        }

        #endregion

        #region Event Callbacks

        /// <summary>
        /// ナビゲーション開始
        /// </summary>
        /// <param name="source"></param>
        /// <param name="relativePosition"></param>
        /// <param name="ray"></param>
        private void NavigationRecognizer_NavigationStartedEvent(InteractionSourceKind source, Vector3 relativePosition, Ray ray)
        {
            // IsNavigating を true にする
            this.IsNavigating = true;

            // NavigationPosition を relativePosition にする
            this.NavigationPosition = relativePosition;
        }

        /// <summary>
        /// ナビゲーション中
        /// </summary>
        /// <param name="source"></param>
        /// <param name="relativePosition"></param>
        /// <param name="ray"></param>
        private void NavigationRecognizer_NavigationUpdatedEvent(InteractionSourceKind source, Vector3 relativePosition, Ray ray)
        {
            // IsNavigating を true にする
            this.IsNavigating = true;

            // NavigationPosition を relativePosition にする
            this.NavigationPosition = relativePosition;

            DebugLog.Instance.Log = "NavigationPosition: " + NavigationPosition.ToString();
        }

        /// <summary>
        /// ナビゲーション終了
        /// </summary>
        /// <param name="source"></param>
        /// <param name="relativePosition"></param>
        /// <param name="ray"></param>
        private void NavigationRecognizer_NavigationCompletedEvent(InteractionSourceKind source, Vector3 relativePosition, Ray ray)
        {
            // IsNavigating を false にする
            this.IsNavigating = false;
        }

        /// <summary>
        /// ナビゲーションが何らかの原因でキャンセルされた
        /// </summary>
        /// <param name="source"></param>
        /// <param name="relativePosition"></param>
        /// <param name="ray"></param>
        private void NavigationRecognizer_NavigationCanceledEvent(InteractionSourceKind source, Vector3 relativePosition, Ray ray)
        {
            // IsNavigating を false にする
            this.IsNavigating = false;
        }

        /// <summary>
        /// マニピュレーション開始
        /// </summary>
        /// <param name="source"></param>
        /// <param name="position"></param>
        /// <param name="ray"></param>
        private void ManipulationRecognizer_ManipulationStartedEvent(InteractionSourceKind source, Vector3 position, Ray ray)
        {
            if (MyHandsManager.Instance.FocusedGameObject != null)
            {
                this.IsManipulating = true;

                this.ManipulationPosition = position;

                MyHandsManager.Instance.FocusedGameObject.SendMessageUpwards("PerformManipulationStart", position);
            }
        }

        /// <summary>
        /// マニピュレーション中
        /// </summary>
        /// <param name="source"></param>
        /// <param name="position"></param>
        /// <param name="ray"></param>
        private void ManipulationRecognizer_ManipulationUpdatedEvent(InteractionSourceKind source, Vector3 position, Ray ray)
        {
            if (MyHandsManager.Instance.FocusedGameObject != null)
            {
                this.IsManipulating = true;

                this.ManipulationPosition = position;

                MyHandsManager.Instance.FocusedGameObject.SendMessageUpwards("PerformManipulationUpdate", position);
            }
        }

        /// <summary>
        /// マニピュレーション終了
        /// </summary>
        /// <param name="source"></param>
        /// <param name="position"></param>
        /// <param name="ray"></param>
        private void ManipulationRecognizer_ManipulationCompletedEvent(InteractionSourceKind source, Vector3 position, Ray ray)
        {
            this.IsManipulating = false;
        }

        /// <summary>
        /// マニピュレーションが何らかの原因でキャンセルされた
        /// </summary>
        /// <param name="source"></param>
        /// <param name="position"></param>
        /// <param name="ray"></param>
        private void ManipulationRecognizer_ManipulationCanceledEvent(InteractionSourceKind source, Vector3 position, Ray ray)
        {
            this.IsManipulating = false;
        }

        /*
        private void NavigationRecognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray ray)
        {
            GameObject focusedObject = InteractibleManager.Instance.FocusedGameObject;

            if (focusedObject != null)
            {
                focusedObject.SendMessageUpwards("OnSelect");
            }
        }
        */

        #endregion
    }
}