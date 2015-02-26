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
		public static bool rightStickAim;
		public static bool useTriggers;
		public static bool ballControlRightStick;
		public static bool leftStickAim;
	public static float respawnDelay = 5;

		//settings
		public int playerNum;
		public Team team;
		public Color color;
		public int health = 3;
		public float speed = 6;
		public float ballInfluence = .05f;
		//List<Weapon> heldWeapons;
		

		GameObject visual;
		public WeaponManager weaponManager;
		private LineCircle[] healthBalls;
		private MeshFilter[] dmgBalls;
		float defenseRadius = 1.4f;
		Vector3 respawnPosition;
		protected LineCircle ring;
		bool defenseAvailable = true;
		public GameObject[] healthBars;
		public AudioClip catchSound;
		public bool isDead = false;

		//controls
		protected PlayerIndex gamepadNum;
		protected GamePadState gamepad;
		Vector3 leftStick;
		Vector3 rightStick;
		bool fireHeld = false;

		//public Barrier barrier; // the barrier this player owns

		
		// Use this for initialization
		void Start ()
		{
				Init ();
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
								weaponManager.Fire ();
						}
						if (Input.GetButtonDown ("Fire2") && defenseAvailable) {
								Catch ();
								StartCoroutine ("DefenseCooldown");


						}
						if (Input.GetButtonUp ("Fire3")) {
								weaponManager.DropBarrier();
						}
				} else {
						if (((!useTriggers && gamepad.Buttons.A == ButtonState.Pressed) ||
								(useTriggers && gamepad.Triggers.Right > 0)) && !fireHeld) {
								fireHeld = true;
								weaponManager.Fire ();
						}
						if (((!useTriggers && gamepad.Buttons.B == ButtonState.Pressed) ||
								(useTriggers && gamepad.Triggers.Left > 0))
								&& defenseAvailable) {
								Catch ();
								StartCoroutine ("DefenseCooldown");

						}
						if (gamepad.Buttons.LeftShoulder == ButtonState.Pressed) {
								weaponManager.DropBarrier ();
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
				visual.renderer.material.color = c;
				particleSystem.startColor = c;
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
		
		void Catch ()
		{
				Collider2D[] objectsInRing = Physics2D.OverlapCircleAll (new Vector2 (transform.position.x, transform.position.y), defenseRadius + .1f);
				foreach (Collider2D obj in objectsInRing) {
						if (obj.gameObject.tag == "Ball") {
								Ball ball = obj.GetComponent<Ball> ();
								ball.Deflect ((ball.transform.position - this.transform.position).normalized);
								Camera.main.audio.PlayOneShot (catchSound);

								/*
								//StartCoroutine("SlowMotion", .1f);

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
								*/
						}
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
				DisplayHealth ();
				weaponManager = GetComponent<WeaponManager> ();
				visual = transform.FindChild ("Visual").gameObject;

				
				ring = transform.FindChild ("Ring").GetComponent<LineCircle> ();
				ring.SetRadius (defenseRadius);
				respawnPosition = this.transform.position;


				SetColor (color);

				GameManager.Instance.AddPlayer (this);

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
								StartCoroutine (Respawn(respawnDelay));
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
		}

		void Die ()
		{
				isDead = true;
				foreach (CircleCollider2D col in transform.GetComponents<CircleCollider2D> ()) {
						col.enabled = false;	
				}

				visual.renderer.enabled = false;
				ring.gameObject.SetActive (false);
				transform.FindChild ("Balls").gameObject.SetActive (false);
		weaponManager.Die ();

				//DROP BARRIER
				//DROP WEAPONS

		}

		void Spawn ()
		{
				isDead = false;
				foreach (CircleCollider2D col in transform.GetComponents<CircleCollider2D> ()) {
						col.enabled = true;		
				}
				visual.renderer.enabled = true;
				ring.gameObject.SetActive (true);
				transform.FindChild ("Balls").gameObject.SetActive (true);
				this.transform.position = respawnPosition;
				SetHealth (GameManager.Instance.startingHealth);

		weaponManager.Show ();

		}

		IEnumerator Respawn (float delay)
		{
				Die ();
				this.enabled = false;
		
				yield return new WaitForSeconds (delay);

				Spawn ();
				particleSystem.startSpeed = - Mathf.Abs (particleSystem.startSpeed);
				particleSystem.Emit (10);

				yield return new WaitForSeconds (particleSystem.time * 2f/3f);

				particleSystem.startSpeed = Mathf.Abs (particleSystem.startSpeed);
				this.enabled = true;

	
		}
	
}
