using UnityEngine;
using System.Collections;

public class WeaponPickup : MonoBehaviour {

	public Weapon weapon;
	public GameObject visual;

	public void Activate(){
		visual.SetActive (true);
		collider2D.enabled = true;

	}

	public void Deactivate(){
		visual.SetActive (false);
		collider2D.enabled = false;
	}

	public Weapon GetPickup(){
		return weapon;
	}

}
