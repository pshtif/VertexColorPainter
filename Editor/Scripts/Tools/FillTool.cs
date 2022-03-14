/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VertexColorPainter.Editor
{
    public class FillTool : ToolBase
    {
        public override void HandleMouseHitInternal(RaycastHit p_hit, Transform p_hitTransform)
        {
            var meshFilter = Core.PaintedMesh;
            if (Event.current.control)
            {
                int submesh = MeshUtils.GetSubMeshIndexFromTriangle(meshFilter.sharedMesh, p_hit.triangleIndex);
                
                Core.SelectionMaterial.SetPass(0);
                Graphics.DrawMeshNow(meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix, submesh);

                if (Event.current.button == 0 && !Event.current.alt && (Event.current.type == EventType.MouseDrag ||
                                           Event.current.type == EventType.MouseDown))
                {
                    
                    _selectedSubmesh = submesh;
                    Core.Config.brushColor = Core.SubmeshColors[_selectedSubmesh];
                }

                if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
                {
                    _selectedSubmesh = submesh;
                    Core.Config.brushColor = Core.SubmeshColors[_selectedSubmesh];
                    var color = Core.SubmeshColors[_selectedSubmesh];
                    ColorPickerWrapper.Show(c => OnColorChange(meshFilter, c), color);
                }
            } else {
                var gizmoSize = HandleUtility.GetHandleSize(p_hit.point) / 10f;
                Handles.color = Color.white;
                //Handles.CircleHandleCap(2, _mousePosition, rotation, _minBrushSize+_minBrushSize/10f, EventType.Repaint);
                Handles.DrawSolidDisc(p_hit.point, p_hit.normal, gizmoSize + gizmoSize / 5);
                Handles.color = Core.Config.brushColor;
                Handles.DrawSolidDisc(p_hit.point, p_hit.normal, gizmoSize);

                if (Event.current.button == 0 && !Event.current.alt && (Event.current.type == EventType.MouseDrag ||
                                                                        Event.current.type == EventType.MouseDown))
                {
                    FillColorOnHit(p_hit, meshFilter);
                }
            }
        }

        void OnColorChange(MeshFilter p_meshFilter, Color p_color)
        {
            Core.Config.brushColor = p_color;
            FillSubMeshColor(p_meshFilter, _selectedSubmesh, p_color);
        }
        
        void FillColorOnHit(RaycastHit p_hit, MeshFilter p_meshFilter)
        {
            Undo.RegisterCompleteObjectUndo(p_meshFilter.sharedMesh, "Fill Color");
            Mesh mesh = p_meshFilter.sharedMesh;
            var triangles = mesh.triangles;

            if (mesh.subMeshCount > 1)
            {
                int firstVertexIndex = p_hit.triangleIndex * 3;
                int submeshIndex = MeshUtils.GetSubMeshFromVertexIndex(mesh, firstVertexIndex);
                
                SubMeshDescriptor desc = mesh.GetSubMesh(submeshIndex);

                for (int j = 0; j < desc.indexCount; j++)
                {
                    Core.CachedColors[triangles[desc.indexStart + j]] = Core.Config.brushColor;
                }
            }
            else
            {
                Core.CachedColors = Enumerable.Repeat(Core.Config.brushColor, Core.CachedColors.Length).ToArray();
            }
            
            mesh.colors = Core.CachedColors;

            EditorUtility.SetDirty(p_meshFilter);
        }
        
        void FillSubMeshColor(MeshFilter p_meshFilter, int p_submeshIndex, Color p_color)
        {
            Undo.RegisterCompleteObjectUndo(p_meshFilter.sharedMesh, "Fill Color");
            Mesh mesh = p_meshFilter.sharedMesh;
            var triangles = mesh.triangles;

            if (mesh.subMeshCount > 1)
            {
                SubMeshDescriptor desc = mesh.GetSubMesh(p_submeshIndex);

                for (int j = 0; j < desc.indexCount; j++)
                {
                    Core.CachedColors[triangles[desc.indexStart + j]] = p_color;
                }
            }
            else
            {
                Core.CachedColors = Enumerable.Repeat(p_color, Core.CachedColors.Length).ToArray();
            }
            
            mesh.colors = Core.CachedColors;

            EditorUtility.SetDirty(p_meshFilter);
        }
        
        public override void DrawGUI(SceneView p_sceneView)
        {
            var space = 8;
            var style = new GUIStyle("label");
            style.fontStyle = FontStyle.Bold;

            GUILayout.Space(space);
            
            GUILayout.Label("Brush Color: ", style, GUILayout.Width(80));
            EditorGUI.BeginChangeCheck();
            Core.Config.brushColor = EditorGUILayout.ColorField(Core.Config.brushColor, GUILayout.Width(60));

            if (EditorGUI.EndChangeCheck())
            {
                if (Core.Config.autoFill)
                {
                    FillSubMeshColor(Core.PaintedMesh, _selectedSubmesh, Core.Config.brushColor);
                }
            }

            GUILayout.Space(space);

            GUILayout.Label("Autofill", style, GUILayout.Width(60));
            Core.Config.autoFill = EditorGUILayout.Toggle(Core.Config.autoFill, GUILayout.Width(20));

            if (Core.SubmeshColors.Count > 1 && Core.Config.autoFill)
            {
                GUILayout.Label("Submesh: ", style, GUILayout.Width(65));

                if (GUILayout.Button(Core.SubmeshNames[_selectedSubmesh]))
                {
                    SubMeshList.Show(Core.PaintedMesh.sharedMesh, Event.current.mousePosition, i =>
                    {
                        Core.Config.brushColor = Core.SubmeshColors[i];
                        _selectedSubmesh = i;
                    });
                }
            }
            
            GUILayout.Space(space);
            var renderer = Core.PaintedMesh?.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                if (GUILayout.Button("Material Color Fill"))
                {
                    for (int i = 0; i < Core.PaintedMesh.sharedMesh.subMeshCount; i++)
                    {
                        Color color = renderer.sharedMaterials.Length > i ? renderer.sharedMaterials[i].color : Color.white;
                        FillSubMeshColor(Core.PaintedMesh, i, color);
                    }

                    if (Core.Config.vertexColorMaterial != null)
                    {
                        var materials = new Material[Core.PaintedMesh.sharedMesh.subMeshCount];

                        for (int i = 0; i < Core.PaintedMesh.sharedMesh.subMeshCount; i++)
                        {
                            materials[i] = Core.Config.vertexColorMaterial;
                        }

                        renderer.sharedMaterials = materials;
                    }
                }
            }
        }
        
        public override void DrawHelpGUI(SceneView p_sceneView)
        {
            // Help
            var rect = p_sceneView.camera.GetScaledPixelRect();
            GUILayout.BeginArea(new Rect(rect.width / 2 - 500, rect.height-50, 1000, 30));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(" Left Mouse: ", Core.Skin.GetStyle("keylabel"), GUILayout.Height(16));
            GUILayout.Label("Fill color of mesh/submesh ", Core.Skin.GetStyle("keyfunction"), GUILayout.Height(16));
            GUILayout.Space(8);
            
            GUILayout.Label(" Ctrl + Left Mouse: ", Core.Skin.GetStyle("keylabel"), GUILayout.Height(16));
            GUILayout.Label("Select submesh ", Core.Skin.GetStyle("keyfunction"), GUILayout.Height(16));
            GUILayout.Space(8);
            
            GUILayout.Label(" Ctrl + Right Mouse: ", Core.Skin.GetStyle("keylabel"), GUILayout.Height(16));
            GUILayout.Label("Change color of submesh ", Core.Skin.GetStyle("keyfunction"), GUILayout.Height(16));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}