/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VertexColorPainter.Editor
{
    public class FillTool
    {
        public static VertexColorPainterEditorConfig Config => VertexColorPainterEditorCore.Config;
        
        private static int _selectedSubmesh;
        
        public static void Handle(RaycastHit p_hit, MeshFilter p_meshFilter)
        {
            if (Event.current.control)
            {
                int submesh = MeshUtils.GetSubMeshIndexFromTriangle(p_meshFilter.sharedMesh, p_hit.triangleIndex);
                
                VertexColorPainterEditorCore.SelectionMaterial.SetPass(0);
                Graphics.DrawMeshNow(p_meshFilter.sharedMesh, p_meshFilter.transform.localToWorldMatrix, submesh);

                if (Event.current.button == 0 && !Event.current.alt && (Event.current.type == EventType.MouseDrag ||
                                           Event.current.type == EventType.MouseDown))
                {
                    
                    _selectedSubmesh = submesh;
                    Config.brushColor = VertexColorPainterEditorCore.SubmeshColors[_selectedSubmesh];
                }

                if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
                {
                    _selectedSubmesh = submesh;
                    Config.brushColor = VertexColorPainterEditorCore.SubmeshColors[_selectedSubmesh];
                    var color = VertexColorPainterEditorCore.SubmeshColors[_selectedSubmesh];
                    ColorPickerWrapper.Show(c => OnColorChange(p_meshFilter, c), color);
                }
            } else {
                var gizmoSize = HandleUtility.GetHandleSize(p_hit.point) / 10f;
                Handles.color = Color.white;
                //Handles.CircleHandleCap(2, _mousePosition, rotation, _minBrushSize+_minBrushSize/10f, EventType.Repaint);
                Handles.DrawSolidDisc(p_hit.point, p_hit.normal, gizmoSize + gizmoSize / 5);
                Handles.color = Config.brushColor;
                Handles.DrawSolidDisc(p_hit.point, p_hit.normal, gizmoSize);

                if (Event.current.button == 0 && !Event.current.alt && (Event.current.type == EventType.MouseDrag ||
                                                                        Event.current.type == EventType.MouseDown))
                {
                    FillColorOnHit(p_hit, p_meshFilter);
                }
            }
        }

        static void OnColorChange(MeshFilter p_meshFilter, Color p_color)
        {
            Config.brushColor = p_color;
            FillSubMeshColor(_selectedSubmesh, p_meshFilter);
        }
        
        static void FillColorOnHit(RaycastHit p_hit, MeshFilter p_meshFilter)
        {
            Undo.RegisterCompleteObjectUndo(p_meshFilter.sharedMesh, "Fill Color");
            Mesh mesh = p_meshFilter.sharedMesh;

            if (mesh.subMeshCount > 1)
            {
                int firstVertexIndex = p_hit.triangleIndex * 3;
                int submeshIndex = MeshUtils.GetSubMeshFromVertexIndex(mesh, firstVertexIndex);
                
                SubMeshDescriptor desc = mesh.GetSubMesh(submeshIndex);

                for (int j = 0; j < desc.indexCount; j++)
                {
                    VertexColorPainterEditorCore.CachedColors[mesh.triangles[desc.indexStart + j]] = Config.brushColor;
                }
            }
            else
            {
                VertexColorPainterEditorCore.CachedColors = Enumerable.Repeat(Config.brushColor, VertexColorPainterEditorCore.CachedColors.Length).ToArray();
            }
            
            mesh.colors = VertexColorPainterEditorCore.CachedColors;

            EditorUtility.SetDirty(p_meshFilter);
        }
        
        static void FillSubMeshColor(int p_submeshIndex, MeshFilter p_meshFilter)
        {
            Undo.RegisterCompleteObjectUndo(p_meshFilter.sharedMesh, "Fill Color");
            Mesh mesh = p_meshFilter.sharedMesh;

            if (mesh.subMeshCount > 1)
            {
                SubMeshDescriptor desc = mesh.GetSubMesh(p_submeshIndex);

                for (int j = 0; j < desc.indexCount; j++)
                {
                    VertexColorPainterEditorCore.CachedColors[mesh.triangles[desc.indexStart + j]] = Config.brushColor;
                }
            }
            else
            {
                VertexColorPainterEditorCore.CachedColors = Enumerable.Repeat(Config.brushColor, VertexColorPainterEditorCore.CachedColors.Length).ToArray();
            }
            
            mesh.colors = VertexColorPainterEditorCore.CachedColors;

            EditorUtility.SetDirty(p_meshFilter);
        }
        
        static public void DrawGUI(float p_space, MeshFilter p_meshFilter)
        {
            var style = new GUIStyle("label");
            style.fontStyle = FontStyle.Bold;

            GUILayout.Space(p_space);
            
            GUILayout.Label("Brush Color: ", style, GUILayout.Width(80));
            EditorGUI.BeginChangeCheck();
            Config.brushColor = EditorGUILayout.ColorField(Config.brushColor, GUILayout.Width(60));

            if (EditorGUI.EndChangeCheck())
            {
                if (Config.autoFill)
                {
                    FillSubMeshColor(_selectedSubmesh, p_meshFilter);
                }
            }

            GUILayout.Space(p_space);

            GUILayout.Label("Autofill", style, GUILayout.Width(60));
            Config.autoFill = EditorGUILayout.Toggle(Config.autoFill, GUILayout.Width(20));

            if (VertexColorPainterEditorCore.SubmeshColors.Count > 1 && Config.autoFill)
            {
                GUILayout.Label("Submesh: ", style, GUILayout.Width(65));

                if (GUILayout.Button(VertexColorPainterEditorCore.SubmeshNames[_selectedSubmesh]))
                {
                    SubMeshList.Show(p_meshFilter.sharedMesh, Event.current.mousePosition, i =>
                    {
                        Config.brushColor = VertexColorPainterEditorCore.SubmeshColors[i];
                        _selectedSubmesh = i;
                    });
                }
            }
        }
    }
}