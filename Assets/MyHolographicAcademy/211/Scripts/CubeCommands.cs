using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    public class CubeCommands : MonoBehaviour, IInputClickHandler
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
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            DebugLog.Instance.Log += "OnInputClicked\n";
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