/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEditor;

namespace VertexColorPainter
{
    public class VertexColorPainterEditorMenu
    {
        [MenuItem("Tools/Vertex Color Painter/Enabled")]
        private static void ToggleEnabled()
        {
            VertexColorPaintEditorCore.Config.enabled = !VertexColorPaintEditorCore.Config.enabled;
        }

        [MenuItem("Tools/Vertex Color Painter/Mesh Isolation (Experimental)")]
        private static void ToggleMeshIsolation()
        {
            VertexColorPaintEditorCore.Config.meshIsolation = !VertexColorPaintEditorCore.Config.meshIsolation;
        }
        
        [MenuItem("Tools/Vertex Color Painter/Mesh Framing")]
        private static void ToggleMeshFraming()
        {
            VertexColorPaintEditorCore.Config.meshFraming = !VertexColorPaintEditorCore.Config.meshFraming;
        }
        
        [MenuItem("Tools/Vertex Color Painter/Enabled", true)]
        [MenuItem("Tools/Vertex Color Painter/Mesh Isolation (Experimental)", true)]
        [MenuItem("Tools/Vertex Color Painter/Mesh Framing", true)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked("Tools/Vertex Color Painter/Enabled", VertexColorPaintEditorCore.Config.enabled);
            Menu.SetChecked("Tools/Vertex Color Painter/Mesh Isolation (Experimental)", VertexColorPaintEditorCore.Config.meshIsolation);
            Menu.SetChecked("Tools/Vertex Color Painter/Mesh Framing", VertexColorPaintEditorCore.Config.meshFraming);
            return true;
        }
    }
}