/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace VertexColorPainter.Editor
{
    public class ColorWindow : EditorWindow
    {
        private Mesh _importedMesh;

        private ReorderableList _importedSubMeshList;
        
        public static ColorWindow Instance { get; private set; }
        
        public static ColorWindow InitWindow()
        {
            Instance = GetWindow<ColorWindow>();
            Instance.titleContent = new GUIContent("Color Editor");

            return Instance;
        }

        public void OnGUI()
        {
            var style = new GUIStyle();
            style.normal.textColor = new Color(1, 0.5f, 0);
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 18;

            GUILayout.Label("Color Editor", style, GUILayout.Height(24));

            if (VCPEditorCore.PaintedMesh == null)
            {
                GUILayout.Label("No mesh is being painted.");
                return;
            }

            var uniqueColors = VCPEditorCore.Cache.GetAllColors(VCPEditorCore.Config.channelType).Distinct();

            foreach (var color in uniqueColors)
            {
                var newColor = EditorGUILayout.ColorField(color);
                if (newColor != color)
                {
                    ChangeMeshColor(VCPEditorCore.PaintedMesh, color, newColor);
                }
            }
        }
        
        void ChangeMeshColor(Mesh p_mesh, Color p_oldColor, Color p_newColor)
        {
            for (int i = 0; i < p_mesh.vertexCount; i++)
            {
                if (VCPEditorCore.Cache.GetColorAtIndex(i, VCPEditorCore.Config.channelType) == p_oldColor)
                {
                    VCPEditorCore.Cache.SetColorAtIndex(i, p_newColor, VCPEditorCore.Config.channelType);
                }
            }

            VCPEditorCore.Cache.InvalidateMeshColors(VCPEditorCore.PaintedMesh, VCPEditorCore.Config.channelType);

            EditorUtility.SetDirty(VCPEditorCore.PaintedObject);
        }
    }
}