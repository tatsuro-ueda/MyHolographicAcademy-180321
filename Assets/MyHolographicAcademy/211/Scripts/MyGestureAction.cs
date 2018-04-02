using UnityEngine;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    /// <summary>
    /// GestureAction performs custom actions based on
    /// which gesture is being performed.
    /// </summary>
    public class MyGestureAction : MonoBehaviour
    {
        #region Public Valuables

        [Tooltip("ナビゲーションの繊細性")]
        public float RotationSensitivity = 0.002f;

        private Vector3 manipulationPreviousPosition;

        /// <summary>
        /// 回転速度
        /// </summary>
        private float rotationFactor;

        #endregion

        #region MonoBehaviour Lifecycle

        /// <summary>
        /// することは、回転だけ
        /// </summary>
        private void Update()
        {
            this.PerformRotation();
        }

        /// <summary>
        /// NavigationX の値から回転速度を決め、GameObject を回転させる
        /// </summary>
        private void PerformRotation()
        {
            if (MyGestureManager.Instance.IsNavigating &&
            MyHandsManager.Instance.FocusedGameObject == this.gameObject)
        {
                // MyGestureManager の NavigationPosition.X に RotationSensitivity をかけて、
                // rotationFactor（回転速度）を計算する
                this.rotationFactor = MyGestureManager.Instance.NavigationPosition.x * this.RotationSensitivity;

                // rotationFactor を使って Y 軸に対して transform.Rotate する
                transform.Rotate(new Vector3(0, -1 * rotationFactor, 0));
            }
        }

        #endregion

        #region Manipulation

        void PerformManipulationStart(Vector3 position)
        {
            manipulationPreviousPosition = position;
        }

        void PerformManipulationUpdate(Vector3 positionUpdated)
        {
            if (MyGestureManager.Instance.IsManipulating)
            {
                Vector3 moveVector = Vector3.zero;

                // moveVector を(位置 - manipulationPreviousPosition)で計算する
                moveVector = positionUpdated - manipulationPreviousPosition;

                // manipulationPreviousPosition を現在の位置で更新する
                manipulationPreviousPosition = positionUpdated;

                // 位置に moveVector を足す
                transform.position += moveVector;
            }
        }

        #endregion
    }
}