// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System;

namespace HoloToolkit.Unity.InputModule
{
    /// <summary>
    /// HoloLens上のオブジェクトを手でドラッグすることができるようにするコンポーネントです。
    /// 現在と直前の手の位置の角度とz座標の変化を計算し、
    /// そこにオブジェクトを配置することで、ドラッグを実現しています。
    /// </summary>
    public class MyHandDraggable : MonoBehaviour, IFocusable, IInputHandler, ISourceStateHandler
    {
        /// <summary>
        /// ドラッグが始まったときに引き起こされるイベント
        /// </summary>
        public event Action StartedDragging;

        /// <summary>
        /// ドラッグが止まったときに引き起こされるイベント
        /// </summary>
        public event Action StoppedDragging;

        [Tooltip("ドラッグされるTransform。デフォルトでは、このコンポーネントを含むオブジェクト")]
        public Transform HostTransform;

        [Tooltip("z軸に沿った手の移動の何倍だけドラッグするオブジェクトを動かすかの縮尺")]
        public float DistanceScale = 2f;

        public enum RotationModeEnum
        {
            Default,
            LockObjectRotation,  // Rotation固定
            OrientTowardUser,  // ユーザの方を向く
            OrientTowardUserAndKeepUpright
        }

        public RotationModeEnum RotationMode = RotationModeEnum.Default;

        [Tooltip("オブジェクトが目標位置まで補間される速さ")]
        [Range(0.01f, 1.0f)]
        public float PositionLerpSpeed = 0.2f;

        [Tooltip("オブジェクトが目標角度まで補間される速さ")]
        [Range(0.01f, 1.0f)]
        public float RotationLerpSpeed = 0.2f;

        public bool IsDraggingEnabled = true;

        private bool isDragging;
        private bool isGazed;
        private Vector3 objRefForward;
        private Vector3 objRefUp;
        private float objRefDistance;
        private Quaternion gazeAngularOffset;
        private float handRefDistance;
        private Vector3 objRefGrabPoint;

        private Vector3 draggingPosition;
        private Quaternion draggingRotation;

        private IInputSource currentInputSource;
        private uint currentInputSourceId;
        private Rigidbody hostRigidbody;

        private void Start()
        {
            if (HostTransform == null)
            {
                HostTransform = transform;
            }

            hostRigidbody = HostTransform.GetComponent<Rigidbody>();
        }

        private void OnDestroy()
        {
            if (isDragging)
            {
                StopDragging();
            }

            if (isGazed)
            {
                OnFocusExit();
            }
        }

        private void Update()
        {
            if (IsDraggingEnabled && isDragging)
            {
                UpdateDragging();
            }
        }

