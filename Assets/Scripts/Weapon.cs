using UnityEngine;
using System.Collections;

public enum WeaponState{IDLE, HELD, ACTIVE}

public abstract class Weapon : MonoBehaviour {
	public Player owner;
	public bool isNeutral = true;
	protected WeaponState state = WeaponState.IDLE;
	protected Vector3 homePos;//where the weapon respawns

	public abstract void ButtonDown(); //called when weapon button is pressed
	public abstract void ButtonUp(); //called when weapon button is released
	public abstract void SetOwner(Player p); //called when the weapon gets picked up

	protected void Start(){
		homePos = this.transform.position;
	}
}
