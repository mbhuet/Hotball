using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class Pitfall : Hazard {

	void Start(){
		BoxCollider2D col = this.GetComponent<BoxCollider2D> ();
		col.size = new Vector2 ((transform.localScale.x - 1.5f)/transform.localScale.x,
		                        (transform.localScale.y - 1.5f)/transform.localScale.y);
	}


	public override void Effect (Player player){
		Camera.main.GetComponent<AudioSource>().PlayOneShot (effectSound);
		player.Kill ();
	}

	public override void OnTouch (Player player){
		player.StartCoroutine(player.Panic(this));
	}


}
