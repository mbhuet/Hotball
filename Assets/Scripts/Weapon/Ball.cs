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

		public float maneuverability = .05f;

		public int rem_ricochets = 1;
		public float knockbackForce = 10;
		public float min_speed = .1f;
		public float returnDelay = 1;

		protected int max_richochets;
		protected float launch_speed = .5f;
		protected float max_speed = 3;
		protected Flag flag;
		protected bool isAwake = false;
		
		protected void Awake ()
		{
				Init ();
		}

		public override void ButtonDown ()
		{
				Activate (owner.transform.right * launch_speed);
		}

		public override void ButtonUp ()
		{

		}

		protected void OnCollisionEnter2D (Collision2D col)
		{
				Vector3 normal = col.contacts [0].normal;

				switch (LayerMask.LayerToName (col.collider.gameObject.layer)) {

				case "Wall":
						if (col.transform.tag == "Neutralize") {
								SetNeutral ();
						}
						Deflect (normal);
						Camera.main.GetComponent<AudioSource>().PlayOneShot (wall_hit);
						break;

				case "Player":
						Player other_player = col.gameObject.GetComponent<Player> ();
						if (other_player.team == owner.team) {
								Camera.main.GetComponent<AudioSource>().PlayOneShot (wall_hit);
				
						} else if (other_player.team != owner.team) {
								other_player.DecrementHealth ();
								other_player.ApplyKnockback (moveDirection.magnitude * -normal * knockbackForce, this.owner);
								Camera.main.GetComponent<AudioSource>().PlayOneShot (player_hit);
						}
						Deflect (normal);
						break;

				case "Ball":
						Ball other_ball = col.gameObject.GetComponent<Ball> ();

						
						if (other_ball.owner.team == owner.team) {
								Deflect (normal);
								Camera.main.GetComponent<AudioSource>().PlayOneShot (wall_hit);
			
						} else if (other_ball.owner.team != owner.team) {
								Deflect (normal);
								Camera.main.GetComponent<AudioSource>().PlayOneShot (wall_hit);
						}
						
						break;

				default :
						Deflect (normal);
						break;
				}


		}

		protected void OnTriggerEnter2D (Collider2D other)
		{
				if (other.transform.tag == "Flag") {
//						Debug.Log ("here");
						Flag f = other.gameObject.GetComponent<Flag> ();
						if (owner.team != f.team) {
								flag = f;
								f.Grab (this);
						}
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
		

				this.GetComponent<Rigidbody2D>().MovePosition (this.transform.position + move);
				

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
				ring.SetThickness (.075f);

				if (state == WeaponState.IDLE)
						SetNeutral ();

		}

		public override void Init ()
		{
				ring = transform.FindChild ("Ring").GetComponent<LineCircle> ();
				trail = transform.FindChild ("Trail").gameObject.GetComponent<TrailRenderer> ();
				max_richochets = rem_ricochets;

				isAwake = true;
		}

		protected void FixedUpdate ()
		{
//		Debug.Log ("fixed");
				if (owner != null && owner.isDead && !GameManager.Instance.ballsReturn) { 
						SetNeutral ();
				}

				if (state == WeaponState.ACTIVE) {
						MoveBall ();
				}
		}

		protected override void SetColor (Color color)
		{
//		Debug.Log ("setcolor");
				GetComponent<Renderer>().material.color = color;
				visual.GetComponent<Renderer>().material.color = color;
				trail.GetComponent<Renderer>().material.color = color;
				GetComponent<ParticleSystem>().startColor = color;

		}
	
		public override void SetOwner (Player p)
		{
				base.SetOwner (p);
				this.gameObject.layer = LayerMask.NameToLayer ("Weapon");

				owner = p;
				SetColor (owner.color);
				rem_ricochets = max_richochets;

				state = WeaponState.HELD;
				trail.enabled = false;
				visual.GetComponent<Renderer>().enabled = true;

				transform.parent = owner.transform;
				transform.localPosition = Vector3.right * .8f;
				transform.localRotation = Quaternion.Euler (0, 0, -90);
		}
	
		public void SetNeutral ()
		{
				GetComponent<Collider2D>().isTrigger = true;
				state = WeaponState.IDLE;
				owner = null;
				SetColor (Color.white);
				this.gameObject.layer = LayerMask.NameToLayer ("Pickup");
				this.GetComponent<Rigidbody2D>().isKinematic = true;
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

		public void Redirect (Vector3 direction, float force)
		{
				if (rem_ricochets > 0) {
						rem_ricochets --;
						moveDirection = direction.normalized * moveDirection.magnitude + direction.normalized * force;

				} else {
						if (GameManager.Instance.ballRespawn)
								StartCoroutine (ReturnHome ());
						else
								StartCoroutine (Die ());
				}
		}

		public void Deflect (Vector3 normal, float force)
		{
				Deflect (normal);
				moveDirection = moveDirection + moveDirection.normalized * force;
		}

		public void Activate (Vector3 direction)
		{
				state = WeaponState.ACTIVE;
				GetComponent<Collider2D>().enabled = true;
				trail.enabled = true;
				transform.parent = null;
				transform.localRotation = Quaternion.Euler (0, 0, -90);
				GetComponent<Rigidbody2D>().isKinematic = false;
				moveDirection = direction;
		}

		public override void Hide ()
		{
				visual.GetComponent<Renderer>().enabled = false;
				GetComponent<Collider2D>().enabled = false;
		}

		public override void Show ()
		{
				visual.GetComponent<Renderer>().enabled = true;
				//collider2D.enabled = true;
		}

		protected IEnumerator ReturnHome ()
		{

				SetNeutral ();

				float effectSpeed = 2;
				GetComponent<Collider2D>().enabled = false;
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
				GetComponent<Collider2D>().enabled = true;
				trail.enabled = true;
				trail.time = .3f;
		}

		public override IEnumerator Die ()
		{
				if (flag != null) {
						flag.Drop ();
						flag = null;
				}
				owner.weaponManager.RemoveActiveWeapon (this);
				state = WeaponState.IDLE;
				moveDirection = Vector3.zero;
				GetComponent<ParticleSystem>().Emit (20);
				Hide ();

				yield return new WaitForSeconds (trail.time);
		
		
				if (GameManager.Instance.ballsReturn) {
						yield return new WaitForSeconds (returnDelay);
						owner.weaponManager.AddWeapon (this);		
				} else {
						GameObject.Destroy (this.gameObject);
				}
		}
}