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