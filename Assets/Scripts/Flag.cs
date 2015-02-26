using UnityEngine;
using System.Collections;

public class Flag : MonoBehaviour {
	GameObject flag;
	public Team team;
	public AudioClip captureSound;
	public AudioClip grabSound;

	Vector3 homePosition;
	// Use this for initialization
	void Start () {
		flag = transform.FindChild ("flag").gameObject;
		homePosition = this.transform.position;
	}
	
	void OnTriggerEnter2D (Collider2D other)
	{

		if (other.gameObject.layer == LayerMask.NameToLayer ("Line")) {
			particleSystem.Emit(10);
			Camera.main.audio.PlayOneShot(captureSound);
			Drop ();
			this.transform.position = homePosition;
		}
	}

	void Update(){
		this.transform.rotation = Quaternion.identity;
	}

	void SetColor(Color c){
		flag.renderer.material.color = c;
		particleSystem.startColor = c;
	
	}

	public void Grab(Ball b){
		Camera.main.audio.PlayOneShot (grabSound);
		SetColor (b.owner.color);
		this.transform.parent = b.transform;
		transform.localPosition = Vector3.zero;
	}

	public void Drop(){
		transform.parent = null;
		SetColor (Color.white);
	}
}
