using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NFCSpoofer))]
public class NFCSpooferEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NFCSpoofer spoofer = (NFCSpoofer)target;

        if(GUILayout.Button("Spoof data") && EditorApplication.isPlaying)
        {
            spoofer.SpoofNFCData();
        }
    }
}
