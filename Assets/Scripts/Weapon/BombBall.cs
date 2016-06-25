using UnityEngine;
using System.Collections;

using XInputDotNetPure;

public class BombBall : Ball {
	float explodeRadius = 0;
	float maxRadius = 5;
	public AudioClip explodeSound;
	public AudioClip fizzleSound;

	protected override void SetColor (Color color)
	{
		base.SetColor (color);
		ring.GetComponent<LineRenderer> ().material.color = color;
		
	}
	public override void ButtonDown ()
	{
		ring.enabled = true;
		StartCoroutine ("Expand");
		Activate (owner.transform.right * .5f);

		//StartCoroutine ("Charge");
	}
	
	public override void ButtonUp ()
	{
		//Debug.Log ("up");
		
		StopCoroutine ("Expand");
		StartCoroutine (Explode ());

	}
	
	protected override void MoveBall ()
	{
		//		Debug.Log ("move");
		
		if (owner != null) {
			float sp = moveDirection.magnitude;
			moveDirection += owner.GetBallControlVector () * maneuverability;
			moveDirection = moveDirection.normalized * sp;
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

	protected void OnCollisionEnter2D (Collision2D col)
	{
		StartCoroutine (Explode());
		
		
	}

	IEnumerator Expand(){
		float rate = 3;
		while (explodeRadius < maxRadius) {
			explodeRadius += Time.deltaTime * rate;
			ring.SetRadius(explodeRadius);
			yield return null;
		}

		yield return new WaitForSeconds (.5f);
		StartCoroutine (Fizzle ());
	}

	IEnumerator Fizzle(){
		ring.enabled = false;
		Camera.main.GetComponent<AudioSource> ().PlayOneShot (fizzleSound);
		yield return null;
		StartCoroutine (Die ());

	}
	


	IEnumerator Explode(){
		Camera.main.GetComponent<AudioSource> ().PlayOneShot (explodeSound);
		LayerMask mask = LayerMask.GetMask ("Player");
		Collider2D[] players = Physics2D.OverlapCircleAll (this.transform.position, explodeRadius/2f, mask);
		//Collider2D[] players = Physics2D.OverlapCircleAll (Vector3.zero, 100, mask);
		Debug.DrawRay (this.transform.position, Vector3.right * explodeRadius, Color.yellow, 1);
		foreach (Collider2D col in players) {
			Player p = col.GetComponent<Player>();
			if (p != null && !col.isTrigger){
				p.ApplyKnockback((p.transform.position - this.transform.position).normalized* explodeRadius * knockbackForce, this.owner);
			}
			Debug.Log(p.name);
		}
		yield return null;
		StartCoroutine (Die ());
	}
	
	public override IEnumerator Die ()
	{
		owner.weaponManager.RemoveActiveWeapon (this);
		state = WeaponState.IDLE;
		moveDirection = Vector3.zero;
		GetComponent<ParticleSystem>().Emit (150);
		Hide ();
		
		yield return new WaitForSeconds (trail.time);
		
		

			GameObject.Destroy (this.gameObject);

	}
	
	void OnDestroy(){
		base.OnDestroy ();
		
	}
}
