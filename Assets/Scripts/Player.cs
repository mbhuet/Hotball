using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using XInputDotNetPure;

public enum Team
{
		ONE,
		TWO,
		THREE,
		FOUR,
		FIVE,
		SIX,
		SEVEN,
		EIGHT}
;

public class Player : MonoBehaviour
{
		public int playerNum;
		public bool rightStickAim;
		public bool useTriggers;
		//public bool ballControlLeftStick;
		public bool ballControlRightStick;
		public bool leftStickAim;
		GameObject visual;
		public Team team;
		public int health = 3;
		public float speed = 6;
		public float ballInfluence = .05f;
		List<Weapon> heldWeapons;
		//bool isThrowing;
		//float scaleFactor;
		//bool againstWall;
		//public Vector3 moveVector;
		//public Vector3 ballPullVector;
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
		Vector3 leftStick;
		Vector3 rightStick;
//		public Barrier barrierPrefab;
		public Barrier barrier; // the barrier this player owns

		bool fireHeld = false;
		// Use this for initialization
		void Start ()
		{
				Init ();

				if (GameManager.Instance.ballsReturn) {
						GameObject myBall = GameObject.Instantiate (GameManager.Instance.ballPrefab, this.transform.position, Quaternion.identity) as GameObject;

						//PickupWeapon(myBall.GetComponent<Ball>());

				}

		}
	
		// Update is called once per frame
		void Update ()
		{
				gamepad = GamePad.GetState (gamepadNum);
				UpdateSticks ();

				if (rightStickAim || leftStickAim) {
						Aim ();
				}

				if (playerNum == 0) {
						if (Input.GetButtonDown ("Fire1")) {
								Throw ();
						}
						if (Input.GetButtonDown ("Fire2") && defenseAvailable) {
								Catch ();
								StartCoroutine ("DefenseCooldown");


						}
						if (Input.GetButtonUp ("Fire3")) {
								DropBarrier ();
						}
				} else {
						if (((!useTriggers && gamepad.Buttons.A == ButtonState.Pressed) ||
								(useTriggers && gamepad.Triggers.Right > 0)) && !fireHeld) {
								fireHeld = true;
								Throw ();
						}
						if (((!useTriggers && gamepad.Buttons.B == ButtonState.Pressed) ||
								(useTriggers && gamepad.Triggers.Left > 0))
								&& defenseAvailable) {
								Catch ();
								StartCoroutine ("DefenseCooldown");

						}
						if (gamepad.Buttons.LeftShoulder == ButtonState.Pressed) {
								DropBarrier ();
						}

						if ((!useTriggers && gamepad.Buttons.A == ButtonState.Released) ||
								(useTriggers && gamepad.Triggers.Right < .01f)) {
								fireHeld = false;
						}
				}
		}

		void FixedUpdate ()
		{
				Move ();
		}

		void SetColor (Color c)
		{
				//renderer.material.color = color;
				visual.renderer.material.color = c;
				particleSystem.startColor = c;
		}

		void SetTeamLayer(){
		string layerName = "Default";
		switch (team) {
		case Team.ONE:
			layerName = "TeamOne";
			break;
		case Team.TWO:
			layerName = "TeamTwo";
			break;
		case Team.THREE:
			layerName = "TeamThree";
			break;
		case Team.FOUR:
			layerName = "TeamFour";
			break;
		case Team.FIVE:
			layerName = "TeamFive";
				break;
		case Team.SIX:
			layerName = "TeamSix";
			break;
		case Team.SEVEN:
			layerName = "TeamSeven";
			break;
		case Team.EIGHT:
			layerName = "TeamEight";
			break;
		default:
			break;
		}
		gameObject.layer = LayerMask.NameToLayer (layerName);

	}

		void UpdateSticks ()
		{

				float leftX, leftY, rightX, rightY;
				//for debugging without a controller
				if (playerNum == 0) {
						leftX = Input.GetAxis ("Horizontal");
						leftY = Input.GetAxis ("Vertical");
						rightX = Input.GetAxis ("Horizontal");
						rightY = Input.GetAxis ("Vertical");
				} else {
						leftX = gamepad.ThumbSticks.Left.X;
						leftY = gamepad.ThumbSticks.Left.Y;
						rightX = gamepad.ThumbSticks.Right.X;
						rightY = gamepad.ThumbSticks.Right.Y;
				}
				leftStick = new Vector3 (leftX, leftY, 0);
				rightStick = new Vector3 (rightX, rightY, 0);
		}

		void Throw ()
		{
				if (heldWeapons.Count > 0) {
						heldWeapons [0].ButtonDown ();
						heldWeapons.RemoveAt (0);
						PlayThrowSound ();
						ArrangeWeapons ();
				}
		}

