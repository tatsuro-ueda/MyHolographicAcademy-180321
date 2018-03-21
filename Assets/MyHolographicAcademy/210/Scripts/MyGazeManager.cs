using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace EDUCATION.FEELPHYSICS.MY_HOLOGRAPHIC_ACADEMY
{
    /// <summary>
    /// MyGazeManager はユーザーの gaze の場所（当たった場所と法線）を決定する
    /// </summary>
    public class MyGazeManager : MonoBehaviour
    {
        #region Public Valiables

        /// <summary>
        /// シングルトン化するための変数
        /// </summary>
        public static MyGazeManager Instance;

        [Tooltip("gazeが当たるかどうか計算する最大距離")]
        public float MaxGazeDistance = 5.0f;

        [Tooltip("視線がターゲットとするレイヤーを選んで下さい")]
        public LayerMask RaycastLayerMask = Physics.DefaultRaycastLayers;

        /// <summary>
        /// Physics.Raycast がホログラムに当たると true を返す
        /// </summary>
        public bool Hit { get; private set; }

        /// <summary>
        /// HitInfo プロパティを使って
        /// RaycastHit の public なメンバーにアクセスすることができる
        /// </summary>
        public RaycastHit HitInfo { get; private set; }

        /// <summary>
        /// ユーザの gaze の位置
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// RaycastHit の法線方向
        /// </summary>
        public Vector3 Normal { get; private set; }

        #endregion

        #region Private Valuables

        /// <summary>
        /// gaze スタビライザー
        /// </summary>
        private GazeStabilizer gazeStabilizer;

        /// <summary>
        /// gaze の始点
        /// </summary>
        private Vector3 gazeOrigin;

        /// <summary>
        /// gaze の方向
        /// </summary>
        private Vector3 gazeDirection;

        #endregion

        #region MonoBehavior CallBacks

        /// <summary>
        /// 本クラスをシングルトン化する
        /// </summary>
        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// gaze の Raycast の hitInfo を更新し続ける
        /// </summary>
        private void Update()
        {
            // メインカメラの位置をを gazeOrigin 割り当てる
            this.gazeOrigin = Camera.main.transform.position;

            // メインカメラの transform の前方向を gazeDirection に割り当てる
            this.gazeDirection = Camera.main.transform.forward;

            this.UpdateRaycast();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Raycast が当たる場所と法線を計算する
        /// </summary>
        private void UpdateRaycast()
        {
            // RaycastHit 型のhitInfo 変数をつくる
            RaycastHit hitInfo;

            // Unity の Physics Raycast を実行する
            // public なプロパティである Hit の中に返り値を集める
            // 始点を gazeOrigin、方向を gazeDirection として pass する
            // TODO: pass する？
            // hitInfo の情報を集める
            // MaxGazeDistance と RaycastLayerMask を pass する
            this.Hit = Physics.Raycast(
                this.gazeOrigin,
                this.gazeDirection,
                out hitInfo,
                this.MaxGazeDistance,
                this.RaycastLayerMask);

            // 他のクラスがアクセスできるように、hitoInfo 変数を HitInfo の public プロパティに割り当てる
            this.HitInfo = hitInfo;

            if (this.Hit)
            {
                // もし raycast がホログラムに当たったら…

                // プロパティ Position に hitInto の点を割り当てる
                this.Position = hitInfo.point;

                // プロパティ Normal に hitInfo の法線を割り当てる
                this.Normal = hitInfo.normal;
            }
            else
            {
                // もし raycast がホログラムに当たらなかったら…
                // デフォルト値を保存する…

                // プロパティ Position に gazeOrigin ＋ MaxgazeDistance × gazeDirection を割り当てる
                this.Position = this.gazeOrigin + (this.gazeDirection * this.MaxGazeDistance);

                // プロパティ Normal に gazeDirection を割り当てる
                this.Normal = this.gazeDirection;
            }
        }

        #endregion
    }
}