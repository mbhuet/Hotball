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

public enum PlayerState
{
		WALK,
		DEAD,
		PANIC}
;

public class Player : MonoBehaviour
{
		public static bool rightStickAim;
		public static bool useTriggers;
		public static bool ballControlRightStick;
		public static bool leftStickAim;
		public static float respawnDelay = 5;
		public bool keyboardControls = false;

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
		public GameObject[] healthBars;
		public AudioClip catchSound;
		public AudioClip dashSound;
		public bool isDead = false;
		bool defenseAvailable = true;
		bool defenseActive = false;
		bool canMove = true;
		GameObject sprite;
		SpriteRenderer spriteRenderer;
		Animator spriteAnim;
		Vector3 lastSafePosition;
	public Vector3 movementVector = Vector3.zero; //this is a combination of all forces affecting position

		//controls
		protected PlayerIndex gamepadNum;
		protected GamePadState gamepad;
		Vector3 leftStick;
		Vector3 rightStick;
		Vector3 knockbackDir = Vector3.zero;
		Vector3 dashDir = Vector3.zero;
		bool fireHeld = false;
		bool slapHeld = false;
		bool dashing = false;
		PlayerState state = PlayerState.WALK;

		//for point tracking
		public int points;
		Player last_attacker;
		int knockbacks_in_effect = 0;
		public Rect scoreRect;




		//public Barrier barrier; // the barrier this player owns
		
		
		// Use this for initialization
		void Start ()
		{
				Init ();
		}

		public void Init ()
		{
				switch (playerNum) {
				case 1:
						gamepadNum = PlayerIndex.One;
						scoreRect = new Rect (0, 0, 100, 100);
						Debug.Log ("Player 1 Registered");
						break;
				case 2:
						gamepadNum = PlayerIndex.Two;
						scoreRect = new Rect (Screen.width - 100, 0, 100, 100);
						Debug.Log ("Player 2 Registered");
						break;
				case 3:
						gamepadNum = PlayerIndex.Three;
						scoreRect = new Rect (0, Screen.height - 100, 100, 100);
						Debug.Log ("Player 3 Registered");
						break;
				case 4:
						gamepadNum = PlayerIndex.Four;
						scoreRect = new Rect (Screen.width - 100, Screen.height - 100, 100, 100);
						Debug.Log ("Player 4 Registered");
						break;
				default :
						playerNum = 0;
						break;
				}
				name = ("Player " + playerNum);
				//DisplayHealth ();
				weaponManager = GetComponent<WeaponManager> ();
				visual = transform.FindChild ("Visual").gameObject;
		
		
				ring = transform.FindChild ("Ring").GetComponent<LineCircle> ();
				ring.SetRadius (.55f);
				ring.GetComponent<LineRenderer> ().enabled = false;
				respawnPosition = this.transform.position;
		
				sprite = transform.FindChild ("sprite").gameObject;
				spriteRenderer = sprite.GetComponent<SpriteRenderer> ();
				spriteAnim = sprite.GetComponent<Animator> ();
		
				SetColor (color);

				GameManager.Instance.AddPlayer (this);
				GUIManager.Instance.UpdateScore (playerNum, points);
		
		}
	
		// Update is called once per frame
		void Update ()
		{
				gamepad = GamePad.GetState (gamepadNum);
				UpdateSticks ();

				if (rightStickAim || leftStickAim) {
						Aim ();
				}

				if (keyboardControls) {
						if (Input.GetButtonDown ("Fire1")) {
								weaponManager.Fire ();
						}
						if (Input.GetButtonUp ("Fire1")) {
								weaponManager.ButtonUp ();
						}
						if (Input.GetButtonDown ("Fire2") && defenseAvailable) {
								//Catch ();
								//StartCoroutine ("DefenseCooldown");
						}
						if (Input.GetButtonDown ("Fire3")) {
								if (!dashing && leftStick.magnitude > 0 && canMove) {
										StartCoroutine (Dash ());
								}
						}
						if (Input.GetButtonDown ("Fire4")) {
								weaponManager.Slap ();
						}
				} else {
						if (((!useTriggers && gamepad.Buttons.A == ButtonState.Pressed) ||
								(useTriggers && gamepad.Triggers.Right > 0)) && !fireHeld) {
								fireHeld = true;
								weaponManager.Fire ();
						}
						if (((!useTriggers && gamepad.Buttons.A == ButtonState.Released) ||
								(useTriggers && gamepad.Triggers.Right < .01f)) && fireHeld) {
								fireHeld = false;
								weaponManager.ButtonUp ();
						}

						if (((!useTriggers && gamepad.Buttons.B == ButtonState.Pressed) ||
								(useTriggers && gamepad.Triggers.Left > 0))
								&& defenseAvailable) {
								//Catch ();
								//StartCoroutine (ActivateDefense ());

						}
						if (gamepad.Buttons.LeftShoulder == ButtonState.Pressed) {
								if (!dashing && leftStick.magnitude > 0 && canMove) {
										//Debug.Log(dashing);
										StartCoroutine (Dash ());
										//Debug.Log(dashing);
								}
						}
						if (gamepad.Buttons.RightShoulder == ButtonState.Pressed) {
								if (!slapHeld) {
										weaponManager.Slap ();
										slapHeld = true;
								}
						} else
								slapHeld = false;

						
				}

				sprite.transform.rotation = Quaternion.identity;
				if (state == PlayerState.WALK)
						CheckGround ();
		}

