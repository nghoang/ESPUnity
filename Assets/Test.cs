using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {
	
	public GameObject go;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI()	
	{
		if (GUI.Button(new Rect(50,50,50,50),"Test Me"))
		{
			float width = 0;
			float depth = 0;
			
			foreach (Transform o in go.transform)
			{
				Renderer renderer = o.GetComponentInChildren< Renderer >();
				if (renderer != null)
				{
					width += renderer.bounds.size.x;
					depth += renderer.bounds.size.z;
				}
			}
			
			Debug.Log(width);
			Debug.Log(depth);
		}
	}
}
