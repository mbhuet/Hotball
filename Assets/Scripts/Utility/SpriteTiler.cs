using UnityEngine;
using System.Collections;

public class SpriteTiler : MonoBehaviour {
	public GameObject tilePrefab;
	public int rows;
	public int cols;
	float size;
	// Use this for initialization
	void Start () {
		size = tilePrefab.transform.localScale.x;
		ArrangeTiles ();
	}
	
	void ArrangeTiles(){
		for (int x = 0; x<cols; x++) {
			for(int y = 0; y<rows; y++){
				GameObject obj = GameObject.Instantiate(tilePrefab, 
				                                        this.transform.position - new Vector3(cols/2f, rows/2f, 0) + 
											                                      new Vector3(size/2f, size/2f, 0) + 
											                                      new Vector3(x,y,0), 
				                                        Quaternion.identity) as GameObject;

				obj.transform.parent = this.transform;

			}
		}
	}
}
