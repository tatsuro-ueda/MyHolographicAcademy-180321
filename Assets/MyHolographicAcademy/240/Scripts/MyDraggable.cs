using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class MyDraggable : MonoBehaviour, IManipulationHandler
{
    [Tooltip("デバッグログ")]
    public TextMesh DebugLog;

    private Vector3 positionOnManipulationStarted;

    public void OnManipulationCanceled(ManipulationEventData eventData)
    {
        Debug.Log("OnManipulationCanceled");
        InputManager.Instance.RemoveGlobalListener(this.gameObject);
    }

    public void OnManipulationCompleted(ManipulationEventData eventData)
    {
        Debug.Log("OnManipulationCompleted");
        InputManager.Instance.RemoveGlobalListener(this.gameObject);
    }

    public void OnManipulationStarted(ManipulationEventData eventData)
    {
        Debug.Log("OnManipulationStarted");
        positionOnManipulationStarted = transform.position;
        InputManager.Instance.AddGlobalListener(this.gameObject);
    }

    public void OnManipulationUpdated(ManipulationEventData eventData)
    {
        transform.position = positionOnManipulationStarted + eventData.CumulativeDelta;
    }

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
