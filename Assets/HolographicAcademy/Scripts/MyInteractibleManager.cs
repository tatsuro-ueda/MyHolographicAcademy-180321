using UnityEngine;

namespace EDUCATION.FEELPHYSICS.HOLOGRAPHIC_ACADEMY
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

        public GameObject FocusedGameObject { get; private set; }

        #endregion

        #region Private Valuables

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

        // Use this for initialization
        void Start()
        {
            this.FocusedGameObject = null;
        }

        // Update is called once per frame
        void Update()
        {
            oldFocusedGameObject = FocusedGameObject;

            if (MyGazeManager.Instance.Hit)
            {
                RaycastHit hitInfo = MyGazeManager.Instance.HitInfo;
                if (hitInfo.collider != null)
                {
                    // hitInfo に格納された衝突した GameObject を FocusedGameObject に割り当てる
                    FocusedGameObject = hitInfo.collider.gameObject;
                }
                else
                {
                    FocusedGameObject = null;
                }
            }
            else
            {
                FocusedGameObject = null;
            }
        }

        #endregion
    }
}