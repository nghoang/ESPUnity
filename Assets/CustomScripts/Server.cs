using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Collections.Specialized;
using System;

public delegate void CallbackFunction();

public class Server : MonoBehaviour , IServerThread{
	
	public float ObjectScale = 1;
	public Vector3 ObjectPos = new Vector3(0,0,0);
	public GameObject mainObject = null;
    //private GameObject pfGo;
	public AstarPath pf;

    ServerThread serverth = new ServerThread();
	private WebClient wclient = new WebClient();
	Thread thServer = null;
	public string host_id = "";
	Vector3 currentClickedLocation = Vector3.zero;
	public Vector3 startPathLocation = Vector3.zero;
	public Vector3 endPathLocation = Vector3.zero;
	public List<string> ids;
	public List<string> statuses;
	private string status_content = "";
	private string revitClientIP = "";
    Boolean is_loading_model = false;
    string choseClientType = "";
    C3JoinVoice voice;
    Material transparent;
    Material transparent_alt;
    Material transparent_emergency;
    Material non_transparent_alt;
    Material non_transparent_emergency;
    Material non_transparent;
    public List<Vector3> multiPathPoint = null;
    CustomSeeker cusSeeker;
	string server_host = "127.0.0.1";//this is inputed store server ip.
	int current_mat_type = 1;
	public string pathMode = "single";
	string cursorText = "";
	Vector2 mousePos_2D;
	
	//save to feed to later connections
	string last__status_content = "";
	string last_modelUrl = "";

    public void SetSemanticInfo(string info)
    {
        status_content = info;
    }
	
	public void ChangeSemanticInfo(string newid, string semantic)
	{
		String debug = "";
		foreach (String i in statuses)
		{
			debug += i + "\n";
		}
		Debug.Log(debug);
		List<string> newSemantic = new List<string>();
		for (int index=0;index < statuses.Count;index++)
		{
			string id = ids[index];
			string status = statuses[index];
			
			if (newid == id)
			{
				string substatus = "";
				if (status.IndexOf(",") >= 0)
				{
					substatus = status.Split(',')[1];
					status = status.Split(',')[0];
				}
				if (substatus != "")
					status = semantic + "," + substatus;
				else 
					status = semantic;
				newSemantic.Add(status);
				Debug.Log("Change semantic " + newid + " to " + semantic);
			}
			else 
			{
				newSemantic.Add(status);
			}
		}
		statuses = newSemantic;
		
		debug = "";
		foreach (String i in statuses)
		{
			debug += i + "\n";
		}
		Debug.Log(debug);
	}

    void Start()
    {
        voice = this.GetComponent<C3JoinVoice>();
        transparent = Resources.Load("Unity3dGlassTranspareny") as Material;
        transparent_alt = Resources.Load("Unity3dGlassTranspareny_alt") as Material;
        transparent_emergency = Resources.Load("Unity3dGlassTranspareny_emergency") as Material;
        non_transparent_emergency = Resources.Load("non_transparent_emergency") as Material;
        non_transparent_alt = Resources.Load("non_transparent_alt") as Material;
        non_transparent = Resources.Load("non_transparent") as Material;
        multiPathPoint = null;
        cusSeeker = this.GetComponent<CustomSeeker>();
    }

	void ReadStatus()
	{
		ids = new List<string>();
		statuses = new List<string>();
		
		string[] lines = status_content.Split('\n');
		foreach (string line in lines)
		{
			if (line.Trim() == "")
				continue;
			string[] parts = line.Trim().Split('-');
			ids.Add(parts[0]);
			statuses.Add(parts[1]);
		}
        cusSeeker.Rescan = true;
	}
	
	void OnApplicationQuit()
	{
		Debug.Log ("Quit");
        if (Network.peerType == NetworkPeerType.Server && host_id != "")
        {
            NameValueCollection pa = new NameValueCollection();
            pa.Add("sid", host_id);
            byte[] debug = wclient.UploadValues(AppConst.SERVER_DOMAIN + AppConst.SERVER_PATH + "?act=CloseHost&sid", pa);
        }
        serverth.server_status = "offline";
	}

