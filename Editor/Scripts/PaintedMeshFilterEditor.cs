/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEditor;
using UnityEngine;
using VertexColorPainter.Runtime;

namespace VertexColorPainter.Editor
{
    [CustomEditor(typeof(PaintedMeshFilter))]
    public class PaintedMeshFilterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            MeshFilter filter = (target as PaintedMeshFilter).filter;
            
            EditorGUILayout.LabelField("MeshFilter driven by painted data: "+filter.sharedMesh.name);
        }
    }
}