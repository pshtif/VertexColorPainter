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
  
        [MenuItem("Tools/Vertex Color Painter/Enabled", true)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked("Tools/Vertex Color Painter/Enabled", VertexColorPaintEditorCore.Config.enabled);
            return true;
        }
    }
}