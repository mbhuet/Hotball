using UnityEngine;
using System.Collections;
using XInputDotNetPure;


public enum Team{ONE, TWO};

public class Player : MonoBehaviour {
	public int playerNum;

	GameObject visual;

	public Team team;

	public int health = 3;
	public float speed;
	public Ball heldBall;
	public bool isThrowing;
	public float scaleFactor;
	public bool againstWall;
	public Vector3 moveVector;
	public Color color;
	private LineCircle[] healthBalls;
	private MeshFilter[] dmgBalls;

	float defenseRadius = 1.4f;
	protected PlayerIndex gamepadNum;
	protected GamePadState gamepad;
	protected LineCircle ring;

	bool defenseAvailable = true;

	public GameObject[] healthBars;
	public AudioClip throwSound;
	public AudioClip pickupSound;
	public AudioClip catchSound;

	public bool isDead = false;

	
	// Use this for initialization
	void Start () {
		Init ();
	}
	
	// Update is called once per frame
	void Update () {
		gamepad = GamePad.GetState(gamepadNum);

		if (playerNum == 0){
			if (Input.GetButtonDown("Fire1")){
				Throw ();
			}
			if (Input.GetButtonDown("Fire2") && defenseAvailable){
				//Debug.Log("Fire2 down");
				//StartCoroutine ("ActiveDefense");
				Catch ();
				StartCoroutine("DefenseCooldown");


			}
			if (Input.GetButtonUp("Fire2") && !defenseAvailable && defenseRadius >0){
				//Debug.Log("Fire2 up");

				//Catch ();
				//StopCoroutine("ActiveDefense");
				//StartCoroutine("DefenseCooldown");
			}
		}
		else{
			if (gamepad.Buttons.A == ButtonState.Pressed){
				Throw ();
			}
			if (gamepad.Buttons.B == ButtonState.Pressed && defenseAvailable){
				Catch ();
				StartCoroutine("DefenseCooldown");

				//StartCoroutine ("ActiveDefense");
			}
			if (gamepad.Buttons.B == ButtonState.Released && !defenseAvailable  && defenseRadius >0){
				//Catch ();
				//StopCoroutine("ActiveDefense");
				//StartCoroutine("DefenseCooldown");
			}
		}
	}

	void FixedUpdate(){
		Move ();
	}

	void SetColor(Color c){
		//renderer.material.color = color;
		visual.renderer.material.color = color;
		particleSystem.startColor = color;
	}

	void Throw(){
		if(heldBall != null){
			heldBall.Release (this.transform.right);
			heldBall = null;
			PlayThrowSound();
		}
	}

	void Block(Ball ball){

	}

	void Catch(){
		Collider2D[] objectsInRing = Physics2D.OverlapCircleAll (new Vector2 (transform.position.x, transform.position.y), defenseRadius + .1f);
		foreach (Collider2D obj in objectsInRing) {
			if (obj.gameObject.tag == "Ball"){
				//StartCoroutine("SlowMotion", .1f);
				Ball ball  = obj.GetComponent<Ball>();
				//catching an opponent's ball
				if(!ball.isNeutral && ball.owner.team != this.team){
					Camera.main.audio.PlayOneShot (catchSound);
					ball.particleSystem.Emit(10);
					if (heldBall == null){
						Pickup(ball);
						RecoverHealth();
					}
					else{
						ball.Deflect(ball.transform.position - this.transform.position);
						ball.SetNeutral();

					}
				}
				/*
				if (ball.isNeutral){
				}
				else{
					if (heldBall == null){
						ball.particleSystem.Emit(10);
						//Camera.main.audio.PlayOneShot (catchSound);

						Pickup(ball);
					}
					else{ 
						ball.Deflect(ball.transform.position - this.transform.position);
						//Camera.main.audio.PlayOneShot (catchSound);

						ball.particleSystem.Emit(10);
						ball.SetNeutral();
					}
				}
				*/
			}
		}
	}

	void Pickup(Ball b){
//		Debug.Log("Pickup Ball");
		PlayPickupSound ();
		heldBall = b;
		heldBall.SetOwner (this);
	}

	void Move(){
		float leftX, leftY;
		if (playerNum == 0){
			leftX = Input.GetAxis ("Horizontal");
			leftY = Input.GetAxis ("Vertical");
		}
		else{
			leftX = gamepad.ThumbSticks.Left.X;
			leftY = gamepad.ThumbSticks.Left.Y;
		}
		moveVector = new Vector3 (leftX, leftY, 0);
		this.rigidbody2D.MovePosition (this.transform.position + moveVector * Time.fixedDeltaTime * speed);
	}



