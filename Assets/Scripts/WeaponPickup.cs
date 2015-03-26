using UnityEngine;
using System.Collections;

public class WeaponPickup : MonoBehaviour {

	public Weapon weapon;
	public float delay = 1;
	public GameObject visual;

	public void Activate(){
		StartCoroutine (DelayedActivation());

	}

	public void Deactivate(){
		visual.SetActive (false);
		GetComponent<Collider2D>().enabled = false;
	}

	public Weapon GetPickup(){
		return weapon;
	}

	public void WeaponDestroyCallback(){
		Activate ();
	}

	IEnumerator DelayedActivation(){
		yield return new WaitForSeconds(delay);
		visual.SetActive (true);
		GetComponent<Collider2D>().enabled = true;
	}

}
