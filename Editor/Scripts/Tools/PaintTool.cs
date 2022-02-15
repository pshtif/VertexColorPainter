/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VertexColorPainter.Editor
{
    public class PaintTool
    {
        public static VertexColorPainterEditorConfig Config => VertexColorPainterEditorCore.Config;

        private static int _selectedSubmesh;
        
        public static void Handle(Transform p_hitTransform, RaycastHit p_hit, MeshFilter p_meshFilter, int p_subMeshIndex)
        {
            Handles.color = Color.white;
            var gizmoSize = HandleUtility.GetHandleSize(p_hit.point) / 10f;
            Handles.DrawSolidDisc(p_hit.point, p_hit.normal, gizmoSize * Config.brushSize + gizmoSize / 5);
            Handles.color = Config.brushColor;
            Handles.DrawSolidDisc(p_hit.point, p_hit.normal, gizmoSize * Config.brushSize);

            if (Event.current.button == 0 && !Event.current.alt && Event.current.type == EventType.MouseDown)
            {
                Undo.RegisterCompleteObjectUndo(p_meshFilter.sharedMesh, "Paint Color");
            }
            
            if (Event.current.button == 0 && !Event.current.alt && (Event.current.type == EventType.MouseDrag ||
                                                                    Event.current.type == EventType.MouseDown))
            {
                Paint(p_hitTransform, p_hit, p_meshFilter, p_subMeshIndex);
            }

            if (Event.current.button == 0 && !Event.current.alt && Event.current.type == EventType.MouseUp)
            {
                EditorUtility.SetDirty(p_meshFilter);
            }
        }
        
        static void Paint(Transform p_hitTransform, RaycastHit p_hit, MeshFilter p_meshFilter, int p_subMeshIndex)
        {
            if (VertexColorPainterEditorCore.CachedVertices == null)
                return;
            
            var brushSize = HandleUtility.GetHandleSize(p_hit.point) / 10f * Config.brushSize;

            if (Config.lockToSubmesh)
            {
                Mesh mesh = p_meshFilter.sharedMesh;
                SubMeshDescriptor desc = mesh.GetSubMesh(p_subMeshIndex);
                
                for (int i = 0; i < desc.indexCount; i++)
                {
                    int index = VertexColorPainterEditorCore.CachedIndices[i + desc.indexStart];
                    if (Vector3.Distance(p_hitTransform.TransformPoint(VertexColorPainterEditorCore.CachedVertices[index]), p_hit.point) <
                        brushSize)
                    {
                        VertexColorPainterEditorCore.CachedColors[index] = Config.brushColor;
                    }
                }
            }
            else
            {
                for (int i = 0; i < VertexColorPainterEditorCore.CachedVertices.Length; i++)
                {
                    if (Vector3.Distance(p_hitTransform.TransformPoint(VertexColorPainterEditorCore.CachedVertices[i]), p_hit.point) <
                        brushSize)
                    {
                        VertexColorPainterEditorCore.CachedColors[i] = Config.brushColor;
                    }
                }
            }

            p_meshFilter.sharedMesh.colors = VertexColorPainterEditorCore.CachedColors;
        }

        static public void DrawGUI(float p_space)
        {
            var style = new GUIStyle("label");
            style.fontStyle = FontStyle.Bold;
            
            GUILayout.Space(p_space);
            
            GUILayout.Label("Brush Color: ", style, GUILayout.Width(80));
            Config.brushColor = EditorGUILayout.ColorField(Config.brushColor, GUILayout.Width(60));

            GUILayout.Space(p_space);
            
            GUILayout.Label("Brush Size: ", style, GUILayout.Width(80));
            Config.brushSize =
                EditorGUILayout.Slider(Config.brushSize, Config.forcedMinBrushSize, Config.forcedMaxBrushSize, GUILayout.Width(200));

            GUILayout.Space(p_space);

            if (VertexColorPainterEditorCore.SubmeshColors.Count > 1)
            {
                GUILayout.Space(p_space);

                GUILayout.Label("Lock to Submesh: ", style, GUILayout.Width(110));
                Config.lockToSubmesh = EditorGUILayout.Toggle(Config.lockToSubmesh, GUILayout.Width(20));

                if (Config.lockToSubmesh)
                {
                    GUILayout.Label("Submesh: ", style, GUILayout.Width(65));
                    _selectedSubmesh =
                        EditorGUILayout.Popup(_selectedSubmesh, VertexColorPainterEditorCore.SubmeshNames.ToArray(), GUILayout.Width(120));
                }
            }
        }
    }
}