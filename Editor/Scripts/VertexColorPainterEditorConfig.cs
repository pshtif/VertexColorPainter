/*
 *	Created by:  Peter @sHTiF Stefcek
 */

namespace VertexColorPainter
{
    using System;
    using UnityEngine;

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
        public bool previousOutlineSetting = false;
        
        [HideInInspector]
        public bool meshIsolation = false;

        [HideInInspector]
        public bool meshFraming = false;

        public float brushOutlineSize = 0.004f;

        public bool forceMinMaxBrushSize = false;

        public float forcedMinBrushSize = 0.1f;

        public float forcedMaxBrushSize = 1f;
    }
}