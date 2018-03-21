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
            var focusedObject = MyInteractibleManager.Instance.FocusedGameObject;
            string focusedName;
            if (focusedObject == null)
            {
                focusedName = "null";
            }
            else
            {
                focusedName = focusedObject.name;
            }

            this.MyTextMesh.text = ""
                + "Position: " + MyGazeManager.Instance.Position.ToString()
                + "\nNormal: " + MyGazeManager.Instance.Normal.ToString()
                + "\nFocusedGameObject: " + focusedName
                + "\nLog:\n" + this.Log
                ;
        }

        #endregion
    }
}