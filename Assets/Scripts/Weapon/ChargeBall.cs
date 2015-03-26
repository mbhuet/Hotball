using UnityEngine;
using System.Collections;

using XInputDotNetPure;

public class ChargeBall : Ball {
	public float max_charge = 3;
	float min_charge = 1;
	float charge;

	public override void ButtonDown ()
	{
		//Debug.Log ("down");
		StartCoroutine ("Charge");
	}
	
	public override void ButtonUp ()
	{
		//Debug.Log ("up");

		owner.SetVibration (0);
		StopCoroutine ("Charge");
		Activate (owner.transform.right * charge);
		
	}

	protected override void MoveBall ()
	{
//		Debug.Log ("move");
		
		if (owner != null) {
			float sp = moveDirection.magnitude;
			moveDirection += owner.GetBallControlVector ();
			moveDirection = moveDirection.normalized * sp;
			if (moveDirection.magnitude < min_speed) {
				moveDirection = moveDirection.normalized * min_speed;
			}
		} else
			moveDirection *= (1 - Time.fixedDeltaTime * .5f);
		
		if (moveDirection.magnitude > max_speed) {
			moveDirection = moveDirection.normalized * max_speed;
			
		}
		
		Vector3 move = moveDirection * Time.fixedDeltaTime * speed;
		
		
		this.GetComponent<Rigidbody2D>().MovePosition (this.transform.position + move);
		
		
		//keeps the arrow pointing in the direction of travel
		if (moveDirection.magnitude > .01f) {
			float angle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.AngleAxis (angle - 90, Vector3.forward);
		} 
		
	}

	IEnumerator Charge(){
		charge = min_charge;
		while (charge < max_charge) {
			charge += Time.deltaTime;
			owner.SetVibration(Mathf.Min(charge/max_charge,1));
//			Debug.Log((charge/max_charge));
			yield return null;
		}
	}

	public override IEnumerator Die ()
	{
		if (flag != null) {
			flag.Drop ();
			flag = null;
		}

		StopCoroutine ("Charge");
		owner.SetVibration (0);

		owner.weaponManager.RemoveActiveWeapon (this);
		state = WeaponState.IDLE;
		moveDirection = Vector3.zero;
		GetComponent<ParticleSystem>().Emit (20);
		Hide ();
		
		yield return new WaitForSeconds (trail.time);
		
		
		if (GameManager.Instance.ballsReturn) {
			yield return new WaitForSeconds (returnDelay);
			owner.weaponManager.AddWeapon (this);		
		} else {
			GameObject.Destroy (this.gameObject);
		}
	}

	void OnDestroy(){
		base.OnDestroy ();

	}
}
