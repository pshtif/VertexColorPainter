/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Rendering;

namespace VertexColorPainter.Editor
{
    public class PaintTool : ToolBase
    {
        public override void HandleMouseHitInternal(RaycastHit p_hit, Transform p_hitTransform)
        {
            DrawHandle(p_hit);

            if (Event.current.button == 0 && !Event.current.alt && Event.current.type == EventType.MouseDown)
            {
                Undo.RegisterCompleteObjectUndo(Core.PaintedMesh.sharedMesh, "Paint Color");
            }
            
            if (Event.current.button == 0 && !Event.current.alt && (Event.current.type == EventType.MouseDrag ||
                                                                    Event.current.type == EventType.MouseDown))
            {
                Paint(p_hitTransform, p_hit);
            }

            if (Event.current.button == 0 && !Event.current.alt && Event.current.type == EventType.MouseUp)
            {
                EditorUtility.SetDirty(Core.PaintedMesh);
            }
        }

        void DrawHandle(RaycastHit p_hit)
        {
            Handles.color = Color.white;
            var gizmoSize = HandleUtility.GetHandleSize(p_hit.point) / 10f;
            Handles.DrawSolidDisc(p_hit.point, p_hit.normal, gizmoSize * Core.Config.brushSize + gizmoSize / 5);
            Handles.color = Core.Config.brushColor;
            Handles.DrawSolidDisc(p_hit.point, p_hit.normal, gizmoSize * Core.Config.brushSize);
        }
        
        void Paint(Transform p_hitTransform, RaycastHit p_hit)
        {
            if (Core.CachedVertices == null)
                return;
            
            var brushSize = HandleUtility.GetHandleSize(p_hit.point) / 10f * Core.Config.brushSize;

            if (Core.Config.lockToSubmesh)
            {
                Mesh mesh = Core.PaintedMesh.sharedMesh;
                SubMeshDescriptor desc = mesh.GetSubMesh(_selectedSubmesh);
                
                for (int i = 0; i < desc.indexCount; i++)
                {
                    int index = Core.CachedIndices[i + desc.indexStart];
                    if (Vector3.Distance(p_hitTransform.TransformPoint(Core.CachedVertices[index]), p_hit.point) <
                        brushSize)
                    {
                        Core.CachedColors[index] = Core.Config.brushColor;
                    }
                }
            }
            else
            {
                for (int i = 0; i < Core.CachedVertices.Length; i++)
                {
                    if (Vector3.Distance(p_hitTransform.TransformPoint(Core.CachedVertices[i]), p_hit.point) <
                        brushSize)
                    {
                        Core.CachedColors[i] = Core.Config.brushColor;
                    }
                }
            }

            Core.PaintedMesh.sharedMesh.colors = Core.CachedColors;
        }

        public override void DrawGUI()
        {
            var space = 8;
            var style = new GUIStyle("label");
            style.fontStyle = FontStyle.Bold;
            
            GUILayout.Space(space);
            
            GUILayout.Label("Brush Color: ", style, GUILayout.Width(80));
            Core.Config.brushColor = EditorGUILayout.ColorField(Core.Config.brushColor, GUILayout.Width(60));

            GUILayout.Space(space);
            
            GUILayout.Label("Brush Size: ", style, GUILayout.Width(80));
            Core.Config.brushSize =
                EditorGUILayout.Slider(Core.Config.brushSize, Core.Config.forcedMinBrushSize, Core.Config.forcedMaxBrushSize, GUILayout.Width(200));

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
        }
    }
}