using UnityEngine;
using System.Collections;

/// <summary>
/// A class the provides alternate t-values for different Lerp transitions. More will be added later
/// </summary>
public class LerpUtility {
	
	public static float Spherical(float time){
		return (1-(Mathf.Cos(time * Mathf.PI)))/2f;
	}

	public static float AntiSpherical(float time){
		float temp = Mathf.Sin(time * Mathf.PI)/2f;
		return Mathf.Abs(time) > .5f ? Mathf.Sign(time) - temp : temp;
	}

	public static float HemiSpherical(float time){
		return Mathf.Sin(time * Mathf.PI / 2f);
	}
	
	public static float AntiHemiSpherical(float time){
		return Mathf.Sign(time) * (1 - Mathf.Sin(time * Mathf.PI / 2f + Mathf.PI /2f));
	}

	public static float Quadratic(float time){
		return time*time;	
	}

	public static float UnLerp(Vector3 start, Vector3 end, Vector3 position){
		Vector3 dir = end - start;
		return Mathf.Clamp01(Vector3.Dot(dir.normalized, (position - start)/dir.magnitude));
	}
}