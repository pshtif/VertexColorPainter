/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEditor;

namespace VertexColorPainter.Editor
{
    public class VertexColorPainterEditorMenu
    {
        [MenuItem("Tools/Vertex Color Painter/Enabled")]
        private static void ToggleEnabled()
        {
            VertexColorPainterEditorCore.Config.enabled = !VertexColorPainterEditorCore.Config.enabled;
        }

        [MenuItem("Tools/Vertex Color Painter/Auto Mesh Isolation (Experimental)")]
        private static void ToggleMeshIsolation()
        {
            VertexColorPainterEditorCore.Config.autoMeshIsolation = !VertexColorPainterEditorCore.Config.autoMeshIsolation;
        }
        
        [MenuItem("Tools/Vertex Color Painter/Auto Mesh Framing")]
        private static void ToggleMeshFraming()
        {
            VertexColorPainterEditorCore.Config.autoMeshFraming = !VertexColorPainterEditorCore.Config.autoMeshFraming;
        }
        
        [MenuItem("Tools/Vertex Color Painter/Enabled", true)]
        [MenuItem("Tools/Vertex Color Painter/Auto Mesh Isolation (Experimental)", true)]
        [MenuItem("Tools/Vertex Color Painter/Auto Mesh Framing", true)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked("Tools/Vertex Color Painter/Enabled", VertexColorPainterEditorCore.Config.enabled);
            Menu.SetChecked("Tools/Vertex Color Painter/Auto Mesh Isolation (Experimental)", VertexColorPainterEditorCore.Config.autoMeshIsolation);
            Menu.SetChecked("Tools/Vertex Color Painter/Auto Mesh Framing", VertexColorPainterEditorCore.Config.autoMeshFraming);
            return true;
        }
    }
}