using UnityEngine;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    /// <summary>
    /// GestureAction performs custom actions based on
    /// which gesture is being performed.
    /// </summary>
    public class MyGestureAction : MonoBehaviour
    {
        [Tooltip("Rotation max speed controls amount of rotation.")]
        public float RotationSensitivity = 10.0f;

        private Vector3 manipulationPreviousPosition;

        private float rotationFactor;

        void Update()
        {
            PerformRotation();
        }

        private void PerformRotation()
        {
            if (MyGestureManager.Instance.IsNavigating &&
            MyHandsManager.Instance.FocusedGameObject == gameObject)
        {
                /* TODO: DEVELOPER CODING EXERCISE 2.c */

                // 2.c: Calculate rotationFactor based on MyGestureManager&#39;s NavigationPosition.X and multiply by RotationSensitivity.
                // This will help control the amount of rotation.
                rotationFactor = MyGestureManager.Instance.NavigationPosition.x * RotationSensitivity;

                // 2.c: transform.Rotate along the Y axis using rotationFactor.
                transform.Rotate(new Vector3(0, -1 * rotationFactor, 0));
            }
        }

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
    }
}