/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VertexColorPainter.Editor
{
    public class ColorPickerWrapper
    {
        public static void Show(Action<Color> p_callback, Color p_color)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().ToList()
                .Find(a => a.FullName.IndexOf("UnityEditor,") == 0);
            Type colorPickerType = assembly.GetType("UnityEditor.ColorPicker");
            MethodInfo method = colorPickerType.GetMethod("Show", new Type[] { typeof(Action<Color>), typeof(Color), typeof(bool), typeof(bool) } );
            method.Invoke(null, new object[] { p_callback, p_color, true, false });
        }
    }
}