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
        private Color _pickedColor;
        
        public override void HandleMouseHitInternal(SceneView p_sceneView, RaycastHit p_hit, Transform p_hitTransform)
        {
            if (Event.current.control)
            {
                int submesh = MeshUtils.GetSubMeshIndexFromTriangle(VCPEditorCore.PaintedMesh, p_hit.triangleIndex);
                int index = MeshUtils.GetClosesVertexIndex(VCPEditorCore.PaintedMesh, VCPEditorCore.PaintedObject.transform.worldToLocalMatrix, p_hit);
                _pickedColor = VCPEditorCore.Cache.GetColorAtIndex(index, VCPEditorCore.Config.channelType);

                //if (Core.Config.autoFill)
                {
                    //Core.SelectionMaterial.SetPass(0);
                    //Graphics.DrawMeshNow(Core.PaintedMesh, Core.PaintedObject.transform.localToWorldMatrix, submesh);
                    Graphics.DrawMesh(VCPEditorCore.PaintedMesh, VCPEditorCore.PaintedObject.transform.localToWorldMatrix, VCPMaterials.SelectionMaterial, 0, p_sceneView.camera, submesh);
                }

                if (Event.current.button == 0 && !Event.current.alt && (Event.current.type == EventType.MouseDrag ||
                                                                        Event.current.type == EventType.MouseDown))
                {
                    
                    _selectedSubmesh = submesh;
                    VCPEditorCore.Config.brushColor = _pickedColor;
                }

                if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
                {
                    _selectedSubmesh = submesh;
                    VCPEditorCore.Config.brushColor = _pickedColor;
                    var color = _pickedColor;
                    ColorPickerWrapper.Show(c => OnColorChange(VCPEditorCore.PaintedMesh, c), color);
                }
            } else {

                if (Event.current.button == 0 && !Event.current.alt && (Event.current.type == EventType.MouseDrag ||
                                                                        Event.current.type == EventType.MouseDown))
                {
                    FillColorOnHit(p_hit, VCPEditorCore.PaintedMesh);
                }
            }
            
            DrawHandle(p_hit);
        }
        
        void DrawHandle(RaycastHit p_hit)
        {
            Handles.color = Color.white;
            var gizmoSize = HandleUtility.GetHandleSize(p_hit.point) / 10f;
            Handles.DrawSolidDisc(p_hit.point, p_hit.normal, gizmoSize * VCPEditorCore.Config.brushSize + gizmoSize / 5);
            Handles.color = _pickedColor;
            Handles.DrawSolidDisc(p_hit.point, p_hit.normal, gizmoSize * VCPEditorCore.Config.brushSize);
        }

        void OnColorChange(Mesh p_mesh, Color p_color)
        {
            VCPEditorCore.Config.brushColor = p_color;
            FillSubMeshColor(_selectedSubmesh, p_color);
        }
        
        void FillColorOnHit(RaycastHit p_hit, Mesh p_mesh)
        {
            Undo.RegisterCompleteObjectUndo(p_mesh, "Fill Color");
            var triangles = p_mesh.triangles;

            if (p_mesh.subMeshCount > 1)
            {
                int firstVertexIndex = p_hit.triangleIndex * 3;
                int submeshIndex = MeshUtils.GetSubMeshFromVertexIndex(p_mesh, firstVertexIndex);
                
                SubMeshDescriptor desc = p_mesh.GetSubMesh(submeshIndex);

                for (int j = 0; j < desc.indexCount; j++)
                {
                    VCPEditorCore.Cache.SetColorAtIndex(triangles[desc.indexStart + j], VCPEditorCore.Config.brushColor, VCPEditorCore.Config.channelType);
                }
            }
            else
            {
                VCPEditorCore.Cache.SetAllColors(VCPEditorCore.Config.brushColor, VCPEditorCore.Config.channelType);
            }

            VCPEditorCore.Cache.InvalidateMeshColors(VCPEditorCore.PaintedMesh, VCPEditorCore.Config.channelType);

            EditorUtility.SetDirty(VCPEditorCore.PaintedObject);
        }
        
        void FillSubMeshColor(int p_submeshIndex, Color p_color)
        {
            Undo.RegisterCompleteObjectUndo(VCPEditorCore.PaintedMesh, "Fill Color");
            var triangles = VCPEditorCore.PaintedMesh.triangles;

            if (VCPEditorCore.PaintedMesh.subMeshCount > 1)
            {
                SubMeshDescriptor desc = VCPEditorCore.PaintedMesh.GetSubMesh(p_submeshIndex);

                for (int j = 0; j < desc.indexCount; j++)
                {
                    VCPEditorCore.Cache.SetColorAtIndex(triangles[desc.indexStart + j], p_color, VCPEditorCore.Config.channelType);
                }
            }
            else
            {
                VCPEditorCore.Cache.SetAllColors(p_color, VCPEditorCore.Config.channelType);
            }
            
            VCPEditorCore.Cache.InvalidateMeshColors(VCPEditorCore.PaintedMesh, VCPEditorCore.Config.channelType);

            EditorUtility.SetDirty(VCPEditorCore.PaintedObject);
        }

        public override void DrawSettingsGUI()
        {
            EditorGUILayout.LabelField("Fill Tool Settings", Skin.GetStyle("settingslabel"), GUILayout.Height(24));
                
            GUILayout.Space(4);
            
            HandleChannelSelection();
            
            EditorGUI.BeginChangeCheck();
            VCPEditorCore.Config.brushColor = EditorGUILayout.ColorField("Brush Color", VCPEditorCore.Config.brushColor);
            
            if (EditorGUI.EndChangeCheck())
            {
                if (VCPEditorCore.Config.autoFill)
                {
                    FillSubMeshColor(_selectedSubmesh, VCPEditorCore.Config.brushColor);
                }
            }
            
            VCPEditorCore.Config.autoFill = EditorGUILayout.Toggle("Autofill", VCPEditorCore.Config.autoFill);
            //
            // if (VCPEditorCore.Cache.SubmeshColors.Count > 1 && VCPEditorCore.Config.autoFill)
            // {
            //     GUILayout.BeginHorizontal();
            //     GUILayout.Label("Submesh");
            //
            //     if (_selectedSubmesh >= VCPEditorCore.Cache.SubmeshNames.Count)
            //         _selectedSubmesh = 0;
            //
            //     if (GUILayout.Button(VCPEditorCore.Cache.SubmeshNames[_selectedSubmesh]))
            //     {
            //         SubMeshList.Show(VCPEditorCore.PaintedMesh, Event.current.mousePosition, i =>
            //         {
            //             VCPEditorCore.Config.brushColor = VCPEditorCore.Cache.SubmeshColors[i];
            //             _selectedSubmesh = i;
            //         });
            //     }
            //     GUILayout.EndHorizontal();
            // }
            
            var renderer = VCPEditorCore.PaintedObject?.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                if (GUILayout.Button("Material Color Fill"))
                {
                    for (int i = 0; i < VCPEditorCore.PaintedMesh.subMeshCount; i++)
                    {
                        Material material = renderer.sharedMaterials.Length > i
                            ? renderer.sharedMaterials[i]
                            : null; 
                        
                        Color color = material != null && material.HasColor("_Color") ? renderer.sharedMaterials[i].color : Color.white;
                        FillSubMeshColor(i, color);
                    }

                    if (VCPEditorCore.Config.vertexColorMaterial != null)
                    {
                        var materials = new Material[VCPEditorCore.PaintedMesh.subMeshCount];

                        for (int i = 0; i < VCPEditorCore.PaintedMesh.subMeshCount; i++)
                        {
                            materials[i] = VCPEditorCore.Config.vertexColorMaterial;
                        }

                        renderer.sharedMaterials = materials;
                    }
                }
            }
        }
        
        public override void DrawSceneGUI(SceneView p_sceneView)
        {
            var space = 8;
            var style = new GUIStyle("label");
            style.fontStyle = FontStyle.Bold;

            GUILayout.Space(space);
            
            // GUILayout.Label("Brush Color: ", style, GUILayout.Width(80));
            // EditorGUI.BeginChangeCheck();
            // VCPEditorCore.Config.brushColor = EditorGUILayout.ColorField(VCPEditorCore.Config.brushColor, GUILayout.Width(60));
            //
            // if (EditorGUI.EndChangeCheck())
            // {
            //     if (VCPEditorCore.Config.autoFill)
            //     {
            //         FillSubMeshColor(_selectedSubmesh, VCPEditorCore.Config.brushColor);
            //     }
            // }

            // GUILayout.Space(space);
            //
            // GUILayout.Label("Autofill", style, GUILayout.Width(60));
            // VCPEditorCore.Config.autoFill = EditorGUILayout.Toggle(VCPEditorCore.Config.autoFill, GUILayout.Width(20));

            // if (VCPEditorCore.Cache.SubmeshColors.Count > 1 && VCPEditorCore.Config.autoFill)
            // {
            //     GUILayout.Label("Submesh: ", style, GUILayout.Width(65));
            //
            //     if (_selectedSubmesh >= VCPEditorCore.Cache.SubmeshNames.Count)
            //         _selectedSubmesh = 0;
            //
            //     if (GUILayout.Button(VCPEditorCore.Cache.SubmeshNames[_selectedSubmesh]))
            //     {
            //         SubMeshList.Show(VCPEditorCore.PaintedMesh, Event.current.mousePosition, i =>
            //         {
            //             VCPEditorCore.Config.brushColor = VCPEditorCore.Cache.SubmeshColors[i];
            //             _selectedSubmesh = i;
            //         });
            //     }
            // }
            
            GUILayout.Space(space);
            // var renderer = VCPEditorCore.PaintedObject?.GetComponent<MeshRenderer>();
            // if (renderer != null)
            // {
            //     if (GUILayout.Button("Material Color Fill"))
            //     {
            //         for (int i = 0; i < VCPEditorCore.PaintedMesh.subMeshCount; i++)
            //         {
            //             Color color = renderer.sharedMaterials.Length > i ? renderer.sharedMaterials[i].color : Color.white;
            //             FillSubMeshColor(i, color);
            //         }
            //
            //         if (VCPEditorCore.Config.vertexColorMaterial != null)
            //         {
            //             var materials = new Material[VCPEditorCore.PaintedMesh.subMeshCount];
            //
            //             for (int i = 0; i < VCPEditorCore.PaintedMesh.subMeshCount; i++)
            //             {
            //                 materials[i] = VCPEditorCore.Config.vertexColorMaterial;
            //             }
            //
            //             renderer.sharedMaterials = materials;
            //         }
            //     }
            // }
        }
        
        public override void DrawHelpGUI(SceneView p_sceneView)
        {
            // Help
            var rect = p_sceneView.camera.GetScaledPixelRect();
            GUILayout.BeginArea(new Rect(rect.width / 2 - 500, rect.height-50, 1000, 30));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(" Left Mouse: ", VCPEditorCore.Skin.GetStyle("keylabel"), GUILayout.Height(16));
            GUILayout.Label("Fill color of mesh/submesh ", VCPEditorCore.Skin.GetStyle("keyfunction"), GUILayout.Height(16));
            GUILayout.Space(8);
            
            GUILayout.Label(" Ctrl + Left Mouse: ", VCPEditorCore.Skin.GetStyle("keylabel"), GUILayout.Height(16));
            if (VCPEditorCore.Config.autoFill)
            {
                GUILayout.Label("Pick submesh & color ", VCPEditorCore.Skin.GetStyle("keyfunction"), GUILayout.Height(16));
            }
            else
            {
                GUILayout.Label("Pick color ", VCPEditorCore.Skin.GetStyle("keyfunction"), GUILayout.Height(16));
            }

            GUILayout.Space(8);
            
            GUILayout.Label(" Ctrl + Right Mouse: ", VCPEditorCore.Skin.GetStyle("keylabel"), GUILayout.Height(16));
            GUILayout.Label("Change color of submesh ", VCPEditorCore.Skin.GetStyle("keyfunction"), GUILayout.Height(16));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}