    [RPC]
    public void DownloadModel(string hid, string modelUrl)
    {
		
    }
    public void ScalingObject(string _status_content)
	{
		//if (ids == null || ids.Count == 0)//we just need to read this once
        
		if (_status_content != "")
		{
			status_content = _status_content;
			ReadStatus();
		}
		
		mainObject.transform.position = ObjectPos;

        Debug.Log("Setup PF...");
		foreach (Transform o in mainObject.transform)
		{
			string id = o.name.ToLower();
			id = HoUtility.SimpleRegexSingle ("\\[(\\d+)\\]",id,1);
			if (id == "")
				continue;
			
			int index = ids.IndexOf(id);
			if (index == -1)
				continue;
			
			string status = statuses[index];
			string substatus = "";
			if (status.IndexOf(",") >= 0)
			{
				substatus = status.Split(',')[1];
				status = status.Split(',')[0];
			}
			
			Debug.Log("Item id: " + id + " at index " + index + " is " + status);
			
			if (status == "ground")
			{
				o.gameObject.layer = 8;
				o.gameObject.AddComponent<MeshCollider>();
				if (substatus == "level 1")
				{
					o.gameObject.renderer.material = transparent;
				}
				else 
				{
					o.gameObject.renderer.material = transparent_alt;
				}
			}
			else if (status == "obstacle")
			{
				o.gameObject.layer = 9;
                o.gameObject.AddComponent<MeshCollider>();
                o.gameObject.renderer.material = transparent;
			}
			else if (status == "emergency")
			{
				o.gameObject.layer = 10;
                o.gameObject.AddComponent<MeshCollider>();
                o.gameObject.renderer.material = transparent_emergency;
			}
			else if (status == "openeddoor")
			{
				//do nothing with opened door
                o.gameObject.AddComponent<MeshCollider>();
			}
			else if (status == "stair")
			{
				foreach (Transform so in o.transform)
				{
					so.gameObject.layer = 8;
					so.gameObject.AddComponent<MeshCollider>();
				}
			}
			else
			{
				Destroy(o.gameObject);
			}
			current_mat_type = 1;
			
		}
		SetUpPathFinding();
	}
	
	[RPC]
	public void ChangeMat()
	{
		if (current_mat_type == 0)
		{
			foreach (Transform o in mainObject.transform)
			{
				string id = o.name.ToLower();
				id = HoUtility.SimpleRegexSingle ("\\[(\\d+)\\]",id,1);
				if (id == "")
					continue;
				
				int index = ids.IndexOf(id);
				if (index == -1)
					continue;
				string status = statuses[index];
				string substatus = "";
				if (status.IndexOf(",") >= 0)
				{
					substatus = status.Split(',')[1];
					status = status.Split(',')[0];
				}
				
				if (status == "ground")
				{
					if (substatus == "level 1")
					{
						o.gameObject.renderer.material = transparent;
					}
					else 
					{
						o.gameObject.renderer.material = transparent_alt;
					}
				}
				else if (status == "obstacle")
				{
	                o.gameObject.renderer.material = transparent;
				}
				else if (status == "emergency")
				{
	                o.gameObject.renderer.material = transparent_emergency;
				}
			}
			current_mat_type = 1;
		}
		else if (current_mat_type == 1)
		{
			int ground = 0;
			foreach (Transform o in mainObject.transform)
			{
				string id = o.name.ToLower();
				id = HoUtility.SimpleRegexSingle ("\\[(\\d+)\\]",id,1);
				if (id == "")
					continue;
				
				int index = ids.IndexOf(id);
				if (index == -1)
					continue;
				string status = statuses[index];
				string substatus = "";
				if (status.IndexOf(",") >= 0)
				{
					substatus = status.Split(',')[1];
					status = status.Split(',')[0];
				}
				
				if (status == "ground")
				{
					if (substatus == "level 1")
					{
						o.gameObject.renderer.material = non_transparent;
					}
					else 
					{
						o.gameObject.renderer.material = non_transparent_alt;
					}
				}
				else if (status == "obstacle")
				{
	                o.gameObject.renderer.material = non_transparent;
				}
				else if (status == "emergency")
				{
	                o.gameObject.renderer.material = non_transparent_emergency;
				}
			}
			current_mat_type = 0;
		}
	}
	
	[RPC]
	public void ScaleModel(float scale)
	{		
		if (mainObject != null)
		{
			Debug.Log ("Scale Objects...");
			mainObject.transform.localScale = new Vector3(scale,scale,scale);
		}
	}
	
