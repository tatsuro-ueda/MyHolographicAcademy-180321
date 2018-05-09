using HoloToolkit.Sharing;
using UnityEngine;

/// <summary>
/// サーバIP手動設定パネルを、サーバに接続したら非表示にし、サーバから切断されたら表示する
/// </summary>
public class ManualIpConfigUtilityManager : MonoBehaviour
{
    /// <summary>
    /// サーバIP手動設定パネルのオブジェクト
    /// 表示・非表示を切り替えるためにUnityのフィールドから設定する
    /// </summary>
    public GameObject ManualIpConfigUtility;

    /// <summary>
    /// サーバに接続したときと、サーバから切断されたときのイベントを準備する
    /// </summary>
    private void Start()
    {
        SharingStage.Instance.SharingManagerConnected += this.Connected;
        SharingStage.Instance.SharingManagerDisconnected += this.DisConnected;
    }

    /// <summary>
    /// サーバに接続したら、サーバIP手動設定パネルを消す
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Connected(object sender, System.EventArgs e)
    {
        this.ManualIpConfigUtility.SetActive(false);
    }

    /// <summary>
    /// サーバから切断されたら、サーバIP手動設定パネルを表示する
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DisConnected(object sender, System.EventArgs e)
    {
        this.ManualIpConfigUtility.SetActive(true);
    }
}
