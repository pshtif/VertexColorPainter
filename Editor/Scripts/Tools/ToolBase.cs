/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VertexColorPainter.Editor
{
    public abstract class ToolBase
    {
        public VCPEditorConfig Config => VCPEditorCore.Config;
        public GUISkin Skin => (GUISkin)Resources.Load("Skins/VertexColorPainterSkin");
        
        protected RaycastHit _mouseRaycastHit;
        protected Vector3 _lastMousePosition;
        protected Transform _mouseHitTransform;
        protected Mesh _mouseHitMesh;
        
        protected int _selectedSubmesh = 0;

        public void HandleMouseHit(SceneView p_sceneView)
        {
            if (Event.current.isMouse)
            {
                RaycastMouse(p_sceneView);
            }
            
            if (_mouseHitTransform != VCPEditorCore.PaintedObject.transform)
                return;
            
            HandleMouseHitInternal(p_sceneView, _mouseRaycastHit, _mouseHitTransform);
            
            p_sceneView.Repaint();
        }
        
        void RaycastMouse(SceneView p_sceneView)
        {
            RaycastHit hit;

            if (EditorRaycast.RaycastWorld(Event.current.mousePosition, out hit, out _mouseHitTransform,
                out _mouseHitMesh, null, new [] {VCPEditorCore.PaintedObject}))
            {
                _mouseRaycastHit = hit;
            }
        }

        public abstract void HandleMouseHitInternal(SceneView p_sceneView, RaycastHit p_hit, Transform p_transform);

        public abstract void DrawSceneGUI(SceneView p_sceneView);

        public abstract void DrawSettingsGUI();

        public abstract void DrawHelpGUI(SceneView p_sceneView);
        
        public void HandleChannelSelection()
        {
            int channelType = (int)Config.channelType;
            
            EditorGUI.BeginChangeCheck();
            
            channelType = EditorGUILayout.Popup("Channel", channelType, EnumNames.GetNames<ChannelType>());
            
            if (EditorGUI.EndChangeCheck())
            {
                VCPChannel.ChangeChannel((ChannelType)channelType);
            }
        }
    }
}