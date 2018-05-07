// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    /// <summary>
    /// Broadcasts the Magnet transform of the local user to other users in the session,
    /// and adds and updates the Magnet transforms of remote users.
    /// Magnet transforms are sent and received in the local coordinate space of the GameObject this component is on.
    /// </summary>
    public class RemoteMagnetManager : Singleton<RemoteMagnetManager>
    {
        public class RemoteMagnetInfo
        {
            public long UserID;
            public GameObject MagnetObject;
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

        public bool hasSpawnedMagnet = false;

        /// <summary>
        /// Keep a list of the remote Magnets, indexed by XTools userID
        /// </summary>
        public Dictionary<long, RemoteMagnetInfo> RemoteMagnets = new Dictionary<long, RemoteMagnetInfo>();

        private bool hasUpdatedbyRemote;

        private Transform previousTransform;

        private void Start()
        {
            /*
            CustomMessagesMyHolographicAcademy.Instance.MessageHandlers
                [CustomMessagesMyHolographicAcademy.TestMessageID.MagnetTransform] = UpdateMagnetTransform;
            */
            //CustomMessages.Instance.MessageHandlers[CustomMessages.TestMessageID.HeadTransform] = UpdateMagnetTransform;
            DebugLogText.text += "\n[Magnet] Set UpdateMagnetTransform as MessageHandlers";

            // SharingStage should be valid at this point, but we may not be connected.
            if (SharingStage.Instance.IsConnected)
            {
                Connected();
            }
            else
            {
                SharingStage.Instance.SharingManagerConnected += Connected;
                DebugLogText.text += "\n[Magnet] Add event SharingManagerConnected";
            }
        }

        private void Connected(object sender = null, EventArgs e = null)
        {
            DebugLogText.text += "\n[Magnet] Connected";
            SharingStage.Instance.SharingManagerConnected -= Connected;

            SharingStage.Instance.SessionUsersTracker.UserJoined += UserJoinedSession;
            DebugLogText.text += "\n[Magnet] Add event UserJoined";
            SharingStage.Instance.SessionUsersTracker.UserLeft += UserLeftSession;
        }

        private void Update()
        {
            if (previousTransform != null)
            {
                DebugLog2Text.text = "\nPrevious position: " + previousTransform.position.ToString() +
                    "\nPresent position: " + transform.position.ToString() +
                    "\ntransform.hasChanged: " + transform.hasChanged.ToString();
            }

            /*
            if (!hasUpdatedbyRemote)
            {
            */
            /*
            // Grab the current Magnet transform and broadcast it to all the other users in the session
            Transform MagnetTransform = CameraCache.Main.transform;

            // Transform the Magnet position and rotation from world space into local space
            Vector3 MagnetPosition = transform.InverseTransformPoint(MagnetTransform.position);
            Quaternion MagnetRotation = Quaternion.Inverse(transform.rotation) * MagnetTransform.rotation;
            CustomMessages.Instance.SendHeadTransform(MagnetPosition, MagnetRotation);

            DebugLog2Text.text = "\nHead > "
                + "\nPosition: " + MagnetTransform.position.ToString()
                + "\nMagnet > "
                + "\nPosition: " + Magnet2Transform.position.ToString();
            */
            if (transform.hasChanged && !hasUpdatedbyRemote)
            {
                // Transform the head position and rotation from world space into local space
                Vector3 MagnetPositionFromSharingPrefab = SharingPrefabObject.transform.
                        InverseTransformPoint(transform.position);
                /*
                Vector3 MagnetPosition = GameObject.Find("Sharing").transform.
                    InverseTransformPoint(transform.position);
                */
                /*
                Quaternion MagnetRotation = Quaternion.Inverse(transform.rotation) *
                    GameObject.Find("Sharing").transform.rotation;
                */
                /*
                Quaternion MagnetRotation = Quaternion.Euler(GameObject.Find("Sharing").transform.
                    InverseTransformDirection(GameObject.Find("Sharing Magnet").transform.eulerAngles));
                */
                Quaternion MagnetRotationFromSharingPrefabObject =
                    Quaternion.Euler(SharingPrefabObject.transform.InverseTransformDirection(
                        transform.eulerAngles));
                /*
                CustomMessagesMyHolographicAcademy.Instance.SendMagnetTransform(
                    MagnetPositionFromSharingPrefab, MagnetRotationFromSharingPrefabObject);
                */
                DebugLog2Text.text += "\nSendMagnetTransform > " +
                    "\nMagnetPositionFromSharingPrefab: " + MagnetPositionFromSharingPrefab.ToString();
            }
            previousTransform = transform;
            transform.hasChanged = false;
            hasUpdatedbyRemote = false;
            /*
            }
            else
            {
                DebugLog2Text.text = "\nSend Magnet > " + "\nPosition: has updated";
                hasUpdatedbyRemote = false;
            }
            */
        }

        private void LateUpdate()
        {
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
        }

        /// <summary>
        /// Called when a new user is leaving the current session.
        /// </summary>
        /// <param name="user">User that left the current session.</param>
        private void UserLeftSession(User user)
        {
            DebugLogText.text += "\n[Magnet] UserLeftSession(User user) > user.GetID(): " + user.GetID().ToString();
            /*
            int userId = user.GetID();
            if (userId != SharingStage.Instance.Manager.GetLocalUser().GetID())
            {
                RemoveRemoteMagnet(remoteMagnets[userId].MagnetObject);
                remoteMagnets.Remove(userId);
            }
            */
        }

        /// <summary>
        /// Called when a user is joining the current session.
        /// </summary>
        /// <param name="user">User that joined the current session.</param>
        private void UserJoinedSession(User user)
        {
            DebugLogText.text += "\n[Magnet] UserJoinedSession(User user) > user.GetID(): " +
                user.GetID().ToString();
            DebugLogText.text += "\n[Magnet] UserJoinedSession(User user) > " +
                "SharingStage.Instance.Manager.GetLocalUser().GetID(): " +
                SharingStage.Instance.Manager.GetLocalUser().GetID().ToString();
            if (user.GetID() != SharingStage.Instance.Manager.GetLocalUser().GetID())
            {
                GetRemoteMagnetInfo(user.GetID());
            }
        }

        /// <summary>
        /// Gets the data structure for the remote users' Magnet position.
        /// </summary>
        /// <param name="userId">User ID for which the remote Magnet info should be obtained.</param>
        /// <returns>RemoteMagnetInfo for the specified user.</returns>
        public RemoteMagnetInfo GetRemoteMagnetInfo(long userId)
        {
            //AnchorDebugText.text += "\nGet remote Magnet info";
            RemoteMagnetInfo MagnetInfo;

            // Get the Magnet info if its already in the list, otherwise add it
            if (!RemoteMagnets.TryGetValue(userId, out MagnetInfo))
            {
                MagnetInfo = new RemoteMagnetInfo();
                MagnetInfo.UserID = userId;
                DebugLogText.text += "\n[Magnet] GetRemoteMagnetInfo > user ID: " + userId.ToString();
                MagnetInfo.MagnetObject = this.gameObject;

                /*
                remoteMagnets.Add(userId, MagnetInfo);
                DebugLogText.text += "\n[Magnet] GetRemoteMagnetInfo > add Magnet to remote Magnets dictionary";
                */
            }

            return MagnetInfo;
        }

        /// <summary>
        /// Called when a remote user sends a Magnet transform.
        /// </summary>
        /// <param name="msg"></param>
        private void UpdateMagnetTransform(NetworkInMessage msg)
        {
            if (!transform.hasChanged && !hasUpdatedbyRemote)
            {
                hasUpdatedbyRemote = true;

                // Parse the message
                long userID = msg.ReadInt64();

                Vector3 magnetPosition = CustomMessagesMyHolographicAcademy.Instance.ReadVector3(msg);
                Vector3 magnetDirection = 
                    CustomMessagesMyHolographicAcademy.Instance.ReadQuaternion(msg).eulerAngles;
                RemoteMagnetInfo magnetInfo = GetRemoteMagnetInfo(userID);

                magnetInfo.MagnetObject.transform.position = SharingPrefabObject.transform.TransformPoint(magnetPosition);
                /*
                transform.position = GameObject.Find("Sharing").transform.TransformPoint(
                    CustomMessagesMyHolographicAcademy.Instance.ReadVector3(msg));
                */
                /*
                transform.rotation = Quaternion.Inverse(GameObject.Find("Sharing").transform.rotation) *
                    transform.rotation;
                */
                magnetInfo.MagnetObject.transform.rotation = Quaternion.Euler(
                    SharingPrefabObject.transform.TransformDirection(magnetDirection));
                /*
                transform.rotation = Quaternion.Euler(GameObject.Find("Sharing").transform.TransformDirection(
                    CustomMessagesMyHolographicAcademy.Instance.ReadQuaternion(msg).eulerAngles));
                DebugLog2Text.text += "\nUpdate Magnet > " +
                    "\nPosition: " + transform.position.ToString();
                */
                DebugLog2Text.text += "\nNetworkInMessage msg > " +
                    "\nuserID: " + userID.ToString();
            }
        }

        /*
        /// <summary>
        /// Creates a new game object to represent the user's Magnet.
        /// </summary>
        /// <returns></returns>
        private GameObject CreateRemoteMagnet()
        {
            GameObject newMagnetObj = GameObject.CreatePrimitive(PrimitiveType.Magnet);
            DebugLogText.text += "\n[Magnet] CreateRemoteMagnet > Create Magnet";
            newMagnetObj.transform.parent = gameObject.transform;
            newMagnetObj.transform.localScale = Vector3.one * 0.05f;
            return newMagnetObj;
        }

        /// <summary>
        /// When a user has left the session this will cleanup their
        /// Magnet data.
        /// </summary>
        /// <param name="remoteMagnetObject"></param>
        private void RemoveRemoteMagnet(GameObject remoteMagnetObject)
        {
            DestroyImmediate(remoteMagnetObject);
        }
        */
    }
}