	void OnGUI()
	{
		if (cursorText != "")
		{
			Vector2 mousePos_2D = new Vector2(Input.mousePosition.x, (Screen.height - Input.mousePosition.y));
			GUI.Label(new Rect(mousePos_2D.x,mousePos_2D.y-20,200,30),cursorText);
		}
		
        if (choseClientType == "")
        {
			server_host = GUI.TextField(new Rect(Screen.width / 2, 240, 100, 20), server_host);
            if (GUI.Button(new Rect(Screen.width / 2 - 100, 240, 100, 20), "Create Host"))
            {
                choseClientType = "server";
                try
                {
					AppConst.SERVER_DOMAIN = "http://"+server_host;
                    StartServer();
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.StackTrace);
                }
            }

            if (GUI.Button(new Rect(Screen.width / 2 - 100, 200, 100, 20), "Join Host"))
            {
                choseClientType = "client";
            }
        }
        
        if (Network.peerType == NetworkPeerType.Server)
        {
            ObjectScale = float.Parse(GUI.TextField(new Rect(80,10,30,20),ObjectScale.ToString()));
			if (GUI.Button(new Rect(120, 10, 80, 20), "Scale"))
            {
				if (mainObject != null)
				{
					networkView.RPC("ScaleModel", RPCMode.All, ObjectScale);
				}
			}
            if (serverth.server_status == "online" || Network.peerType == NetworkPeerType.Server)
            {
                if (currentClickedLocation != Vector3.zero)
                {
                    if (currentClickedLocation != Vector3.zero)
                    {
                        if (pathMode == "single")
                        {
                            GUI.Label(new Rect(10, 170, 200, 30), "Set As Start Location (Press V)");
                            GUI.Label(new Rect(10, 210, 200, 30), "Set As End Location (Press B)");
                        }
                        else
                        {
                            GUI.Label(new Rect(10, 170, 200, 30), "Add Point (Press V)");
                        }
                    }
                }
                

                if (GUI.Button(new Rect(10, 60, 200, 30), "Stop Server ("+Network.player.ipAddress+")"))
                {
                    Network.Disconnect(1000);
                    CloseConnection();
                }
				
                if (pathMode == "single")
                {
                    if (GUI.Button(new Rect(10, 140, 200, 30), "Multiple Path Mode"))
                    {
						pathMode = "multi";
						Debug.Log ("change mode 1");
                        cusSeeker.ClearMultiPathOverNetwork();
                        multiPathPoint = new List<Vector3>();
                    }
                }
                else if (pathMode == "multi")
                {
                    if (GUI.Button(new Rect(10, 140, 200, 30), "Single Path Mode"))
                    {
						pathMode = "single";
						Debug.Log ("change mode 2");
                        cusSeeker.ClearMultiPathOverNetwork();
                        multiPathPoint = null;
                    }
                }
            }

        }