        /// <summary>
        /// オブジェクトのドラッグを開始する
        /// </summary>
        public void StartDragging(Vector3 initialDraggingPosition)
        {
            if (!IsDraggingEnabled)
            {
                return;
            }

            if (isDragging)
            {
                return;
            }

            // マニピュレーションのあいだのすべての入力を取得するため、自身をモーダル入力対象に追加する
            InputManager.Instance.PushModalInputHandler(gameObject);

            isDragging = true;

            Transform cameraTransform = CameraCache.Main.transform;

            // 手もしくはコントローラーの位置を取得する
            Vector3 inputPosition = Vector3.zero;
#if UNITY_2017_2_OR_NEWER
            InteractionSourceInfo sourceKind;
            currentInputSource.TryGetSourceKind(currentInputSourceId, out sourceKind);
            switch (sourceKind)
            {
                case InteractionSourceInfo.Hand:
                    currentInputSource.TryGetGripPosition(currentInputSourceId, out inputPosition);
                    break;
                case InteractionSourceInfo.Controller:
                    currentInputSource.TryGetPointerPosition(currentInputSourceId, out inputPosition);
                    break;
            }
#else
            currentInputSource.TryGetPointerPosition(currentInputSourceId, out inputPosition);
#endif

            // 自身（pivot）の位置を取得する
            Vector3 pivotPosition = GetHandPivotPosition(cameraTransform);

            // 自身と手とのあいだの距離
            handRefDistance = Vector3.Magnitude(inputPosition - pivotPosition);

            // 自身とオブジェクトのあいだの距離
            objRefDistance = Vector3.Magnitude(initialDraggingPosition - pivotPosition);

            Vector3 objForward = HostTransform.forward;
            Vector3 objUp = HostTransform.up;

            // オブジェクトがつかまれた場所を保持する
            objRefGrabPoint = cameraTransform.transform.InverseTransformDirection(HostTransform.position - initialDraggingPosition);

            Vector3 objDirection = Vector3.Normalize(initialDraggingPosition - pivotPosition);
            Vector3 handDirection = Vector3.Normalize(inputPosition - pivotPosition);

            objForward = cameraTransform.InverseTransformDirection(objForward);       // カメラ空間での
            objUp = cameraTransform.InverseTransformDirection(objUp);                 // カメラ空間での
            objDirection = cameraTransform.InverseTransformDirection(objDirection);   // カメラ空間での
            handDirection = cameraTransform.InverseTransformDirection(handDirection); // カメラ空間での

            objRefForward = objForward;
            objRefUp = objUp;

            // 手とオブジェクトのあいだの最初の位置の差分を保持する
            // これによりドラッグ中にこれを考慮できる
            gazeAngularOffset = Quaternion.FromToRotation(handDirection, objDirection);
            draggingPosition = initialDraggingPosition;

            StartedDragging.RaiseEvent();
        }

        /// <summary>
        /// 自身の位置（首の根元辺り）を取得する
        /// </summary>
        /// <returns>自身の位置</returns>
        private Vector3 GetHandPivotPosition(Transform cameraTransform)
        {
            // カメラよりも若干下で後ろ
            return cameraTransform.position + new Vector3(0, -0.2f, 0) - cameraTransform.forward * 0.2f;
        }

        /// <summary>
        /// ドラッグを有効・無効にする
        /// </summary>
        /// <param name="isEnabled">ドラッグを有効にするか否か</param>
        public void SetDragging(bool isEnabled)
        {
            if (IsDraggingEnabled == isEnabled)
            {
                return;
            }

            IsDraggingEnabled = isEnabled;

            if (isDragging)
            {
                StopDragging();
            }
        }

