using HoloToolkit.Sharing;
using HoloToolkit.Sharing.Spawning;
using HoloToolkit.Sharing.Tests;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    /// <summary>
    /// 自分がシェアリングサーバに参加したときに、磁石を生成する
    /// </summary>
    public class MagnetSpawner : MonoBehaviour
    {
        /// <summary>
        /// PrefabSpawnManagerへの参照を取るためのフィールド
        /// </summary>
        [SerializeField]
        private PrefabSpawnManager spawnManager;

        /// <summary>
        /// デバッグログの表示先
        /// </summary>
        public TextMesh DebugLogText;

        /// <summary>
        /// ユーザーID
        /// </summary>
        private long userId;

        /// <summary>
        /// シェアリングサーバーに接続するのを待つ
        /// </summary>
        private void Start()
        {
            // SharingStage should be valid at this point, but we may not be connected.
            if (SharingStage.Instance.IsConnected)
            {
                this.Connected();
            }
            else
            {
                SharingStage.Instance.SharingManagerConnected += this.Connected;
                this.DebugLogText.text += "\n[MagnetSpawner] Add event SharingManagerConnected";
            }
        }

        /// <summary>
        /// シェアリングサーバに接続すると呼ばれる
        /// ここからさらにユーザが参加するのを待つ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connected(object sender = null, System.EventArgs e = null)
        {
            this.DebugLogText.text += "\n[MagnetSpawner] Connected";
            SharingStage.Instance.SharingManagerConnected -= this.Connected;

            SharingStage.Instance.SessionUsersTracker.UserJoined += this.UserJoinedSession;
            this.DebugLogText.text += "\n[MagnetSpawner] Add event UserJoined";
            SharingStage.Instance.SessionUsersTracker.UserLeft += this.UserLeftSession;
        }

        /// <summary>
        /// 新しいユーザが、現在のセッションから退出すると呼ばれる
        /// </summary>
        /// <param name="user">現在のセッションから退出したユーザ</param>
        private void UserLeftSession(User user)
        {
            this.DebugLogText.text += "\n[MagnetSpawner] UserLeftSession(User user) > user.GetID(): " + user.GetID().ToString();
            int userId = user.GetID();
            /*
            if (userId != SharingStage.Instance.Manager.GetLocalUser().GetID())
            {
                this.RemoveRemoteMagnet(RemoteMagnetManager.Instance.RemoteMagnets[userId].MagnetObject);
                RemoteMagnetManager.Instance.RemoteMagnets.Remove(userId);
            }
            IEnumerable<SyncSpawnedObject> query = SharingStage.Instance.Root.InstantiatedPrefabs.Where(o => o.OwnerId == userId);
            foreach (SyncSpawnedObject sso in query)
            {
                SharingStage.Instance.Root.InstantiatedPrefabs.RemoveObject(sso);
            }
            */
            // 自分の磁石を削除する
            /*
            IEnumerable<SyncSpawnedObject> query = SharingStage.Instance.Root.InstantiatedPrefabs.Where(o => o.OwnerId == userId);
            foreach (SyncSpawnedObject sso in query)
            {
                GameObject go = sso.GameObject;
                Object.DestroyImmediate(go);
                spawnManager.Delete(sso);
                // 以下の行を有効にすると、UserLeftSessionしたきりになってしまう。なぜ？
                // SharingStageに格納されるSyncRootの情報は、以下の行で正しく削除される
                //SharingStage.Instance.Root.InstantiatedPrefabs.RemoveObject(sso);
            }
            */

            this.DeleteMyMagnets();
        }

        /// <summary>
        /// 新しいユーザが、現在のセッションに参加すると呼ばれる
        /// </summary>
        /// <param name="user">現在のセッションに参加したユーザ</param>
        private void UserJoinedSession(User user)
        {
            this.userId = SharingStage.Instance.Manager.GetLocalUser().GetID();
            this.DebugLogText.text += "\n[MagnetSpawner] UserJoinedSession(User user) > user.GetID(): " +
                user.GetID().ToString();
            this.DebugLogText.text += "\n[MagnetSpawner] UserJoinedSession(User user) > " +
                "SharingStage.Instance.Manager.GetLocalUser().GetID(): " +
                SharingStage.Instance.Manager.GetLocalUser().GetID().ToString();

            this.DeleteMyMagnets();

            this.CreateMagnet(this.userId);
        }

        /// <summary>
        /// 新たに参加したユーザのユーザIDの磁石がない場合、PrefabSpawnManagerを使ってSpawnする
        /// </summary>
        /// <param name="userId"></param>
        private void CreateMagnet(long userId)
        {
            /*
            RemoteMagnetManager.RemoteMagnetInfo magnetInfo;

            if (!RemoteMagnetManager.Instance.RemoteMagnets.TryGetValue(userId, out magnetInfo))
            {
            */
            Vector3 position = new Vector3(0, 0, 1.5f);
                Quaternion rotation = Quaternion.identity;
                var spawnedObject = new SyncSpawnedMagnet();
                this.spawnManager.Spawn(spawnedObject, position, rotation, null, "SpawnedMagnet", true);

                /*
                // 生成した磁石にuserIdを紐付ける
                magnetInfo = new RemoteMagnetManager.RemoteMagnetInfo();
                magnetInfo.UserID = userId;
                magnetInfo.MagnetObject = GameObject.Find("SpawnedMagnet");

                RemoteMagnetManager.Instance.RemoteMagnets.Add(userId, magnetInfo);
                */
            //}
        }

        /// <summary>
        /// ユーザがセッションから退出した際に磁石を削除する
        /// </summary>
        /// <param name="remoteMagnetObject"></param>
        private void RemoveRemoteMagnet(GameObject remoteMagnetObject)
        {
            Object.DestroyImmediate(remoteMagnetObject);
        }

        private void DeleteMyMagnets()
        {
            var magnets = GameObject.FindGameObjectsWithTag("Magnet");
            foreach (var magnet in magnets)
            {
                var syncModelAccessor = magnet.GetComponent<DefaultSyncModelAccessor>();
                if (syncModelAccessor != null)
                {
                    var syncSpawnObject = (SyncSpawnedObject)syncModelAccessor.SyncModel;
                    if(syncSpawnObject.OwnerId ==
                        SharingStage.Instance.Manager.GetLocalUser().GetID())
                    {
                        spawnManager.Delete(syncSpawnObject);
                    }
                }
            }
        }
    }
}