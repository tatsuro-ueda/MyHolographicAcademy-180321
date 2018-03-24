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