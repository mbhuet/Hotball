using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(PolygonRenderer))]
public class PolygonRendererEditor : Editor {

	int n;
	float height;

	public void SceneGUI(SceneView sceneView)
	{

	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();

		PolygonRenderer target = (serializedObject.targetObject as PolygonRenderer);



		// Update Polygon if a vertex is changed
		if (target.VerticesChanged())
		{
			target.Build();
		}

		GUILayout.BeginHorizontal();
		GUILayout.Space(EditorGUIUtility.labelWidth);

		if (GUILayout.Button("Create N-Gon"))
		{
			target.CreateNGon(n,height);

		}

		GUILayout.Space(10);
		GUILayout.BeginVertical();
		GUILayout.Label("N");
		GUILayout.Label("Radius");
		GUILayout.EndVertical();
		GUILayout.BeginVertical();
		n = EditorGUILayout.IntField(n);
		height = EditorGUILayout.FloatField(height);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		if (GUILayout.Button("Rebuild"))
		{
			target.Build();
		}

		if (GUILayout.Button("Save Mesh"))
		{
			MeshFilter m = target.GetComponent<MeshFilter>();
			AssetDatabase.CreateAsset(m.mesh, "Assets/Meshes/" + m.gameObject.name + " Mesh.asset");
		}
	}
}
