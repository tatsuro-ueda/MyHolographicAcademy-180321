using UnityEngine;

namespace EDUCATION.FEELPHYSICS.HOLOGRAPHIC_ACADEMY
{
    /// <summary>
    /// 書いたスクリプトが正常に動作しているか、3D Textに表示して確認する
    /// </summary>
    public class DebugLog : MonoBehaviour
    {

        [Tooltip("3D Text の Text Mesh")]
        public TextMesh MyTextMesh;

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

            this.MyTextMesh.text =
                "Position: " + MyGazeManager.Instance.Position.ToString()
                + "\nNormal: " + MyGazeManager.Instance.Normal.ToString()
                + "\nFocusedGameObject: " + focusedName
                ;
        }
    }
}