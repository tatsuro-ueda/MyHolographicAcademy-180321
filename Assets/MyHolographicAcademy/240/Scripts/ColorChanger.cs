using System;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    /// <summary>
    /// Select されたら GameObject の色を変える
    /// </summary>
    public class ColorChanger : MonoBehaviour, IInputClickHandler
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
        /// ユーザーが エアタップした色を切り替える
        /// </summary>
        public void OnInputClicked(InputClickedEventData eventData)
        {
            DebugLog.Instance.Log += "OnClicked\n";
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