	public void Init () {
		switch (playerNum){
		case 1: gamepadNum = PlayerIndex.One;
			Debug.Log("Player 1 Registered");
			break;
		case 2: gamepadNum = PlayerIndex.Two;
			Debug.Log("Player 2 Registered");
			break;
		case 3: gamepadNum = PlayerIndex.Three;
			Debug.Log("Player 3 Registered");
			break;
		case 4 : gamepadNum = PlayerIndex.Four;
			Debug.Log("Player 4 Registered");
			break;
		default :
			playerNum = 0;
			break;
		}
		name = ("Player " + playerNum);
		//healthBalls = new LineCircle[health];
		//dmgBalls = new MeshFilter[health];
		DisplayHealth();

		if (this.transform.position.x > 0) {
			transform.Rotate(0,0,180);
		} 
		else {
			transform.FindChild("Balls").GetComponent<Transform>().Rotate(0,0,180);
			Vector3 currPos = transform.FindChild("Balls").GetComponent<Transform>().localPosition;
			Vector3 newPos = new Vector3(currPos.x, -currPos.y, currPos.z);

			transform.FindChild("Balls").GetComponent<Transform>().localPosition = newPos;

		}
		visual = transform.FindChild ("Visual").gameObject;

		SetColor (color);
		ring = transform.FindChild ("Ring").GetComponent<LineCircle>();
		ring.SetRadius (defenseRadius);

	}

	void PlayThrowSound(){
		Camera.main.audio.PlayOneShot (throwSound);
		//AudioSource.PlayClipAtPoint(throwSound, transform.position);
//		Debug.Log("throw sound");
	}
	
	void PlayPickupSound(){
		Camera.main.audio.PlayOneShot (pickupSound);
		//AudioSource.PlayClipAtPoint(pickupSound, transform.position);
		Debug.Log("pickup sound");
	}
	
	//function to display health as tiny circles within the player
	void DisplayHealth(){
		//draw 3 circles
		/*
		healthBalls[0] = transform.FindChild("HPball0").GetComponent<LineCircle>();
		healthBalls[1] = transform.FindChild("HPball1").GetComponent<LineCircle>();
		healthBalls[2] = transform.FindChild("HPball2").GetComponent<LineCircle>();
		dmgBalls[0] = transform.FindChild("Balls").FindChild("black0").GetComponent<MeshFilter>();
		dmgBalls[1] = transform.FindChild("black1").GetComponent<MeshFilter>();
		dmgBalls[2] = transform.FindChild("black2").GetComponent<MeshFilter>();*/

		for(int i = 0; i < 3; i++){
			if (i<health)
			healthBars[i].renderer.enabled = false;
			else
				healthBars[i].renderer.enabled = true;

		}
	}


	public void DecrementHealth(){
		//makes one of the circles black
		health--;
		particleSystem.Emit(10);
		DisplayHealth ();
		if (health < 0) {
			//Throw ();
			StartCoroutine("Death");		
		}
	}

	public void RecoverHealth(){
		if (health < 3) {
						health++;
				}
		DisplayHealth ();
	}


	void OnTriggerEnter2D(Collider2D other){
		if(other.tag == "Ball"){
			Ball ball = other.GetComponent<Ball>();
			if (ball.isNeutral && heldBall == null){
				Pickup(ball);
			}
			
		}
	}

	IEnumerator ActiveDefense(){
		float maxRadius = 1.25f;
		float growSpeed = 2;
		//Debug.Log ("Activate Defense");
		defenseAvailable = false;
		defenseRadius = .55f;
		ring.SetRadius(defenseRadius);
		while (defenseRadius < maxRadius) {
			defenseRadius += Time.deltaTime * growSpeed;
			ring.SetRadius(defenseRadius);
			yield return null;
		}
		if (defenseRadius >= maxRadius) {
						defenseRadius = maxRadius;
						ring.SetRadius (defenseRadius);
						yield return new WaitForSeconds (.3f);
		}
		//Debug.Log("after");
		defenseRadius = 0;
		ring.SetRadius (defenseRadius);

		StartCoroutine ("DefenseCooldown");
	}

	IEnumerator DefenseCooldown(){
		defenseAvailable = false;
		float effectSpeed = 10;
		//float maxDefenseRadius = defenseRadius;
		//Debug.Log("DefenseCooldown");
		float t = defenseRadius;
		//defenseRadius = 0;

		while (t > .55f) {
			t -= Time.deltaTime * effectSpeed;
			ring.SetRadius(t);
			yield return null;
		}
		ring.SetRadius (.55f);
		t = .55f;
		yield return new WaitForSeconds (1);

		while (t < defenseRadius) {
			t += Time.deltaTime * effectSpeed;
			ring.SetRadius(t);
			yield return null;
		}

		ring.SetRadius (defenseRadius);
		defenseAvailable = true;
		//Debug.Log ("Defense Available");
	}

	IEnumerator SlowMotion(float scale){
		Time.timeScale = 0;
		while (Time.timeScale < 1) {
			Time.timeScale += Time.unscaledDeltaTime * scale;
			yield return null;
		}

		Time.timeScale = 1;
	}

	IEnumerator Death(){
		isDead = true;
		Throw ();
		this.enabled = false;
		//GameObject.Destroy (this.collider2D);
		//GameObject.Destroy (this.collider2D);
		foreach (CircleCollider2D col in transform.GetComponents<CircleCollider2D> ()) {
			col.enabled = false;		
		}
		//collider2D.enabled = false;
		visual.renderer.enabled = false;
		ring.gameObject.SetActive (false);
		transform.FindChild ("Balls").gameObject.SetActive (false);

		yield return new WaitForSeconds (2);

		GameObject.Destroy (this.gameObject);

	}
	
}
