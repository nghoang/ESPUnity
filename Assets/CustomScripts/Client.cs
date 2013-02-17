using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Specialized;

public class Client : MonoBehaviour {

    //int movingSpeed = 10;
    //public Camera mainCam;
    //public string IPConnectTo = "127.0.0.1";
    //LineRenderer pathLine;
    //private WebClient client = new WebClient();
    //private GameObject mainObject = null;
    //public Vector3 ObjectPos = new Vector3(0, 0, 0);
    //public bool usingThisObj = false;
    //Server server;

    //void Start()
    //{
    //    pathLine = this.GetComponent<LineRenderer>();
    //    server = this.GetComponent<Server>();
    //}
	
    //void OnGUI() {

    //    if (server.usingThisObj == false && usingThisObj == false)
    //    {
    //        if (GUI.Button(new Rect(Screen.width / 2 - 100, 200, 100, 20), "Join Host"))
    //        {
    //            usingThisObj = true;
    //        }
    //    }
    //    else if (usingThisObj == true)
    //    {
    //        if (Network.peerType == NetworkPeerType.Disconnected)
    //        {
    //            GUI.TextField(new Rect(10, 10, 200, 50), IPConnectTo);
    //            if (GUI.Button(new Rect(220, 10, 50, 50), "Connect"))
    //            {
    //                Network.Connect(IPConnectTo, AppConst.PORTNUMBER);
    //            }
    //        }
    //        else
    //        {
    //            if (GUI.Button(new Rect(220, 10, 50, 80), "Sync Scene"))
    //            {

    //            }
    //        }
    //    }
    //}

    //void Update(){
    //    if (usingThisObj)
    //    { }
    //}

    //[RPC]
    //void NewClientJoin()
    //{ }


    //[RPC]
    //void DownloadModel(string hid, string modelUrl)
    //{
    //    NameValueCollection pa = new NameValueCollection();
    //    pa.Add("sid", hid);
    //    System.IO.File.Delete("building-client.fbx");
    //    client.DownloadFile(modelUrl, "building-client.fbx");
    //    client.UploadValues(AppConst.SERVER + "?act=GetSemanticFile", pa);
    //}

    //[RPC]
    //public void LoadLocalFBX()
    //{
    //    mainObject = ImportModel("building-client.fbx");
    //    ScalingObject();
    //}

    //void ScalingObject()
    //{
    //    mainObject.transform.position = ObjectPos;
    //}

    //[RPC]
    //public void AddPointPath(int i, Vector3 p)
    //{
    //    pathLine.SetPosition(i, p);
    //}

    //[RPC]
    //void DrawPath(int length)
    //{
    //    Debug.Log("path length: " + length);
    //    pathLine.SetVertexCount(length);
    //}

    //[RPC]
    //void LoadPrefabs(string file)
    //{
    //    GameObject pf = Resources.Load("pre-" + file) as GameObject;
    //    GameObject go = (GameObject)Instantiate(pf, mainCam.transform.position + mainCam.transform.forward, Quaternion.identity);
    //}

    //[RPC]
    //void MovePrefabObject(string objId)
    //{
    //    GameObject movingObject = GameObject.Find(objId);


    //    if (Input.GetKey(KeyCode.J))
    //    {
    //        movingObject.transform.position += new Vector3(Time.deltaTime * movingSpeed * (-1), 0, 0);
    //    }
    //    else if (Input.GetKey(KeyCode.L))
    //    {
    //        movingObject.transform.position += new Vector3(Time.deltaTime * movingSpeed, 0, 0);
    //    }
    //    else if (Input.GetKey(KeyCode.I))
    //    {
    //        movingObject.transform.position += new Vector3(0, 0, Time.deltaTime * movingSpeed * (-1));
    //    }
    //    else if (Input.GetKey(KeyCode.K))
    //    {
    //        movingObject.transform.position += new Vector3(0, 0, Time.deltaTime * movingSpeed);
    //    }
    //    else if (Input.GetKey(KeyCode.O))
    //    {
    //        movingObject.transform.position += new Vector3(0, Time.deltaTime * movingSpeed * (-1), 0);
    //    }
    //    else if (Input.GetKey(KeyCode.P))
    //    {
    //        movingObject.transform.position += new Vector3(0, Time.deltaTime * movingSpeed, 0);
    //    }
    //}

    //GameObject ImportModel(string f)
    //{
    //    return ImportModel(f, 2, 2, 1);
    //}

    //GameObject ImportModel(string f, int n, int t, int m)
    //{
    //    //Option: 0 - None, 1 - Import, 2 - Compute/Generate(Normal/Tangent only)
    //    FbxPluginOptions options = new FbxPluginOptions() { filename = f, normalsOption = n, tangentsOption = t, materialsOption = m };
    //    FbxPlugin fp = new FbxPlugin();
    //    fp.ImportModel(options);
    //    GameObject go = fp.DisplayScene();
    //    return go;
    //}
}
