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
        
        private PaintedMeshFilter _paintedMeshFilter;
        
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

            var uniqueColors = VCPEditorCore.Instance.CachedColors.Distinct();

            foreach (var color in uniqueColors)
            {
                var newColor = EditorGUILayout.ColorField(color);
                if (newColor != color)
                {
                    ChangeMeshColor(VCPEditorCore.Instance.PaintedMesh, color, newColor);
                }
            }
        }
        
        void ChangeMeshColor(MeshFilter p_meshFilter, Color p_oldColor, Color p_newColor)
        {
            Mesh mesh = p_meshFilter.sharedMesh;

            for (int i = 0; i < mesh.vertexCount; i++)
            {
                if (VCPEditorCore.Instance.CachedColors[i] == p_oldColor)
                {
                    VCPEditorCore.Instance.CachedColors[i] = p_newColor;
                }
            }

            mesh.colors = VCPEditorCore.Instance.CachedColors;

            EditorUtility.SetDirty(p_meshFilter);
        }
    }
}