using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class Longshot : Weapon
{
		public float knockbackForce = 10;
		public float userKnockback = 10;
		ParticleSystem particleSystem;
		//float maxSpeed = 2;
		public SpriteRenderer sprite;
		public AudioClip shotSound;
		public AudioClip cockSound;
		Vector3 moveDirection;
		LineRenderer line;
		TrailRenderer trail;
		float speed = 200;

	Vector3 launchPos;
	
		protected void Awake ()
		{
				Init ();
		}
	
		public override void Init ()
		{
				particleSystem = this.GetComponent<ParticleSystem> ();

				line = transform.FindChild ("Line").GetComponent<LineRenderer> ();
				trail = transform.FindChild ("Trail").GetComponent<TrailRenderer> ();

				trail.enabled = false;
				line.enabled = false;
				moveDirection = Vector3.zero;
				//ConeCast (transform.right, 90, 5);
		}
	
		public override void Hide ()
		{
				sprite.enabled = false;
				line.enabled = false;
				GetComponent<Collider2D> ().enabled = false;
		}
	
		public override void Show ()
		{
				sprite.enabled = true;
				//collider2D.enabled = true;
		}
	
		public override IEnumerator Die ()
		{
				Debug.Log ("longshot die");
				owner.weaponManager.RemoveActiveWeapon (this);
				state = WeaponState.IDLE;
				Hide ();
		
//		yield return new WaitForSeconds (particleSystem.duration + particleSystem.startLifetime);
				yield return new WaitForSeconds (trail.time);		
		
		
				GameObject.Destroy (this.gameObject);
		
		
		}
	
		public override void ButtonDown ()
		{
				Camera.main.GetComponent<AudioSource> ().PlayOneShot (cockSound);
				line.enabled = true;

		}
	
		public override void ButtonUp ()
		{
				Activate (owner.transform.right);
		}
	
		protected override void SetColor (Color color)
		{
				//		Debug.Log ("setcolor");
				GetComponent<Renderer> ().material.color = color;
				this.GetComponent<ParticleSystem> ();
				transform.FindChild ("Line").GetComponent<LineRenderer> ().material.color = color;
				transform.FindChild ("Trail").GetComponent<TrailRenderer> ().material.color = color;

				sprite.color = color;
		
		}

		protected void Activate (Vector3 direction)
		{
		launchPos = this.transform.position;
				state = WeaponState.ACTIVE;
				trail.enabled = true;
				Camera.main.GetComponent<AudioSource> ().PlayOneShot (shotSound);
				this.transform.parent = null;
		moveDirection = owner.transform.right;

				//this.GetComponent<Collider2D> ().enabled = true;
				line.enabled = false;

				//StartCoroutine (Travel ());
		}

		void Update ()
		{
				if (state == WeaponState.ACTIVE)
						Move ();
		}

		void Move ()
		{
		if (Vector3.Distance (launchPos, this.transform.position) > 100) {
			StartCoroutine(Die ());
			return;		
		}
				LayerMask mask = LayerMask.GetMask ("Player", "Wall");
				RaycastHit2D hit = Physics2D.CircleCast (this.transform.position, 1.5f, moveDirection, speed * Time.deltaTime, mask);
				Debug.DrawRay (this.transform.position, moveDirection.normalized * speed * Time.deltaTime, Color.yellow, 1);
				
				if (hit.collider != null && hit.collider.gameObject != owner.gameObject && !hit.collider.isTrigger) {
			Debug.Log("hit");
						switch (LayerMask.LayerToName (hit.collider.gameObject.layer)) {
				
						case "Wall":
								Debug.Log (hit.collider.name);
								this.transform.position = (hit.point);

								StartCoroutine (Die ());
								break;
						case "Player":
								Player other_player = hit.collider.gameObject.GetComponent<Player> ();
								if (other_player.team == owner.team) {
										//StartCoroutine (Die ());
					
								} else if (other_player.team != owner.team) {
										this.transform.position = (hit.point);

										other_player.DecrementHealth ();
					other_player.ApplyKnockback (moveDirection.normalized * knockbackForce * Vector3.Distance(launchPos, hit.point), this.owner);
										//Camera.main.GetComponent<AudioSource>().PlayOneShot (player_hit);
										StartCoroutine (Die ());
										Debug.Log (hit.collider.name);

								}
								break;
						}		
				} else {
			Debug.Log("here");
						this.transform.position = (this.transform.position + moveDirection.normalized * speed * Time.deltaTime);
				}
		}

	/*
		IEnumerator Travel ()
		{


				float t = 0;
				speed = 3;
				while (speed < maxSpeed) {
						t += Time.deltaTime;
						//speed = Mathf.Pow(t, 3);
						Debug.Log (speed);
						yield return null;
				}
				StartCoroutine (Die ());

		}
		*/

		/*
	protected void OnCollisionEnter2D (Collision2D col)
	{
		Vector3 normal = col.contacts [0].normal;
		
		switch (LayerMask.LayerToName (col.collider.gameObject.layer)) {
			
				case "Wall":
						StartCoroutine (Die ());
						break;
				case "Player":
						Player other_player = col.gameObject.GetComponent<Player> ();
						if (other_player.team == owner.team) {
								StartCoroutine (Die ());

						} else if (other_player.team != owner.team) {
								other_player.DecrementHealth ();
								other_player.ApplyKnockback (moveDirection.normalized * knockbackForce * speed);
								//Camera.main.GetComponent<AudioSource>().PlayOneShot (player_hit);
								StartCoroutine (Die ());

						}
						break;
				}

	}
	*/
	

	
	
}
