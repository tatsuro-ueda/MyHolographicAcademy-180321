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

        #region Not Used

        void PerformManipulationStart(Vector3 position)
        {
            manipulationPreviousPosition = position;
        }

        void PerformManipulationUpdate(Vector3 position)
        {
            if (MyGestureManager.Instance.IsManipulating)
            {
                /* TODO: DEVELOPER CODING EXERCISE 4.a */

                Vector3 moveVector = Vector3.zero;

                // 4.a: Calculate the moveVector as position - manipulationPreviousPosition.

                // 4.a: Update the manipulationPreviousPosition with the current position.

                // 4.a: Increment this transform&#39;s position by the moveVector.
            }
        }

        #endregion
    }
}