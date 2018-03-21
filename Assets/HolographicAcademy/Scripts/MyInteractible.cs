using UnityEngine;

namespace EDUCATION.FEELPHYSICS.HOLOGRAPHIC_ACADEMY
{
    /// <summary>
    /// このクラスは GameObject をインタラクト可能にする。gaze されたとき何が起きるかを決定する。
    /// </summary>
    public class MyInteractible : MonoBehaviour
    {
        #region Public Valuables

        [Tooltip("DebugLog オブジェクトをドラッグして下さい")]
        public GameObject MyDebugLog;

        #endregion

        #region Public Methods

        /// <summary>
        /// gaze された瞬間に呼ばれる
        /// </summary>
        public void GazeEntered()
        {
            DebugLog.Instance.Message = gameObject.name + ": GazeEntered";
        }

        /// <summary>
        /// gaze が外れた瞬間に呼ばれる
        /// </summary>
        public void GazeExited()
        {
            DebugLog.Instance.Message = gameObject.name + ": GazeExited";
        }

        #endregion
    }
}