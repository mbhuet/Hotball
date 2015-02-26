﻿using UnityEngine;
using System.Collections;

public class Barrier : Weapon
{
		int health;
		int maxHealth = 1;
		protected LineCircle ring;//used to discretely shrink the weapon.

		GameObject activeImage;
		GameObject invalidImage;
		public float respawnTime = 1;

	public AudioClip dropSound;
	public AudioClip breakSound;


	void Awake(){
		Init ();
		}

	public override void Init(){
		activeImage = transform.FindChild ("active_image").gameObject;
		invalidImage = transform.FindChild ("invalid_image").gameObject;
		ring = transform.FindChild ("Ring").GetComponent<LineCircle> ();
		invalidImage.renderer.enabled = false;
		}


	void Start ()
		{
				health = maxHealth;
				homePos = this.transform.position;
				
				
				ring.SetThickness (.075f);
		}

	public override void ButtonDown(){
		Drop ();
		}
	public override void ButtonUp(){
		}
	public override void Hide(){
		activeImage.renderer.enabled = false;
		collider2D.enabled = false;
	}
	public override void Show(){
		activeImage.renderer.enabled = true;
		collider2D.enabled = true;
	}

		void OnCollisionEnter2D (Collision2D col)
		{
				if (col.gameObject.layer == LayerMask.NameToLayer ("Weapon")) {
					if ( this.owner != null && this.owner.team == col.gameObject.GetComponent<Weapon>().owner.team){
						Physics2D.IgnoreCollision(this.collider2D, col.collider, true);
			}
			else{
						health--;
						if (health <= 0)
							StartCoroutine(Die ());
			}
			
				}
		}

		IEnumerator ReturnHome ()
		{
				yield return null;
				Camera.main.audio.PlayOneShot (breakSound);

				this.gameObject.layer = LayerMask.NameToLayer ("Dead");
				float effectSpeed = 2;

				
				health = maxHealth;
				state = WeaponState.IDLE;
				particleSystem.Emit (20);
				collider2D.enabled = false;
				activeImage.SetActive (false);
		
				//wait for particles to die
				yield return new WaitForSeconds (particleSystem.time);
		
				yield return new WaitForSeconds (respawnTime);
		
				this.transform.position = homePos;
				transform.rotation = Quaternion.identity;
				float t = 0;
				while (t < 1) {
						t += Time.deltaTime * effectSpeed;	
						this.transform.localScale = Vector3.one * t;
						yield return null;
				}
				this.transform.localScale = Vector3.one * 1;
				ring.SetRadius (0);
				collider2D.enabled = true;


		}
	
		public override IEnumerator Die ()
		{
			yield return null;
			if (owner != null)	owner.weaponManager.RemoveActiveBarrier (this);

				Camera.main.audio.PlayOneShot (breakSound);
				
				Hide ();
				state = WeaponState.IDLE;
				particleSystem.Emit (20);


				yield return new WaitForSeconds (particleSystem.startLifetime);
				GameObject.Destroy (this.gameObject);

		}



	public void SetOwner(Player p){
		base.SetOwner (p);

		transform.localPosition = Vector3.right * 2;

		this.gameObject.layer = LayerMask.NameToLayer ("Wall");
		state = WeaponState.HELD;

		collider2D.enabled = true;
		collider2D.isTrigger = false;

		activeImage.SetActive (true);
	}

	public void Drop(){
		Camera.main.audio.PlayOneShot (dropSound);
		state = WeaponState.ACTIVE;
		transform.parent = null;
		collider2D.enabled = true;
	}

	protected override void SetColor(Color c){
//		Debug.Log (activeImage);
		activeImage.renderer.material.color = c;
		particleSystem.startColor = c;
		invalidImage.renderer.material.color = c;
	}
}
