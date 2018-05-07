using HoloToolkit.Sharing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameLogger : MonoBehaviour {

    public TextMesh DebugLogText;

	// Use this for initialization
	void Start () {
        int userId = gameObject.GetComponent<DefaultSyncModelAccessor>().SyncModel.OwnerId;
        DebugLogText = GameObject.Find("Debug Log").GetComponent<TextMesh>();
        DebugLogText.text += "\nownerId = " + userId.ToString();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
