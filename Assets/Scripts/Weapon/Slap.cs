using UnityEngine;
using System.Collections;

public class Slap : Weapon {

	public float knockbackForce = 10;
	SpriteRenderer sprite;
	Animator animator;
	public AudioClip slapSound;
	public AudioClip whooshSound;
	
	
	protected void Awake ()
	{
		Init ();
	}
	
	public override void Init(){
		sprite = this.GetComponent<SpriteRenderer> ();
		animator = this.GetComponent<Animator> ();
		transform.localPosition = new Vector3(.25f, .1f, -1);

	}

	void Start(){

	}
	
	public override void Hide ()
	{
		sprite.enabled = false;
	}
	
	public override void Show ()
	{
		sprite.enabled = true;
	}
	
	public override IEnumerator Die(){
		yield return null;
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
		sprite.color = color;
		
	}
	
	protected void Activate(Vector3 direction){
		bool connected = false;
		Camera.main.GetComponent<AudioSource> ().PlayOneShot (whooshSound);
		animator.SetTrigger ("activate");

		LayerMask mask = LayerMask.GetMask ("Player");
		Collider2D[] players = Physics2D.OverlapCircleAll (this.transform.position, .5f, mask);
		Debug.DrawRay (this.transform.position, owner.transform.right * 10, Color.yellow);
		foreach (Collider2D col in players) {
			Player p = col.GetComponent<Player>();
			if (p != null && !col.isTrigger && p!= this.owner){
				connected = true;
				p.ApplyKnockback((p.transform.position - this.transform.position).normalized * knockbackForce, this.owner);
			}
		}
		if (connected) {

						Camera.main.GetComponent<AudioSource> ().PlayOneShot (slapSound);
				}
	}

	public override void SetOwner(Player p){
		this.owner = p;
		SetColor (owner.color);
		
	}
	

}
