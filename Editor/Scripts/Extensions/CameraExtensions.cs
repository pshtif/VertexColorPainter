/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEditor;
using UnityEngine;

namespace VertexColorPainter.Editor
{
    #if UNITY_EDITOR
    public static class CameraExtensions
    {
        public static Rect GetScaledPixelRect(this Camera p_camera)
        {
            return new Rect(0, 0, p_camera.pixelRect.width / EditorGUIUtility.pixelsPerPoint,
                p_camera.pixelRect.height / EditorGUIUtility.pixelsPerPoint);
        }
    }
    #endif
}