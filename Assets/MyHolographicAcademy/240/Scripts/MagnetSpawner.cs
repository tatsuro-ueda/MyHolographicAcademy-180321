using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetSpawner : MonoBehaviour {

	// Use this for initialization
	void Start () {
        // プレハブを取得
        GameObject prefab = (GameObject)Resources.Load("Prefabs/Sharing Magnet");
        Vector3 position = new Vector3(0, 0, 2.0f);
        // プレハブからインスタンスを生成
        Instantiate(prefab, position, Quaternion.identity);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
