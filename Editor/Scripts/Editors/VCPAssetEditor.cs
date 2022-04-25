/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
        private Texture2D _thumbnailTexture;
        
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
            
            var readable = GUILayout.Toggle(asset.GetMesh(0).isReadable, "IsReadable");
            if (readable != asset.GetMesh(0).isReadable)
            {
                ChangeReadable(readable);
            }
            
            Repaint();
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

        void GeneratePreviewTexture()
        {
            if (target == null)
                return;
            
            var mesh = (target as VCPAsset).GetMesh(0);
            if (mesh == null)
                return;
            
            var texture = AssetPreview.GetAssetPreview((target as VCPAsset).GetMesh(0));
            
            if (texture == null)
                return;

            if (texture != null) 
            {
                Texture2D vcpTexture = Resources.Load("Textures/vcpasset") as Texture2D;
                Color[] colors1 = texture.GetPixels(0, 0, 128, 128);
                Color[] colors2 = vcpTexture.GetPixels(0, 0, 128, 128);
                Color[] colors = new Color[colors1.Length];
                for (int i = 0; i < colors.Length; i++)
                    colors[i] = new Color(Mathf.Lerp(colors1[i].r, colors2[i].r, colors2[i].a),
                        Mathf.Lerp(colors1[i].g, colors2[i].g, colors2[i].a),
                        Mathf.Lerp(colors1[i].b, colors2[i].b, colors2[i].a), 1);
                
                texture.SetPixels(colors);
                texture.Apply();
                _thumbnailTexture = texture;
            }
        }

        private void OnEnable()
        {
            Debug.Log("OnEnable");
            
            GeneratePreviewTexture();

            if (_thumbnailTexture == null)
            {
                Thread.Sleep(100);
                GeneratePreviewTexture();
            }
            
            Debug.Log("OnEnable2");
        }

        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        {
            Debug.Log("RenderStaticPreview: "+_thumbnailTexture);

            return _thumbnailTexture;
        }
    }
}