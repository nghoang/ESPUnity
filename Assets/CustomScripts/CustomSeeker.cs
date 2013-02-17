using UnityEngine;
using System.Collections;
using Pathfinding;
using System.Collections.Generic;

public class CustomSeeker : MonoBehaviour
{

    public Path path;
    private Seeker sk;
    private Server server;
    float total_time = 0;
    LineRenderer pathLine;
    public GameObject movingObject;
    int currentWaypoint = 0;
    bool isLetItMove = false;
    bool isFirstPerson = false;
    public float speed = 10;
    string MultipathPointString = "";
    public bool Rescan = true;
	public GameObject camera;
	Vector3 BeforeMoving;

    // Use this for initialization
    void Start()
    {
        sk = GetComponent<Seeker>();
        //controller = GetComponent<CharacterController>();		
        server = this.GetComponent<Server>();
        pathLine = this.GetComponent<LineRenderer>();
    }

    void OnGUI()
    {
        if (path != null && isLetItMove == false && server.pathMode == "single")
        {
            if (GUI.Button(new Rect(10, 95, 200, 30), "Move Follow Paths"))
            {
				BeforeMoving = camera.transform.position;
                isLetItMove = true;
                currentWaypoint = 0;
                movingObject.transform.position = new Vector3(server.startPathLocation.x, server.startPathLocation.y + 1.5F, server.startPathLocation.z);
            }
			string text = "Top Down View";
			if (isFirstPerson == true)
			{
				text = "Third Person View";
			}
			if (GUI.Button(new Rect(220, 95, 200, 30), text))
            {
                isFirstPerson = !isFirstPerson;
            }
        }
    }

    public void ClearMultiPathOverNetwork()
    {
        networkView.RPC("ClearPreviousAll", RPCMode.All);
    }

    public void ClearPrevious()
    {
        for (int i = 0; i < 999; i++)
        {
            GameObject go = GameObject.Find("LineRenderer_" + i);
            if (go == null)
                break;
            Destroy(go);
        }
    }
	
	[RPC]
    public void ClearPreviousAll()
    {
        MultipathPointString = "";
        pathLine.SetVertexCount(0);
        for (int i = 0; i < 999; i++)
        {
            GameObject go = GameObject.Find("LineRenderer_" + i);
            if (go == null)
                break;
            Destroy(go);
        }
    }

    public void onCompleteFunction(Path p)
    {
        //Debug.Log ("We have done the path... " + p.error + " " + p.errorLog);

        //check if it is jumping

        if (p.GetType() == typeof(MultiTargetPath))
        {
            MultiTargetPath mp = p as MultiTargetPath;
            if (!mp.error)
            {
                //Debug.Log("mp.foundEnd:" + mp.foundEnd);
                //Debug.Log("mp.errorLog:" + mp.errorLog);
                //Debug.Log("mp.endPoint:" + mp.endPoint);
                //Debug.Log("mp.endHint:" + mp.endHint);
                //string multipathPointString = "";
                networkView.RPC("ResetDrawMultiPath", RPCMode.All);
                for (int j = 0; j < mp.vectorPaths.Length; j++)
                {
                    if (mp.vectorPaths[j] == null)
                        continue;

                    //for (int i = 1; i < mp.vectorPath.Length; i++)
                    //{
                    //    Vector3 pre = mp.vectorPath[i - 1];
                    //    Vector3 cur = mp.vectorPath[i];
                    //    if (Mathf.Abs(pre.y - cur.y) > 1 || Mathf.Abs(pre.x - cur.x) > 1 || Mathf.Abs(pre.z - cur.z) > 1)
                    //    {
                    //        //Debug.Log("break because unwalkable");
                    //        server.multiPathPoint.RemoveAt(server.multiPathPoint.Count - 1);
                    //        return;
                    //    }
                    //}

                    string pathPointString = "";
                    Vector3[] vpath = mp.vectorPaths[j];
                    for (int i = 0; i < vpath.Length; i++)
                    {
                        if (pathPointString == "")
                            pathPointString += vpath[i].x + "," + vpath[i].y + "," + vpath[i].z;
                        else
                            pathPointString += ";" + vpath[i].x + "," + vpath[i].y + "," + vpath[i].z;
                    }
                    if (pathPointString != "")
                    {
                        if (j == 0)
                            networkView.RPC("AddDrawMultiPath", RPCMode.All, pathPointString);
                        else
                            networkView.RPC("AddDrawMultiPath", RPCMode.All, "|" + pathPointString);
                    }
                }
                networkView.RPC("DrawMultiPath", RPCMode.All);
            }
            else
            {
                Debug.Log("Delete Node");
                server.multiPathPoint.RemoveAt(server.multiPathPoint.Count - 1);
            }
        }
        else
        {
            if (!p.error)
            {
                path = p;
                //for (int i = 1; i < path.vectorPath.Length; i++)
                //{
                //    Vector3 pre = path.vectorPath[i - 1];
                //    Vector3 cur = path.vectorPath[i];
                //    if (Mathf.Abs(pre.y - cur.y) > 1 || Mathf.Abs(pre.x - cur.x) > 1 || Mathf.Abs(pre.z - cur.z) > 1)
                //    {
                //        path = null;
                //        networkView.RPC("DrawPath", RPCMode.All, "");
                //        return;
                //    }
                //}

                string pathPointString = "";
                for (int i = 0; i < path.vectorPath.Length; i++)
                {
                    if (pathPointString == "")
                        pathPointString += path.vectorPath[i].x + "," + path.vectorPath[i].y + "," + path.vectorPath[i].z;
                    else
                        pathPointString += ";" + path.vectorPath[i].x + "," + path.vectorPath[i].y + "," + path.vectorPath[i].z;
                }
                networkView.RPC("DrawPath", RPCMode.All, pathPointString);
            }
        }

    }

