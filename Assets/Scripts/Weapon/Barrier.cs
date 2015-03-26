using UnityEngine;
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
		invalidImage.GetComponent<Renderer>().enabled = false;
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
		activeImage.GetComponent<Renderer>().enabled = false;
		GetComponent<Collider2D>().enabled = false;
	}
	public override void Show(){
		activeImage.GetComponent<Renderer>().enabled = true;
		GetComponent<Collider2D>().enabled = true;
	}

		void OnCollisionEnter2D (Collision2D col)
		{
				if (col.gameObject.layer == LayerMask.NameToLayer ("Weapon")) {
					if ( this.owner != null && this.owner.team == col.gameObject.GetComponent<Weapon>().GetOwner().team){
						Physics2D.IgnoreCollision(this.GetComponent<Collider2D>(), col.collider, true);
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
				Camera.main.GetComponent<AudioSource>().PlayOneShot (breakSound);

				this.gameObject.layer = LayerMask.NameToLayer ("Dead");
				float effectSpeed = 2;

				
				health = maxHealth;
				state = WeaponState.IDLE;
				GetComponent<ParticleSystem>().Emit (20);
				GetComponent<Collider2D>().enabled = false;
				activeImage.SetActive (false);
		
				//wait for particles to die
				yield return new WaitForSeconds (GetComponent<ParticleSystem>().time);
		
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
				GetComponent<Collider2D>().enabled = true;


		}
	
		public override IEnumerator Die ()
		{
			yield return null;
			if (owner != null)	owner.weaponManager.RemoveActiveBarrier (this);

				Camera.main.GetComponent<AudioSource>().PlayOneShot (breakSound);
				
				Hide ();
				state = WeaponState.IDLE;
				GetComponent<ParticleSystem>().Emit (20);


				yield return new WaitForSeconds (GetComponent<ParticleSystem>().startLifetime);
				GameObject.Destroy (this.gameObject);

		}



	public void SetOwner(Player p){
		base.SetOwner (p);

		transform.localPosition = Vector3.right * 2;

		this.gameObject.layer = LayerMask.NameToLayer ("Wall");
		state = WeaponState.HELD;

		GetComponent<Collider2D>().enabled = true;
		GetComponent<Collider2D>().isTrigger = false;

		activeImage.SetActive (true);
	}

	public void Drop(){
		Camera.main.GetComponent<AudioSource>().PlayOneShot (dropSound);
		state = WeaponState.ACTIVE;
		transform.parent = null;
		GetComponent<Collider2D>().enabled = true;
	}

	protected override void SetColor(Color c){
//		Debug.Log (activeImage);
		activeImage.GetComponent<Renderer>().material.color = c;
		GetComponent<ParticleSystem>().startColor = c;
		invalidImage.GetComponent<Renderer>().material.color = c;
	}
}
