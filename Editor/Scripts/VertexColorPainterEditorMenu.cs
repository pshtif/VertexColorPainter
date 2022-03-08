/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEditor;

namespace VertexColorPainter.Editor
{
    public class VertexColorPainterEditorMenu
    {
        static VertexColorPainterEditorCore Core => VertexColorPainterEditorCore.Instance; 
        
        [MenuItem("Tools/Vertex Color Painter/Enabled")]
        private static void ToggleEnabled()
        {
            Core.Config.enabled = !Core.Config.enabled;
        }

        [MenuItem("Tools/Vertex Color Painter/Auto Mesh Isolation (Experimental)")]
        private static void ToggleMeshIsolation()
        {
            Core.Config.autoMeshIsolation = !Core.Config.autoMeshIsolation;
        }
        
        [MenuItem("Tools/Vertex Color Painter/Auto Mesh Framing")]
        private static void ToggleMeshFraming()
        {
            Core.Config.autoMeshFraming = !Core.Config.autoMeshFraming;
        }
        
        [MenuItem("Tools/Vertex Color Painter/Enabled", true)]
        [MenuItem("Tools/Vertex Color Painter/Auto Mesh Isolation (Experimental)", true)]
        [MenuItem("Tools/Vertex Color Painter/Auto Mesh Framing", true)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked("Tools/Vertex Color Painter/Enabled", Core.Config.enabled);
            Menu.SetChecked("Tools/Vertex Color Painter/Auto Mesh Isolation (Experimental)", Core.Config.autoMeshIsolation);
            Menu.SetChecked("Tools/Vertex Color Painter/Auto Mesh Framing", Core.Config.autoMeshFraming);
            return true;
        }
    }
}