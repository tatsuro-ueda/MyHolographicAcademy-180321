using UnityEngine;

namespace EDUCATION.FEELPHYSICS.MY_HOLOGRAPHIC_ACADEMY
{
    public class CubeCommands : MonoBehaviour
    {

        #region Private Valuables

        private Material material;

        private bool isBlue;

        #endregion

        #region MonoBehaviour CallBacks

        private void Awake()
        {
            material = this.gameObject.GetComponent<Renderer>().material;
            material.SetColor("_Color", Color.blue);
            this.isBlue = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// ユーザーが Select ジェスチャーを行ったときに MyGazeGestureManager から呼ばれる
        /// </summary>
        public void OnSelect()
        {
            DebugLog.Instance.Log += "OnSelect\n";
            if (this.isBlue)
            {
                material.SetColor("_Color", Color.red);
            }
            else
            {
                material.SetColor("_Color", Color.blue);
            }

            this.isBlue = !this.isBlue;
        }

        #endregion
    }

}