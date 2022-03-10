/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEditor;
using UnityEngine;

namespace VertexColorPainter.Editor
{
    public class VCPAsset : ScriptableObject
    {
        public Mesh mesh;

        #if UNITY_EDITOR
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