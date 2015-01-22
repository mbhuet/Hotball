using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
	public int numBalls;
	public GameObject ball;

	float court_height = 10;
	// Use this for initialization
	void Start () {
		PlaceBalls();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Alpha1)){
			Application.LoadLevel(0);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2)){
			Application.LoadLevel(1);
		}
	}

	void PlaceBalls(){
		for (int i= 0; i< numBalls; i++) {
			GameObject.Instantiate(ball, Vector3.zero + Vector3.up * (court_height/2 - court_height/(numBalls + 1) * (i+1)), Quaternion.identity);
		}
	}
}
