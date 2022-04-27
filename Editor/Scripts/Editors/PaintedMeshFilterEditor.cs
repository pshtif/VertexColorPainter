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
    [Obsolete]
    [CustomEditor(typeof(PaintedMeshFilter))]
    public class PaintedMeshFilterEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            PaintedMeshFilter painted = (target as PaintedMeshFilter);
            painted.filter.hideFlags = HideFlags.None;
        }

        public override void OnInspectorGUI()
        {
            PaintedMeshFilter painted = (target as PaintedMeshFilter);
            
            var style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Italic;
            style.alignment = TextAnchor.MiddleRight;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Original: ", GUILayout.Width(70));

            EditorGUILayout.LabelField(painted.fbxAssetPath, style);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Save To VCPAsset"))
            {
                painted.filter.sharedMesh = VCPEditorCore.Instance.SaveToVCPAsset(painted.filter.sharedMesh, painted.fbxAssetPath);
                //DestroyImmediate(painted);
            }
        }
    }
}