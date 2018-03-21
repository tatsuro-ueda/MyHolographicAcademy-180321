using UnityEngine;

namespace EDUCATION.FEELPHYSICS.MY_HOLOGRAPHIC_ACADEMY
{
    /// <summary>
    /// InteractibleManager は、どの GameObject が現在フォーカスされているかを保持する
    /// </summary>
    public class MyInteractibleManager : MonoBehaviour
    {
        #region Public Valiables

        /// <summary>
        /// このクラスをシングルトンとして使用するための変数
        /// </summary>
        public static MyInteractibleManager Instance;

        /// <summary>
        /// フォーカスされた GameObject を格納する変数
        /// </summary>
        public GameObject FocusedGameObject { get; private set; }

        #endregion

        #region Private Valuables

        /// <summary>
        /// 1つ前のフレームでフォーカスされた GameObject を格納する変数
        /// </summary>
        private GameObject oldFocusedGameObject = null;

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
        /// FocusedGameObject 変数を初期化する
        /// </summary>
        private void Start()
        {
            this.FocusedGameObject = null;
        }

        /// <summary>
        /// gaze がヒットしたら対象 GameObject を FocusedGameObject 変数に格納する
        /// </summary>
        private void Update()
        {
            this.oldFocusedGameObject = this.FocusedGameObject;

            if (MyGazeManager.Instance.Hit)
            {
                RaycastHit hitInfo = MyGazeManager.Instance.HitInfo;
                if (hitInfo.collider != null)
                {
                    // hitInfo に格納された衝突した GameObject を FocusedGameObject に割り当てる
                    this.FocusedGameObject = hitInfo.collider.gameObject;
                }
                else
                {
                    this.FocusedGameObject = null;
                }
            }
            else
            {
                this.FocusedGameObject = null;
            }

            // フォーカスした瞬間にフォーカスしたオブジェクトに「GazeEntered」メッセージを送る
            if (this.FocusedGameObject != this.oldFocusedGameObject)
            {
                // フォーカスが外れたオブジェクトに「GazeExited」メッセージを送る
                this.ResetFocusedInteractible();

                if (this.FocusedGameObject != null)
                {
                    if (this.FocusedGameObject.GetComponent<MyInteractible>() != null)
                    {
                        // FocusedGameObject に GazeEntered メッセージを送る
                        this.FocusedGameObject.SendMessage("GazeEntered");
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// フォーカスが外れたオブジェクトに「GazeExited」メッセージを送る
        /// </summary>
        private void ResetFocusedInteractible()
        {
            if (this.oldFocusedGameObject != null)
            {
                if (this.oldFocusedGameObject.GetComponent<MyInteractible>() != null)
                {
                    // oldFocusedGameObject に GazeExited メッセージを送る
                    this.oldFocusedGameObject.SendMessage("GazeExited");
                }
            }
        }

        #endregion
    }
}