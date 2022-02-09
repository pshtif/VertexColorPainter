/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace VertexColorPainter.Editor
{
    public class AnnotationUtilityUtil
    {
        private static PropertyInfo _showSelectedOutline;
        
        static void CacheUnityInternalCall()
        {
            var annotationUtility = typeof(UnityEditor.Editor).Assembly.GetTypes().FirstOrDefault(t => t.Name == "AnnotationUtility");
            _showSelectedOutline = annotationUtility.GetProperty("showSelectionOutline", (BindingFlags.Static | BindingFlags.NonPublic));
        }
        
        public static bool showSelectedOutline
        {
            get {
                if (_showSelectedOutline == null)
                    CacheUnityInternalCall();

                return (bool)_showSelectedOutline.GetValue(null);
            }
            set {
                _showSelectedOutline.SetValue(null, value);
            }
        }
    }
}