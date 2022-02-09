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
            VertexColorPaintEditorCore.Config.enabled = !VertexColorPaintEditorCore.Config.enabled;
        }

        [MenuItem("Tools/Vertex Color Painter/Auto Mesh Isolation (Experimental)")]
        private static void ToggleMeshIsolation()
        {
            VertexColorPaintEditorCore.Config.autoMeshIsolation = !VertexColorPaintEditorCore.Config.autoMeshIsolation;
        }
        
        [MenuItem("Tools/Vertex Color Painter/Auto Mesh Framing")]
        private static void ToggleMeshFraming()
        {
            VertexColorPaintEditorCore.Config.autoMeshFraming = !VertexColorPaintEditorCore.Config.autoMeshFraming;
        }
        
        [MenuItem("Tools/Vertex Color Painter/Enabled", true)]
        [MenuItem("Tools/Vertex Color Painter/Auto Mesh Isolation (Experimental)", true)]
        [MenuItem("Tools/Vertex Color Painter/Auto Mesh Framing", true)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked("Tools/Vertex Color Painter/Enabled", VertexColorPaintEditorCore.Config.enabled);
            Menu.SetChecked("Tools/Vertex Color Painter/Auto Mesh Isolation (Experimental)", VertexColorPaintEditorCore.Config.autoMeshIsolation);
            Menu.SetChecked("Tools/Vertex Color Painter/Auto Mesh Framing", VertexColorPaintEditorCore.Config.autoMeshFraming);
            return true;
        }
    }
}