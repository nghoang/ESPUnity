/* VRCustomEditor
 * MiddleVR
 * (c) i'm in VR
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using MiddleVR_Unity3D;

[CustomEditor(typeof(VRManagerScript))]
public class VRCustomEditor : Editor
{
    //This will just be a shortcut to the target, ex: the object you clicked on.
    private VRManagerScript mgr;

    static private bool m_SettingsApplied = false;

    void Awake()
    {
        mgr = (VRManagerScript)target;

        if( !m_SettingsApplied ) 
        {
            ApplyVRSettings();
            m_SettingsApplied = true;
        }
    }

    public void ApplyVRSettings()
    {
        PlayerSettings.defaultIsFullScreen = false;
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
        PlayerSettings.runInBackground = true;
        PlayerSettings.captureSingleScreen = false;
        //PlayerSettings.usePlayerLog = false;

        QualitySettings.vSyncCount = 0;

        MiddleVRTools.Log("VR Player settings changed:");
        MiddleVRTools.Log("- DefaultIsFullScreen = false");
        MiddleVRTools.Log("- DisplayResolutionDialog = Disabled");
        MiddleVRTools.Log("- RunInBackground = true");
        MiddleVRTools.Log("- CaptureSingleScreen = false");
        //MiddleVRTools.Log("- UsePlayerLog = false");

#if UNITY_3_4
        MiddleVRTools.Log("Quality settings changed for the current quality level (" + QualitySettings.currentLevel + ") :" );
#else
        string[] names = QualitySettings.names;

        MiddleVRTools.Log("Quality settings changed for the current quality level (" + names[QualitySettings.GetQualityLevel()] + ") :");
#endif

        MiddleVRTools.Log("- VSyncCount = 0");
    }

    public override void OnInspectorGUI()
    {

        GUILayout.BeginVertical();

        if (GUILayout.Button("Re-apply VR player settings"))
        {
            ApplyVRSettings();
        }

        if (GUILayout.Button("Pick configuration file"))
        {
            string path = EditorUtility.OpenFilePanel("Please choose MiddleVR configuration file", "", "vrx");
            MiddleVRTools.Log("[+] Picked " + path );
            mgr.ConfigFile = path;
            EditorUtility.SetDirty(mgr);
        }

        DrawDefaultInspector();
        GUILayout.EndVertical();
        
    }
}