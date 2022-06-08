/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VertexColorPainter.Editor
{
    [CustomEditor(typeof(VCPAsset))]
    public class VCPAssetEditor : UnityEditor.Editor
    {
        public VCPAsset asset => target as VCPAsset;
        
        private Texture2D _thumbnailTexture;

        public override void OnInspectorGUI()
        {
            var style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Italic;
            style.alignment = TextAnchor.MiddleRight;

            var meshes = asset.GetMeshes();
            for (int i = 0; i<meshes.Length; i++)
            {
                DrawMeshGUI(meshes[i], i);
            }

            Repaint();
        }

        void DrawMeshGUI(Mesh p_mesh, int p_index)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(24));
            GUILayout.Label(p_mesh.name, GUILayout.Height(24), GUILayout.MaxWidth(200));
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Reimport", GUILayout.Height(24), GUILayout.Width(70)))
            {
                ReimportWindow.InitReimportWindow(asset, p_index);
            }
            
            if (GUILayout.Button("Export", GUILayout.Height(24), GUILayout.Width(60)))
            {
                FBXUtils.ExportToFBX(asset.GetMesh(p_index));
            }
            
            var readable = GUILayout.Toggle(p_mesh.isReadable, "Readable", GUILayout.Height(24));
            if (readable != p_mesh.isReadable)
            {
                ChangeReadable(readable);
            }
            GUILayout.EndHorizontal();
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
            GeneratePreviewTexture();

            if (_thumbnailTexture == null)
            {
                Thread.Sleep(100);
                GeneratePreviewTexture();
            }
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            //Debug.Log("RenderStaticPreview: "+_thumbnailTexture);

            return _thumbnailTexture;
        }
    }
}