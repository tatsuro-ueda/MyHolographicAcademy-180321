// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System;
using HoloToolkit.Sharing;

namespace HoloToolkit.Unity.InputModule
{
    /// <summary>
    /// HoloLens��̃I�u�W�F�N�g����Ńh���b�O���邱�Ƃ��ł���悤�ɂ���R���|�[�l���g�ł��B
    /// ���݂ƒ��O�̎�̈ʒu�̊p�x��z���W�i�O��j�̕ω����v�Z���A
    /// �����ɃI�u�W�F�N�g��z�u���邱�ƂŁA�h���b�O���������Ă��܂��B
    /// </summary>
    public class MyHandDraggable : MonoBehaviour, IFocusable, IInputHandler, ISourceStateHandler
    {        
        /// <summary>
        /// �h���b�O���n�܂����Ƃ��Ɉ����N�������C�x���g
        /// </summary>
        public event Action StartedDragging;

        /// <summary>
        /// �h���b�O���~�܂����Ƃ��Ɉ����N�������C�x���g
        /// </summary>
        public event Action StoppedDragging;

        [Tooltip("�h���b�O�����Transform�B�f�t�H���g�ł́A���̃R���|�[�l���g���܂ރI�u�W�F�N�g")]
        public Transform HostTransform;

        [Tooltip("z���ɉ�������̈ړ��̉��{�����h���b�O����I�u�W�F�N�g�𓮂������̔{��")]
        public float DistanceScale = 2f;

        public enum RotationModeEnum
        {
            Default,
            LockObjectRotation,  // Rotation�Œ�
            OrientTowardUser,  // ���[�U�̕�������
            OrientTowardUserAndKeepUpright
        }

        public RotationModeEnum RotationMode = RotationModeEnum.Default;

        [Tooltip("�I�u�W�F�N�g���ڕW�ʒu�܂ŕ�Ԃ���鑬��")]
        [Range(0.01f, 1.0f)]
        public float PositionLerpSpeed = 0.2f;

        [Tooltip("�I�u�W�F�N�g���ڕW�p�x�܂ŕ�Ԃ���鑬��")]
        [Range(0.01f, 1.0f)]
        public float RotationLerpSpeed = 0.2f;

        public bool IsDraggingEnabled = true;

        public long UserID;

        private bool isDragging;
        private bool isGazed;
        private Vector3 objRefForward;
        private Vector3 objRefUp;
        private float objRefDistance;

        /// <summary>
        /// ��ƃI�u�W�F�N�g�̂������̍ŏ��̉�]�̍���
        /// </summary>
        private Quaternion gazeAngularOffset;

        /// <summary>
        /// ���񂾂Ƃ��̊�_�Ǝ�Ƃ̂������̋���
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

            // �ǉ�
            int ownerId = GetComponent<DefaultSyncModelAccessor>().SyncModel.OwnerId;
            int userId = SharingStage.Instance.Manager.GetLocalUser().GetID();
            if (ownerId == userId)
            {
                InputManager.Instance.PushFallbackInputHandler(gameObject);
            }
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

            // �ǉ�
            InputManager.Instance.PopFallbackInputHandler();
        }

        private void Update()
        {
            if (IsDraggingEnabled && isDragging)
            {
                UpdateDragging();
            }
        }


        /// <summary>
        /// �I�u�W�F�N�g�̃h���b�O���J�n����
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

            // �}�j�s�����[�V�����̂������̂��ׂĂ̓��͂��擾���邽�߁A���g�����[�_�����͑Ώۂɒǉ�����
            InputManager.Instance.PushModalInputHandler(gameObject);

            isDragging = true;

            Transform cameraTransform = CameraCache.Main.transform;

            // ��������̓R���g���[���[�̈ʒu���擾����
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

            // ��_�̈ʒu���擾����
            Vector3 pivotPosition = GetHandPivotPosition(cameraTransform);

            // ���񂾂Ƃ��̊�_�Ǝ�Ƃ̂������̋���
            handRefDistance = Vector3.Magnitude(inputPosition - pivotPosition);

            // ��_�ƃI�u�W�F�N�g�̂������̋���
            objRefDistance = Vector3.Magnitude(initialDraggingPosition - pivotPosition);

            Vector3 objForward = HostTransform.forward;
            Vector3 objUp = HostTransform.up;

            // �I�u�W�F�N�g�����܂ꂽ�ꏊ��ێ�����
            objRefGrabPoint = cameraTransform.transform.InverseTransformDirection(HostTransform.position - initialDraggingPosition);

            Vector3 objDirection = Vector3.Normalize(initialDraggingPosition - pivotPosition);
            Vector3 handDirection = Vector3.Normalize(inputPosition - pivotPosition);

            objForward = cameraTransform.InverseTransformDirection(objForward);       // �J������Ԃł�
            objUp = cameraTransform.InverseTransformDirection(objUp);                 // �J������Ԃł�
            objDirection = cameraTransform.InverseTransformDirection(objDirection);   // �J������Ԃł�
            handDirection = cameraTransform.InverseTransformDirection(handDirection); // �J������Ԃł�

            objRefForward = objForward;
            objRefUp = objUp;

