using UnityEngine;
using System.Collections;

public class RFX4_StartDelay : MonoBehaviour {

    public GameObject ActivatedGameObject;
    public float Delay = 1;
	public GameObject Avatar;

	// Use this for initialization
	void OnEnable () {
        ActivatedGameObject.SetActive(false);
        Invoke("ActivateGO", Delay);
	}
	
	// Update is called once per frame
	void ActivateGO () {
		ActivatedGameObject.SetActive(true);
		Avatar = GameObject.Find("Character(Clone)");
		Avatar.GetComponent<Animator>().Play("Attack1");
	}

    void OnDisable()
    {
        CancelInvoke("ActivateGO");
    }
}
