using UnityEngine;
using System.Collections;

public class Shake : MonoBehaviour {
	public float speed = .05f;
	public float radius = .1f;

	Vector3 origin;
	Vector3 prev;
	Vector3 next;
	float t;

	// Use this for initialization
	void Start () {
		origin = this.transform.localPosition;
		Vector2 offset = Random.insideUnitCircle;
		next = origin + new Vector3(offset.x, offset.y, 0) * radius;
		prev = origin;
		t = 0;
	}
	
	// Update is called once per frame
	void Update () {
		t += speed/Vector3.Distance(prev, next);
		transform.localPosition = Vector3.Lerp(prev, next, t);

		if (t>=1){
			prev = next;
			Vector2 offset = Random.insideUnitCircle;
			next = origin + new Vector3(offset.x, offset.y, 0) * radius;
			t = 0;
		}
	}
}