            // ��ƃI�u�W�F�N�g�̂������̍ŏ��̉�]�̍�����ێ�����
            // ����ɂ��h���b�O���ɂ�����l���ł���
            gazeAngularOffset = Quaternion.FromToRotation(handDirection, objDirection);
            draggingPosition = initialDraggingPosition;

            StartedDragging.RaiseEvent();
        }

        /// <summary>
        /// ��_�̈ʒu�i��̍����ӂ�j���擾����
        /// </summary>
        /// <returns>��_�̈ʒu</returns>
        private Vector3 GetHandPivotPosition(Transform cameraTransform)
        {
            // �J���������኱���Ō��
            return cameraTransform.position + new Vector3(0, -0.2f, 0) - cameraTransform.forward * 0.2f;
        }

        /// <summary>
        /// �h���b�O��L���E�����ɂ���
        /// </summary>
        /// <param name="isEnabled">�h���b�O��L���ɂ��邩�ۂ�</param>
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
        /// �h���b�O����Ă���I�u�W�F�N�g�̈ʒu���X�V����
        /// </summary>
        private void UpdateDragging()
        {
            Transform cameraTransform = CameraCache.Main.transform;

            // ��������̓R���g���[���[�̈ʒu���擾����
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

            // ��_���̈ʒu���擾����
            Vector3 pivotPosition = GetHandPivotPosition(cameraTransform);

            // --- �V�����ʒu���v�Z����A�������� ---
            // �V�����A��_���猩����̈ʒu
            Vector3 newHandDirection = Vector3.Normalize(inputPosition - pivotPosition);

            // �J������Ԃł̎�̕���
            newHandDirection = cameraTransform.InverseTransformDirection(newHandDirection);

            // ��ƃI�u�W�F�N�g�̂������̍ŏ��̉�]�̍���
            Vector3 targetDirection = Vector3.Normalize(gazeAngularOffset * newHandDirection);

            // ���E��Ԃɖ߂�
            targetDirection = cameraTransform.TransformDirection(targetDirection);

            float currentHandDistance = Vector3.Magnitude(inputPosition - pivotPosition);

            // ���݂̊�_�Ǝ�Ƃ̂������̋����^���񂾂Ƃ��̊�_�Ǝ�Ƃ̂������̋���
            // ��O�ł��ނقǑ傫���O��ɓ��������Ƃ��ł���
            float distanceRatio = currentHandDistance / handRefDistance;

            // public�ϐ���DistanceScale�iz���ɉ�������̈ړ��̉��{�����h���b�O����I�u�W�F�N�g�𓮂������̔{���j�𔽉f������
            float distanceOffset = distanceRatio > 0 ? (distanceRatio - 1f) * DistanceScale : 0;
            float targetDistance = objRefDistance + distanceOffset;

            draggingPosition = pivotPosition + (targetDirection * targetDistance);
            // --- �V�����ʒu���v�Z����A�����܂� ---

            // --- �V������]���v�Z����A�������� ---
            if (RotationMode == RotationModeEnum.OrientTowardUser || RotationMode == RotationModeEnum.OrientTowardUserAndKeepUpright)
            {
                draggingRotation = Quaternion.LookRotation(HostTransform.position - pivotPosition);
            }
            else if (RotationMode == RotationModeEnum.LockObjectRotation)
            {
                draggingRotation = HostTransform.rotation;
            }
            else // RotationModeEnum.Default �̏ꍇ
            {
                Vector3 objForward = cameraTransform.TransformDirection(objRefForward); // in world space
                Vector3 objUp = cameraTransform.TransformDirection(objRefUp);           // in world space
                draggingRotation = Quaternion.LookRotation(objForward, objUp);
            }
            // --- �V������]���v�Z����A�����܂� ---

            // --- �V�����ʒu�E��]��K�p����A�������� ---
            Vector3 newPosition = Vector3.Lerp(HostTransform.position, draggingPosition + cameraTransform.TransformDirection(objRefGrabPoint), PositionLerpSpeed);
            // �ŏI�I�Ȉʒu��K�p����
            if (hostRigidbody == null)
            {
                HostTransform.position = newPosition;
            }
            else
            {
                hostRigidbody.MovePosition(newPosition);
            }

            // �ŏI�I�ȉ�]��K�p����
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
            // --- �V�����ʒu�E��]��K�p����A�����܂� ---
        }

        /// <summary>
        /// �I�u�W�F�N�g�̃h���b�O���~�߂�
        /// </summary>
        public void StopDragging()
        {
            if (!isDragging)
            {
                return;
            }

            // ���[�_�����͑Ώۂ��玩�g���폜����
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
                eventData.Use(); // �C�x���g���g��ꂽ���Ƃ��L�^���āA���̏����Ɏ󂯎����̂�h��

                StopDragging();
            }
        }

        public void OnInputDown(InputEventData eventData)
        {
            if (isDragging)
            {
                // ���łɃh���b�O�̓��͂��󂯎���ď������Ă���̂ŁA�V�����h���b�O����͊J�n���Ȃ�
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

            /* �R�����g�A�E�g
            FocusDetails? details = FocusManager.Instance.TryGetFocusDetails(eventData);

            Vector3 initialDraggingPosition = (details == null)
                ? HostTransform.position
                : details.Value.Point;
            */
            // �ǉ�
            Vector3 initialDraggingPosition = HostTransform.position;

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