        /// <summary>
        /// Update the position of the object being dragged.
        /// </summary>
        private void UpdateDragging()
        {
            Transform cameraTransform = CameraCache.Main.transform;

            Vector3 inputPosition = Vector3.zero;
#if UNITY_2017_2_OR_NEWER
            InteractionSourceInfo sourceKind;
            currentInputSource.TryGetSourceKind(currentInputSourceId, out sourceKind);
            switch (sourceKind)
            {
                case InteractionSourceInfo.Hand:
                    currentInputSource.TryGetGripPosition(currentInputSourceId, out inputPosition);
                    break;
                case InteractionSourceInfo.Controller:
                    currentInputSource.TryGetPointerPosition(currentInputSourceId, out inputPosition);
                    break;
            }
#else
            currentInputSource.TryGetPointerPosition(currentInputSourceId, out inputPosition);
#endif

            Vector3 pivotPosition = GetHandPivotPosition(cameraTransform);

            Vector3 newHandDirection = Vector3.Normalize(inputPosition - pivotPosition);

            newHandDirection = cameraTransform.InverseTransformDirection(newHandDirection); // in camera space
            Vector3 targetDirection = Vector3.Normalize(gazeAngularOffset * newHandDirection);
            targetDirection = cameraTransform.TransformDirection(targetDirection); // back to world space

            float currentHandDistance = Vector3.Magnitude(inputPosition - pivotPosition);

            float distanceRatio = currentHandDistance / handRefDistance;
            float distanceOffset = distanceRatio > 0 ? (distanceRatio - 1f) * DistanceScale : 0;
            float targetDistance = objRefDistance + distanceOffset;

            draggingPosition = pivotPosition + (targetDirection * targetDistance);

            if (RotationMode == RotationModeEnum.OrientTowardUser || RotationMode == RotationModeEnum.OrientTowardUserAndKeepUpright)
            {
                draggingRotation = Quaternion.LookRotation(HostTransform.position - pivotPosition);
            }
            else if (RotationMode == RotationModeEnum.LockObjectRotation)
            {
                draggingRotation = HostTransform.rotation;
            }
            else // RotationModeEnum.Default
            {
                Vector3 objForward = cameraTransform.TransformDirection(objRefForward); // in world space
                Vector3 objUp = cameraTransform.TransformDirection(objRefUp);           // in world space
                draggingRotation = Quaternion.LookRotation(objForward, objUp);
            }

            Vector3 newPosition = Vector3.Lerp(HostTransform.position, draggingPosition + cameraTransform.TransformDirection(objRefGrabPoint), PositionLerpSpeed);
            // Apply Final Position
            if (hostRigidbody == null)
            {
                HostTransform.position = newPosition;
            }
            else
            {
                hostRigidbody.MovePosition(newPosition);
            }

            // Apply Final Rotation
            Quaternion newRotation = Quaternion.Lerp(HostTransform.rotation, draggingRotation, RotationLerpSpeed);
            if (hostRigidbody == null)
            {
                HostTransform.rotation = newRotation;
            }
            else
            {
                hostRigidbody.MoveRotation(newRotation);
            }

            if (RotationMode == RotationModeEnum.OrientTowardUserAndKeepUpright)
            {
                Quaternion upRotation = Quaternion.FromToRotation(HostTransform.up, Vector3.up);
                HostTransform.rotation = upRotation * HostTransform.rotation;
            }
        }

        /// <summary>
        /// Stops dragging the object.
        /// </summary>
        public void StopDragging()
        {
            if (!isDragging)
            {
                return;
            }

            // Remove self as a modal input handler
            InputManager.Instance.PopModalInputHandler();

            isDragging = false;
            currentInputSource = null;
            currentInputSourceId = 0;
            StoppedDragging.RaiseEvent();
        }

        public void OnFocusEnter()
        {
            if (!IsDraggingEnabled)
            {
                return;
            }

            if (isGazed)
            {
                return;
            }

            isGazed = true;
        }

        public void OnFocusExit()
        {
            if (!IsDraggingEnabled)
            {
                return;
            }

            if (!isGazed)
            {
                return;
            }

            isGazed = false;
        }

        public void OnInputUp(InputEventData eventData)
        {
            if (currentInputSource != null &&
                eventData.SourceId == currentInputSourceId)
            {
                eventData.Use(); // Mark the event as used, so it doesn't fall through to other handlers.

                StopDragging();
            }
        }

        public void OnInputDown(InputEventData eventData)
        {
            if (isDragging)
            {
                // We're already handling drag input, so we can't start a new drag operation.
                return;
            }

#if UNITY_2017_2_OR_NEWER
            InteractionSourceInfo sourceKind;
            eventData.InputSource.TryGetSourceKind(eventData.SourceId, out sourceKind);
            if (sourceKind != InteractionSourceInfo.Hand)
            {
                if (!eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
                {
                    // The input source must provide positional data for this script to be usable
                    return;
                }
            }
#else
            if (!eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
            {
                // The input source must provide positional data for this script to be usable
                return;
            }
#endif

            eventData.Use(); // Mark the event as used, so it doesn't fall through to other handlers.

            currentInputSource = eventData.InputSource;
            currentInputSourceId = eventData.SourceId;

            FocusDetails? details = FocusManager.Instance.TryGetFocusDetails(eventData);

            Vector3 initialDraggingPosition = (details == null)
                ? HostTransform.position
                : details.Value.Point;

            StartDragging(initialDraggingPosition);
        }

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            // Nothing to do
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            if (currentInputSource != null && eventData.SourceId == currentInputSourceId)
            {
                StopDragging();
            }
        }
    }
}
