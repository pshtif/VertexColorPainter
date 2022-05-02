/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using VertexColorPainter.Runtime;

namespace VertexColorPainter.Editor
{
    public class ReimportWindow : EditorWindow
    {
        private Mesh _importedMesh;
        
        private VCPAsset _vcpAsset;
        
        private ReorderableList _importedSubMeshList;
        
        public static ReimportWindow Instance { get; private set; }
        
        public static ReimportWindow InitReimportWindow(VCPAsset p_asset)
        {
            Instance = GetWindow<ReimportWindow>();
            Instance._vcpAsset = p_asset;
            Instance.titleContent = new GUIContent("Vertex Color Editor Reimport");
            Instance.minSize = new Vector2(800, p_asset.mesh == null ? 165 : p_asset.mesh.subMeshCount * 22 + 165);
            Instance.maxSize = new Vector2(800, p_asset.mesh == null ? 165 : p_asset.mesh.subMeshCount * 22 + 165);

            return Instance;
        }

        public void OnGUI()
        {
            var style = new GUIStyle();
            style.normal.textColor = new Color(1, 0.5f, 0);
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 18;

            GUILayout.Label("Reimport Painted Mesh", style, GUILayout.Height(24));
            
            EditorGUI.BeginChangeCheck();

            GUILayout.Space(2);
            _importedMesh = (Mesh)EditorGUILayout.ObjectField("New Mesh", _importedMesh, typeof(Mesh), false);
            GUILayout.Space(4);
            if (EditorGUI.EndChangeCheck())
            {
                RefreshList(MeshUtils.GetSubMeshDescriptors(_importedMesh));
            }
            
            var originalSubMeshes = MeshUtils.GetSubMeshDescriptors(_vcpAsset.mesh);
            var originalColors = MeshUtils.GetSubMeshColors(_vcpAsset.mesh);
            var importedSubMeshes = MeshUtils.GetSubMeshDescriptors(_importedMesh);

            style.fontSize = 12;
            style.normal.background = Texture2D.whiteTexture;
            style.fontStyle = FontStyle.Bold;
            GUI.backgroundColor = new Color(.1f, .1f, .1f);
            GUILayout.Label("SubMesh Reimport Mapping", style, GUILayout.Height(24));
            GUI.backgroundColor = Color.white;
            GUILayout.BeginHorizontal();
            
            // Labels
            GUILayout.BeginVertical(GUILayout.Width(100));
            style.alignment = TextAnchor.MiddleLeft;
            style.normal.background = null;
            GUILayout.Label(" SubMesh", style, GUILayout.Height(22));
            GUILayout.Space(6);
            for (int i = 0; i < Math.Max(originalSubMeshes.Length, importedSubMeshes.Length); i++)
            {
                GUILayout.Label("SubMesh #" + (i + 1), GUILayout.Height(21));
            }
            GUILayout.EndVertical();
            
            // Original "colored" geometry
            GUILayout.BeginVertical(GUILayout.Width(130));
            GUILayout.Label(" Original", style, GUILayout.Height(22));
            GUILayout.Space(6);
            for (int i = 0; i<originalSubMeshes.Length; i++)
            {
                GUILayout.Label(originalSubMeshes[i].vertexCount.ToString()+" verts", GUILayout.Height(21));
                
                GUI.color = originalColors[i];
                var rect = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(new Rect(rect.x + rect.width - 14, rect.y + 2, 14, 14), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
            GUILayout.EndVertical();
            
            // New imported geometry
            GUILayout.BeginVertical();
            if (_importedMesh != null)
            {
                GUI.backgroundColor = new Color(.18f, .18f, .18f);
                GUILayout.Label(" New", style, GUILayout.Height(22));
                GUI.backgroundColor = Color.white;
                _importedSubMeshList?.DoLayoutList();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            
            if (GUI.Button(new Rect(5, position.height-45, position.width/2-4, 40), "Cancel"))
            {
                Close();
            }

            if (_importedMesh != null)
            {
                GUI.color = new Color(1, .75f, .5f);
            }
            else
            {
                GUI.enabled = false;
            }

            if (GUI.Button(new Rect(5+position.width/2, position.height-45, position.width/2-8, 40), "Reimport"))
            {
                Reimport();
                
                Close();
            }

            GUI.color = Color.white;
            GUI.enabled = false;
        }
        
        private void RefreshList(SubMeshDescriptor[] p_subMeshes)
        {
            _importedSubMeshList = new ReorderableList(p_subMeshes.ToList(), typeof(SubMeshDescriptor), true, false, false, false);
            
            _importedSubMeshList.drawElementCallback += DrawListItems;

            var count = Math.Max(_vcpAsset.mesh == null ? 0 : _vcpAsset.mesh.subMeshCount, p_subMeshes.Length);
            
            Instance.minSize = new Vector2(800, count * 22 + 165);
            Instance.maxSize = new Vector2(800, count * 22 + 165);
        }

        private void DrawListItems(Rect p_rect, int p_index, bool p_isActive, bool p_isFocused)
        {
            SubMeshDescriptor item = (SubMeshDescriptor)_importedSubMeshList.list[p_index];
            
            //EditorGUI.PropertyField(new Rect(p_rect.x, p_rect.y, 100, EditorGUIUtility.singleLineHeight), , GUIContent.none);
            EditorGUI.LabelField(new Rect(p_rect.x, p_rect.y, 100, EditorGUIUtility.singleLineHeight), item.vertexCount+" verts");
        }

        private void Reimport()
        {
            var originalColors = MeshUtils.GetSubMeshColors(_vcpAsset.mesh);
            
            var newMesh = Instantiate(_importedMesh);
            newMesh.name = _vcpAsset.name;
            var colors =  newMesh.colors;
            if (colors.Length == 0) colors = new Color[newMesh.vertexCount];
            
            for (int i = 0; i < _importedSubMeshList.list.Count; i++)
            {
                for (int j = 0; j < ((SubMeshDescriptor)_importedSubMeshList.list[i]).indexCount; j++)
                {
                    int index = _importedMesh.triangles[((SubMeshDescriptor)_importedSubMeshList.list[i]).indexStart + j];
                    colors[index] = i>=originalColors.Length ? Color.white : originalColors[i];
                }
            }
            newMesh.colors = colors;

            // If we don't have original mesh (can be malformed)
            if (_vcpAsset.mesh == null)
            {
                var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(_vcpAsset));
                foreach (var asset in subAssets)
                {
                    if (asset == null)
                        continue;
                    
                    AssetDatabase.RemoveObjectFromAsset(asset);
                }

                AssetDatabase.SaveAssets();

                _vcpAsset.mesh = newMesh;

                AssetDatabase.AddObjectToAsset(newMesh, _vcpAsset);
                AssetDatabase.SaveAssets();
            }
            // If we have original mesh we don't want to create new asset it would break references to it already made
            else
            {
                _vcpAsset.mesh.vertices = newMesh.vertices;
                _vcpAsset.mesh.bindposes = newMesh.bindposes;
                _vcpAsset.mesh.normals = newMesh.normals;
                _vcpAsset.mesh.colors = newMesh.colors;
                _vcpAsset.mesh.colors32 = newMesh.colors32;
                _vcpAsset.mesh.triangles = newMesh.triangles;
                _vcpAsset.mesh.uv = newMesh.uv;
                _vcpAsset.mesh.uv2 = newMesh.uv2;
                for (int i = 0; i < newMesh.subMeshCount; i++)
                {
                    _vcpAsset.mesh.subMeshCount = newMesh.subMeshCount;
                    _vcpAsset.mesh.SetSubMesh(i, newMesh.GetSubMesh(i));
                }
            }
        }
    }
}