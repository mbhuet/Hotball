using UnityEngine;
using System.Collections;

public class Seeker : Ball {

	public GameObject target;
	public float seekInfluence = 1;

	protected void Start(){
		base.Start ();
		//speed = Random.Range (8, 15);
	}

	protected void FixedUpdate(){
		base.FixedUpdate ();
	}

	protected override void MoveBall(){

		moveDirection = moveDirection * (1f-seekInfluence);
			moveDirection += (target.transform.position - this.transform.position).normalized * seekInfluence;
			if (moveDirection.magnitude < .5f){
				//moveDirection = moveDirection.normalized * .5f;
			}
			

		if (moveDirection.magnitude > 3){
			moveDirection = moveDirection.normalized * 3;
			
		}

		Vector3 move = moveDirection * Time.fixedDeltaTime * speed;
		

		this.GetComponent<Rigidbody2D>().MovePosition (this.transform.position + move);
		if (moveDirection.magnitude > .01f) {
			float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.AngleAxis(angle -90, Vector3.forward);
				} 

	}




}