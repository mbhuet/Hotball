using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
		//should handle all pickup collision
		//should keep track of all held weapons and owned 
		//should communicate with barrier manager to set up collision ignores
		Player player;
	public AudioClip pickupSound;
	public AudioClip fireSound;

		public Weapon[] startingWeapons;
		List<Weapon> heldWeaponList;
		List<Weapon> activeWeaponsList;
		static int maxWeapons = 3;
		List<Barrier> placedBarrierList;
		List<Barrier> heldBarrierList;
		static int maxBarriers = 1;
		bool hidden = false;

		void Start ()
		{
				player = this.GetComponent<Player> ();
				heldWeaponList = new List<Weapon> ();
				activeWeaponsList = new List<Weapon> ();
				placedBarrierList = new List<Barrier> ();
				heldBarrierList = new List<Barrier> ();

				LoadStartingWeapons ();
		}

		public void Fire ()
		{
				if (heldWeaponList.Count > 0) {
			Camera.main.audio.PlayOneShot(fireSound);

						Weapon currentWeapon = heldWeaponList [0];
						heldWeaponList.RemoveAt (0);
						activeWeaponsList.Add (currentWeapon);
						currentWeapon.ButtonDown ();
						ArrangeWeapons ();
						
				}

		}

		public void DropBarrier ()
		{
				if (heldBarrierList.Count > 0) {
						Barrier bar = heldBarrierList [0];
						heldBarrierList.RemoveAt (0);
						placedBarrierList.Add (bar);
						bar.ButtonDown ();
				}
		}

		void LoadStartingWeapons ()
		{
				foreach (Weapon w in startingWeapons) {
						Weapon startWeapon = GameObject.Instantiate (w, this.transform.position, Quaternion.identity) as Weapon;
						startWeapon.Init ();
						AddWeapon (startWeapon);
				}
		}

		public void AddWeapon (Weapon w)
		{
//				Debug.Log ("add weapon " + player.name);
				if (heldWeaponList.Count < maxWeapons) {
			Camera.main.audio.PlayOneShot(pickupSound);
						w.SetOwner (player);
						heldWeaponList.Add (w);
						ArrangeWeapons ();
						IgnoreWeaponBarrierCollision (w);
						if (hidden)
								w.Hide ();
				}

		}

		public void RemoveActiveWeapon (Weapon w)
		{
				activeWeaponsList.Remove (w);
		}

		public void RemoveActiveBarrier (Barrier bar)
		{
				placedBarrierList.Remove (bar);
		heldBarrierList.Remove (bar);
		}

		public void AddBarrier (Barrier b)
		{
				if (heldBarrierList.Count < maxBarriers) {
			Camera.main.audio.PlayOneShot(pickupSound);

						b.SetOwner (player);
						heldBarrierList.Add (b);
						IgnoreWeaponBarrierCollision (b);
				}

		}

		void IgnoreWeaponBarrierCollision (Barrier b)
		{
				foreach (Weapon w in heldWeaponList) {
						Physics2D.IgnoreCollision (w.collider2D, b.collider2D);
				}
				foreach (Weapon w in activeWeaponsList) {
						Physics2D.IgnoreCollision (w.collider2D, b.collider2D);
				}
		Physics2D.IgnoreCollision (b.collider2D, player.collider2D);
		}

		void IgnoreWeaponBarrierCollision (Weapon w)
		{
				foreach (Barrier b in heldBarrierList) {
						Physics2D.IgnoreCollision (w.collider2D, b.collider2D);
				}
				foreach (Barrier b in placedBarrierList) {
						Physics2D.IgnoreCollision (w.collider2D, b.collider2D);
				}
				foreach (Weapon w2 in activeWeaponsList) {
						Physics2D.IgnoreCollision (w.collider2D, w2.collider2D);

				}
				foreach (Weapon w2 in heldWeaponList) {
						Physics2D.IgnoreCollision (w.collider2D, w2.collider2D);
			
				}
		}

		public void Die ()
		{
				hidden = true;
				foreach (Weapon w in heldWeaponList) {
						w.Hide ();		
				}
				while (activeWeaponsList.Count > 0) {
						Debug.Log (activeWeaponsList.Count);
						Weapon toDestroy = activeWeaponsList [0];
						activeWeaponsList.Remove(toDestroy);
						StartCoroutine (toDestroy.Die ());

				}
				DropBarrier ();
		}

		public void Show ()
		{
				hidden = false;
				foreach (Weapon w in heldWeaponList) {
						w.Show ();		
				}
		}

		void ArrangeWeapons ()
		{
				int dir = 1;
				for (int i = 0; i< heldWeaponList.Count; i++) {
						Weapon w = heldWeaponList [i];
						w.transform.localPosition = Vector3.right * .8f;
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
						WeaponPickup pickup = other.GetComponent<WeaponPickup> ();
						Weapon newWeaponPrefab = pickup.GetPickup ();
						if (newWeaponPrefab.tag == "Barrier") {
								if (heldBarrierList.Count < maxBarriers) {

										Weapon newWeapon = GameObject.Instantiate (pickup.GetPickup (), this.transform.position, this.transform.rotation) as Weapon;
										AddBarrier (newWeapon.GetComponent<Barrier> ());
					newWeapon.SetPickup(pickup);
										pickup.Deactivate ();
								}
						} else {
								if (heldWeaponList.Count < maxWeapons) {
										Weapon newWeapon = GameObject.Instantiate (pickup.GetPickup (), this.transform.position, this.transform.rotation) as Weapon;

										AddWeapon (newWeapon);
					newWeapon.SetPickup(pickup);
										pickup.Deactivate ();
								}
						}


				}
		}
}
