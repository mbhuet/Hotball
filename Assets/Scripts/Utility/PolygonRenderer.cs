using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class PolygonRenderer : MonoBehaviour {

	public Vector2[] Vertices;
	private Vector2[] prevVertices;
	public float Thickness;
	public bool Filled;

	Vector2[] vertexAxes;
	float winding = 1;
	bool built = false;

	void Update()
	{
		Build();
	}

	public bool VerticesChanged()
	{
		bool changed = false;
		if (built)
		{
			for (int i = 0; i < Vertices.Length; i++)
			{
				if (prevVertices[i] != Vertices[i])
				{
					changed = true;
					break;
				}
			}
		}

		return changed;
	}

	public void MoveVertex(int index, Vector2 position)
	{
		Vertices[index] = position;
	}

	public void Instance(){
		MeshFilter m = GetComponent<MeshFilter>();
		Mesh oldMesh = m.mesh;
		Mesh newMesh = new Mesh();
		newMesh.name = oldMesh.name;
		newMesh.vertices = oldMesh.vertices;
		newMesh.triangles = oldMesh.triangles;
		newMesh.normals = oldMesh.normals;
		newMesh.uv = oldMesh.uv;
		m.mesh = newMesh;
	}

	public void Build(){
		if (Vertices != null && Vertices.Length > 0){
			CalculateAxes();
			StartCoroutine(CreateMesh());
			prevVertices = (Vertices.Clone() as Vector2[]);
			built = true;
		}
	}

	IEnumerator CreateMesh(){
		Mesh m = GetComponent<MeshFilter>().sharedMesh;

		bool newMesh = false;
		if (m == null){
			m = new Mesh();
			m.name = "Polygon";
			newMesh = true;
		}

		int amount = 6 * Vertices.Length;
		if (Filled){
			amount += 3 * Vertices.Length;
		}

		int[] triangles = newMesh ? new int[amount] : m.triangles;
		if (amount != triangles.Length){
			triangles = new int[amount];
		}

		amount = 2 * Vertices.Length;

		if (Filled){
			amount++;
		}

		Vector3[] vertices = newMesh ? new Vector3[amount] : m.vertices;
		if (vertices.Length != amount){
			vertices = new Vector3[amount];
		}

		if (Filled){
			amount--;
		}

		for (int i = 0; i < Vertices.Length; i++){
			vertices[2 * i] = Vertices[i] + vertexAxes[i] * Thickness;
			vertices[2 * i + 1] = Vertices[i] - vertexAxes[i] * Thickness;

			triangles[6 * i] = 2 * i;
			triangles[6 * i + 1] = 2 * i + 1;
			triangles[6 * i + 2] = (2 * i + 2) % amount;
			
			triangles[6 * i + 3] = (2 * i + 2) % amount;
			triangles[6 * i + 4] = (2 * i + 1) % amount;
			triangles[6 * i + 5] = (2 * i + 3) % amount;

			if (Filled){
				triangles[triangles.Length - 3 * Vertices.Length + (3 * i + 0)] = vertices.Length - 1;
				triangles[triangles.Length - 3 * Vertices.Length + (3 * i + 1)] = 2 * i;
				triangles[triangles.Length - 3 * Vertices.Length + (3 * i + 2)] = 2 * ((i + 1)%Vertices.Length);
			}
		}

		if (Filled){
			Vector2 center = Vector2.zero;
			for (int i = 0; i < Vertices.Length; i++){
				center += Vertices[i];
			}
			vertices[vertices.Length - 1] = center / Vertices.Length;
		}

		m.Clear();
		
		if (Application.isPlaying){
			m.vertices = vertices;
			m.triangles = triangles;
			
			yield return 0;
		}
		m.vertices = vertices;
		m.triangles = triangles;
		m.uv = new Vector2[vertices.Length];
		m.normals = new Vector3[vertices.Length];

		GetComponent<MeshFilter>().mesh = m;
		yield return 0;
	}

	/*public Vector2 toLocalSpace(Vector2 worldPosition)
	{

	}*/

	void CalculateAxes(){
		if (vertexAxes == null || vertexAxes.Length != Vertices.Length){
			vertexAxes = new Vector2[Vertices.Length];
		}
		
		float sum = 0;
		for (int i = 0; i < Vertices.Length; i++){
			int nextIndex = i == Vertices.Length - 1 ? 0 : i + 1;
			sum += (Vertices[nextIndex].x - Vertices[i].x) * (Vertices[nextIndex].y + Vertices[i].y);
		}
		
		winding = sum >= 0 ? 1 : -1;

		for (int i = 0; i < Vertices.Length; i++){
			int prevIndex = i == 0 ? Vertices.Length - 1 : i - 1;
			int nextIndex = i == Vertices.Length - 1 ? 0 : i + 1;
			
			Vector2 bisector = ((Vertices[prevIndex] - Vertices[i]).normalized + (Vertices[nextIndex] - Vertices[i]).normalized).normalized;
			
			vertexAxes[i] = winding * bisector * 1f / Mathf.Sin(Mathf.Deg2Rad * Vector2.Angle(Vertices[prevIndex] - Vertices[i], bisector));
		}
	}

	public void CreateNGon(int n, float height){
		Vector2[] verts = new Vector2[n];
		
		for (int i = 0; i < verts.Length; i++){
			verts[i] = new Vector2(Mathf.Sin(i * Mathf.PI * 2f / n), Mathf.Cos(i * Mathf.PI * 2f / n)) * height;
		}
		
		//PolygonRenderer poly = target;
		Vertices = verts;
		Build();
	}
}
