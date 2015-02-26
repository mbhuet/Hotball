using UnityEngine;
using System.Collections;

public class Ball : Weapon
{
		public GameObject visual;
		protected TrailRenderer trail;
		protected float speed = 10;
		protected LineCircle ring;//used to discretely shrink the weapon.
		protected Vector3 moveDirection;
		public AudioClip player_hit;
		public AudioClip wall_hit;
		public int rem_ricochets = 1;
		protected int max_richochets;
		protected float launch_speed = .5f;
		protected float min_speed = .1f;
		protected float max_speed = 3;
		
	bool isAwake = false;
		
	void Awake(){
		Init ();
	}

		public override void ButtonDown ()
		{
				Activate (owner.transform.right * launch_speed);
		}

		public override void ButtonUp ()
		{

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
						if (other_player.team == owner.team) {
								Camera.main.audio.PlayOneShot (wall_hit);
				
						} else if (other_player.team != owner.team) {
								other_player.DecrementHealth ();
								Camera.main.audio.PlayOneShot (player_hit);
						}
						Deflect (normal);
						break;

				case "Ball":
						Ball other_ball = col.gameObject.GetComponent<Ball> ();

						
						if (other_ball.owner.team == owner.team) {
								Deflect (normal);
								Camera.main.audio.PlayOneShot (wall_hit);
			
						} else if (other_ball.owner.team != owner.team) {
								Deflect (normal);
								Camera.main.audio.PlayOneShot (wall_hit);
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
						if (moveDirection.magnitude < min_speed) {
								moveDirection = moveDirection.normalized * min_speed;
						}
				} else
						moveDirection *= (1 - Time.fixedDeltaTime * .5f);

				if (moveDirection.magnitude > max_speed) {
						moveDirection = moveDirection.normalized * max_speed;
			
				}

				Vector3 move = moveDirection * Time.fixedDeltaTime * speed;
		

				this.rigidbody2D.MovePosition (this.transform.position + move);
				

				//keeps the arrow pointing in the direction of travel
				if (moveDirection.magnitude > .01f) {
						float angle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
						transform.rotation = Quaternion.AngleAxis (angle - 90, Vector3.forward);
				} 
				
		}

		

		// Use this for initialization
		protected void Start ()
		{
				base.Start ();
				max_richochets = rem_ricochets;
				ring.SetThickness (.075f);

				if (state == WeaponState.IDLE)
						SetNeutral ();

		}

		public override void Init ()
		{
				ring = transform.FindChild ("Ring").GetComponent<LineCircle> ();
				trail = transform.FindChild ("Trail").gameObject.GetComponent<TrailRenderer> ();
		isAwake = true;
		}

		protected void FixedUpdate ()
		{
				if (owner != null && owner.isDead && !GameManager.Instance.ballsReturn) { 
						SetNeutral ();
				}

				if (state == WeaponState.ACTIVE) {
						MoveBall ();
				}
		}

		protected override void SetColor (Color color)
		{
		Debug.Log ("setcolor");
				renderer.material.color = color;
				visual.renderer.material.color = color;
				trail.renderer.material.color = color;
				particleSystem.startColor = color;

		}
	
		public override void SetOwner (Player p)
		{
//		Debug.Log (rem_ricochets);
				this.gameObject.layer = LayerMask.NameToLayer ("Weapon");

				owner = p;
				SetColor (owner.color);
				rem_ricochets = max_richochets;

				state = WeaponState.HELD;
				collider2D.isTrigger = false;

				collider2D.enabled = false;
				trail.enabled = false;
				visual.renderer.enabled = true;

				transform.parent = owner.transform;
				transform.localPosition = Vector3.right * .8f;
				transform.localRotation = Quaternion.Euler (0, 0, -90);
				rigidbody2D.isKinematic = true;
		}
	
		public void SetNeutral ()
		{
				collider2D.isTrigger = true;
				state = WeaponState.IDLE;
				owner = null;
				SetColor (Color.white);
				this.gameObject.layer = LayerMask.NameToLayer ("Pickup");
				this.rigidbody2D.isKinematic = true;
		}

		void ResetPosition ()
		{
				transform.position = Vector3.zero;
				moveDirection = Vector3.zero;
		}

		public void Deflect (Vector3 normal)
		{
				if (rem_ricochets > 0) {
						rem_ricochets --;
						Vector3 reflection = Vector3.Reflect (moveDirection, normal);
						moveDirection = reflection;
				} else {
						if (GameManager.Instance.ballRespawn)
								StartCoroutine (ReturnHome ());
						else
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

				SetNeutral ();

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
				visual.renderer.enabled = false;

				float t = trail.time;
				while (t > 0) {
						t -= Time.deltaTime;
						yield return null;
				}
				t = 0;
		
		
				yield return null;
				if (GameManager.Instance.ballsReturn) {
						owner.weaponManager.AddWeapon (this);		
				} else {
						GameObject.Destroy (this.gameObject);
				}
		}
}