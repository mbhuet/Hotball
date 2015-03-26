using UnityEngine;
using System.Collections;

public class CapturePoint : MonoBehaviour {
	SpriteRenderer sprite;
	public AudioClip captureSound;
	// Use this for initialization
	void Start () {
		sprite = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.rotation = Quaternion.identity;
	}

	void OnCollisionEnter2D(Collision2D col){
		
		if (LayerMask.NameToLayer("Ball") == col.collider.gameObject.layer) {
			Ball other = col.gameObject.GetComponent<Ball>();
			Vector3 normal = col.contacts[0].normal;
			Player other_owner = other.GetOwner();
			if (other_owner != null){
				sprite.color = other_owner.color;
				GetComponent<ParticleSystem>().startColor =  other_owner.color;
				GetComponent<ParticleSystem>().Emit(10);
				GetComponent<AudioSource>().PlayOneShot(captureSound);
			}



		}
		
		
	}
}
