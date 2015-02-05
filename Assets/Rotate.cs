using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {
	public float speed;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.Rotate (Vector3.forward * speed);
	}
}
