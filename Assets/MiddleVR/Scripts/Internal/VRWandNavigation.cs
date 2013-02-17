/* VRWandInteraction
 * MiddleVR
 * (c) i'm in VR
 */

using UnityEngine;
using System.Collections;
using MiddleVR_Unity3D;
using System;

[RequireComponent (typeof(VRWandInteraction))]
public class VRWandNavigation : MonoBehaviour {
    public string NodeToMove = "CenterNode";
    public string DirectionReferenceNode = "HandNode";
    public string TurnAroundNode = "HeadNode";

    public float NavigationSpeed = 1.0f;
    public float RotationSpeed = 1.0f;

    public bool Fly = true;

    private bool m_SearchedRefNode = false;
    private bool m_SearchedNodeToMove = false;
    private bool m_SearchedRotationNode = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
    void Update()
    {
        GameObject directionRefNode = null;
        GameObject nodeToMove       = null;
        GameObject turnNode         = null;

        directionRefNode = GameObject.Find(DirectionReferenceNode);
        nodeToMove       = GameObject.Find(NodeToMove);
        turnNode         = GameObject.Find(TurnAroundNode);

        if ( m_SearchedRefNode == false && directionRefNode == null )
        {
            MiddleVRTools.Log("[X] VRWandNavigation: Couldn't find '" + DirectionReferenceNode + "'");
            m_SearchedRefNode = true;
        }

        if (m_SearchedNodeToMove == false && nodeToMove == null)
        {
            MiddleVRTools.Log("[X] VRWandNavigation: Couldn't find '" + NodeToMove + "'");
            m_SearchedNodeToMove = true;
        }

        if (m_SearchedRotationNode == false && TurnAroundNode.Length > 0 && turnNode == null)
        {
            MiddleVRTools.Log("[X] VRWandNavigation: Couldn't find '" + TurnAroundNode + "'");
            m_SearchedRotationNode = true;
        }

        if (directionRefNode != null && nodeToMove != null )
        {
            float speed = 0.0f; 
            float speedR = 0.0f;

            GameObject vrmgr = GameObject.Find("VRManager");
            if( vrmgr != null )
            {
                VRManagerScript script = vrmgr.GetComponent<VRManagerScript>();

                if( script != null )
                {
                    /// FORWARD
                    float forward = script.WandAxisVertical;

                    //MiddleVRTools.Log("Forward: " + forward);

                    float deltaTime = (float)MiddleVR.VRKernel.GetDeltaTime();

                    if (Math.Abs(forward) > 0.1) speed = forward * NavigationSpeed * deltaTime;

                    /// ROTATION
                    float rotation = script.WandAxisHorizontal;

                    if (Math.Abs(rotation) > 0.1) speedR = rotation * RotationSpeed * deltaTime;

                    /// Computing direction
                    Vector3 translationVector = new Vector3(0, 0, 1);
                    Vector3 tVec = translationVector;
                    Vector3 nVec = new Vector3(tVec.x * speed, tVec.y * speed, tVec.z * speed );

                    Vector3 mVec = directionRefNode.transform.TransformDirection(nVec);

                    if( Fly == false )
                    {
                        mVec.y = 0.0f;
                    }

                    nodeToMove.transform.Translate(mVec,Space.World);

                    if (turnNode != null)
                    {
                        nodeToMove.transform.RotateAround(turnNode.transform.position, new Vector3(0, 1, 0), speedR);
                    }
                    else
                    {
                        nodeToMove.transform.Rotate(new Vector3(0, 1, 0), speedR);
                    }
                }
            }
        }
    }
}
