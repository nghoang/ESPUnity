/* VRCameraCB
 * MiddleVR
 * (c) i'm in VR
 */

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class VRCameraCB : MonoBehaviour {
	[DllImport ("RenderingPlugin")]
	private static extern void InitRenderPlugin();
	
	void Start()
	{
        MiddleVR.VRLog(2, "[ ] Initializing RenderPlugin" );
        InitRenderPlugin();
	}
		
	void OnPostRender()
	{
		MiddleVR.VRLog(3,"[ ] PostRender : " + name );
		vrCamera cam = MiddleVR.VRDisplayMgr.GetCamera( name );
		
		int id = -1;
		
		if( cam != null )
		{
			id = (int)cam.GetId();
		}
		
		GL.IssuePluginEvent (id);
	}
}