		void Block (Ball ball)
		{

		}

		void Catch ()
		{
				Collider2D[] objectsInRing = Physics2D.OverlapCircleAll (new Vector2 (transform.position.x, transform.position.y), defenseRadius + .1f);
				foreach (Collider2D obj in objectsInRing) {
						if (obj.gameObject.tag == "Ball") {
								//StartCoroutine("SlowMotion", .1f);
								Ball ball = obj.GetComponent<Ball> ();
								//catching an opponent's ball
								if (!ball.isNeutral && ball.owner.team != this.team) {
										Camera.main.audio.PlayOneShot (catchSound);
										ball.particleSystem.Emit (10);
										if (heldWeapons.Count == 0) {
												PickupWeapon (ball);
												//RecoverHealth ();
										} else {
												//ball.Deflect(ball.transform.position - this.transform.position);
												ball.SetNeutral ();

										}
								}
								/*
				if (ball.isNeutral){
				}
				else{
					if (heldWeapon == null){
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

		void Aim ()
		{
				if (rightStickAim && rightStick.magnitude > 0) {
						float angle = Mathf.Atan2 (rightStick.y, rightStick.x) * Mathf.Rad2Deg;
						transform.rotation = Quaternion.AngleAxis (angle, Vector3.forward);
						
				} else if (leftStickAim && leftStick.magnitude > 0) {
						float angle = Mathf.Atan2 (leftStick.y, leftStick.x) * Mathf.Rad2Deg;
						transform.rotation = Quaternion.AngleAxis (angle, Vector3.forward);
			
				}
		}

		public void PickupWeapon (Weapon w)
		{
				if (GameManager.Instance.holdMultipleWeapons || heldWeapons.Count == 0) {
						PlayPickupSound ();
						heldWeapons.Add (w);
						w.SetOwner (this);
						ArrangeWeapons ();
				}
		}

		public void PickupBarrier (Barrier bar)
		{
				if (barrier == null) {
						PlayPickupSound ();
						barrier = bar;
						bar.SetOwner (this);
				}

		}

		void ArrangeWeapons ()
		{
				int dir = 1;
				for (int i = 0; i< heldWeapons.Count; i++) {
						Weapon w = heldWeapons [i];
						w.transform.localPosition = Vector3.right * .8f;
						transform.localRotation = Quaternion.Euler (0, 0, 270);
						if (i == 0) {
				
						} else {
								float a = 40 * dir * Mathf.Floor ((i + 1) / 2f);
								//w.transform.localRotation = Quaternion.AngleAxis(Vector3.forward, a);
								w.transform.RotateAround (this.transform.position, Vector3.forward, a);
//				Debug.Log(i + ", " + 40 * dir * Mathf.Floor((i+1)/2f));
						} 
						dir = -dir;
				}
		}

		void Move ()
		{
				this.rigidbody2D.MovePosition (this.transform.position + leftStick * Time.fixedDeltaTime * speed);
		}

		public Vector3 GetBallControlVector ()
		{
				Vector3 ballPullVector = Vector3.zero;
				if (ballControlRightStick) {
						ballPullVector = rightStick;
				} else
						ballPullVector = leftStick;

				return ballPullVector * ballInfluence;
		}

		public void Init ()
		{
				heldWeapons = new List<Weapon> ();
				switch (playerNum) {
				case 1:
						gamepadNum = PlayerIndex.One;
						Debug.Log ("Player 1 Registered");
						break;
				case 2:
						gamepadNum = PlayerIndex.Two;
						Debug.Log ("Player 2 Registered");
						break;
				case 3:
						gamepadNum = PlayerIndex.Three;
						Debug.Log ("Player 3 Registered");
						break;
				case 4:
						gamepadNum = PlayerIndex.Four;
						Debug.Log ("Player 4 Registered");
						break;
				default :
						playerNum = 0;
						break;
				}
				name = ("Player " + playerNum);
				//healthBalls = new LineCircle[health];
				//dmgBalls = new MeshFilter[health];
				DisplayHealth ();
				/* I'm not sure why this block exists
				if (rightStickAim) {
					
				}
		//players on the right side of the screen are rotated to point left.
		else if (this.transform.position.x > 0) {
						transform.Rotate (0, 0, 180);
				} else {
						transform.FindChild ("Balls").GetComponent<Transform> ().Rotate (0, 0, 180);
						Vector3 currPos = transform.FindChild ("Balls").GetComponent<Transform> ().localPosition;
						Vector3 newPos = new Vector3 (currPos.x, -currPos.y, currPos.z);

						transform.FindChild ("Balls").GetComponent<Transform> ().localPosition = newPos;

				}
				*/
				visual = transform.FindChild ("Visual").gameObject;

				
				ring = transform.FindChild ("Ring").GetComponent<LineCircle> ();
				ring.SetRadius (defenseRadius);

				SetColor (color);

				GameManager.Instance.AddPlayer (this);

		}

		void PlayThrowSound ()
		{
				Camera.main.audio.PlayOneShot (throwSound);
		}
	
		void PlayPickupSound ()
		{
				Camera.main.audio.PlayOneShot (pickupSound);
		}

		void DropBarrier ()
		{
				if (barrier != null) {
						barrier.Drop ();
						barrier = null;
				}
		}

		

	
		//function to display health as tiny circles within the player
		void DisplayHealth ()
		{

				for (int i = 0; i < 3; i++) {
						if (i < health)
								healthBars [i].renderer.enabled = false;
						else
								healthBars [i].renderer.enabled = true;

				}
		}

		public void SetHealth (int h)
		{
				health = h;
				DisplayHealth ();
		}

		public void DecrementHealth ()
		{
				//makes one of the circles black
				health--;
				particleSystem.Emit (10);
				DisplayHealth ();
				if (health < 0) {
						switch (GameManager.Instance.healthMode) {
						case HealthMode.STOCK:
								StartCoroutine ("Death");
								break;
						case HealthMode.STUN:
								StartCoroutine ("Stun");
								break;
						}
					
				}
		}

		public void RecoverHealth ()
		{
				if (health < 3) {
						health++;
				}
				DisplayHealth ();
		}

		void OnTriggerEnter2D (Collider2D other)
		{
				if (other.tag == "Ball") {
						Ball ball = other.GetComponent<Ball> ();
						if ((ball.isNeutral)) {
								PickupWeapon (ball);
						}
			
				}
				if (other.tag == "Barrier") {
						Barrier bar = other.GetComponent<Barrier> ();
						if ((bar.state == WeaponState.IDLE)) {
								PickupBarrier (bar);
						}
			

				}
		}

		IEnumerator ActiveDefense ()
		{
				float maxRadius = 1.25f;
				float growSpeed = 2;
				//Debug.Log ("Activate Defense");
				defenseAvailable = false;
				defenseRadius = .55f;
				ring.SetRadius (defenseRadius);
				while (defenseRadius < maxRadius) {
						defenseRadius += Time.deltaTime * growSpeed;
						ring.SetRadius (defenseRadius);
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

		IEnumerator DefenseCooldown ()
		{
				defenseAvailable = false;
				float effectSpeed = 10;
				//float maxDefenseRadius = defenseRadius;
				//Debug.Log("DefenseCooldown");
				float t = defenseRadius;
				//defenseRadius = 0;

				while (t > .55f) {
						t -= Time.deltaTime * effectSpeed;
						ring.SetRadius (t);
						yield return null;
				}
				ring.SetRadius (.55f);
				t = .55f;
				yield return new WaitForSeconds (1);

				while (t < defenseRadius) {
						t += Time.deltaTime * effectSpeed;
						ring.SetRadius (t);
						yield return null;
				}

				ring.SetRadius (defenseRadius);
				defenseAvailable = true;
				//Debug.Log ("Defense Available");
		}

		IEnumerator SlowMotion (float scale)
		{
				Time.timeScale = 0;
				while (Time.timeScale < 1) {
						Time.timeScale += Time.unscaledDeltaTime * scale;
						yield return null;
				}

				Time.timeScale = 1;
		}

		IEnumerator Death ()
		{
				isDead = true;

				if (!GameManager.Instance.ballsReturn)
						Throw ();

				DropBarrier ();
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

		IEnumerator Stun ()
		{
				isDead = true;
				if (!GameManager.Instance.ballsReturn)
						Throw ();

				DropBarrier ();

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
		
				yield return new WaitForSeconds (3);
		
				this.transform.position = Vector3.zero;
				particleSystem.startSpeed = - Mathf.Abs (particleSystem.startSpeed);
				particleSystem.Emit (10);

				yield return new WaitForSeconds (.5f);



				isDead = false;
				this.enabled = true;
				foreach (CircleCollider2D col in transform.GetComponents<CircleCollider2D> ()) {
						col.enabled = true;		
				}
				visual.renderer.enabled = true;
				ring.gameObject.SetActive (true);
				transform.FindChild ("Balls").gameObject.SetActive (true);

				SetHealth (GameManager.Instance.startingHealth);

				particleSystem.startSpeed = Mathf.Abs (particleSystem.startSpeed);

	
		}
	
}
