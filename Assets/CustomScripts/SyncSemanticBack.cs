using UnityEngine;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Collections;
using System.Collections.Generic;

public class SyncSemanticBack : MonoBehaviour {
	
	Server server;
	WebClient wclient = new WebClient();
	
	// Use this for initialization
	void Start () {
		server = this.GetComponent<Server>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI()
	{
		if (GUI.Button(new Rect(Screen.width - 150,10,100,50),"Sync Back"))
		{
			string semantic = ""; 
			foreach (Transform o in server.mainObject.transform)
			{
				string id = o.name.ToLower();
				id = HoUtility.SimpleRegexSingle ("\\[(\\d+)\\]",id,1);
				if (id == "")
					continue;
				int index = server.ids.IndexOf(id);
				if (index == -1)
					continue;
				string status = server.statuses[index];
				string substatus = "";
				if (status.IndexOf(",") >= 0)
				{
					substatus = status.Split(',')[1];
					status = status.Split(',')[0];
				}
				semantic += id+"-"+status + "\n";
			}
			
			Debug.Log ("Semantic Back:\n" + semantic);
			NameValueCollection pa = new NameValueCollection();
			pa.Add("sid", server.host_id);
			pa.Add("semantic", semantic);
			wclient.UploadValues(AppConst.SERVER_DOMAIN + AppConst.SERVER_PATH + "?act=UploadSemanticBack",pa);
		}
	}
}
