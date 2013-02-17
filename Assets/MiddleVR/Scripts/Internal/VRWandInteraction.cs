/* VRWandInteraction
 * MiddleVR
 * (c) i'm in VR
 */

using UnityEngine;
using System.Collections;
using MiddleVR_Unity3D;

public class VRWandInteraction : MonoBehaviour {

    public float RayLength = 2;

    public bool Highlight = true;
    public Color HighlightColor = new Color();
    public Color GrabColor = new Color();

    public bool RepeatAction = false;

    GameObject m_ObjectInHand = null;
    GameObject m_CurrentObject = null;

    
    bool m_ObjectWasKinematic = true;

    private vrButtons m_Buttons = null;
    private bool      m_SearchedButtons = false;

    private GameObject m_Ray = null;

	// Use this for initialization
	void Start () {
        m_Ray = GameObject.Find("WandRay");

        if (m_Ray != null)
        {
            m_Ray.transform.localScale = new Vector3( 0.01f, RayLength / 2.0f, 0.01f );
            m_Ray.transform.localPosition = new Vector3( 0,0, RayLength / 2.0f );
        }
	}

    private Collider GetClosestHit()
    {
        // Detect objects
        RaycastHit[] hits;
        Vector3 dir = transform.localToWorldMatrix * Vector3.forward;

        hits = Physics.RaycastAll(transform.position, dir, RayLength);

        int i = 0;
        Collider closest = null;
        float distance = Mathf.Infinity;

        while (i < hits.Length)
        {
            RaycastHit hit = hits[i];

            //print("HIT : " + i + " : " + hit.collider.name);

            if( hit.distance < distance && hit.collider.name != "VRWand" && hit.collider.GetComponent<VRActor>() != null )
            {
                distance = hit.distance;
                closest = hit.collider;
            }

            i++;
        }

        return closest;
    }
	
    private void HighlightObject( GameObject obj, bool state )
    {
        HighlightObject(obj, state, HighlightColor);
    }

    private void HighlightObject( GameObject obj, bool state, Color hCol )
    {
        GameObject hobj = m_Ray;

        if (hobj != null && hobj.renderer != null && Highlight)
        {
            if( state )
            {
                hobj.renderer.material.color = hCol;
            }
            else
            {
                //m_CurrentObject.renderer.material.color = Color.white;
                hobj.renderer.material.color = Color.white;
            }
        }
    }

    private void Grab( GameObject iObject )
    {
        //MiddleVRTools.Log("Take :" + m_CurrentObject.name);

        m_ObjectInHand = iObject;
        m_ObjectInHand.transform.parent = transform.parent;

        if (m_ObjectInHand.rigidbody != null)
        {
            m_ObjectWasKinematic = m_ObjectInHand.rigidbody.isKinematic;
            m_ObjectInHand.rigidbody.isKinematic = true;
        }

        HighlightObject(m_ObjectInHand, true, GrabColor);
    }

    private void Ungrab()
    {
        //MiddleVRTools.Log("Release : " + m_ObjectInHand);

        m_ObjectInHand.transform.parent = null;

        if (m_ObjectInHand.rigidbody != null)
        {
            if (!m_ObjectWasKinematic)
                m_ObjectInHand.rigidbody.isKinematic = false;
        }

        m_ObjectInHand = null;

        HighlightObject(m_CurrentObject, false, HighlightColor);

        m_CurrentObject = null;
    }

	// Update is called once per frame
	void Update () {
        if (m_Buttons == null)
        {
            m_Buttons = MiddleVR.VRDeviceMgr.GetWandButtons();
        }
        
        if( m_Buttons == null )
        {
            if (m_SearchedButtons == false)
            {
                //MiddleVRTools.Log("[~] VRWandInteraction: Wand buttons undefined. Please specify Wand Buttons in the configuration tool.");
                m_SearchedButtons = true;
            }
        }

        Collider hit = GetClosestHit();

        if( hit != null )
        {
            //print("Closest : " + hit.name);

            if( m_CurrentObject != hit.gameObject &&  m_ObjectInHand == null )
            {
                //MiddleVRTools.Log("Enter other : " + hit.name);
                HighlightObject( m_CurrentObject, false );
                m_CurrentObject = hit.gameObject;
                HighlightObject(m_CurrentObject, true );
                //MiddleVRTools.Log("Current : " + m_CurrentObject.name);
            }
        }
        // Else
        else
        {
            //MiddleVRTools.Log("No touch ! ");

            if (m_CurrentObject != null && m_CurrentObject != m_ObjectInHand)
            {
                HighlightObject(m_CurrentObject, false, HighlightColor );
                m_CurrentObject = null;
            }
        }

        //MiddleVRTools.Log("Current : " + m_CurrentObject);

        if (m_Buttons != null && m_CurrentObject != null )
        {
            uint MainButton = MiddleVR.VRDeviceMgr.GetWandButton0();

            VRActor script = m_CurrentObject.GetComponent<VRActor>();

            //MiddleVRTools.Log("Trying to take :" + m_CurrentObject.name);
            if (script != null)
            {
                // Grab
                if (m_Buttons.IsToggled(MainButton))
                {
                    if (script.Grabable)
                    {
                        Grab(m_CurrentObject);
                    }
                }

                // Release
                if (m_Buttons.IsToggled(MainButton, false) && m_ObjectInHand != null)
                {
                    Ungrab();
                }

                // Action
                if (((!RepeatAction && m_Buttons.IsToggled(MainButton)) || (RepeatAction&& m_Buttons.IsPressed(MainButton))))
                {
                    m_CurrentObject.SendMessage("VRAction", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
	}
}
