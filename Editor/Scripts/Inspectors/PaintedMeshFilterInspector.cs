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
    [CustomEditor(typeof(PaintedMeshFilter))]
    public class PaintedMeshFilterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            PaintedMeshFilter pmf = (target as PaintedMeshFilter);
            
            var style = new GUIStyle();
            style.normal.textColor = new Color(1, 0.5f, 0);
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 14;
            EditorGUILayout.LabelField("MeshFilter driven by internal painted data.", style);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Original: ", GUILayout.Width(70));
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Italic;
            style.alignment = TextAnchor.MiddleRight;
            EditorGUILayout.LabelField(pmf.fbxAssetPath, style);
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Go to original asset"))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Mesh>(pmf.fbxAssetPath));
            }

            if (GUILayout.Button("Open Reimport", GUILayout.Height(32)))
            {
                ReimportWindow.InitReimportWindow(pmf);
            }
        }
    }
}