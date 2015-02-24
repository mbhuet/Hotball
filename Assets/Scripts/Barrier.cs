using UnityEngine;
using System.Collections;

public class Barrier : MonoBehaviour
{
		public WeaponState state = WeaponState.IDLE;
		Vector3 homePos;
		public Player owner;
		int health;
		int maxHealth = 1;
		protected LineCircle ring;//used to discretely shrink the weapon.

		GameObject idleImage;
		GameObject activeImage;
		GameObject invalidImage;
	public float respawnTime = 1;

	public AudioClip dropSound;
	public AudioClip breakSound;

		void Start ()
		{
				health = maxHealth;
				homePos = this.transform.position;
				
				idleImage = transform.FindChild ("idle_image").gameObject;
				activeImage = transform.FindChild ("active_image").gameObject;
				invalidImage = transform.FindChild ("invalid_image").gameObject;


				ring = transform.FindChild ("Ring").GetComponent<LineCircle> ();
				ring.SetThickness (.075f);

				SetNeutral ();
		}

		void OnCollisionEnter2D (Collision2D col)
		{
				if (col.gameObject.layer == LayerMask.NameToLayer ("Weapon")) {
					if (this.owner.team == col.gameObject.GetComponent<Weapon>().owner.team){
				Physics2D.IgnoreCollision(this.collider2D, col.collider, true);
			}
			else{
						health--;
						if (health <= 0)
							StartCoroutine(ReturnHome ());
								//Die ();
			}
			
				}
		}

		IEnumerator ReturnHome ()
		{
		yield return null;
				Camera.main.audio.PlayOneShot (breakSound);

				this.gameObject.layer = LayerMask.NameToLayer ("Dead");
				float effectSpeed = 2;

				SetNeutral ();
				idleImage.SetActive (false);
				
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

				SetNeutral ();

		}
	
		IEnumerator Die ()
		{
				state = WeaponState.IDLE;
				particleSystem.Emit (20);
				collider2D.enabled = false;

				yield return new WaitForSeconds (particleSystem.time);
				GameObject.Destroy (this.gameObject);

		}

		void SetNeutral ()
		{
				if (owner != null) {
						owner.barrier = null;
						owner = null;
				}

				this.gameObject.layer = LayerMask.NameToLayer ("Pickup");
		collider2D.isTrigger = true;

				transform.parent = null;
				state = WeaponState.IDLE;
				invalidImage.SetActive (false);
				idleImage.SetActive (true);
				activeImage.SetActive (false);
		}

	public void SetOwner(Player p){
		owner = p;
		SetColor(owner.color);
		transform.parent = owner.transform;
		transform.localPosition = Vector3.right * 2;
		transform.localRotation = Quaternion.identity;
		

		this.gameObject.layer = LayerMask.NameToLayer ("Wall");
		state = WeaponState.HELD;

		collider2D.enabled = true;
		collider2D.isTrigger = false;

		rigidbody2D.isKinematic = true;

		idleImage.SetActive (false);
		activeImage.SetActive (true);
	}

	public void Drop(){
		Camera.main.audio.PlayOneShot (dropSound);
		state = WeaponState.ACTIVE;
		transform.parent = null;
		collider2D.enabled = true;
	}

	void SetColor(Color c){
		activeImage.renderer.material.color = c;
		particleSystem.startColor = c;
		invalidImage.renderer.material.color = c;
	}
}