        if (choseClientType == "client")
        {
            if (Network.peerType == NetworkPeerType.Disconnected)
            {
                server_host = GUI.TextField(new Rect(10, 10, 200, 30), server_host);
                if (GUI.Button(new Rect(220, 10, 100, 30), "Connect"))
                {
					AppConst.SERVER_DOMAIN = "http://"+server_host;
                    Network.Connect(server_host, AppConst.PORTNUMBER);
                }
            }
            else
            {
            }
        }
	}
	
	void OnPlayerConnected(NetworkPlayer player) {
		if (serverth.current_server_activity == "server_downloaded")
		{
			networkView.RPC("LoadLocalFBX", RPCMode.Others, last__status_content, last_modelUrl,0);
		}
	}

	void CloseConnection()
	{	
		NameValueCollection pa = new NameValueCollection();
		pa.Add("sid", host_id);
		byte[] debug = wclient.UploadValues(AppConst.SERVER_DOMAIN + AppConst.SERVER_PATH + "?act=CloseHost&sid",pa);
		Debug.Log("server closed. " + HoUtility.ByteToString(debug));
        serverth.server_status = "offline";
        choseClientType = "";
        if (thServer != null)
            thServer.Abort();
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

    public void Log(string log)
    {
        Debug.Log(log);
    }

    public void DownloadModelCb()
    {
        Debug.Log("DownloadModel " + host_id + ".fbx");
        is_loading_model = true;
    }
        

    void StartServer()
    {
        serverth.server_status = "online";
        host_id = wclient.DownloadString(AppConst.SERVER_DOMAIN + AppConst.SERVER_PATH + "?act=OpenHost");
        Debug.Log("host id: " + host_id);
        serverth.host_id = host_id;
        serverth.revitClientIP = revitClientIP;
        serverth.main = this;
        thServer = new Thread(new ThreadStart(serverth.ServerLoop));
        thServer.Start();
        Network.InitializeServer(5, AppConst.PORTNUMBER, false);
    }
	
	void Clean()
	{
		if (thServer != null)
			thServer.Abort();
	}
	
	private void SetUpPathFinding()
	{
        //pfGo = GameObject.Find("PathFinding");
		pf = this.GetComponent<AstarPath>();
		pf.Scan ();
        cusSeeker.Rescan = true;
	}



	void Update()
	{
		Ray ray;
        RaycastHit hit;
		
        if (voice.enabled == false)
        {
            if (Network.peerType == NetworkPeerType.Server || Network.peerType == NetworkPeerType.Client)
            {
                voice.enabled = true;
            }
        }

        if (Network.peerType == NetworkPeerType.Server)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject.layer == 8)
                    {
                        currentClickedLocation = hit.point;
                        Debug.Log(hit.point);
                    }
                }
            }
            if (pathMode == "single")
            {
                if (Input.GetKey(KeyCode.V) && currentClickedLocation != Vector3.zero)
                {
                    startPathLocation = currentClickedLocation;//new Vector3(currentClickedLocation.x,currentClickedLocation.y + 5,currentClickedLocation.z);
                    currentClickedLocation = Vector3.zero;
                }
                if (Input.GetKey(KeyCode.B) && currentClickedLocation != Vector3.zero)
                {
                    endPathLocation = currentClickedLocation;//new Vector3(currentClickedLocation.x,currentClickedLocation.y + 5,currentClickedLocation.z);
                    currentClickedLocation = Vector3.zero;
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.V) && currentClickedLocation != Vector3.zero)
                {
                    multiPathPoint.Add(currentClickedLocation);
                    currentClickedLocation = Vector3.zero;
                }
            }
			
			if (Input.GetKeyUp(KeyCode.Mouse1))
			{
				ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject.layer == 10)
                    {
						networkView.RPC("RemoveEmergencyRegion",RPCMode.All,hit.collider.gameObject.name);
					}
				}
			}
        }
		
		if (Input.GetKeyUp(KeyCode.M))
		{
			networkView.RPC("ChangeMat",RPCMode.All);
		}

        if (is_loading_model == true)
        {
            //networkView.RPC("DownloadModel", RPCMode.All, host_id, AppConst.SERVER_DOMAIN + AppConst.SERVER_PATH + AppConst.SERVER_MODEL_PATH + host_id + ".fbx");
            networkView.RPC("LoadLocalFBX", RPCMode.All, status_content, AppConst.SERVER_PATH + AppConst.SERVER_MODEL_PATH + host_id + ".fbx",1);
            is_loading_model = false;
        }
		
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject != null)
			{
				string temp_name = hit.collider.gameObject.name;
				string id = HoUtility.SimpleRegexSingle("\\[([^\\]]*)\\]",temp_name,1);
				
				for (int i=0;i<ids.Count;i++)
				{
					if (ids[i] == id)
					{
						cursorText = ids[i] + ": " + statuses[i];
						break;
					}
				}
			}
			else 
			{
				cursorText = "";
			}
		}
	}
	
	[RPC]
	public void RemoveEmergencyRegion(string goname)
	{
		GameObject go = GameObject.Find(goname);
		go.layer = 8;
		string temp_id = HoUtility.SimpleRegexSingle("\\[([^\\]]*)\\]",goname,1);
		ChangeSemanticInfo(temp_id,"ground");
		ScalingObject("");
	}
	
    [RPC]
    public void LoadLocalFBX(string _status_content, string modelUrl, int LoadType)
    {
		//Loadtype: 0: first load only, 1:always load 
		if (LoadType == 0 && mainObject != null)
			return;
		
		last__status_content = _status_content;
		last_modelUrl = modelUrl;
		Debug.Log("Download client model:" + modelUrl);
		if (Network.peerType == NetworkPeerType.Client)
		{
			WebClient client = new WebClient();
        	System.IO.File.Delete("building-client.fbx");
        	client.DownloadFile(AppConst.SERVER_DOMAIN + modelUrl, "building-client.fbx");
		}
        if (mainObject != null)
            GameObject.Destroy(mainObject);
		if (Network.peerType == NetworkPeerType.Server)
        	mainObject = ImportModel("building.fbx");
		else if (Network.peerType == NetworkPeerType.Client)
			mainObject = ImportModel("building-client.fbx");
        ScalingObject(_status_content);
		networkView.RPC("ScaleModel", RPCMode.All, ObjectScale);
    }
	
	public void LoadObjects(string modelFileName)
	{
		
	}
}
