using UnityEngine;

public class ActivateWithDelay : MonoBehaviour {

    public GameObject ToActivate;
    public float Delay = 1;

    // Use this for initialization
    private void OnEnable () {
        ToActivate.SetActive(false);
        Invoke(nameof(ActivateGo), Delay);
    }
	
    // Update is called once per frame
    private void ActivateGo () {
        ToActivate.SetActive(true);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(ActivateGo));
    }
}
