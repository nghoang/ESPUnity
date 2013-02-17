using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Sample : MonoBehaviour
{
	Dictionary<string, GameObject> models = new Dictionary<string, GameObject> ();
	string path = "";
	int selectedModel = 0;

	// Use this for initialization
	void Start ()
	{
	}
	
	void OnGUI ()
	{
		string[] modelNames = new string[models.Count];
		int i = 0;
		foreach (string s in models.Keys) {
			modelNames [i] = s;
			i++;
		}
		path = GUILayout.TextField (path);
		
		if (GUILayout.Button ("Import")) {
			GameObject go = ImportModel (path);
			models.Add (go.name, go);
		}
		if (GUILayout.Button ("Delete")) {
			Destroy (models [modelNames [selectedModel]]);
			models.Remove (modelNames [selectedModel]);
		}
		selectedModel = GUILayout.SelectionGrid (selectedModel, modelNames, 4);
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	GameObject ImportModel (string f)
	{
		return ImportModel (f, 2, 2, 1);
	}
	
	GameObject ImportModel (string f, int n, int t, int m)
	{
		//Option: 0 - None, 1 - Import, 2 - Compute/Generate(Normal/Tangent only)
		FbxPluginOptions options = new FbxPluginOptions () { filename = f, normalsOption = n, tangentsOption = t, materialsOption = m };
		FbxPlugin fp = new FbxPlugin ();
		fp.ImportModel (options);
		GameObject go = fp.DisplayScene ();
		return go;
	}
}
