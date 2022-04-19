/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using VertexColorPainter.Runtime;

namespace VertexColorPainter.Editor
{
    [CustomEditor(typeof(VCPAsset))]
    public class VCPAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            VCPAsset asset = (target as VCPAsset);
            
            var style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Italic;
            style.alignment = TextAnchor.MiddleRight;

            //EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField("Original: ", GUILayout.Width(70));

            // EditorGUILayout.LabelField(asset.fbxAssetPath, style);
            // EditorGUILayout.EndHorizontal();
            //
            // if (GUILayout.Button("Go to original asset"))
            // {
            //     EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Mesh>(asset.fbxAssetPath));
            // }

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
            
            if (GUILayout.Button("Reset UV1", GUILayout.Height(32)))
            {
                asset.mesh.SetUVs(1, Enumerable.Repeat(Vector4.one, asset.mesh.vertexCount).ToList());
            }

            var readable = GUILayout.Toggle(asset.mesh.isReadable, "IsReadable");
            if (readable != asset.mesh.isReadable)
            {
                ChangeReadable(readable);
            }
        }

        private void ChangeReadable(bool p_readable)
        {
            var assetPath = AssetDatabase.GetAssetPath(target);
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
            filePath = filePath.Replace("/", "\\");
            string fileText = File.ReadAllText(filePath);
            if (p_readable)
            {
                fileText = fileText.Replace("m_IsReadable: 0", "m_IsReadable: 1");
            }
            else
            {
                fileText = fileText.Replace("m_IsReadable: 1", "m_IsReadable: 0");
            }
            Debug.Log(fileText);
            File.WriteAllText(filePath, fileText);
            AssetDatabase.Refresh();
        }
    }
}