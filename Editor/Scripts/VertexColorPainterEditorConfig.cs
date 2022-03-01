/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using UnityEngine;

namespace VertexColorPainter.Editor
{
    [Serializable]
    public class VertexColorPainterEditorConfig : ScriptableObject
    {
        [HideInInspector]
        public bool enabled = false;

        [HideInInspector]
        public BrushType brushType;

        [HideInInspector]
        public float brushSize = 1;

        [HideInInspector]
        public Color brushColor = Color.white;

        [HideInInspector]
        public bool lockToSubmesh = false;
        
        [HideInInspector]
        public bool autoFill = false;

        [HideInInspector]
        public bool previousOutlineSetting = false;
        
        [HideInInspector]
        public bool autoMeshIsolation = false;

        [HideInInspector]
        public bool autoMeshFraming = false;

        public float brushOutlineSize = 0.004f;

        public float forcedMinBrushSize = 1f;

        public float forcedMaxBrushSize = 10f;
    }
}