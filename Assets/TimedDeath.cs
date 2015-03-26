using UnityEngine;
using System.Collections;

public class TimedDeath : MonoBehaviour {
	public float lifetime = 1;
	// Use this for initialization
	void Start () {
		StartCoroutine (Countdown());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	IEnumerator Countdown(){
		float t = lifetime;
		while (t>0) {
			t -= Time.deltaTime;
			yield return null;
				
		}
		GameObject.Destroy (this.gameObject);

	}
}
