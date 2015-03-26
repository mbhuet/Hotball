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
			GetComponent<ParticleSystem>().Emit(10);
			Camera.main.GetComponent<AudioSource>().PlayOneShot(captureSound);
			Drop ();
			this.transform.position = homePosition;
		}
	}

	void Update(){
		this.transform.rotation = Quaternion.identity;
	}

	void SetColor(Color c){
		flag.GetComponent<Renderer>().material.color = c;
		GetComponent<ParticleSystem>().startColor = c;
	
	}

	public void Grab(Ball b){
		Camera.main.GetComponent<AudioSource>().PlayOneShot (grabSound);
		SetColor (b.GetOwner().color);
		this.transform.parent = b.transform;
		transform.localPosition = Vector3.zero;
	}

	public void Drop(){
		transform.parent = null;
		SetColor (Color.white);
	}
}
