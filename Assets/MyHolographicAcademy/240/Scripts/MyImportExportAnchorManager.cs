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

        /// <summary>
        /// 格納されたアンカーデータを保持する
        /// </summary>
        private byte[] rawAnchorData;

        /// <summary>
        /// 接続した現在のルームを保持する。アンカー（複数）はルームに保持されている。
        /// </summary>
        private Room currentRoom;

        /// <summary>
        /// シェアリングサービスが準備完了しているか否かを保持する。
        /// シェアリングアンカーのためにデータをアップロードしたりダウンロードしたりするためには、
        /// シェアリングサービスが準備完了でなければならない。
        /// </summary>
        private bool sharingServiceReady;

        #endregion

        #region MonoBehaviour Lifecycle

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

        private void RoomManagerListener_AnchorsDownloaded(bool succesful, AnchorDownloadRequest request, XString failureReason)
        {
            // rawAnchorData にアンカー情報を格納する
            if (succesful)
            {
                int dataSize = request.GetDataSize();
                AnchorDebugText.text += string.Format("\nAnchor size: {0} bytes.", dataSize.ToString());

                this.rawAnchorData = new byte[dataSize];

                // Update() でインポートする
                currentState = ImportExportState.DataReady;
            }
            else
            {
                // ダウンロードに失敗したら、再試行する
                AnchorDebugText.text += string.Format("\nAnchor download failed " + failureReason);
                MakeAnchorDataRequest();
            }
        }

        /// <summary>
        /// ルーム内のアンカーが変化したときに呼ばれる
        /// </summary>
        /// <param name="obj"></param>
        private void RoomManagerListener_AnchorsChanged(Room room)
        {
            AnchorDebugText.text += string.Format("\nAnchors in Room {0} changed", room.GetName());

            // アンカーが変化したルームにいるなら…
            if (currentRoom == room)
            {
                ResetState();
            }
        }

        /// <summary>
        /// ユーザがセッションに参加すると呼ばれる
        /// ルーム関連のリクエストをする準備をシェアリングサービスが完了したことを知らせるイベント
        /// </summary>
        /// <param name="session">参加したセッション</param>
        private void CurrentUserJoinedSession(Session session)
        {
            if (SharingStage.Instance.Manager.GetLocalUser().IsValid())
            {
                // Update() でInitApi() する条件
                sharingServiceReady = true;
            }
            else
            {
                AnchorDebugText.text += "\nUnable to get local user on session joined";
            }
        }

        /// <summary>
        /// ユーザがセッションから退席すると呼ばれる
        /// シェアリングサービスはルーム関連のリクエストを止めなければならない
        /// </summary>
        /// <param name="session">Session left.</param>
        private void CurrentUserLeftSession(Session session)
        {
            // Update() でInitApi() する条件
            sharingServiceReady = false;

            // セッションに再接続して新しいルームに参加できるように状態をリセットする
            ResetState();
        }

        #endregion

        #region Private Methods

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

            // Update() で InitRoomApi() を実行する
            currentState = ImportExportState.Ready;
        }

        /// <summary>
        /// 共有アンカーをインポートするためのデータを取得する
        /// ダウンロードが終了すると、RoomManager は RoomManagerListener_AnchorsDownloaded を起こす
        /// </summary>
        private void MakeAnchorDataRequest()
        {
            // DownloadAnchor でルームからアンカーをダウンロードする
            if (roomManager.DownloadAnchor(currentRoom, currentRoom.GetAnchorName(0)))
            {
                currentState = ImportExportState.DataRequested;
            }
            else
            {
                AnchorDebugText.text += "Anchor Manager: Couldn't make the download request.";
                currentState = ImportExportState.Failed;
            }
        }

        /// <summary>
        /// Anchor Manager の状態をリセットする
        /// </summary>
        private void ResetState()
        {
            if (anchorStore != null)
            {
                // Update() で InitRoomApi() を実行する
                currentState = ImportExportState.Ready;
            }
            else
            {
                currentState = ImportExportState.AnchorStore_Initializing;
            }
        }

        #endregion
    }
}