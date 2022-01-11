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
        public bool enabled = false;

        public float brushSize = 1;

        public Color brushColor = Color.white;

        public bool lockToSubmesh = false;

        public bool previousOutlineSetting = false;
    }
}