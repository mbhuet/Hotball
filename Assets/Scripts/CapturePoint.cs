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
			if (other.owner != null){
				sprite.color = other.owner.color;
				particleSystem.startColor =  other.owner.color;
				particleSystem.Emit(10);
				audio.PlayOneShot(captureSound);
			}



		}
		
		
	}
}
