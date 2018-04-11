// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Sharing;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    /// <summary>
    /// Broadcasts the Cube transform of the local user to other users in the session,
    /// and adds and updates the Cube transforms of remote users.
    /// Cube transforms are sent and received in the local coordinate space of the GameObject this component is on.
    /// </summary>
    public class RemoteCubeManager1 : Singleton<RemoteCubeManager1>
    {
        public class RemoteCubeInfo
        {
            public long UserID;
            public GameObject CubeObject;
        }

        private GameObject localCubeObject;

        /// <summary>
        /// Keep a list of the remote Cubes, indexed by XTools userID
        /// </summary>
        private Dictionary<long, RemoteCubeInfo> remoteCubes = new Dictionary<long, RemoteCubeInfo>();

        private void Start()
        {
            localCubeObject = CreateLocalCube();

            CustomMessagesMyHolographicAcademy.Instance.MessageHandlers
                [CustomMessagesMyHolographicAcademy.TestMessageID.MagnetTransform] = UpdateCubeTransform;

            // SharingStage should be valid at this point, but we may not be connected.
            if (SharingStage.Instance.IsConnected)
            {
                Connected();
            }
            else
            {
                SharingStage.Instance.SharingManagerConnected += Connected;
            }
        }

        private void Connected(object sender = null, EventArgs e = null)
        {
            SharingStage.Instance.SharingManagerConnected -= Connected;

            SharingStage.Instance.SessionUsersTracker.UserJoined += UserJoinedSession;
            SharingStage.Instance.SessionUsersTracker.UserLeft += UserLeftSession;
        }

        private void Update()
        {


            // Grab the current Cube transform and broadcast it to all the other users in the session
            Transform CameraTransform = CameraCache.Main.transform;
            // カメラの前方1.5m
            Vector3 CubePosition = CameraTransform.position + CameraTransform.forward * 1.5f;
            // 向きはカメラの向き
            Quaternion CubeRotation = Quaternion.Inverse(transform.rotation) * CameraTransform.rotation;

            // Transform the Cube position and rotation from world space into local space
            Vector3 RemoteCubePosition = transform.InverseTransformPoint(CubePosition);
            Quaternion RemoteCubeRotation = Quaternion.Inverse(transform.rotation) * CameraTransform.rotation;

            CustomMessagesMyHolographicAcademy.Instance.SendMagnetTransform(CubePosition, CubeRotation);
        }

        protected override void OnDestroy()
        {
            if (SharingStage.Instance != null)
            {
                if (SharingStage.Instance.SessionUsersTracker != null)
                {
                    SharingStage.Instance.SessionUsersTracker.UserJoined -= UserJoinedSession;
                    SharingStage.Instance.SessionUsersTracker.UserLeft -= UserLeftSession;
                }
            }

            base.OnDestroy();
        }

        /// <summary>
        /// Called when a new user is leaving the current session.
        /// </summary>
        /// <param name="user">User that left the current session.</param>
        private void UserLeftSession(User user)
        {
            int userId = user.GetID();
            if (userId != SharingStage.Instance.Manager.GetLocalUser().GetID())
            {
                RemoveRemoteCube(remoteCubes[userId].CubeObject);
                remoteCubes.Remove(userId);
            }
        }

        /// <summary>
        /// Called when a user is joining the current session.
        /// </summary>
        /// <param name="user">User that joined the current session.</param>
        private void UserJoinedSession(User user)
        {
            if (user.GetID() != SharingStage.Instance.Manager.GetLocalUser().GetID())
            {
                GetRemoteCubeInfo(user.GetID());
            }
        }

        /// <summary>
        /// Gets the data structure for the remote users' Cube position.
        /// </summary>
        /// <param name="userId">User ID for which the remote Cube info should be obtained.</param>
        /// <returns>RemoteCubeInfo for the specified user.</returns>
        public RemoteCubeInfo GetRemoteCubeInfo(long userId)
        {
            RemoteCubeInfo CubeInfo;

            // Get the Cube info if its already in the list, otherwise add it
            if (!remoteCubes.TryGetValue(userId, out CubeInfo))
            {
                CubeInfo = new RemoteCubeInfo();
                CubeInfo.UserID = userId;
                CubeInfo.CubeObject = CreateRemoteCube();

                remoteCubes.Add(userId, CubeInfo);
            }

            return CubeInfo;
        }

        /// <summary>
        /// Called when a remote user sends a Cube transform.
        /// </summary>
        /// <param name="msg"></param>
        private void UpdateCubeTransform(NetworkInMessage msg)
        {
            // Parse the message
            long userID = msg.ReadInt64();

            Vector3 CubePos = CustomMessagesMyHolographicAcademy.Instance.ReadVector3(msg);

            Quaternion CubeRot = CustomMessagesMyHolographicAcademy.Instance.ReadQuaternion(msg);

            RemoteCubeInfo CubeInfo = GetRemoteCubeInfo(userID);
            CubeInfo.CubeObject.transform.localPosition = CubePos;
            CubeInfo.CubeObject.transform.localRotation = CubeRot;
        }

        /// <summary>
        /// Creates a new game object to represent the user's Cube.
        /// </summary>
        /// <returns></returns>
        private GameObject CreateRemoteCube()
        {
            GameObject remoteCubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            remoteCubeObj.transform.parent = gameObject.transform;
            remoteCubeObj.transform.localScale = Vector3.one * 0.1f;
            return remoteCubeObj;
        }

        private GameObject CreateLocalCube()
        {
            GameObject localCubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //localCubeObj.transform.parent = gameObject.transform;
            localCubeObj.transform.position = Vector3.forward * 1.5f;
            localCubeObj.transform.rotation = Quaternion.identity;
            localCubeObj.transform.localScale = Vector3.one * 0.1f;
            return localCubeObj;
        }

        /// <summary>
        /// When a user has left the session this will cleanup their
        /// Cube data.
        /// </summary>
        /// <param name="remoteCubeObject"></param>
        private void RemoveRemoteCube(GameObject remoteCubeObject)
        {
            DestroyImmediate(remoteCubeObject);
        }
    }
}