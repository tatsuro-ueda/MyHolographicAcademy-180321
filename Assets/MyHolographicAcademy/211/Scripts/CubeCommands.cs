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