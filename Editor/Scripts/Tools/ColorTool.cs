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
            if (_lastCachedMesh != Core.PaintedMesh)
            {
                CacheColorIndices(Core.Config.colorChangeCurrent, Core.PaintedMesh);
            }

            if (Event.current.control)
            {
                int index = MeshUtils.GetClosesVertexIndex(Core.PaintedMesh, Core.PaintedObject.transform.worldToLocalMatrix, p_hit);
                _pickedColor = Core.GetColorAtIndex(index);
                int submesh = MeshUtils.GetSubMeshIndexFromTriangle(Core.PaintedMesh, p_hit.triangleIndex);
                
                DrawHandle(p_hit);
                
                // Core.SelectionMaterial.SetPass(0);
                // Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix, submesh);

                if (Event.current.button == 0 && !Event.current.alt && (Event.current.type == EventType.MouseDrag ||
                                           Event.current.type == EventType.MouseDown))
                {
                    _selectedSubmesh = submesh;
                    Core.Config.colorChangeCurrent = _pickedColor;
                    CacheColorIndices(Core.Config.colorChangeCurrent, Core.PaintedMesh);
                }
            }
        }
        
        void DrawHandle(RaycastHit p_hit)
        {
            Handles.color = Color.white;
            var gizmoSize = HandleUtility.GetHandleSize(p_hit.point) / 10f;
            Handles.DrawSolidDisc(p_hit.point, p_hit.normal, gizmoSize * Core.Config.brushSize + gizmoSize / 5);
            Handles.color = _pickedColor;
            Handles.DrawSolidDisc(p_hit.point, p_hit.normal, gizmoSize * Core.Config.brushSize);
        }
        
        public override void DrawGUI(SceneView p_sceneView)
        {
            var space = 8;
            
            if (Core.PaintedObject != null && _lastCachedMesh != Core.PaintedMesh)
            {
                CacheColorIndices(Core.Config.colorChangeCurrent, Core.PaintedMesh);
            }
            
            var style = new GUIStyle("label");
            style.fontStyle = FontStyle.Bold;

            GUILayout.Space(space);
            
            GUILayout.Label("Current Color: ", style, GUILayout.Width(85));
            Core.Config.colorChangeCurrent = EditorGUILayout.ColorField(Core.Config.colorChangeCurrent, GUILayout.Width(60));
            GUILayout.Space(8);
            
            GUILayout.Label("New Color: ", style, GUILayout.Width(67));
            EditorGUI.BeginChangeCheck();
            Core.Config.colorChangeNew = EditorGUILayout.ColorField(Core.Config.colorChangeNew, GUILayout.Width(60));

            if (EditorGUI.EndChangeCheck())
            {
                ChangeMeshColor(Core.Config.lockToSubmesh ? _selectedSubmesh : -1);
            }
            
            GUILayout.Space(space);

            if (Core.SubmeshColors.Count > 1)
            {
                GUILayout.Space(space);

                GUILayout.Label("Lock to Submesh: ", style, GUILayout.Width(110));
                Core.Config.lockToSubmesh = EditorGUILayout.Toggle(Core.Config.lockToSubmesh, GUILayout.Width(20));

                if (Core.Config.lockToSubmesh)
                {
                    GUILayout.Label("Submesh: ", style, GUILayout.Width(65));
                    _selectedSubmesh =
                        EditorGUILayout.Popup(_selectedSubmesh, Core.SubmeshNames.ToArray(), GUILayout.Width(120));
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
            Undo.RegisterCompleteObjectUndo(Core.PaintedMesh, "Change Color");

            int minIndex = 0;
            int maxIndex = Core.PaintedMesh.vertexCount;
            if (p_submeshIndex >= 0)
            {
                var submesh = Core.PaintedMesh.GetSubMesh(p_submeshIndex);
                minIndex = submesh.indexStart;
                maxIndex = submesh.indexStart + submesh.indexCount;
            }

            for (int i = 0; i < _cachedColorIndices.Count; i++)
            {
                if (p_submeshIndex >= 0 && (_cachedColorIndices[i] < minIndex || _cachedColorIndices[i] >= maxIndex))
                    continue;

                Core.SetColorAtIndex(_cachedColorIndices[i], Core.Config.colorChangeNew);
            }
            
            Core.InvalidateMeshColors();

            EditorUtility.SetDirty(Core.PaintedObject);
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

            GUILayout.Label(" Ctrl + Left Mouse: ", Core.Skin.GetStyle("keylabel"), GUILayout.Height(16));
            GUILayout.Label("Pick vertex color ", Core.Skin.GetStyle("keyfunction"), GUILayout.Height(16));
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}