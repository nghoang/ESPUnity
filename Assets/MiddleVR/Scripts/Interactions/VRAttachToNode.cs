/* VRAttachToNode
 * MiddleVR
 * (c) i'm in VR
 */

using UnityEngine;
using System.Collections;
using MiddleVR_Unity3D;

public class VRAttachToNode  : MonoBehaviour {
    public string VRNode = "HandNode";
    bool attached = false;

    private bool m_Searched = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (!attached)
        {
            GameObject node = GameObject.Find(VRNode);

            if( VRNode.Length == 0 )
            {
                MiddleVRTools.Log(0, "[X] AttachToNode: Please specify a valid vrNode name.");
            }

            if (node != null)
            {
                transform.parent = node.transform;
                transform.localPosition = new Vector3(0, 0, 0);
                transform.localRotation = new Quaternion(0, 0, 0, 1);
                MiddleVRTools.Log( 2, "[+] AttachToNode: " + this.name + " attached to : " + node.name );
                attached = true;
            }
            else
            {
                if (m_Searched == false)
                {
                    MiddleVRTools.Log(0, "[X] AttachToNode: Failed to find Game object '" + VRNode + "'");
                    m_Searched = true;
                }
            }
        }
	}
}
