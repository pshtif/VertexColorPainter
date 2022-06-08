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
        protected Color _pickedColor;
        
        public override void HandleMouseHitInternal(SceneView p_sceneView, RaycastHit p_hit, Transform p_hitTransform)
        {
            DrawHandle(p_hit);

            if (Event.current.button == 0 && !Event.current.alt && Event.current.type == EventType.MouseDown)
            {
                Undo.RegisterCompleteObjectUndo(VCPEditorCore.PaintedMesh, "Paint Color");
            }

            if (Event.current.control)
            {
                int index = MeshUtils.GetClosesVertexIndex(VCPEditorCore.PaintedMesh, VCPEditorCore.PaintedObject.transform.worldToLocalMatrix, p_hit);
                _pickedColor = VCPEditorCore.Cache.GetColorAtIndex(index, VCPEditorCore.Config.channelType);
            }
            else
            {
                _pickedColor = VCPEditorCore.Config.brushColor;
            }

            if (Event.current.button == 0 && !Event.current.alt && (Event.current.type == EventType.MouseDrag ||
                                                                    Event.current.type == EventType.MouseDown))
            {
                if (Event.current.control)
                {
                    VCPEditorCore.Config.brushColor = _pickedColor;
                }
                else
                {
                    Paint(p_hitTransform, p_hit);
                }
            }

            if (Event.current.button == 0 && !Event.current.alt && Event.current.type == EventType.MouseUp)
            {
                EditorUtility.SetDirty(VCPEditorCore.PaintedObject);
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
        
        void Paint(Transform p_hitTransform, RaycastHit p_hit)
        {
            if (VCPEditorCore.Cache.Vertices == null)
                return;
            
            var brushSize = HandleUtility.GetHandleSize(p_hit.point) / 10f * VCPEditorCore.Config.brushSize;
            int closestIndex = MeshUtils.GetClosesVertexIndex(VCPEditorCore.PaintedMesh, VCPEditorCore.PaintedObject.transform.worldToLocalMatrix, p_hit);

            if (VCPEditorCore.Config.lockToSubmesh)
            {
                SubMeshDescriptor desc = VCPEditorCore.PaintedMesh.GetSubMesh(_selectedSubmesh);
                
                for (int i = 0; i < desc.indexCount; i++)
                {
                    int index = VCPEditorCore.Cache.Indices[i + desc.indexStart];
                    if (Vector3.Distance(p_hitTransform.TransformPoint(VCPEditorCore.Cache.Vertices[index]), p_hit.point) <
                        brushSize || (VCPEditorCore.Config.enableClosestPaint && i+desc.indexStart == closestIndex))
                    {
                        VCPEditorCore.Cache.SetColorAtIndex(index, VCPEditorCore.Config.brushColor, VCPEditorCore.Config.channelType);
                    }
                }
            }
            else
            {
                for (int i = 0; i < VCPEditorCore.Cache.Vertices.Length; i++)
                {
                    if (Vector3.Distance(p_hitTransform.TransformPoint(VCPEditorCore.Cache.Vertices[i]), p_hit.point) <
                        brushSize || (VCPEditorCore.Config.enableClosestPaint && i == closestIndex))
                    {
                        VCPEditorCore.Cache.SetColorAtIndex(i, VCPEditorCore.Config.brushColor, VCPEditorCore.Config.channelType);
                    }
                }
            }

            VCPEditorCore.Cache.InvalidateMeshColors(VCPEditorCore.PaintedMesh, VCPEditorCore.Config.channelType);
        }
        
        public override void DrawSettingsGUI()
        {
            EditorGUILayout.LabelField("Paint Tool Settings", Skin.GetStyle("settingslabel"), GUILayout.Height(24));
                
            GUILayout.Space(4);
            
            HandleChannelSelection();
            
            VCPEditorCore.Config.brushColor = EditorGUILayout.ColorField("Brush Color", VCPEditorCore.Config.brushColor);
            VCPEditorCore.Config.brushSize = EditorGUILayout.Slider("Brush Size", VCPEditorCore.Config.brushSize,
                VCPEditorCore.Config.forcedMinBrushSize, VCPEditorCore.Config.forcedMaxBrushSize);
            
            if (VCPEditorCore.Cache.SubmeshColors.Count > 1)
            {
                VCPEditorCore.Config.lockToSubmesh =
                    EditorGUILayout.Toggle("Lock to Submesh", VCPEditorCore.Config.lockToSubmesh);

                if (VCPEditorCore.Config.lockToSubmesh)
                {
                    _selectedSubmesh =
                        EditorGUILayout.Popup("Submesh:", _selectedSubmesh, VCPEditorCore.Cache.SubmeshNames);
                }
            }

            VCPEditorCore.Config.enableClosestPaint = EditorGUILayout.Toggle(new GUIContent("Closest Vertex Painting",
                "If there are no vertices in the range of brush paint closest vertex outside of range."), VCPEditorCore.Config.enableClosestPaint);
        }

        public override void DrawSceneGUI(SceneView p_sceneView)
        {
            var space = 8;
            var style = new GUIStyle("label");
            style.fontStyle = FontStyle.Bold;
            
            GUILayout.Space(space);
            
            // GUILayout.Label("Brush Color: ", style, GUILayout.Width(80));
            // VCPEditorCore.Config.brushColor = EditorGUILayout.ColorField(VCPEditorCore.Config.brushColor, GUILayout.Width(60));
            //
            // GUILayout.Space(space);
            //
            // GUILayout.Label("Brush Size: ", style, GUILayout.Width(80));
            // VCPEditorCore.Config.brushSize =
            //     EditorGUILayout.Slider(VCPEditorCore.Config.brushSize, VCPEditorCore.Config.forcedMinBrushSize, VCPEditorCore.Config.forcedMaxBrushSize, GUILayout.Width(200));

            // GUILayout.Space(space);
            //
            // if (VCPEditorCore.Cache.SubmeshColors.Count > 1)
            // {
            //     GUILayout.Space(space);
            //
            //     GUILayout.Label("Lock to Submesh: ", style, GUILayout.Width(110));
            //     VCPEditorCore.Config.lockToSubmesh = EditorGUILayout.Toggle(VCPEditorCore.Config.lockToSubmesh, GUILayout.Width(20));
            //
            //     if (VCPEditorCore.Config.lockToSubmesh)
            //     {
            //         GUILayout.Label("Submesh: ", style, GUILayout.Width(65));
            //         _selectedSubmesh =
            //             EditorGUILayout.Popup(_selectedSubmesh, VCPEditorCore.Cache.SubmeshNames.ToArray(), GUILayout.Width(120));
            //     }
            // }
        }

        public override void DrawHelpGUI(SceneView p_sceneView)
        {
            var rect = p_sceneView.camera.GetScaledPixelRect();
            GUILayout.BeginArea(new Rect(rect.width / 2 - 500, rect.height-50, 1000, 30));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(" Left Mouse: ", VCPEditorCore.Skin.GetStyle("keylabel"), GUILayout.Height(16));
            GUILayout.Label("Paint vertex color ", VCPEditorCore.Skin.GetStyle("keyfunction"), GUILayout.Height(16));
            GUILayout.Space(8);
            
            GUILayout.Label(" Ctrl + Left Mouse: ", VCPEditorCore.Skin.GetStyle("keylabel"), GUILayout.Height(16));
            GUILayout.Label("Pick vertex color ", VCPEditorCore.Skin.GetStyle("keyfunction"), GUILayout.Height(16));
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}