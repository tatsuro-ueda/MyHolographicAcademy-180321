using System;
using System.Collections.Generic;
using HoloToolkit.Sharing;
using HoloToolkit.Sharing.Tests;
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

        #region MonoBehaviour Lifecycle

        /// <summary>
        /// アタッチされた GameObject にフォーカスし続けるようにする
        /// </summary>
        private void Start()
        {
            CustomMessages.Instance.MessageHandlers[CustomMessages.TestMessageID.HeadTransform]
                = this.GetHeadTransform;

            // API 更新
            SharingStage.Instance.SessionsTracker.CurrentUserJoined += SessionsTracker_CurrentUserJoined;
            // .SessionUsersTracker.UserJoined += SessionUsersTracker_UserJoined;
        }

        private void SessionsTracker_CurrentUserJoined(Session obj)
        {
            DebugLog.Instance.Log += "\nCurrent user joined session";

            if (GotTransform)
            {
                CustomMessages.Instance.SendHeadTransform(transform.localPosition, transform.localRotation);
                DebugLog.Instance.Log += "\nCurrentUserJoined GotTransform > localPosition: " + transform.localPosition.ToString();
            }
        }

        private void SessionUsersTracker_UserJoined(User obj)
        {
            DebugLog.Instance.Log += "\nUser joined session";

            if (GotTransform)
            {
                CustomMessages.Instance.SendHeadTransform(transform.localPosition, transform.localRotation);
                DebugLog.Instance.Log += "\nUserJoined GotTransform > localPosition: " + transform.localPosition.ToString();
            }
        }

        /// <summary>
        /// GameObject がまだ配置されていなければ、現在位置の視点の先の点の中間に配置する
        /// </summary>
        private void Update()
        {
            if (GotTransform)
            {
                if (ImportExportAnchorManager.Instance.AnchorEstablished)
                {

                    DebugLog.Instance.Log += "\nAnchor established";
                    // This triggers the animation sequence for the anchor model and
                    // puts the cool materials on the model.
                    this.gameObject.SendMessage("OnSelect");
                }
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, ProposeTransformPosition(), 0.2f);
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

        /// <summary>
        /// リモートシステムが共有するための transform を持っている場合、取得する
        /// </summary>
        /// <param name="msg"></param>
        void GetHeadTransform(NetworkInMessage msg)
        {
            // We read the user ID but we don't use it here.
            msg.ReadInt64();

            transform.localPosition = CustomMessages.Instance.ReadVector3(msg);
            transform.localRotation = CustomMessages.Instance.ReadQuaternion(msg);
            DebugLog.Instance.Log += "\nStart > GetHeadTransform > localPosition: " + transform.localPosition.ToString();

            // The first time, we'll want to send the message to the anchor to do its animation and
            // swap its materials.
            if (GotTransform == false)
            {
                this.gameObject.SendMessage("OnSelect");
            }

            GotTransform = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// エアタップすると「配置済み」としてフォーカスを外す
        /// </summary>
        /// <param name="eventData">eventData</param>
        public void OnInputClicked(InputClickedEventData eventData)
        {
            DebugLog.Instance.Log += "\nOnInputClicked";
            this.OnSelect();
        }

        public void OnSelect()
        {
            DebugLog.Instance.Log += "\nOnSelect";
            this.GotTransform = true;

            CustomMessages.Instance.SendHeadTransform(transform.localPosition, transform.localRotation);
            DebugLog.Instance.Log += "\nOnSelect > localPosition: " + transform.localPosition.ToString();
        }

        #endregion
    }
}