using UnityEngine;
using System.Collections;

public class Ball : Weapon
{
	protected GameObject arrow;
	protected TrailRenderer trail;
	protected float speed = 10;
	protected LineCircle ring;//used to discretely shrink the weapon.
	protected Vector3 moveDirection;



	public AudioClip player_hit;
	public AudioClip wall_hit;


		public int rem_ricochets = 1;
	protected int max_richochets;


	public override void ButtonDown(){
		Activate (owner.transform.right);
		}

	public override void ButtonUp(){
		}

		void OnCollisionEnter2D (Collision2D col)
		{
				Vector3 normal = col.contacts [0].normal;

				switch (LayerMask.LayerToName (col.collider.gameObject.layer)) {

				case "Wall":
						if (col.transform.tag == "Neutralize") {
								SetNeutral ();
						}
						Deflect (normal);
						Camera.main.audio.PlayOneShot (wall_hit);
						break;

				case "Player":
						Player other_player = col.gameObject.GetComponent<Player> ();
						if (isNeutral) {
								Camera.main.audio.PlayOneShot (wall_hit);
				
						} else if (!isNeutral && owner == null) {
								other_player.DecrementHealth ();
								Camera.main.audio.PlayOneShot (player_hit);

								GameObject.Destroy (this.gameObject);
						} else if (other_player.team == owner.team) {
								Camera.main.audio.PlayOneShot (wall_hit);
				
						} else if (other_player.team != owner.team) {
								other_player.DecrementHealth ();
								//SetNeutral ();
								Camera.main.audio.PlayOneShot (player_hit);
						}
						Deflect (normal);

						break;

				case "Ball":
						Ball other_ball = col.gameObject.GetComponent<Ball> ();

						if (other_ball.isNeutral) {
								Deflect (normal);
								Camera.main.audio.PlayOneShot (wall_hit);
				
						} else if (!this.isNeutral) {
								if (other_ball.owner.team == owner.team) {
										Deflect (normal);
										Camera.main.audio.PlayOneShot (wall_hit);
					
								} else if (other_ball.owner.team != owner.team) {
										Deflect (normal);
										Camera.main.audio.PlayOneShot (wall_hit);
								}
						}
						break;

				default :
						Deflect (normal);
						break;
				}
		}

		protected virtual void MoveBall ()
		{

				if (owner != null) {
						moveDirection += owner.GetBallControlVector ();
						if (moveDirection.magnitude < .1f) {
								moveDirection = moveDirection.normalized * .1f;
						}
				} else
						moveDirection *= (1 - Time.fixedDeltaTime * .5f);

				if (moveDirection.magnitude > 3) {
						moveDirection = moveDirection.normalized * 3;
			
				}

				Vector3 move = moveDirection * Time.fixedDeltaTime * speed;
		

				this.rigidbody2D.MovePosition (this.transform.position + move);
				

				//keeps the arrow pointing in the direction of travel
				if (moveDirection.magnitude > .01f) {
						float angle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
						transform.rotation = Quaternion.AngleAxis (angle - 90, Vector3.forward);
				} 
				//respawns the ball at home position if it has become idle
				if (moveDirection.magnitude < .1f && isNeutral && state == WeaponState.ACTIVE) {
						StartCoroutine ("ReturnHome");
				}
		}

		

		// Use this for initialization
		protected void Start ()
		{
		base.Start();
		max_richochets = rem_ricochets;
				ring = transform.FindChild ("Ring").GetComponent<LineCircle> ();
				ring.SetThickness (.075f);
				arrow = transform.FindChild ("Arrow").gameObject;
				trail = transform.FindChild ("Trail").gameObject.GetComponent<TrailRenderer> ();
				SetNeutral ();

		}

		protected void FixedUpdate ()
		{
				if (owner != null && owner.isDead) { 
						SetNeutral ();
				}

				if (state == WeaponState.ACTIVE) {
						MoveBall ();
				}
		}

		protected void SetColor (Color color)
		{
				renderer.material.color = color;
				arrow.renderer.material.color = color;
				trail.renderer.material.color = color;
				particleSystem.startColor = color;
		}
	
		public override void SetOwner (Player p)
		{
		Debug.Log ("set owner");
				this.gameObject.layer = LayerMask.NameToLayer ("Ball");
				owner = p;
				SetColor (owner.color);
		isNeutral = false;
		rem_ricochets = max_richochets;

				state = WeaponState.HELD;
				collider2D.enabled = false;
				trail.enabled = false;
				transform.parent = owner.transform;
				transform.localPosition = Vector3.right * .8f;
				transform.localRotation = Quaternion.Euler (0, 0, -90);
				rigidbody2D.isKinematic = true;
		}
	
		public void SetNeutral ()
		{
				state = WeaponState.IDLE;
				isNeutral = true;
				owner = null;
				SetColor (Color.white);
				this.gameObject.layer = LayerMask.NameToLayer ("DeadBall");
		}

		void ResetPosition ()
		{
				transform.position = Vector3.zero;
				moveDirection = Vector3.zero;
		}

		protected void Deflect (Vector3 normal)
		{
				if (rem_ricochets > 0) {
						rem_ricochets --;
						Vector3 reflection = Vector3.Reflect (moveDirection, normal);
						moveDirection = reflection;
				} else {
						StartCoroutine (Die ());
				}

		}

		public void Activate (Vector3 direction)
		{
				state = WeaponState.ACTIVE;
				collider2D.enabled = true;
				trail.enabled = true;
				transform.parent = null;
				transform.localRotation = Quaternion.Euler (0, 0, -90);
				rigidbody2D.isKinematic = false;
				moveDirection = direction;
		}

		IEnumerator ReturnHome ()
		{

				state = WeaponState.IDLE;

				float effectSpeed = 2;
				collider2D.enabled = false;
				ring.SetRadius (1);

				trail.time = -1;
				trail.enabled = false;

				float t = this.transform.localScale.x;
				while (t > 0) {
						t -= Time.deltaTime * effectSpeed;
						this.transform.localScale = Vector3.one * t;
						trail.startWidth = t;
						yield return null;
				}
				t = 0;


				yield return null;

				this.transform.position = homePos;
				moveDirection = Vector3.zero;
				transform.rotation = Quaternion.identity;
				while (t < .5f) {
						t += Time.deltaTime * effectSpeed;	
						this.transform.localScale = Vector3.one * t;
						yield return null;
				}
				this.transform.localScale = Vector3.one * .5f;
				trail.startWidth = .5f;
//		Debug.Log ("here");
				ring.SetRadius (0);
				collider2D.enabled = true;
				trail.enabled = true;
				trail.time = .3f;
		}

		IEnumerator Die ()
		{
				state = WeaponState.IDLE;
				moveDirection = Vector3.zero;
				particleSystem.Emit (20);
				collider2D.enabled = false;
				arrow.renderer.enabled = false;

				float t = trail.time;
				while (t > 0) {
						t -= Time.deltaTime;
						yield return null;
				}
				t = 0;
		
		
				yield return null;
		
				GameObject.Destroy (this.gameObject);
		}
}