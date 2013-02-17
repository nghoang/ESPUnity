using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Inventory : MonoBehaviour {

    public Camera mainCam;

    string[] InventoryItems = new string[] { "FireCampfire"/*, 
        "Bath 2", 
        "Lavatory 1", 
        "Lavatory 2",
        "Shower 1",
        "Shower 2",
        "Wash-basin 1",
        "Wash-basin 2"*/};
    GameObject selectedGo;
    int movingSpeed = 10;
    CustomSeeker cusSeeker;
	GameObject lastObject;
	Server server;
	int countFire = 1;
	
	// Use this for initialization
    void Start()
    {
        cusSeeker = this.GetComponent<CustomSeeker>();
		server = this.GetComponent<Server>();
	}
	
	[RPC]
	public void SetTheFire(string goname, Vector3 point)
	{
		if (selectedGo == null)
        {
			GameObject go = GameObject.Find(goname);
            if (go != null && go.layer == 8 && lastObject != null)
            {
                //selectedGo = hit.collider.gameObject;
				lastObject.transform.position = point;
				go.layer = 10;
				string id = HoUtility.SimpleRegexSingle("\\[([^\\]]*)\\]",goname,1);
				server.ChangeSemanticInfo(id,"emergency");
				server.ScalingObject("");
				cusSeeker.Rescan = true;
				lastObject = null;
            }
        }
        else
        {
            selectedGo = null;
        }
	}
	
	[RPC]
	public void UnsetTheFire(string goname)
	{
		GameObject go = GameObject.Find(goname);
		Debug.Log ("Deleting Fire Object:" + go);
		if (go != null && go.layer == 12)
        {
			Debug.Log ("Deleted Fire");
			GameObject.Destroy(go);
			lastObject = null;
		}
	}
	
	// Update is called once per frame
	void Update () {
        if (Network.peerType == NetworkPeerType.Server)
        {
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
					networkView.RPC("SetTheFire", RPCMode.All, hit.collider.gameObject.name, hit.point);
                }
            }
			
			if (Input.GetKeyUp(KeyCode.Mouse1))
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
					Debug.Log ("Deleting Fire");
					networkView.RPC("UnsetTheFire", RPCMode.All, hit.collider.gameObject.name);
				}
			}

            //moving selecting object
            /*if (selectedGo != null)
            {
                Vector3 oldPos = selectedGo.transform.position;
                if (Input.GetKey(KeyCode.J))
                {
                    oldPos += new Vector3(Time.deltaTime * movingSpeed, 0, 0);
                }
                else if (Input.GetKey(KeyCode.L))
                {
                    oldPos += new Vector3(Time.deltaTime * movingSpeed * (-1), 0, 0);
                }
                else if (Input.GetKey(KeyCode.I))
                {
                    oldPos += new Vector3(0, 0, Time.deltaTime * movingSpeed * (-1));
                }
                else if (Input.GetKey(KeyCode.K))
                {
                    oldPos += new Vector3(0, 0, Time.deltaTime * movingSpeed);
                }
                else if (Input.GetKey(KeyCode.O))
                {
                    oldPos += new Vector3(0, Time.deltaTime * movingSpeed * (-1), 0);
                }
                else if (Input.GetKey(KeyCode.P))
                {
                    oldPos += new Vector3(0, Time.deltaTime * movingSpeed, 0);
                }
                if (Input.GetKeyUp(KeyCode.J) || 
                    Input.GetKeyUp(KeyCode.L) || 
                    Input.GetKeyUp(KeyCode.K) || 
                    Input.GetKeyUp(KeyCode.I) || 
                    Input.GetKeyUp(KeyCode.O) || 
                    Input.GetKeyUp(KeyCode.P))
                    cusSeeker.Rescan = true;

                networkView.RPC("MovePrefabObject", RPCMode.All, selectedGo.name, oldPos);
            }*/
        }
	}

    [RPC]
    void MovePrefabObject(string objId, Vector3 newPos)
    {
        GameObject movingObject = GameObject.Find(objId);
        movingObject.transform.position = newPos;
    }

	
	
    [RPC]
    void LoadPrefabs(string file)
    {
        Debug.Log("Create Obstacle: " + file);
        GameObject pf = Resources.Load("pre-" + file) as GameObject;
		pf.name = "fire-" + countFire;
		countFire++;
        lastObject = (GameObject)Instantiate(pf, new Vector3(99999,99999,99999), Quaternion.identity);
		lastObject.AddComponent<BoxCollider>();
		lastObject.layer = 12;
    }

    void OnGUI()
    {
        if (Network.peerType == NetworkPeerType.Server)
        {
            for (int i = 0; i < InventoryItems.Length; i++)
            {
                //Debug.Log("Load Texture: " + InventoryItems[i, 1]);
                Texture img = Resources.Load("thumb-" + InventoryItems[i]) as Texture;
                if (GUI.Button(new Rect(20 + i * (5 + 50), 5, 50, 50), img))
                {
                    networkView.RPC("LoadPrefabs", RPCMode.All, InventoryItems[i]);
                }
            }

            if (selectedGo != null)
            {
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 10, 100, 20), "Editing Mode");
            }
        }
    }
}
