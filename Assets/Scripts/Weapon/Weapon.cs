using UnityEngine;
using System.Collections;

public enum WeaponState{IDLE, HELD, ACTIVE}

public abstract class Weapon : MonoBehaviour {
	protected Player owner;
	protected WeaponPickup pickup;
	protected WeaponState state = WeaponState.IDLE;
	protected Vector3 homePos;//where the weapon respawns


	public abstract void ButtonDown(); //called when weapon button is pressed
	public abstract void ButtonUp(); //called when weapon button is released
	protected abstract void SetColor(Color c);
	public abstract void Init();
	public abstract void Hide ();
	public abstract void Show();

	public virtual void SetOwner(Player p){
		this.owner = p;
		SetColor (owner.color);
		transform.parent = owner.transform;
		transform.localRotation = Quaternion.identity;
		transform.localPosition = Vector3.zero;

		GetComponent<Rigidbody2D>().isKinematic = true;
		GetComponent<Collider2D>().enabled = false;

	}

	protected void Start(){
		homePos = this.transform.position;
	}
	public abstract IEnumerator Die();

	protected void OnDestroy(){
//		Debug.Log ("here");
		if (pickup != null) {
						pickup.WeaponDestroyCallback ();
				}
	}
	public void SetPickup(WeaponPickup pick){
		pickup = pick;
	}

	public Player GetOwner(){
		return owner;
	}


}
