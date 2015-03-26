using UnityEngine;
using System.Collections;

public abstract class Hazard : MonoBehaviour{

	public AudioClip effectSound;

	public abstract void Effect (Player player);



}
