/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;

namespace VertexColorPainter.Editor
{
    public static class VCPMaterials
    {
        public static VCPEditorConfig Config => VCPEditorCore.Config;
        
        private static Material _vertexColorMaterial;

        public static Material VertexColorMaterial
        {
            get
            {
                if (_vertexColorMaterial == null)
                {
                    _vertexColorMaterial = new Material(Shader.Find("Hidden/Vertex Color Painter/VertexColorPainterShader"));
                    _vertexColorMaterial.SetVector("_ChannelMask",
                        new Vector4(Config.channelType == ChannelType.UV0 ? 1 : 0,
                            Config.channelType == ChannelType.UV1 ? 1 : 0,
                            Config.channelType == ChannelType.UV2 ? 1 : 0, 0));
                }

                return _vertexColorMaterial;
            }
        }
        
        private static Material _selectionMaterial; 
        public static Material SelectionMaterial
        {
            get
            {
                if (_selectionMaterial == null)
                {
                    _selectionMaterial = new Material(Shader.Find("Hidden/Vertex Color Painter/SelectionShader"));
                }

                return _selectionMaterial;
            }
        }
    }
}