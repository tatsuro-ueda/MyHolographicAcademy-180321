using HoloToolkit.Sharing;
using HoloToolkit.Sharing.Spawning;
using HoloToolkit.Sharing.Tests;
using UnityEngine;

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

            // 他のユーザが参加したときは磁石の更新は行わない
            if (user.GetID() == this.userId)
            {
                this.DeleteMyMagnets();

                this.CreateMagnet(this.userId);
            }
        }

        /// <summary>
        /// 新たに参加したユーザのユーザIDの磁石がない場合、PrefabSpawnManagerを使ってSpawnする
        /// </summary>
        /// <param name="userId"></param>
        private void CreateMagnet(long userId)
        {
            Vector3 position = new Vector3(0, 0, 1.5f);
            Quaternion rotation = Quaternion.identity;
            var spawnedObject = new SyncSpawnedMagnet();
            this.spawnManager.Spawn(spawnedObject, position, rotation, null, "SpawnedMagnet", true);
        }

        /// <summary>
        /// ユーザがセッションから退出した際に磁石を削除する
        /// </summary>
        /// <param name="remoteMagnetObject"></param>
        private void RemoveRemoteMagnet(GameObject remoteMagnetObject)
        {
            Object.DestroyImmediate(remoteMagnetObject);
        }

        /// <summary>
        /// セッション参加が複数回起きて自分が生成した磁石が複数になった場合のために、
        /// 自分が生成した磁石をすべて削除する
        /// </summary>
        private void DeleteMyMagnets()
        {
            var magnets = GameObject.FindGameObjectsWithTag("Magnet");
            foreach (var magnet in magnets)
            {
                var syncModelAccessor = magnet.GetComponent<DefaultSyncModelAccessor>();
                if (syncModelAccessor != null)
                {
                    var syncSpawnObject = (SyncSpawnedObject)syncModelAccessor.SyncModel;
                    if (syncSpawnObject.OwnerId ==
                        SharingStage.Instance.Manager.GetLocalUser().GetID())
                    {
                        // 磁石のOnDestroyを走らせる
                        Object.DestroyImmediate(magnet);

                        this.spawnManager.Delete(syncSpawnObject);
                    }
                }
            }
        }
    }
}