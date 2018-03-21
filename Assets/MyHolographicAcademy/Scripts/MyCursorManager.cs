using UnityEngine;

namespace EDUCATION.FEELPHYSICS.MY_HOLOGRAPHIC_ACADEMY
{
    /// <summary>
    /// MyCursorManager クラスは Cursor GameObject（複数）を扱います
    /// ホログラム上に表示されるものもありますし、ホログラムを外れたときに表示されるものもあります
    /// ホログラムにヒットしたかどうかで、適切なカーソルを表示します
    /// ヒットした位置に適切なカーソルを設置します
    /// カーソルの法線はヒットした表面の法線に一致します
    /// </summary>
    public class MyCursorManager : MonoBehaviour
    {
        #region Public Valuables

        /// <summary>
        /// シングルトン化するための変数
        /// </summary>
        public static MyCursorManager Instance;

        [Tooltip("ホログラムにヒットしているときに表示するカーソルオブジェクトをドラッグして下さい")]
        public GameObject CursorOnHolograms;

        [Tooltip("ホログラムにヒットしていないときに表示するカーソルオブジェクトをドラッグして下さい")]
        public GameObject CursorOffHolograms;

        #endregion

        #region Private Valuables

        /// <summary>
        /// アクティブなカーソルを入れておく変数（位置と向きを反映させるのに使う）
        /// </summary>
        private GameObject cursor;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// カーソルが指定されていなければスクリプトを終了する
        /// </summary>
        private void Awake()
        {
            if (this.CursorOnHolograms == null || this.CursorOffHolograms == null)
            {
                Debug.LogError("カーソルが指定されていません");
                return;
            }

            // Hide the Cursors to begin with.
            this.CursorOnHolograms.SetActive(false);
            this.CursorOffHolograms.SetActive(false);
        }

        /// <summary>
        /// gaze がオブジェクトにヒットするか否かで、カーソルオブジェクトを切り替える
        /// カーソルオブジェクトの transform に、ヒット地点の法線などを反映させる
        /// </summary>
        private void Update()
        {
            if (MyGazeManager.Instance == null || 
                this.CursorOnHolograms == null || 
                this.CursorOffHolograms == null)
            {
                return;
            }

            if (MyGazeManager.Instance.Hit)
            {
                // ホログラムカーソルをアクティブにして表示する
                this.CursorOnHolograms.SetActive(true);

                // ホログラムカーソルを非アクティブにして非表示にする
                this.CursorOffHolograms.SetActive(false);

                this.cursor = this.CursorOnHolograms;
            }
            else
            {
                // ホログラムなしカーソルをアクティブにして表示する
                this.CursorOffHolograms.SetActive(true);

                // ホログラムなしカーソルを非アクティブにして非表示にする
                this.CursorOnHolograms.SetActive(false);

                this.cursor = this.CursorOffHolograms;
            }

            // GazeManager Position をカーソルオブジェクトの transform.position に割り当てる
            this.cursor.transform.position = MyGazeManager.Instance.Position;

            // GazeManager Normal をカーソルオブジェクトの transform.up に割り当てる
            this.cursor.transform.forward = MyGazeManager.Instance.Normal;
        }

        #endregion
    }
}