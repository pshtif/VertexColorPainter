/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEditor;
using UnityEngine;

namespace VertexColorPainter.Editor
{
    public abstract class ToolBase
    {
        public VCPEditorCore Core => VCPEditorCore.Instance;
        
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

            if (_mouseHitTransform != Core.PaintedMesh.transform)
                return;
            
            HandleMouseHitInternal(_mouseRaycastHit, _mouseHitTransform);
        }
        
        void RaycastMouse(SceneView p_sceneView)
        {
            RaycastHit hit;

            if (EditorRaycast.RaycastWorld(Event.current.mousePosition, out hit, out _mouseHitTransform,
                out _mouseHitMesh, null, new [] {Core.PaintedMesh.gameObject}))
            {
                _mouseRaycastHit = hit;
            }
        }

        public abstract void HandleMouseHitInternal(RaycastHit p_hit, Transform p_transform);

        public abstract void DrawGUI(SceneView p_sceneView);

        public abstract void DrawHelpGUI(SceneView p_sceneView);
    }
}