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

            if (GUILayout.Button("Check Vertex Uniqueness", GUILayout.Height(24)))
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
            
            if (GUILayout.Button("Export to FBX", GUILayout.Height(32)))
            {
                ExportToFBX(asset.GetMesh(0));
            }
            
            var readable = GUILayout.Toggle(asset.GetMesh(0).isReadable, "IsReadable");
            if (readable != asset.GetMesh(0).isReadable)
            {
                ChangeReadable(readable);
            }
            
            Repaint();
        }

        private void ExportToFBX(Mesh p_mesh)
        {
            Type[] types = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == "Unity.Formats.Fbx.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetTypes();
            Type optionsInterfaceType = types.First(x => x.Name == "IExportOptions");
            Type optionsType = types.First(x => x.Name == "ExportOptionsSettingsSerializeBase");
            
            MethodInfo optionsProperty = typeof(ModelExporter).GetProperty("DefaultOptions", BindingFlags.Static | BindingFlags.NonPublic).GetGetMethod(true);
            object optionsInstance = optionsProperty.Invoke(null, null);
            
            FieldInfo exportFormatField = optionsType.GetField("exportFormat", BindingFlags.Instance | BindingFlags.NonPublic);
            exportFormatField.SetValue(optionsInstance, 1);
            
            MethodInfo exportObjectMethod = typeof(ModelExporter).GetMethod("ExportObject", BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new Type[] { typeof(string), typeof(Object), optionsInterfaceType }, null);


            var filter = new GameObject().AddComponent<MeshFilter>();
            filter.sharedMesh = Instantiate(p_mesh);
            var renderer = filter.gameObject.AddComponent<MeshRenderer>();
            var materials = new List<Material>();
            for (int i = 0; i < filter.sharedMesh.subMeshCount; i++)
            {
                materials.Add(new Material(Shader.Find("Standard")));
            }

            renderer.materials = materials.ToArray();


            var path = EditorUtility.SaveFilePanel("FBX", Application.dataPath, p_mesh.name, "fbx");
            if (path != null && path.Length > 0)
            {
                exportObjectMethod.Invoke(null, new object[] { path, filter.gameObject, optionsInstance });
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

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            Debug.Log("RenderStaticPreview: "+_thumbnailTexture);

            return _thumbnailTexture;
        }
    }
}