    [RPC]
    public void AddDrawMultiPath(string path)
    {
        MultipathPointString += path;
    }

    [RPC]
    public void ResetDrawMultiPath()
    {
        MultipathPointString = "";
    }

    bool isdrawing = false;
    [RPC]
    public void DrawMultiPath()
    {
        if (isdrawing == true)
            return;
        isdrawing = true;
        pathLine.SetVertexCount(0);
        //ClearPrevious();

        Debug.Log("drawing client paths");
        int i = 0;
        //Debug.Log(MultipathPointString);
        foreach (string pathPointString in MultipathPointString.Split('|'))
        {
            GameObject ob = null;
            GameObject old_go = GameObject.Find("LineRenderer_" + i);
            if (old_go != null)
            {
                ob = GameObject.Find("LineRenderer_" + i);
                //Destroy(old_go);
                //i++;
                //continue;
            }
            else 
                ob = new GameObject("LineRenderer_" + i, typeof(LineRenderer));
            i++;

            LineRenderer lr = ob.GetComponent<LineRenderer>();
            lr.SetWidth(0.1f, 0.1f);

            lr.SetVertexCount(pathPointString.Split(';').Length);
            int j = 0;
            foreach (string point in pathPointString.Split(';'))
            {
                if (point != "")
                {
                    //Debug.Log(point);
                    Vector3 np = new Vector3(float.Parse(point.Split(',')[0]), float.Parse(point.Split(',')[1]) + 0.5f, float.Parse(point.Split(',')[2]));
                    lr.SetPosition(j, np);
                    j++;
                }
            }
        }
        isdrawing = false;
    }

    [RPC]
    public void DrawPath(string pathPointString)
    {
        ClearPrevious();

        pathLine.SetVertexCount(pathPointString.Split(';').Length);
        int i = 0;
        foreach (string point in pathPointString.Split(';'))
        {
            if (point != "")
            {
                Vector3 np = new Vector3(float.Parse(point.Split(',')[0]), float.Parse(point.Split(',')[1]) + 0.5f, float.Parse(point.Split(',')[2]));
                pathLine.SetPosition(i, np);
                i++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isLetItMove)
        {
            if (currentWaypoint < path.vectorPath.Length)
            {
                Vector3 dir = path.vectorPath[currentWaypoint] - movingObject.transform.position;
				if (isFirstPerson == true)
				{
					camera.transform.LookAt(movingObject.transform);
					if (currentWaypoint == 1)
					{
						Vector3 temp = path.vectorPath[currentWaypoint];
						temp.y += 5;
						camera.transform.position = temp;
					}
                	camera.transform.Translate(speed * dir * Time.deltaTime);
				}
                movingObject.transform.Translate(speed * dir * Time.deltaTime);
				
                if (Vector3.Distance(path.vectorPath[currentWaypoint], movingObject.transform.position) <= 1)
                {
                    currentWaypoint++;
                }
            }
            else
            {
                isLetItMove = false;
				//camera.transform.position = BeforeMoving;
            }
        }

        if (server.startPathLocation != Vector3.zero && server.endPathLocation != Vector3.zero || server.multiPathPoint != null && server.multiPathPoint.Count >= 2)
        {
            if (total_time > 0.5)
            {
                transform.position = server.startPathLocation;
                if (server.pathMode == "single")
                {
                    sk.StartPath(server.startPathLocation, server.endPathLocation,
                        onCompleteFunction);
                }
                else if (server.multiPathPoint.Count >= 2)
                {
                    Vector3[] starts = new Vector3[server.multiPathPoint.Count - 1];
                    for (int i = 1; i < server.multiPathPoint.Count; i++)
                    {
                        starts[i - 1] = server.multiPathPoint[i];
                    }
                    MultiTargetPath mp = new MultiTargetPath(starts, server.multiPathPoint[0], null, onCompleteFunction);
                    sk.StartPath(mp);
                }
                total_time = 0;
                if (Rescan)
                {
                    //ClearPrevious();
                    server.pf.Scan();
                    Rescan = false;
                }
            }
            else
            {
                total_time += Time.deltaTime;
            }
        }
    }
}
