using UnityEngine;
using System.Collections;

public class ResetScene : MonoBehaviour {

	// Use this for initialization

	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Return)) {
			Application.LoadLevel(Application.loadedLevel);		
		}
	
	}
}
