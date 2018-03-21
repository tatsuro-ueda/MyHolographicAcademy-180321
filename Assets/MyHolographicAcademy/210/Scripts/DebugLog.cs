﻿using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace EDUCATION.FEELPHYSICS.MY_HOLOGRAPHIC_ACADEMY
{
    /// <summary>
    /// 書いたスクリプトが正常に動作しているか、3D Textに表示して確認する
    /// </summary>
    public class DebugLog : MonoBehaviour
    {
        #region Public Valiables

        [Tooltip("3D Text の Text Mesh")]
        public TextMesh MyTextMesh;

        /// <summary>
        /// 本クラスをシングルトン化するための変数
        /// </summary>
        public static DebugLog Instance;

        /// <summary>
        /// 表示するメッセージを受け取るための public 変数
        /// </summary>
        public string Log = "";

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// 本クラスをシングルトン化する
        /// </summary>
        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// スクリプトの public 変数を表示し続ける
        /// </summary>
        private void Update()
        {
            this.MyTextMesh.text = ""
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
            RaycastHit hitInfo = GazeManager.Instance.HitInfo;
            string focusedName;

            if (hitInfo.collider != null)
            {
                // hitInfo に格納された衝突した GameObject を FocusedGameObject に割り当てる
                focusedName = hitInfo.collider.gameObject.name;
            }
            else
            {
                focusedName = "null";
            }

            return focusedName;
        }

        #endregion
    }
}