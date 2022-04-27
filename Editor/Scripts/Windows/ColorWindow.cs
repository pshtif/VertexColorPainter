/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using VertexColorPainter.Runtime;

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

            if (VCPEditorCore.Instance.PaintedMesh == null)
            {
                GUILayout.Label("No mesh is being painted.");
                return;
            }

            var uniqueColors = VCPEditorCore.Instance.GetAllColors().Distinct();

            foreach (var color in uniqueColors)
            {
                var newColor = EditorGUILayout.ColorField(color);
                if (newColor != color)
                {
                    ChangeMeshColor(VCPEditorCore.Instance.PaintedMesh, color, newColor);
                }
            }
        }
        
        void ChangeMeshColor(Mesh p_mesh, Color p_oldColor, Color p_newColor)
        {
            for (int i = 0; i < p_mesh.vertexCount; i++)
            {
                if (VCPEditorCore.Instance.GetColorAtIndex(i) == p_oldColor)
                {
                    VCPEditorCore.Instance.SetColorAtIndex(i, p_newColor);
                }
            }

            VCPEditorCore.Instance.InvalidateMeshColors();

            EditorUtility.SetDirty(VCPEditorCore.Instance.PaintedObject);
        }
    }
}