		void FixedUpdate ()
		{
				Move ();
		}

		void CheckGround ()
		{
				LayerMask mask = LayerMask.GetMask ("Ground", "Hazard");
				Collider2D under = Physics2D.OverlapPoint (this.transform.position, mask);
				if (under != null) {
						if (under.gameObject.GetComponent<Hazard> () != null) {
//								Debug.Log (under.name);

								Hazard haz = under.gameObject.GetComponent<Hazard> ();
								haz.OnTouch (this);
						} else if (under.gameObject.layer == LayerMask.NameToLayer ("Ground")) {
								this.transform.parent = under.transform;
								lastSafePosition = this.transform.localPosition;
								Vector3 groundScale = under.transform.localScale;
								//this.transform.localScale = new Vector3(1f/groundScale.y, 1f/groundScale.x, 1f/groundScale.z);
						}
				}

		}

		public void SetVibration (float vib)
		{
				if (!keyboardControls) {
						GamePad.SetVibration (gamepadNum, vib, vib);
				}
		}

		IEnumerator Vibrate (float intensity, float duration)
		{
				SetVibration (intensity);
				float t = 0;
				while (t<duration) {
						t += Time.deltaTime;
						yield return null;
				}
				SetVibration (0);

		}

		void SetColor (Color c)
		{
				spriteRenderer.color = c;
				visual.GetComponent<Renderer> ().material.color = c;
				GetComponent<ParticleSystem> ().startColor = c;
				GUIManager.Instance.SetColor (playerNum, c);
		}

		void UpdateSticks ()
		{
				float leftX, leftY, rightX, rightY;
				//for debugging without a controller
				if (keyboardControls) {
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
						float stick_angle = Mathf.Atan2 (rightStick.y, rightStick.x) * Mathf.Rad2Deg;

						Quaternion cur_rot = this.transform.rotation;
						Quaternion stick_rot = Quaternion.AngleAxis (stick_angle, Vector3.forward);
						Quaternion target_rot = Quaternion.Lerp (cur_rot, stick_rot, .3f);


						float sp_angle = 0;

						//sp_angle = target_rot.ToAngleAxis (out sp_angle, out up);
						sp_angle = target_rot.eulerAngles.z + 90;

						transform.rotation = target_rot;
						//sp_angle = stick_angle + 90;
						if (sp_angle < 0) {
								sp_angle = 360 + sp_angle;
						}
						if (sp_angle > 360) {
								sp_angle = sp_angle - 360;
						}
						spriteAnim.SetFloat ("rotation", sp_angle);
				}
		}

		void Catch ()
		{
				Collider2D[] objectsInRing = Physics2D.OverlapCircleAll (new Vector2 (transform.position.x, transform.position.y), defenseRadius);
				foreach (Collider2D obj in objectsInRing) {
						if (obj.gameObject.tag == "Ball") {
								Ball ball = obj.GetComponent<Ball> ();
								ball.Deflect ((ball.transform.position - this.transform.position).normalized, 1);
								Camera.main.GetComponent<AudioSource> ().PlayOneShot (catchSound);

						}
				}
		}

		void Move ()
		{
		movementVector = ((canMove ? leftStick : Vector3.zero) * Time.fixedDeltaTime * speed +
			(knockbackDir + dashDir) * Time.fixedDeltaTime);
				this.GetComponent<Rigidbody2D> ().MovePosition (this.transform.position + 
						movementVector);


				//Debug.Log (dashDir);
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

				
		
	
		//function to display health as tiny circles within the player
		void DisplayHealth ()
		{
				for (int i = 0; i < 3; i++) {
						if (i < health)
								healthBars [i].GetComponent<Renderer> ().enabled = false;
						else
								healthBars [i].GetComponent<Renderer> ().enabled = true;
				}
		}

		public void SetHealth (int h)
		{
				health = h;
				DisplayHealth ();
		}

		public void AddPoints (int p)
		{
				points += p;
				GUIManager.Instance.UpdateScore (playerNum, points);
		}

		public void RemovePoints (int p)
		{
				points -= p;
				GUIManager.Instance.UpdateScore (playerNum, points);

		}

		public void Kill ()
		{
				GetComponent<ParticleSystem> ().Emit (10);

				//makes one of the circles black
				if (GameManager.Instance.playerRespawn) {
						StartCoroutine (Respawn (respawnDelay));
				} else {
						Die ();
				}
				

				
		}

		public void DecrementHealth ()
		{
				//makes one of the circles black
				switch (GameManager.Instance.healthMode) {
				case HealthMode.STOCK:
						health--;

						if (health < 0) {
								Kill ();
						} else {
								GetComponent<ParticleSystem> ().Emit (10);
								DisplayHealth ();
						}
						break;
				case HealthMode.STUN:
//			Debug.Log("here");
						GetComponent<ParticleSystem> ().Emit (10);
						break;
				}
		}

		public void RecoverHealth ()
		{
				if (health < 3) {
						health++;
				}
				DisplayHealth ();
		}

		IEnumerator Dash ()
		{
				Camera.main.GetComponent<AudioSource> ().PlayOneShot (dashSound);

//				Debug.Log ("dash");
				dashing = true;
				//canMove = false;
				dashDir = leftStick;
				float dashSpeed = speed * 5;
				float decayRate = 1;
				while (dashSpeed>.1f) {
						
						dashSpeed = Mathf.Lerp (dashSpeed, 0, .1f);
						dashDir = dashDir.normalized * dashSpeed;
//						Debug.Log (dashDir);

						yield return null;

				}
				yield return new WaitForSeconds (1);
				dashDir = Vector3.zero;
				dashing = false;

		}

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
												Camera.main.GetComponent<AudioSource> ().PlayOneShot (catchSound);
												ignoreList.Add (ball);
												Debug.Log (ignoreList);
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
				spriteRenderer.enabled = false;
				//visual.renderer.enabled = false;
				ring.gameObject.SetActive (false);
				transform.FindChild ("Balls").gameObject.SetActive (false);
				weaponManager.Die ();
				
				this.enabled = false;

				//DROP BARRIER
				//DROP WEAPONS
				if (last_attacker != null) {
						last_attacker.AddPoints (2);
						last_attacker = null;
				}
				this.RemovePoints (1);
		
		}

