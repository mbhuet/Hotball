using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class MovingPlatform : MonoBehaviour {
	public Vector3[] positions;
	public float speed = 1;
	public float pauseTime = 1;
	int posIndex = 0;
	// Use this for initialization
	void Start () {
		this.transform.position = positions [posIndex];
		posIndex++;
		if (posIndex > positions.Length - 1)
			posIndex = 0;

		StartCoroutine (MoveToPosition(positions[posIndex]));
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	IEnumerator MoveToPosition(Vector3 endPos){
		Vector3 startPos = this.transform.position;
		float t = 0;
		float dist = Vector3.Distance (startPos, endPos);

		while (t<1) {
			t+= Time.deltaTime/dist * speed;
			this.transform.position = Vector3.Lerp(startPos, endPos, t);
			yield return null;
		}

		this.transform.position = endPos;

		yield return new WaitForSeconds (pauseTime);

		posIndex++;
		if (posIndex > positions.Length - 1)
						posIndex = 0;

		StartCoroutine (MoveToPosition (positions [posIndex]));
	}
}
