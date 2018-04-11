// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Sharing;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    /// <summary>
    /// Broadcasts the Cube transform of the local user to other users in the session,
    /// and adds and updates the Cube transforms of remote users.
    /// Cube transforms are sent and received in the local coordinate space of the GameObject this component is on.
    /// </summary>
    public class RemoteCubeManager : MonoBehaviour
    {
        public class RemoteCubeInfo
        {
            public long UserID;
            public GameObject CubeObject;
        }

        /// <summary>
        /// Debug text for displaying information.
        /// </summary>
        public TextMesh DebugLogText;

        /// <summary>
        /// Debug text for displaying information.
        /// </summary>
        public TextMesh DebugLog2Text;

        [Tooltip("Sharingプレハブをヒエラルキービューに入れたもの")]
        public GameObject SharingPrefabObject;

        /// <summary>
        /// Keep a list of the remote Cubes, indexed by XTools userID
        /// </summary>
        private Dictionary<long, RemoteCubeInfo> remoteCubes = new Dictionary<long, RemoteCubeInfo>();

        private void Start()
        {
            CustomMessagesMyHolographicAcademy.Instance.MessageHandlers
                [CustomMessagesMyHolographicAcademy.TestMessageID.MagnetTransform] = UpdateCubeTransform;
            //CustomMessages.Instance.MessageHandlers[CustomMessages.TestMessageID.HeadTransform] = UpdateCubeTransform;
            DebugLogText.text += "\n[Cube] Set UpdateCubeTransform as MessageHandlers";

            // SharingStage should be valid at this point, but we may not be connected.
            if (SharingStage.Instance.IsConnected)
            {
                Connected();
            }
            else
            {
                SharingStage.Instance.SharingManagerConnected += Connected;
                DebugLogText.text += "\n[Cube] Add event SharingManagerConnected";
            }
        }

        private void Connected(object sender = null, EventArgs e = null)
        {
            DebugLogText.text += "\n[Cube] Connected";
            SharingStage.Instance.SharingManagerConnected -= Connected;

            SharingStage.Instance.SessionUsersTracker.UserJoined += UserJoinedSession;
            DebugLogText.text += "\n[Cube] Add event UserJoined";
            SharingStage.Instance.SessionUsersTracker.UserLeft += UserLeftSession;
        }

        private void Update()
        {
            if (transform.hasChanged)
            {
                /*
                // Grab the current Cube transform and broadcast it to all the other users in the session
                Transform CubeTransform = CameraCache.Main.transform;

                // Transform the Cube position and rotation from world space into local space
                Vector3 CubePosition = transform.InverseTransformPoint(CubeTransform.position);
                Quaternion CubeRotation = Quaternion.Inverse(transform.rotation) * CubeTransform.rotation;
                CustomMessages.Instance.SendHeadTransform(CubePosition, CubeRotation);

                DebugLog2Text.text = "\nHead > "
                    + "\nPosition: " + CubeTransform.position.ToString()
                    + "\nCube > "
                    + "\nPosition: " + Cube2Transform.position.ToString();
                */

                // Transform the head position and rotation from world space into local space
                Vector3 cubePosition = SharingPrefabObject.transform.
                    InverseTransformPoint(transform.position);
                /*
                Vector3 cubePosition = GameObject.Find("Sharing").transform.
                    InverseTransformPoint(transform.position);
                */
                /*
                Quaternion cubeRotation = Quaternion.Inverse(transform.rotation) *
                    GameObject.Find("Sharing").transform.rotation;
                */
                /*
                Quaternion cubeRotation = Quaternion.Euler(GameObject.Find("Sharing").transform.
                    InverseTransformDirection(GameObject.Find("Sharing Cube").transform.eulerAngles));
                */
                Quaternion cubeRotation = Quaternion.Euler(SharingPrefabObject.transform.
                    InverseTransformDirection(transform.eulerAngles));
                CustomMessagesMyHolographicAcademy.Instance.SendMagnetTransform(cubePosition, cubeRotation);
                DebugLog2Text.text = "\nSend Cube > " +
                    "\nPosition: " + cubePosition.ToString();
            }
            else
            {
                DebugLog2Text.text = "\nSend Cube > " + "\nPosition: not changed";
            }
        }

        protected void OnDestroy()
        {
            if (SharingStage.Instance != null)
            {
                if (SharingStage.Instance.SessionUsersTracker != null)
                {
                    SharingStage.Instance.SessionUsersTracker.UserJoined -= UserJoinedSession;
                    SharingStage.Instance.SessionUsersTracker.UserLeft -= UserLeftSession;
                }
            }
        }

        /// <summary>
        /// Called when a new user is leaving the current session.
        /// </summary>
        /// <param name="user">User that left the current session.</param>
        private void UserLeftSession(User user)
        {
            DebugLogText.text += "\n[Cube] UserLeftSession > User ID: " + user.GetID().ToString();
            /*
            int userId = user.GetID();
            if (userId != SharingStage.Instance.Manager.GetLocalUser().GetID())
            {
                RemoveRemoteCube(remoteCubes[userId].CubeObject);
                remoteCubes.Remove(userId);
            }
            */
        }

        /// <summary>
        /// Called when a user is joining the current session.
        /// </summary>
        /// <param name="user">User that joined the current session.</param>
        private void UserJoinedSession(User user)
        {
            DebugLogText.text += "\n[Cube] UserJoinedSession > User ID: " + user.GetID().ToString();
            /*
            if (user.GetID() != SharingStage.Instance.Manager.GetLocalUser().GetID())
            {
                GetRemoteCubeInfo(user.GetID());
            }
            */
        }

        /*
        /// <summary>
        /// Gets the data structure for the remote users' Cube position.
        /// </summary>
        /// <param name="userId">User ID for which the remote Cube info should be obtained.</param>
        /// <returns>RemoteCubeInfo for the specified user.</returns>
        public RemoteCubeInfo GetRemoteCubeInfo(long userId)
        {
            //AnchorDebugText.text += "\nGet remote cube info";
            RemoteCubeInfo CubeInfo;

            // Get the Cube info if its already in the list, otherwise add it
            if (!remoteCubes.TryGetValue(userId, out CubeInfo))
            {
                CubeInfo = new RemoteCubeInfo();
                CubeInfo.UserID = userId;
                DebugLogText.text += "\n[Cube] GetRemoteCubeInfo > user ID: " + userId.ToString();
                CubeInfo.CubeObject = CreateRemoteCube();

                remoteCubes.Add(userId, CubeInfo);
                DebugLogText.text += "\n[Cube] GetRemoteCubeInfo > add cube to remote cubes dictionary";
            }

            return CubeInfo;
        }
        */

        /// <summary>
        /// Called when a remote user sends a Cube transform.
        /// </summary>
        /// <param name="msg"></param>
        private void UpdateCubeTransform(NetworkInMessage msg)
        {
            // Parse the message
            long userID = msg.ReadInt64();

            transform.position = SharingPrefabObject.transform.TransformPoint(
                CustomMessagesMyHolographicAcademy.Instance.ReadVector3(msg));
            /*
            transform.position = GameObject.Find("Sharing").transform.TransformPoint(
                CustomMessagesMyHolographicAcademy.Instance.ReadVector3(msg));
            */
            /*
            transform.rotation = Quaternion.Inverse(GameObject.Find("Sharing").transform.rotation) *
                transform.rotation;
            */
            transform.rotation = Quaternion.Euler(SharingPrefabObject.transform.TransformDirection(
                CustomMessagesMyHolographicAcademy.Instance.ReadQuaternion(msg).eulerAngles));
            /*
            transform.rotation = Quaternion.Euler(GameObject.Find("Sharing").transform.TransformDirection(
                CustomMessagesMyHolographicAcademy.Instance.ReadQuaternion(msg).eulerAngles));
            */
            DebugLog2Text.text += "\nUpdate Cube > " +
                "\nPosition: " + transform.position.ToString();
        }

        /*
        /// <summary>
        /// Creates a new game object to represent the user's Cube.
        /// </summary>
        /// <returns></returns>
        private GameObject CreateRemoteCube()
        {
            GameObject newCubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DebugLogText.text += "\n[Cube] CreateRemoteCube > Create Cube";
            newCubeObj.transform.parent = gameObject.transform;
            newCubeObj.transform.localScale = Vector3.one * 0.05f;
            return newCubeObj;
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
        */
    }
}