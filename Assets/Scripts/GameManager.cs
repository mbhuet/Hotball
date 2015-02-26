using UnityEngine;
using System.Collections;
using System.Collections.Generic;


using UnityEngine.UI;

public enum GameMode{DEATHMATCH, OBJECTIVE}
public enum HealthMode{STUN, STOCK}

public class GameManager : MonoBehaviour {
	public static GameManager Instance;
	public HealthMode healthMode;
	public GameMode gameMode;
	public int numBalls;
	public int numBarriers;

	public GameObject ballPickupPrefab;
	public GameObject barrierPickupPrefab;
	List<Player> players;

	public int startingHealth = 3;

	public bool rightAim;
	public bool leftAim;
	public bool useTriggers;
	public bool rightBallCtrl;

//	public bool droppableBarriers;
	public bool ballsReturn;
	public bool holdMultipleWeapons;
	public bool ballRespawn;

	public float court_height = 10;
	// Use this for initialization
	void Start () {
		Instance = this;
		players = new List<Player> ();
		PlaceBalls();
		PlaceBarriers ();

		Player.rightStickAim = rightAim;
		Player.leftStickAim = leftAim;
		Player.useTriggers = useTriggers;
		Player.ballControlRightStick = rightBallCtrl;


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
		if (Input.GetKeyDown(KeyCode.Alpha5)){
			Application.LoadLevel(3);
		}



	}

	void PlaceBalls(){
		for (int i= 0; i< numBalls; i++) {
			GameObject.Instantiate(ballPickupPrefab, Vector3.zero + Vector3.up * (court_height/2 - court_height/(numBalls + 1) * (i+1)), Quaternion.identity);
		}
	}

	void PlaceBarriers(){
		for (int i= 0; i< numBarriers; i++) {
			GameObject.Instantiate(barrierPickupPrefab, Vector3.right * 2 + Vector3.up * (court_height/2 - court_height/(numBarriers + 1) * (i+1)), Quaternion.identity);
		}
		for (int i= 0; i< numBarriers; i++) {
			GameObject.Instantiate(barrierPickupPrefab, Vector3.right * -2 + Vector3.up * (court_height/2 - court_height/(numBarriers + 1) * (i+1)), Quaternion.identity);
		}
	}

	public void AddPlayer(Player p){
		players.Add (p);
		p.SetHealth (startingHealth);

	}


}
