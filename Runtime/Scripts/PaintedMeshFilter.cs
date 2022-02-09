/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;

namespace VertexColorPainter.Runtime
{
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
    }
}