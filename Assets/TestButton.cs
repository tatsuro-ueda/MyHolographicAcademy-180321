using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Sharing.Spawning;

public class TestButton : MonoBehaviour, IInputClickHandler {
    /// <summary>
    /// PrefabSpawnManagerへの参照を取るためのフィールド
    /// </summary>
    [SerializeField]
    private PrefabSpawnManager spawnManager;

    public void OnInputClicked(InputClickedEventData eventData)
    {
        Vector3 position = new Vector3(0, 0, 3f);
        Quaternion rotation = Quaternion.identity;
        var spawnedObject = new SyncSpawnedObject();
        this.spawnManager.Spawn(spawnedObject, position, rotation, null, "SpawnedCube", true);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
