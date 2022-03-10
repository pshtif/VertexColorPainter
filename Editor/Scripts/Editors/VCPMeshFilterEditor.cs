/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using VertexColorPainter.Runtime;

namespace VertexColorPainter.Editor
{
    [CustomEditor(typeof(VCPAsset))]
    public class VCPMeshFilterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            VCPAsset asset = (target as VCPAsset);
            
            var style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Italic;
            style.alignment = TextAnchor.MiddleRight;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Original: ", GUILayout.Width(70));

            EditorGUILayout.LabelField(asset.fbxAssetPath, style);
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Go to original asset"))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Mesh>(asset.fbxAssetPath));
            }

            if (GUILayout.Button("Check Vertex Uniqueness"))
            {
                if (MeshUtils.CheckVertexUniqueness(asset.mesh))
                {
                    EditorUtility.DisplayDialog("Vertex check",
                        "Vertices on mesh " + asset.mesh.name + " are unique.", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Vertex check",
                        "Vertices on mesh " + asset.mesh.name + " are NOT unique.", "OK");
                }
            }
            
            if (GUILayout.Button("Open Reimport", GUILayout.Height(32)))
            {
                ReimportWindow.InitReimportWindow(asset);
            }
        }
    }
}