using HoloToolkit.Sharing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharingMagnetMisc : MonoBehaviour {

    public TextMesh DebugLogText;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start()
    {
        DebugLogText = GameObject.Find("Debug Log").GetComponent<TextMesh>();

        // SharingStage should be valid at this point, but we may not be connected.
        if (SharingStage.Instance.IsConnected)
        {
            Connected();
        }
        else
        {
            SharingStage.Instance.SharingManagerConnected += Connected;
            DebugLogText.text += "\n[MagnetMisc] Add event SharingManagerConnected";
        }

        // プレハブを取得
        GameObject prefab = (GameObject)Resources.Load("Prefabs/Sharing Magnet");
        Vector3 position = new Vector3(0, 0, 2.0f);
        // プレハブからインスタンスを生成
        Instantiate(prefab, position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void Connected(object sender = null, System.EventArgs e = null)
    {
        DebugLogText.text += "\n[MagnetMisc] Connected";
        SharingStage.Instance.SharingManagerConnected -= Connected;

        SharingStage.Instance.SessionUsersTracker.UserJoined += UserJoinedSession;
        DebugLogText.text += "\n[MagnetMisc] Add event UserJoined";
        SharingStage.Instance.SessionUsersTracker.UserLeft += UserLeftSession;
    }

    /// <summary>
    /// Called when a new user is leaving the current session.
    /// </summary>
    /// <param name="user">User that left the current session.</param>
    private void UserLeftSession(User user)
    {
        DebugLogText.text += "\n[MagnetMisc] UserLeftSession(User user) > user.GetID(): " + user.GetID().ToString();
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
        DebugLogText.text += "\n[MagnetMisc] UserJoinedSession(User user) > user.GetID(): " +
            user.GetID().ToString();
        DebugLogText.text += "\n[MagnetMisc] UserJoinedSession(User user) > " +
            "SharingStage.Instance.Manager.GetLocalUser().GetID(): " +
            SharingStage.Instance.Manager.GetLocalUser().GetID().ToString();

        if (user.GetID() != SharingStage.Instance.Manager.GetLocalUser().GetID())
        {
            //GetRemoteMagnetInfo(user.GetID());
        }
    }
}