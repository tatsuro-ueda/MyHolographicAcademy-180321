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
        /// GameObject の現在の色
        /// </summary>
        private Color colorNow;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// マテリアルコンポーネントを取得し、青色で初期化する
        /// </summary>
        private void Awake()
        {
            this.material = this.gameObject.GetComponent<Renderer>().material;
            this.ChangeColor(Color.blue);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// IInputClickHandler インターフェースの実装
        /// クリックするたびに色を変える
        /// </summary>
        /// <param name="eventData">eventData</param>
        public void OnInputClicked(InputClickedEventData eventData)
        {
            DebugLog.Instance.Log += "OnInputClicked\n";
            this.ChangeColor();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 現在の色に応じて色を変える
        /// </summary>
        private void ChangeColor()
        {
            if (this.colorNow == Color.red)
            {
                this.ChangeColor(Color.green);
            }
            else if (this.colorNow == Color.green)
            {
                this.ChangeColor(Color.blue);
            }
            else if (this.colorNow == Color.blue)
            {
                this.ChangeColor(Color.yellow);
            }
            else if (this.colorNow == Color.yellow)
            {
                this.ChangeColor(Color.red);
            }
            else
            {
                throw new Exception("colorNow がおかしいです");
            }
        }

        /// <summary>
        /// 特定の色に変える
        /// </summary>
        /// <param name="color">色</param>
        private void ChangeColor(Color color)
        {
            this.colorNow = color;
            this.material.SetColor("_Color", this.colorNow);
        }

        #endregion
    }
}