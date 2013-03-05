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
			Debug.Log(go.renderer.bounds);
		}
	}
}
