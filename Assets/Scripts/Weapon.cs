using UnityEngine;
using System.Collections;

public enum WeaponState{IDLE, HELD, ACTIVE}

public abstract class Weapon : MonoBehaviour {
	public Player owner;
	public GameObject pickupPrefab;
	protected WeaponState state = WeaponState.IDLE;
	protected Vector3 homePos;//where the weapon respawns


	public abstract void ButtonDown(); //called when weapon button is pressed
	public abstract void ButtonUp(); //called when weapon button is released
	protected abstract void SetColor(Color c);
	public abstract void Init();

	public virtual void SetOwner(Player p){
		this.owner = p;
		SetColor (owner.color);
		transform.parent = owner.transform;
		transform.localRotation = Quaternion.identity;
		transform.localPosition = Vector3.zero;

		rigidbody2D.isKinematic = true;

	}

	protected void Start(){
		homePos = this.transform.position;
	}


}
