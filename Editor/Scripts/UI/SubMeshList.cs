/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VertexColorPainter.Editor
{
    public class SubMeshList : PopupWindowContent
    {
        private int _width;
        private int _height;
        private List<string> _subMeshNames;
        private List<Color> _subMeshColors;
        private Action<int> _callback;
        
        public static SubMeshList Show(Mesh p_mesh, Vector2 p_position, Action<int> p_callback)
        {
            var list = new SubMeshList(p_mesh, p_callback);
            list._width = 124;
            list._height = list._subMeshNames.Count * 21 + 4;
            PopupWindow.Show(new Rect(p_position.x, p_position.y, 0, 0), list);
            return list;
        }
        
        public override Vector2 GetWindowSize()
        {
            return new Vector2(_width, _height);
        }
        
        public SubMeshList(Mesh p_mesh, Action<int> p_callback)
        {
            _callback = p_callback;
            EnumerateSubmeshes(p_mesh);
        }
        
        public override void OnGUI(Rect p_rect)
        {
            for (int i = 0; i < _subMeshNames.Count; i++)
            {
                if (GUILayout.Button(_subMeshNames[i], GUILayout.Width(100)))
                {
                    base.editorWindow.Close();
                    _callback?.Invoke(i);
                }

                GUI.color = _subMeshColors[i];
                var rect = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(new Rect(rect.x + rect.width + 2, rect.y + 2, 14, 14), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
        }
        
        void EnumerateSubmeshes(Mesh p_mesh)
        {
            _subMeshColors = new List<Color>();
            _subMeshNames = new List<string>();

            var cachedColors = p_mesh.colors;
            
            for (int i = 0; i < p_mesh.subMeshCount; i++)
            {
                SubMeshDescriptor desc = p_mesh.GetSubMesh(i);
                _subMeshColors.Add(cachedColors[p_mesh.triangles[desc.indexStart]]);
                _subMeshNames.Add("Submesh " + i);
            }
        }
    }
}