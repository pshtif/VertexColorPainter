/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VertexColorPainter.Editor
{
    public class ColorTool : ToolBase
    {
        private Mesh _lastCachedMesh;
        private List<int> _cachedColorIndices;
        private Color _pickedColor;

        public override void HandleMouseHitInternal(SceneView p_sceneView, RaycastHit p_hit, Transform p_hitTransform)
        {
            if (_lastCachedMesh != VCPEditorCore.PaintedMesh)
            {
                CacheColorIndices(VCPEditorCore.Config.colorChangeCurrent, VCPEditorCore.PaintedMesh);
            }

            if (Event.current.control)
            {
                int index = MeshUtils.GetClosesVertexIndex(VCPEditorCore.PaintedMesh, VCPEditorCore.PaintedObject.transform.worldToLocalMatrix, p_hit);
                _pickedColor = VCPEditorCore.Cache.GetColorAtIndex(index, VCPEditorCore.Config.channelType);
                int submesh = MeshUtils.GetSubMeshIndexFromTriangle(VCPEditorCore.PaintedMesh, p_hit.triangleIndex);
                
                DrawHandle(p_hit);
                
                // VCPEditorCore.SelectionMaterial.SetPass(0);
                // Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix, submesh);

                if (Event.current.button == 0 && !Event.current.alt && (Event.current.type == EventType.MouseDrag ||
                                           Event.current.type == EventType.MouseDown))
                {
                    _selectedSubmesh = submesh;
                    VCPEditorCore.Config.colorChangeCurrent = _pickedColor;
                    CacheColorIndices(VCPEditorCore.Config.colorChangeCurrent, VCPEditorCore.PaintedMesh);
                }
            }
        }
        
        void DrawHandle(RaycastHit p_hit)
        {
            Handles.color = Color.white;
            var gizmoSize = HandleUtility.GetHandleSize(p_hit.point) / 10f;
            Handles.DrawSolidDisc(p_hit.point, p_hit.normal, gizmoSize * VCPEditorCore.Config.brushSize + gizmoSize / 5);
            Handles.color = _pickedColor;
            Handles.DrawSolidDisc(p_hit.point, p_hit.normal, gizmoSize * VCPEditorCore.Config.brushSize);
        }

        public override void DrawSettingsGUI()
        {
            EditorGUILayout.LabelField("Color Tool Settings", Skin.GetStyle("settingslabel"), GUILayout.Height(24));
                
            GUILayout.Space(4);
            
            HandleChannelSelection();
            
            VCPEditorCore.Config.colorChangeCurrent = EditorGUILayout.ColorField("Current Color", VCPEditorCore.Config.colorChangeCurrent);
            
            EditorGUI.BeginChangeCheck();
            VCPEditorCore.Config.colorChangeNew = EditorGUILayout.ColorField("New Color", VCPEditorCore.Config.colorChangeNew);
            
            if (EditorGUI.EndChangeCheck())
            {
                ChangeMeshColor(VCPEditorCore.Config.lockToSubmesh ? _selectedSubmesh : -1);
            }
            
            if (VCPEditorCore.Cache.SubmeshColors.Count > 1)
            {
                VCPEditorCore.Config.lockToSubmesh = EditorGUILayout.Toggle("Lock to Submesh",VCPEditorCore.Config.lockToSubmesh);

                if (VCPEditorCore.Config.lockToSubmesh)
                {
                    _selectedSubmesh = EditorGUILayout.Popup("Submesh", _selectedSubmesh, VCPEditorCore.Cache.SubmeshNames);
                }
            }

            if (GUILayout.Button("Color Editor"))
            {
                ColorWindow.InitWindow();
            }
        }

        public override void DrawSceneGUI(SceneView p_sceneView)
        {
            var space = 8;
            
            if (VCPEditorCore.PaintedObject != null && _lastCachedMesh != VCPEditorCore.PaintedMesh)
            {
                CacheColorIndices(VCPEditorCore.Config.colorChangeCurrent, VCPEditorCore.PaintedMesh);
            }
            
            var style = new GUIStyle("label");
            style.fontStyle = FontStyle.Bold;

            GUILayout.Space(space);
            
            // GUILayout.Label("Current Color: ", style, GUILayout.Width(85));
            // VCPEditorCore.Config.colorChangeCurrent = EditorGUILayout.ColorField(VCPEditorCore.Config.colorChangeCurrent, GUILayout.Width(60));
            // GUILayout.Space(8);
            //
            // GUILayout.Label("New Color: ", style, GUILayout.Width(67));
            // EditorGUI.BeginChangeCheck();
            // VCPEditorCore.Config.colorChangeNew = EditorGUILayout.ColorField(VCPEditorCore.Config.colorChangeNew, GUILayout.Width(60));

            // if (EditorGUI.EndChangeCheck())
            // {
            //     ChangeMeshColor(VCPEditorCore.Config.lockToSubmesh ? _selectedSubmesh : -1);
            // }
            
            GUILayout.Space(space);

            if (VCPEditorCore.Cache.SubmeshColors.Count > 1)
            {
                GUILayout.Space(space);

                GUILayout.Label("Lock to Submesh: ", style, GUILayout.Width(110));
                VCPEditorCore.Config.lockToSubmesh = EditorGUILayout.Toggle(VCPEditorCore.Config.lockToSubmesh, GUILayout.Width(20));

                if (VCPEditorCore.Config.lockToSubmesh)
                {
                    GUILayout.Label("Submesh: ", style, GUILayout.Width(65));
                    _selectedSubmesh =
                        EditorGUILayout.Popup(_selectedSubmesh, VCPEditorCore.Cache.SubmeshNames.ToArray(), GUILayout.Width(120));
                }
            }

            if (GUILayout.Button("Color Editor"))
            {
                ColorWindow.InitWindow();
            }
        }

        void CacheColorIndices(Color p_color, Mesh p_mesh)
        {
            _cachedColorIndices = GetColorIndices(p_color, p_mesh);
            _lastCachedMesh = p_mesh;
        }

        void ChangeMeshColor(int p_submeshIndex)
        {
            Undo.RegisterCompleteObjectUndo(VCPEditorCore.PaintedMesh, "Change Color");

            int minIndex = 0;
            int maxIndex = VCPEditorCore.PaintedMesh.vertexCount;
            if (p_submeshIndex >= 0)
            {
                var submesh = VCPEditorCore.PaintedMesh.GetSubMesh(p_submeshIndex);
                minIndex = submesh.indexStart;
                maxIndex = submesh.indexStart + submesh.indexCount;
            }

            for (int i = 0; i < _cachedColorIndices.Count; i++)
            {
                if (p_submeshIndex >= 0 && (_cachedColorIndices[i] < minIndex || _cachedColorIndices[i] >= maxIndex))
                    continue;

                VCPEditorCore.Cache.SetColorAtIndex(_cachedColorIndices[i], VCPEditorCore.Config.colorChangeNew, VCPEditorCore.Config.channelType);
            }
            
            VCPEditorCore.Cache.InvalidateMeshColors(VCPEditorCore.PaintedMesh, VCPEditorCore.Config.channelType);

            EditorUtility.SetDirty(VCPEditorCore.PaintedObject);
        }

        private List<int> GetColorIndices(Color p_color, Mesh p_mesh)
        {
            List<int> colorIndices = new List<int>();

            if (p_mesh == null)
                return colorIndices;

            var colors = p_mesh.colors;
            for (int i = 0; i < p_mesh.vertexCount; i++)
            {
                if (colors[i] == p_color)
                    colorIndices.Add(i);
            }
            
            return colorIndices;
        } 
        
        public override void DrawHelpGUI(SceneView p_sceneView)
        {
            // Help
            var rect = p_sceneView.camera.GetScaledPixelRect();
            GUILayout.BeginArea(new Rect(rect.width / 2 - 500, rect.height-50, 1000, 30));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(" Ctrl + Left Mouse: ", VCPEditorCore.Skin.GetStyle("keylabel"), GUILayout.Height(16));
            GUILayout.Label("Pick vertex color ", VCPEditorCore.Skin.GetStyle("keyfunction"), GUILayout.Height(16));
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}