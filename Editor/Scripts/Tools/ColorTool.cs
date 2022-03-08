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
        private int _selectedSubmesh;
        private Mesh _lastCachedMesh;
        private List<int> _cachedColorIndices;
        private Color _pickedColor;

        public override void HandleMouseHitInternal(RaycastHit p_hit, Transform p_hitTransform)
        {
            var mesh = Core.PaintedMesh.sharedMesh;
            
            if (_lastCachedMesh != mesh)
            {
                CacheColorIndices(Core.Config.colorChangeCurrent, mesh);
            }

            if (Event.current.control)
            {
                int index = MeshUtils.GetClosesVertexIndex(mesh, p_hit);
                _pickedColor = Core.CachedColors[index];
                int submesh = MeshUtils.GetSubMeshIndexFromTriangle(mesh, p_hit.triangleIndex);
                
                DrawHandle(p_hit);
                
                // Core.SelectionMaterial.SetPass(0);
                // Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix, submesh);

                if (Event.current.button == 0 && !Event.current.alt && (Event.current.type == EventType.MouseDrag ||
                                           Event.current.type == EventType.MouseDown))
                {
                    _selectedSubmesh = submesh;
                    Core.Config.colorChangeCurrent = _pickedColor;
                    CacheColorIndices(Core.Config.colorChangeCurrent, mesh);
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
        
        public override void DrawGUI()
        {
            var space = 8;
            
            if (Core.PaintedMesh != null && _lastCachedMesh != Core.PaintedMesh.sharedMesh)
            {
                CacheColorIndices(Core.Config.colorChangeCurrent, Core.PaintedMesh.sharedMesh);
            }
            
            var style = new GUIStyle("label");
            style.fontStyle = FontStyle.Bold;

            GUILayout.Space(space);
            
            GUILayout.Label("Current Color: ", style, GUILayout.Width(80));
            Core.Config.colorChangeCurrent = EditorGUILayout.ColorField(Core.Config.colorChangeCurrent, GUILayout.Width(60));
            
            GUILayout.Label("New Color: ", style, GUILayout.Width(80));
            EditorGUI.BeginChangeCheck();
            Core.Config.colorChangeNew = EditorGUILayout.ColorField(Core.Config.colorChangeNew, GUILayout.Width(60));

            if (EditorGUI.EndChangeCheck())
            {
                ChangeMeshColor(Core.PaintedMesh, Core.Config.lockToSubmesh ? _selectedSubmesh : -1);
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

        void ChangeMeshColor(MeshFilter p_meshFilter, int p_submeshIndex)
        {
            Undo.RegisterCompleteObjectUndo(p_meshFilter.sharedMesh, "Change Color");
            Mesh mesh = p_meshFilter.sharedMesh;

            int minIndex = 0;
            int maxIndex = mesh.vertexCount;
            if (p_submeshIndex >= 0)
            {
                var submesh = mesh.GetSubMesh(p_submeshIndex);
                minIndex = submesh.indexStart;
                maxIndex = submesh.indexStart + submesh.indexCount;
            }

            for (int i = 0; i < _cachedColorIndices.Count; i++)
            {
                if (p_submeshIndex >= 0 && (_cachedColorIndices[i] < minIndex || _cachedColorIndices[i] >= maxIndex))
                    continue;

                Core.CachedColors[_cachedColorIndices[i]] = Core.Config.colorChangeNew;
            }
            
            mesh.colors = Core.CachedColors;

            EditorUtility.SetDirty(p_meshFilter);
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
    }
}