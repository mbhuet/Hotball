using UnityEngine;
using System.Collections;
using System.Collections.Generic;


using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	public static GameManager Instance;
	public int numBalls;
	public GameObject ball;
	List<Player> players;

	public bool rightAim;
	public bool leftAim;
	public bool useTriggers;
	public bool rightBallCtrl;

	float court_height = 10;
	// Use this for initialization
	void Start () {
		Instance = this;
		players = new List<Player> ();
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
		if (Input.GetKeyDown(KeyCode.Alpha3)){
			Application.LoadLevel(2);
		}
		if (Input.GetKeyDown(KeyCode.Alpha4)){
			Application.LoadLevel(3);
		}



	}

	void PlaceBalls(){
		for (int i= 0; i< numBalls; i++) {
			GameObject.Instantiate(ball, Vector3.zero + Vector3.up * (court_height/2 - court_height/(numBalls + 1) * (i+1)), Quaternion.identity);
		}
	}

	public void AddPlayer(Player p){
		players.Add (p);
		p.rightStickAim = rightAim;
		p.leftStickAim = leftAim;
		p.useTriggers = useTriggers;
		p.ballControlRightStick = rightBallCtrl;
	}

	public void SetRightAim(bool b){
		foreach (Player p in players) {

			p.rightStickAim = b;
		}
		Debug.Log ("right aim");
	}
	public void SetLeftAim(bool b){
		foreach (Player p in players) {
			
			p.leftStickAim = b;
		}
	}
	public void SetTriggers(bool b){
		foreach (Player p in players) {
			
			p.useTriggers = b;
		}
	}

	public void SetRightBallControl(bool b){
		foreach (Player p in players) {
			
			p.ballControlRightStick = b;
		}
	}


}
