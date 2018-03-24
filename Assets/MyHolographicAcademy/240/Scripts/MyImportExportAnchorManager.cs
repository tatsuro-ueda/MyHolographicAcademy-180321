using System;
using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using UnityEngine;
using UnityEngine.XR.WSA.Persistence;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    public class MyImportExportAnchorManager : Singleton<MyImportExportAnchorManager>
    {
        #region Public Valuables

        public enum ImportExportState
        {
            // 全体的な状態
            Start,
            Failed,
            Ready,
            RoomApiInitializing,
            RoomApiInitialized,
            AnchorEstablished,
            // AnchorStore アンカーの格納
            AnchorStore_Initializing,
            // アンカー作成
            InitialAnchorRequired,
            CreatingInitialAnchor,
            ReadyToExportInitialAnchor,
            UploadingInitialAnchor,
            // アンカーの状態
            DataRequested,
            DataReady,
            Importing
        }
        
        public ImportExportState CurrentState;

        /// <summary>
        /// 情報を表示するためのデバッグテキスト
        /// </summary>
        public TextMesh AnchorDebugText;

        /// <summary>
        /// アンカーが全部アップロードされると一度だけ呼ばれる
        /// </summary>
        public event Action<bool> AnchorUploaded;

        /// <summary>
        /// ユーザが全員退席した後にルームを維持するか否か
        /// </summary>
        public bool KeepRoomAlive;

        #endregion

        #region Private Valuables

        /// <summary>
        /// ローカルに格納されたアンカーを保持する
        /// </summary>
        private WorldAnchorStore anchorStore = null;

        /// <summary>
        /// シェアリングサービスのためのルーム管理 API を使うための変数
        /// </summary>
        private RoomManager roomManager;

        /// <summary>
        /// アンカー情報がアップロード・ダウンロードされたら更新を提供する
        /// </summary>
        private RoomManagerAdapter roomManagerListener;

        private ImportExportState currentState = ImportExportState.Start;

        #endregion

        #region Unity APIs

        protected override void Awake()
        {
            base.Awake();

            AnchorDebugText.text += "Import Export Manager starting\n";

            // アンカー格納庫を初期化する
            CurrentState = ImportExportState.AnchorStore_Initializing;
            WorldAnchorStore.GetAsync(AnchorStoreReady);
        }

        // Use this for initialization
        protected void Start()
        {
            // サーバーに接続する
            if (SharingStage.Instance.IsConnected)
            {
                Connected();
            }
            else
            {
                SharingStage.Instance.SharingManagerConnected += Connected;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        #endregion

        #region Event Callbacks

        private void Connected(object sender = null, EventArgs e = null)
        {
            SharingStage.Instance.SharingManagerConnected -= Connected;

            // 接続情報を表示し始める
            AnchorDebugText.text += "\nAnchor Manager: Starting...";
            AnchorDebugText.text += "\nConnected to Server";

            // サーバーに接続したので、ルームを準備する
            roomManager = SharingStage.Instance.Manager.GetRoomManager();

            // ルームを管理するコールバックを準備する
            roomManagerListener = new RoomManagerAdapter();
            roomManagerListener.AnchorUploadedEvent += RoomManagerListener_AnchorUploaded;
            roomManagerListener.AnchorsDownloadedEvent += RoomManagerListener_AnchorsDownloaded;
            roomManagerListener.AnchorsChangedEvent += RoomManagerListener_AnchorsChanged;

            // コールバックをルームに登録する
            roomManager.AddListener(roomManagerListener);

            // ？？？リクエストに関連付けされたルームをつくるためのシェアリングサービスが準備完了したことを示すために
            // セッション参加とセッション退席のコールバックを準備する
            SharingStage.Instance.SessionsTracker.CurrentUserJoined += CurrentUserJoinedSession;
            SharingStage.Instance.SessionsTracker.CurrentUserLeft += CurrentUserLeftSession;
        }

        /// <summary>
        /// アンカーのアップロードが完了したら呼ばれる
        /// </summary>
        private void RoomManagerListener_AnchorUploaded(bool successful, XString failureReason)
        {
            if (successful)
            {
                if (SharingStage.Instance.ShowDetailedLogs)
                {
                    Debug.Log("Anchor Manager: Successfully uploaded anchor");
                }

                if (AnchorDebugText != null)
                {
                    AnchorDebugText.text += "\nSuccessfully uploaded anchor";
                }

                currentState = ImportExportState.AnchorEstablished;
            }
            else
            {
                AnchorDebugText.text += ("\n Upload failed " + failureReason);
                AnchorDebugText.text += ("Anchor Manager: Upload failed " + failureReason);
                currentState = ImportExportState.Failed;
            }

            if (AnchorUploaded != null)
            {
                AnchorUploaded(successful);
            }
        }

        private void RoomManagerListener_AnchorsChanged(Room obj)
        {
            throw new NotImplementedException();
        }

        private void RoomManagerListener_AnchorsDownloaded(bool arg1, AnchorDownloadRequest arg2, XString arg3)
        {
            throw new NotImplementedException();
        }

        private void CurrentUserJoinedSession(Session obj)
        {
            throw new NotImplementedException();
        }

        private void CurrentUserLeftSession(Session obj)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// ローカルアンカー格納庫が準備完了になると呼ばれる
        /// </summary>
        /// <param name="store"></param>
        private void AnchorStoreReady(WorldAnchorStore store)
        {
            anchorStore = store;

            if (!KeepRoomAlive)
            {
                anchorStore.Clear();
            }

            currentState = ImportExportState.Ready;
        }
    }
}