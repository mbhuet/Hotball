using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour {
	public static GUIManager Instance;
	public Text[] scoresUI;

	// Use this for initialization
	void Start () {
		Instance = this;
	}
	
	// Update is called once per frame
	public void UpdateScore(int playerNum, int score){
		if (playerNum < 1)
						return;

		scoresUI [playerNum - 1].text = score.ToString ();
	
	}

	public void SetColor(int playerNum, Color c){
		if (playerNum < 1)
			return;
		scoresUI [playerNum - 1].color = c;
	}


	/*
	void OnGUI() {
		GUIStyle style = new GUIStyle ();
		Font myFont = (Font)Resources.Load("Fonts/GROBOLD", typeof(Font));
		style.font = myFont;
		style.fontSize = 80;

		foreach (Player p in GameManager.Instance.players) {
						style.normal.textColor = p.color;
						GUI.Label (p.scoreRect, p.points.ToString (), style);
				}
		
		
	}
	*/
}
