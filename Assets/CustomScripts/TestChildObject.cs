using UnityEngine;
using System.Collections;

public class TestChildObject : MonoBehaviour {

	// Use this for initialization
	void Start () {
		int i=0;
		foreach (Transform o in transform)
		{
			if (o.name.ToLower().IndexOf("surface") == -1 &&
				o.name.ToLower().IndexOf("wall") == -1 &&
				o.name.ToLower().IndexOf("panel") == -1 &&
				o.name.ToLower().IndexOf("floor") == -1)
			{
				Debug.Log ("Name: " + o.name.ToLower().IndexOf("wall") + " " + o.name.ToLower());
				Destroy(transform.GetChild(i).gameObject);
				continue;
			}
			
			if (o.name.ToLower().IndexOf("floor") >= 0 || o.name.ToLower().IndexOf("surface") >= 0)
			{
				transform.GetChild(i).gameObject.layer = 8;
			}
			
			if (o.name.ToLower().IndexOf("wall") >= 0 || o.name.ToLower().IndexOf("panel") >= 0)
			{
				transform.GetChild(i).gameObject.layer = 9;
			}
			transform.GetChild(i).gameObject.AddComponent<MeshCollider>();
			i++;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
