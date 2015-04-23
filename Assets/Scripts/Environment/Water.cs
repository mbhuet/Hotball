using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class Water : Hazard {
	public GameObject splash;

	void Start(){

	}


	public override void Effect (Player player){
		Camera.main.GetComponent<AudioSource>().PlayOneShot (effectSound);
		GameObject.Instantiate (splash, player.transform.position, Quaternion.identity);
		player.Kill ();
	}

	public override void OnTouch (Player player){
		player.StartCoroutine(player.Panic(this));
	}
	

}
