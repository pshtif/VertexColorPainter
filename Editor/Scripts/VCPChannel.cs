/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VertexColorPainter.Editor
{
    public static class VCPChannel
    {
        public static VCPEditorConfig Config => VCPEditorCore.Config;
        
        public static bool IsValidChannel(ChannelType p_channel, bool p_showDialog)
        {
            List<Vector4> uvs = new List<Vector4>();
            if (p_channel == ChannelType.UV0)
            {
                VCPEditorCore.PaintedMesh.GetUVs(0, uvs);
                if (uvs.Count == 0)
                {
                    if (p_showDialog && EditorUtility.DisplayDialog("UV0 Channel Not Found",
                            "UV0 channel is missing in this mesh do you want to create it?", "YES", "NO"))
                    {
                        VCPEditorCore.Cache.Uv0 = Enumerable.Repeat(Vector4.zero, VCPEditorCore.PaintedMesh.vertexCount).ToArray();
                        VCPEditorCore.PaintedMesh.SetUVs(0, VCPEditorCore.Cache.Uv0);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            
            if (p_channel == ChannelType.UV1)
            {
                VCPEditorCore.PaintedMesh.GetUVs(1, uvs);
                if (uvs.Count == 0)
                {
                    if (p_showDialog && EditorUtility.DisplayDialog("UV1 Channel Not Found",
                            "UV1 channel is missing in this mesh do you want to create it?", "YES", "NO"))
                    {
                        VCPEditorCore.Cache.Uv1 = Enumerable.Repeat(Vector4.zero, VCPEditorCore.PaintedMesh.vertexCount).ToArray();
                        VCPEditorCore.PaintedMesh.SetUVs(1, VCPEditorCore.Cache.Uv1);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            
            if (p_channel == ChannelType.UV2)
            {
                VCPEditorCore.PaintedMesh.GetUVs(2, uvs);
                if (uvs.Count == 0)
                {
                    if (p_showDialog && EditorUtility.DisplayDialog("UV2 Channel Not Found",
                            "UV2 channel is missing in this mesh do you want to create it?", "YES", "NO"))
                    {
                        VCPEditorCore.Cache.Uv2 = Enumerable.Repeat(Vector4.zero, VCPEditorCore.PaintedMesh.vertexCount).ToArray();
                        VCPEditorCore.PaintedMesh.SetUVs(2, VCPEditorCore.Cache.Uv2);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool ChangeChannel(ChannelType p_channel)
        {
            if (p_channel == Config.channelType)
                return false;

            if (!IsValidChannel(p_channel, true))
                return false;
            
            // Do this even for same channel to fix serialization on recompile
            VCPMaterials.VertexColorMaterial.SetVector("_ChannelMask",
                new Vector4(p_channel == ChannelType.UV0 ? 1 : 0,
                    p_channel == ChannelType.UV1 ? 1 : 0,
                    p_channel == ChannelType.UV2 ? 1 : 0, 0));

            Config.channelType = p_channel;
            
            VCPEditorCore.Cache.EnumerateSubmeshColors(VCPEditorCore.PaintedMesh);

            return true;
        }
    }
}