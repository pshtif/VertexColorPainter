/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VertexColorPainter.Editor
{
    public class VCPSceneGUI
    {
        private static Vector3 _previousSceneViewPivot;
        private static Quaternion _previousSceneViewRotation;
        private static float _previousSceneViewSize;

        public static VCPEditorConfig Config => VCPEditorCore.Config;

        public static void Initialize()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }
        
        public static void OnSceneGUI(SceneView p_sceneView)
        {
            if (!Config.enabled)
                return;
            
            if (VCPEditorCore.PaintedObject != null && VCPEditorCore.PaintedMesh != null)
            {
                if (!VCPChannel.IsValidChannel(Config.channelType, false))
                {
                    VCPChannel.ChangeChannel(ChannelType.COLOR);
                }
                
                // if (Selection.activeGameObject != _paintedMesh.gameObject)
                //     Selection.activeGameObject = _paintedMesh.gameObject;
                
                VCPEditorCore.InvalidateCurrentTool();

                Tools.current = Tool.None;
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                var rect = p_sceneView.camera.GetScaledPixelRect();
                rect = new Rect(0, rect.height - 55, rect.width, 55);
                
                // Don't draw tool under gui
                if (!rect.Contains(Event.current.mousePosition))
                {
                    VCPEditorCore.CurrentTool?.HandleMouseHit(p_sceneView);
                }
            }

            DrawGUI(p_sceneView);
        }
        
        private static void DrawGUI(SceneView p_sceneView)
        {
            if (VCPEditorCore.PaintedMesh == null)
            {
                if (Selection.activeGameObject?.GetComponent<MeshFilter>() != null ||
                    Selection.activeGameObject?.GetComponent<SkinnedMeshRenderer>() != null) 
                {
                    DrawDisabledGUI(p_sceneView);
                }
            }
            else
            {
                if (VCPEditorCore.PaintedObject != null)
                {
                    DrawEnabledGUI(p_sceneView);
                }
                else
                {
                    VCPEditorCore.DisablePainting();
                }
            }
        }

        private static void DrawDisabledGUI(SceneView p_sceneView)
        {
            Handles.BeginGUI();

            var rect = p_sceneView.camera.GetScaledPixelRect();
            
            GUI.color = new Color(1, .5f, 0);
            if (GUI.Button(new Rect(5, rect.height - 25, 120, 20), "Enable Paiting"))
            {
                VCPEditorCore.EnablePainting(Selection.activeGameObject);
            }
            GUI.color = Color.white;
            
            Handles.EndGUI();
        }

        private static void DrawEnabledGUI(SceneView p_sceneView)
        {
            var rect = p_sceneView.camera.GetScaledPixelRect();

            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = Texture2D.whiteTexture; // must be white to tint properly
            GUI.color = new Color(0, 0, 0, .4f);
            
            // Handles.BeginGUI();
            //
            // GUI.Box(rect, "", style);
            //
            // Handles.EndGUI();

            // TODO move to a separate function
            if (Config.overlayRender)
            {
                //GL.Clear(true, false, Color.black);
                //VertexColorMaterial.SetPass(0);
                //Graphics.DrawMeshNow(PaintedMesh, PaintedObject.transform.localToWorldMatrix);
                for (int i = 0; i < VCPEditorCore.PaintedMesh.subMeshCount; i++)
                {
                    Graphics.DrawMesh(VCPEditorCore.PaintedMesh, VCPEditorCore.PaintedObject.transform.localToWorldMatrix, VCPMaterials.VertexColorMaterial,
                        0, p_sceneView.camera, i);
                }
            }

            Handles.BeginGUI();
            
            GUI.Box(new Rect(0, rect.height - 30, rect.width , 30), "", style);
            
            GUI.color = Color.white;
            
            int space = 8;

            if (GUI.Button(new Rect(rect.width-125, rect.height - 55, 120, 20),
                    (Config.overlayRender ? "Disable" : "Enable") + " Overlay")) 
            {
                Config.overlayRender = !Config.overlayRender;
                return;
            }

            GUI.color = new Color(1, .5f, 0);
            if (GUI.Button(new Rect(5, rect.height - 55, 120, 20), "Disable Painting"))
            {
                VCPEditorCore.DisablePainting();
                return;
            }
            GUI.color = Color.white;

            GUILayout.BeginArea(new Rect(5, rect.height - 22, rect.width - 5, 20));
            GUILayout.BeginHorizontal();
            
            style = new GUIStyle("label");
            style.fontStyle = FontStyle.Bold;

            // GUILayout.Label("Channel: ", style, GUILayout.Width(60));
            //
            // HandleChannelSelection();

            GUILayout.Space(space);

            GUILayout.Label("Brush Type: ", style, GUILayout.Width(80));
            Config.toolType = (ToolType)EditorGUILayout.Popup((int)Config.toolType, EnumNames.GetNames<ToolType>(),
                GUILayout.Width(120));

            VCPEditorCore.CurrentTool.DrawSceneGUI(p_sceneView);

            GUILayout.FlexibleSpace();

            //_meshIsolationEnabled = GUILayout.Toggle(_meshIsolationEnabled, "Isolate Mesh");
            
            GUILayout.Space(space);
            
            if (GUILayout.Button("Frame", GUILayout.Width(60)))
            {
                Frame();
            }
            
            style.fontStyle = FontStyle.Normal;
            style.normal.textColor = Color.gray;
            GUILayout.Label("VertexColorPainter v"+VCPEditorCore.VERSION, style);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            
            VCPEditorCore.CurrentTool.DrawHelpGUI(p_sceneView);
            
            GUILayout.BeginArea(new Rect(rect.width / 2 - 500, rect.height-75, 1000, 30));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var s = new GUIStyle();
            s.normal.textColor = Color.white;
            GUILayout.Label("Painting object: ", s);
            s.normal.textColor = Color.green;
            s.fontStyle = FontStyle.Bold;
            GUILayout.Label(VCPEditorCore.PaintedObject.name,s);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            
            Handles.EndGUI();
        }
        
        static void Frame()
        {
            var view = SceneView.lastActiveSceneView;
            StoreSceneViewCamera(view);

            switch (VCPEditorCore.PaintedType)
            {
                case PaintedMeshType.STATIC:
                    view.Frame(VCPEditorCore.PaintedObject.GetComponent<MeshRenderer>().bounds, false);
                    break;
                case PaintedMeshType.SKINNED:
                    view.Frame(VCPEditorCore.PaintedObject.GetComponent<SkinnedMeshRenderer>().bounds, false);
                    break;
            }
        }
        
        static void StoreSceneViewCamera(SceneView p_sceneView)
        {
            _previousSceneViewPivot = p_sceneView.pivot;
            _previousSceneViewRotation = p_sceneView.rotation;
            _previousSceneViewSize = p_sceneView.size;
        }
        
        static void RestoreSceneViewCamera(SceneView p_sceneView)
        {
            p_sceneView.pivot = _previousSceneViewPivot;
            p_sceneView.rotation = _previousSceneViewRotation;
            p_sceneView.size = _previousSceneViewSize;
            p_sceneView.Repaint();
        }
    }
}