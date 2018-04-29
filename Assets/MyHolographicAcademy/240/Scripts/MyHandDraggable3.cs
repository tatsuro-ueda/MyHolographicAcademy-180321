// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System;

namespace HoloToolkit.Unity.InputModule
{
    /// <summary>
    /// HoloLens上のオブジェクトを手でドラッグすることができるようにするコンポーネントです。
    /// 現在と直前の手の位置の角度とz座標（前後）の変化を計算し、
    /// そこにオブジェクトを配置することで、ドラッグを実現しています。
    /// </summary>
    public class MyHandDraggable3 : MonoBehaviour, IFocusable, IInputHandler, ISourceStateHandler
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

        [Tooltip("z軸に沿った手の移動の何倍だけドラッグするオブジェクトを動かすかの倍率")]
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

        /// <summary>
        /// 手とオブジェクトのあいだの最初の回転の差分
        /// </summary>
        private Quaternion gazeAngularOffset;

        /// <summary>
        /// つかんだときの基点と手とのあいだの距離
        /// </summary>
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

            // 基点の位置を取得する
            Vector3 pivotPosition = GetHandPivotPosition(cameraTransform);

            // つかんだときの基点と手とのあいだの距離
            handRefDistance = Vector3.Magnitude(inputPosition - pivotPosition);

            // 基点とオブジェクトのあいだの距離
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

            // 手とオブジェクトのあいだの最初の回転の差分を保持する
            // これによりドラッグ中にこれを考慮できる
            gazeAngularOffset = Quaternion.FromToRotation(handDirection, objDirection);
            draggingPosition = initialDraggingPosition;

            StartedDragging.RaiseEvent();
        }

        /// <summary>
        /// 基点の位置（首の根元辺り）を取得する
        /// </summary>
        /// <returns>基点の位置</returns>
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
        /// ドラッグされているオブジェクトの位置を更新する
        /// </summary>
        private void UpdateDragging()
        {
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

            // 基点をの位置を取得する
            Vector3 pivotPosition = GetHandPivotPosition(cameraTransform);

            // --- 新しい位置を計算する、ここから ---
            // 新しい、基点から見た手の位置
            Vector3 newHandDirection = Vector3.Normalize(inputPosition - pivotPosition);

            // カメラ空間での手の方向
            newHandDirection = cameraTransform.InverseTransformDirection(newHandDirection);

            // 手とオブジェクトのあいだの最初の回転の差分
            Vector3 targetDirection = Vector3.Normalize(gazeAngularOffset * newHandDirection);

            // 世界空間に戻す
            targetDirection = cameraTransform.TransformDirection(targetDirection);

            float currentHandDistance = Vector3.Magnitude(inputPosition - pivotPosition);

            // 現在の基点と手とのあいだの距離／つかんだときの基点と手とのあいだの距離
            // 手前でつかむほど大きく前後に動かすことができる
            float distanceRatio = currentHandDistance / handRefDistance;

            // public変数のDistanceScale（z軸に沿った手の移動の何倍だけドラッグするオブジェクトを動かすかの倍率）を反映させる
            float distanceOffset = distanceRatio > 0 ? (distanceRatio - 1f) * DistanceScale : 0;
            float targetDistance = objRefDistance + distanceOffset;

            draggingPosition = pivotPosition + (targetDirection * targetDistance);
            // --- 新しい位置を計算する、ここまで ---

            // --- 新しい回転を計算する、ここから ---
            if (RotationMode == RotationModeEnum.OrientTowardUser || RotationMode == RotationModeEnum.OrientTowardUserAndKeepUpright)
            {
                draggingRotation = Quaternion.LookRotation(HostTransform.position - pivotPosition);
            }
            else if (RotationMode == RotationModeEnum.LockObjectRotation)
            {
                draggingRotation = HostTransform.rotation;
            }
            else // RotationModeEnum.Default の場合
            {
                Vector3 objForward = cameraTransform.TransformDirection(objRefForward); // in world space
                Vector3 objUp = cameraTransform.TransformDirection(objRefUp);           // in world space
                draggingRotation = Quaternion.LookRotation(objForward, objUp);
            }
            // --- 新しい回転を計算する、ここまで ---

            // --- 新しい位置・回転を適用する、ここから ---
            Vector3 newPosition = Vector3.Lerp(HostTransform.position, draggingPosition + cameraTransform.TransformDirection(objRefGrabPoint), PositionLerpSpeed);
            // 最終的な位置を適用する
            if (hostRigidbody == null)
            {
                HostTransform.position = newPosition;
            }
            else
            {
                hostRigidbody.MovePosition(newPosition);
            }

            // 最終的な回転を適用する
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
            // --- 新しい位置・回転を適用する、ここまで ---
        }

        /// <summary>
        /// オブジェクトのドラッグを止める
        /// </summary>
        public void StopDragging()
        {
            if (!isDragging)
            {
                return;
            }

            // モーダル入力対象から自身を削除する
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
                eventData.Use(); // イベントが使われたことを記録して、他の処理に受け取られるのを防ぐ

                StopDragging();
            }
        }

        public void OnInputDown(InputEventData eventData)
        {
            if (isDragging)
            {
                // すでにドラッグの入力を受け取って処理しているので、新しいドラッグ操作は開始しない
                return;
            }

#if UNITY_2017_2_OR_NEWER
            InteractionSourceInfo sourceKind;
            eventData.InputSource.TryGetSourceKind(eventData.SourceId, out sourceKind);
            if (sourceKind != InteractionSourceInfo.Hand)
            {
                if (!eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
                {
                    // 入力元はこのスクリプトに必要な位置情報を提供しなければならない
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

            // イベントが使われたことを記録して、他の処理に受け取られるのを防ぐ
            eventData.Use();

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
            // 何もしない
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
