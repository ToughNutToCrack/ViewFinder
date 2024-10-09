using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

public class MeshInfo : EditorWindow
{

	private int vertexCount;
	private int submeshCount;
	private int triangleCount;

	[MenuItem("Tools/Mesh Info")]
	static void Init()
	{
		// Get existing open window or if none, make a new one:
		MeshInfo window = (MeshInfo)EditorWindow.GetWindow(typeof(MeshInfo));
		window.titleContent.text = "Mesh Info";
	}

	void OnSelectionChange()
	{
		Repaint();
	}


	void do_all(GameObject g)
    {
		MeshFilter m = g.GetComponent<MeshFilter>();
		if (m != null)
		{
			vertexCount += m.sharedMesh.vertexCount;
			triangleCount += m.sharedMesh.triangles.Length / 3;
			submeshCount += m.sharedMesh.subMeshCount;
		}
		foreach (Transform child in g.transform)
		{
			do_all(child.gameObject);
		}
	}


    void OnGUI()
	{
		vertexCount=0;
		submeshCount=0;
		triangleCount=0;

		if (!Selection.activeGameObject) return;
		GameObject g = Selection.activeGameObject;
		do_all(g);

		EditorGUILayout.LabelField(Selection.activeGameObject.name);
		EditorGUILayout.LabelField("Vertices: ", vertexCount.ToString());
		EditorGUILayout.LabelField("Triangles: ", triangleCount.ToString());
		EditorGUILayout.LabelField("SubMeshes: ", submeshCount.ToString());
	}

}
