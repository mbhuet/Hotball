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
		bool defenseActive = false;
		public GameObject[] healthBars;
		public AudioClip catchSound;
		public bool isDead = false;

	GameObject sprite;
	SpriteRenderer spriteRenderer;
	Animator spriteAnim;

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
								weaponManager.DropBarrier ();
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
								//Catch ();
								StartCoroutine (ActivateDefense ());

						}
						if (gamepad.Buttons.LeftShoulder == ButtonState.Pressed) {
								weaponManager.DropBarrier ();
						}

						if ((!useTriggers && gamepad.Buttons.A == ButtonState.Released) ||
								(useTriggers && gamepad.Triggers.Right < .01f)) {
								fireHeld = false;
						}
				}

		sprite.transform.rotation = Quaternion.identity;
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
		Vector3 stick = Vector3.zero;
				if (rightStickAim) {
			stick = rightStick;

				} else if (leftStickAim) {
			stick = leftStick;
				}

		if (stick.magnitude > 0) {
			float angle = Mathf.Atan2 (rightStick.y, rightStick.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.AngleAxis (angle, Vector3.forward);
			float sp_angle = angle + 90;
			if (sp_angle < 0){
				sp_angle = 360 + sp_angle;
			}
			spriteAnim.SetFloat ("rotation", sp_angle);

			if (sp_angle > 348.74 || sp_angle < 11.25){
				spriteAnim.SetTrigger("lookSouth");
			}

			Debug.Log(sp_angle);

			
		}
		}
		
		void Catch ()
		{
				Collider2D[] objectsInRing = Physics2D.OverlapCircleAll (new Vector2 (transform.position.x, transform.position.y), defenseRadius);
				foreach (Collider2D obj in objectsInRing) {
						if (obj.gameObject.tag == "Ball") {
								Ball ball = obj.GetComponent<Ball> ();
								ball.Deflect ((ball.transform.position - this.transform.position).normalized, 1);
								Camera.main.audio.PlayOneShot (catchSound);

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
				ring.SetRadius (.55f);
				respawnPosition = this.transform.position;
				
		sprite = transform.FindChild ("sprite").gameObject;
			spriteRenderer = sprite.GetComponent<SpriteRenderer>();
		spriteAnim = sprite.GetComponent<Animator> ();

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
								StartCoroutine (Respawn (respawnDelay));
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

		/*
		void OnTriggerStay2D (Collider2D other)
		{
				if (defenseActive) {
						if (other.gameObject.tag == "Ball") {
								Ball ball = other.GetComponent<Ball> ();
								ball.Deflect ((ball.transform.position - this.transform.position).normalized, 1);
								Camera.main.audio.PlayOneShot (catchSound);
						}
				}
		}
		*/

		IEnumerator ActivateDefense ()
		{
				float maxRadius = 1.25f;
				float growSpeed = .4f;
				float holdMax = .1f;

				List<Ball> ignoreList = new List<Ball> ();

				//Debug.Log ("Activate Defense");
				defenseAvailable = false;
				defenseActive = true;
				defenseRadius = .55f;
				ring.SetRadius (defenseRadius);

				while (defenseRadius < maxRadius-.1f) {
						defenseRadius = Mathf.Lerp (defenseRadius, maxRadius, growSpeed);
						ring.SetRadius (defenseRadius);
						

						Collider2D[] objectsInRing = Physics2D.OverlapCircleAll (new Vector2 (transform.position.x, transform.position.y), defenseRadius);
						foreach (Collider2D obj in objectsInRing) {
								if (obj.gameObject.tag == "Ball") {
										Ball ball = obj.GetComponent<Ball> ();
										if (!ignoreList.Contains (ball)) {
												ball.Redirect ((ball.transform.position - this.transform.position).normalized, 1);
												Camera.main.audio.PlayOneShot (catchSound);
												ignoreList.Add (ball);
						Debug.Log(ignoreList);
										}
								}
						}
						yield return null;
				}

				defenseRadius = maxRadius;
				ring.SetRadius (defenseRadius);

				float t = holdMax;
				while (t>0) {
						Catch ();
						t -= Time.deltaTime;
						yield return null;		
				}
						
				yield return new WaitForSeconds (.1f);
				//Debug.Log("after");
				
				defenseActive = false;
				defenseRadius = 0;
				ring.SetRadius (defenseRadius);

				StartCoroutine (DefenseCooldown ());
		}

		IEnumerator DefenseCooldown ()
		{
				defenseAvailable = false;
				float cooldownTime = .5f;
				defenseRadius = 0;
				ring.SetRadius (defenseRadius);

				yield return new WaitForSeconds (cooldownTime);
				
				defenseRadius = .55f;
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

				yield return new WaitForSeconds (particleSystem.time * 2f / 3f);

				particleSystem.startSpeed = Mathf.Abs (particleSystem.startSpeed);
				this.enabled = true;

	
		}
	
}
