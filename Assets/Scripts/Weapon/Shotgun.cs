using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//[RequireComponent(typeof(ParticleSystem))]
public class Shotgun : Weapon {
	public float knockbackForce = 10;
	public float userKnockback = 10;
	ParticleSystem particleSystem;

	public float shotAngle = 30;
	public float shotDistance = 4;
	public SpriteRenderer sprite;
	public AudioClip shotSound;


	protected void Awake ()
	{
		Init ();
	}

	public override void Init(){
		particleSystem = transform.FindChild("particles").GetComponent<ParticleSystem> ();
		//ConeCast (transform.right, 90, 5);
	}

	public override void Hide ()
	{
		sprite.enabled = false;
		GetComponent<Collider2D>().enabled = false;
	}
	
	public override void Show ()
	{
		sprite.enabled = true;
		//collider2D.enabled = true;
	}

	public override IEnumerator Die(){
		owner.weaponManager.RemoveActiveWeapon (this);
		state = WeaponState.IDLE;
		Hide ();
		
		yield return new WaitForSeconds (particleSystem.duration + particleSystem.startLifetime);
		
		

			GameObject.Destroy (this.gameObject);

		
	}
	
	public override void ButtonDown ()
	{
		Activate (owner.transform.right);
	}
	
	public override void ButtonUp ()
	{
		
	}

	protected override void SetColor (Color color)
	{
		//		Debug.Log ("setcolor");
		GetComponent<Renderer>().material.color = color;
		transform.FindChild("particles").GetComponent<ParticleSystem> ();
		sprite.color = color;

	}

	protected void Activate(Vector3 direction){
		Camera.main.GetComponent<AudioSource> ().PlayOneShot (shotSound);
		List<Collider2D> hitPlayers = ConeCast (owner.transform.right, shotAngle, shotDistance);
		foreach(Collider2D col in hitPlayers){
			Player player = col.GetComponent<Player>();
			if (player!= null){
				player.ApplyKnockback((player.transform.position - this.transform.position).normalized * knockbackForce);
			}
		}
		owner.ApplyKnockback (-owner.transform.right * knockbackForce);
		particleSystem.Play ();
		StartCoroutine (Die ());
	}

	List<Collider2D> ConeCast(Vector3 direction, float angle, float dist){
		List<Collider2D> colliders = new List<Collider2D> ();
		int numCasts = 10;
		LayerMask mask = LayerMask.GetMask ("Player");
		for (int i = 0; i<numCasts; i++) {
			Vector3 rotatedDir = Quaternion.AngleAxis(-angle/2f + (angle/(numCasts-1)) * i, Vector3.forward) * direction;
			Debug.DrawRay(this.transform.position, rotatedDir * dist, Color.yellow, 1);
			RaycastHit2D[] hits = Physics2D.RaycastAll(this.transform.position, rotatedDir, dist, mask);
			foreach(RaycastHit2D hit in hits){
				if (!colliders.Contains(hit.collider) && hit.collider.gameObject != this.gameObject){
					colliders.Add(hit.collider);
				}
			}
		}
		return colliders;
	}


}
