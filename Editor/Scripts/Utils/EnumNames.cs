/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace VertexColorPainter.Editor
{
    public static class EnumNames
    {
        private static Dictionary<Type, string[]> _names;
        
        public static string[] GetNames<T>()
        {
            if (!typeof(T).IsEnum)
                return null; 
            
            if (_names == null)
            {
                _names = new Dictionary<Type, string[]>();
            }

            if (!_names.ContainsKey(typeof(T)))
            {
                var names = ((T[])Enum.GetValues(typeof(T))).Select(t => t.ToString()).ToArray();

                _names.Add(typeof(T), names);
            }

            return _names[typeof(T)];
        }
    }
}