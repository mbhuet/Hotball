using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class WeaponManager : MonoBehaviour {
	//should handle all pickup collision
	//should keep track of all held weapons and owned 
	//should communicate with barrier manager to set up collision ignores
	Player player;
	public Weapon[] startingWeapons;
	
	List<Weapon> heldWeaponList;
	List<Weapon> activeWeaponsList;
	static int maxWeapons = 3;

	List<Barrier> placedBarrierList;
	List<Barrier> heldBarrierList;
	static int maxBarriers = 1;


	void Start(){
		player = this.GetComponent<Player> ();
		heldWeaponList = new List<Weapon>();
		activeWeaponsList = new List<Weapon> ();
		placedBarrierList = new List<Barrier> ();
		heldBarrierList = new List<Barrier> ();

		LoadStartingWeapons ();
	}

	public void Fire(){
		if (heldWeaponList.Count > 0) {
						Weapon currentWeapon = heldWeaponList [0];
						heldWeaponList.RemoveAt (0);
						activeWeaponsList.Add (currentWeapon);
						currentWeapon.ButtonDown ();
						
				}

	}

	public void DropBarrier(){
		if (heldBarrierList.Count > 0) {
			Barrier bar = heldBarrierList[0];
			heldBarrierList.RemoveAt(0);
			placedBarrierList.Add(bar);
			bar.ButtonDown();
		}
	}

	void LoadStartingWeapons(){
		foreach (Weapon w in startingWeapons) {
			Weapon startWeapon = GameObject.Instantiate(w,this.transform.position, Quaternion.identity) as Weapon;
			startWeapon.Init();
			AddWeapon(startWeapon);
		}
	}

	public void AddWeapon(Weapon w){
		if (heldWeaponList.Count < maxWeapons) {
			w.SetOwner(player);
						heldWeaponList.Add (w);
						ArrangeWeapons ();
			IgnoreWeaponBarrierCollision(w);
		}
	}

	public void AddBarrier(Barrier b){
		if (heldBarrierList.Count < maxBarriers) {
			b.SetOwner(player);
						heldBarrierList.Add (b);
			IgnoreWeaponBarrierCollision(b);
		}

	}

	void IgnoreWeaponBarrierCollision(Barrier b){
		foreach (Weapon w in heldWeaponList) {
				Physics2D.IgnoreCollision(w.collider2D, b.collider2D);
				}
		foreach (Weapon w in activeWeaponsList) {
			Physics2D.IgnoreCollision(w.collider2D, b.collider2D);
		}
	}

	void IgnoreWeaponBarrierCollision(Weapon w){
		foreach (Barrier b in heldBarrierList) {
			Physics2D.IgnoreCollision(w.collider2D, b.collider2D);
		}
		foreach (Barrier b in placedBarrierList) {
			Physics2D.IgnoreCollision(w.collider2D, b.collider2D);
		}
	}

	void ArrangeWeapons ()
	{
		int dir = 1;
		for (int i = 0; i< heldWeaponList.Count; i++) {
			Weapon w = heldWeaponList [i];
			w.transform.localPosition = Vector3.right * .8f;
			transform.localRotation = Quaternion.Euler (0, 0, 270);
			if (i == 0) {
				
			} else {
				float a = 40 * dir * Mathf.Floor ((i + 1) / 2f);
				//w.transform.localRotation = Quaternion.AngleAxis(Vector3.forward, a);
				w.transform.RotateAround (this.transform.position, Vector3.forward, a);
				//				Debug.Log(i + ", " + 40 * dir * Mathf.Floor((i+1)/2f));
			} 
			dir = -dir;
		}
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		if (other.transform.tag == "Pickup") {
			WeaponPickup pickup = other.GetComponent<WeaponPickup>();
			Weapon newWeaponPrefab = pickup.GetPickup();
			if (newWeaponPrefab.tag == "Barrier"){
				if (heldBarrierList.Count < maxBarriers) {

				Weapon newWeapon = GameObject.Instantiate(pickup.GetPickup(), this.transform.position, this.transform.rotation) as Weapon;
				AddBarrier(newWeapon.GetComponent<Barrier>());
					pickup.Deactivate();
				}
			}
			else{
				if (heldWeaponList.Count < maxWeapons) {
					Weapon newWeapon = GameObject.Instantiate(pickup.GetPickup(), this.transform.position, this.transform.rotation) as Weapon;

				AddWeapon(newWeapon);
					pickup.Deactivate();
				}
			}


		}
	}
}
