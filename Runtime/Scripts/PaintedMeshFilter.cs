/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using UnityEditor;
using UnityEngine;

namespace VertexColorPainter.Runtime
{
    // This class is obsolete and will be removed later
    [Obsolete]
    public class PaintedMeshFilter : MonoBehaviour
    {
        private MeshFilter _filter;
        
        public MeshFilter filter
        {
            get
            {
                if (_filter == null)
                {
                    if (!gameObject.TryGetComponent<MeshFilter>(out _filter))
                        return null;
#if UNITY_EDITOR
                    _filter.hideFlags = HideFlags.HideInInspector | HideFlags.NotEditable;
#endif
                }

                return _filter;
            }
        }

#if UNITY_EDITOR
        public bool showAdvanced = false;

        public Mesh reimportMesh;
        
        [SerializeField]
        private GUID _fbxGuid;
        
        public string fbxAssetPath
        {
            get
            {
                return AssetDatabase.GUIDToAssetPath(_fbxGuid);
            }
        }

        public void SetOriginalAsset(Mesh p_mesh)
        {
            if (AssetDatabase.Contains(p_mesh))
            {
                var path = AssetDatabase.GetAssetPath(p_mesh);
                _fbxGuid = AssetDatabase.GUIDFromAssetPath(path);
            }
        }
#endif
    }
}