		void Spawn ()
		{
				isDead = false;
				foreach (CircleCollider2D col in transform.GetComponents<CircleCollider2D> ()) {
						col.enabled = true;		
				}
				//visual.renderer.enabled = true;
				spriteRenderer.enabled = true;
				ring.gameObject.SetActive (true);
				//transform.FindChild ("Balls").gameObject.SetActive (true);
				this.transform.position = respawnPosition;
				SetHealth (GameManager.Instance.startingHealth);

				weaponManager.Show ();

		}

		IEnumerator Respawn (float delay)
		{
				Die ();

				yield return new WaitForSeconds (delay);

				Spawn ();
				GetComponent<ParticleSystem> ().startSpeed = - Mathf.Abs (GetComponent<ParticleSystem> ().startSpeed);
				GetComponent<ParticleSystem> ().Emit (10);

				yield return new WaitForSeconds (GetComponent<ParticleSystem> ().time * 2f / 3f);

				GetComponent<ParticleSystem> ().startSpeed = Mathf.Abs (GetComponent<ParticleSystem> ().startSpeed);
				this.enabled = true;
		}

		public void ApplyKnockback (Vector3 force)
		{
				StopCoroutine ("KnockbackRoutine");
				StartCoroutine ("KnockbackRoutine", force + knockbackDir);
		}

		public void ApplyKnockback (Vector3 force, Player attacker)
		{
				last_attacker = attacker;
				StopCoroutine ("KnockbackRoutine");
				StartCoroutine ("KnockbackRoutine", force + knockbackDir);
				StartCoroutine (Vibrate (1, .1f));
		}

		protected  IEnumerator KnockbackRoutine (Vector3 force)
		{
				float falloff = 6;
				knockbackDir = force;
				while (knockbackDir.magnitude > .5f) {
						knockbackDir.Scale (Vector3.one * (1 - Time.deltaTime * falloff));
						yield return null;
				}
				knockbackDir = Vector3.zero;
				last_attacker = null;
		}

		public IEnumerator Panic (Hazard hazard)
		{
		SetVibration (1);
		knockbackDir = Vector3.zero;
		dashDir = Vector3.zero;

				state = PlayerState.PANIC;
				GameObject exclamation = GameObject.Instantiate ((GameObject)Resources.Load ("exclamation"), this.transform.position + Vector3.up, Quaternion.identity) as GameObject;

				canMove = false;
				spriteAnim.SetBool ("walking", false);
				spriteAnim.SetTrigger ("panic");

				bool tripping;
				float taps = 0;

				int recoveryRequirement = 8;
				bool tapHeld = false;

				float recoveryWindow = 3;
				float t = recoveryWindow;
				while (t>0) {
						t -= Time.deltaTime;

						if ((keyboardControls && !Input.GetKey (KeyCode.A)) || (!keyboardControls && gamepad.Buttons.A == ButtonState.Released)){
				//Debug.Log("here");
								tapHeld = false;
				}
						else if (!tapHeld) {
								tapHeld = true;
								taps++;
								Debug.Log (taps);
						}

						if (taps >= recoveryRequirement) {

								break;
						}
			if (knockbackDir.magnitude > 0) break;
						yield return null;
				}

				//Did not escape from the hazard
				if (taps < recoveryRequirement) {
						hazard.Effect (this);
				} else {
						this.transform.localPosition = lastSafePosition;		
				}
		SetVibration (0);
				spriteAnim.SetBool ("walking", true);
				canMove = true;
				state = PlayerState.WALK;
				GameObject.Destroy (exclamation);

		}






	
}
