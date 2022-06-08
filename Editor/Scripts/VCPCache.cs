/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace VertexColorPainter.Editor
{
    public class VCPCache
    {
        public int[] Indices;
        public Vector3[] Vertices;
        public Color[] Colors;
        public Vector4[] Uv0;
        public Vector4[] Uv1;
        public Vector4[] Uv2;
        public List<Color> SubmeshColors;
        public string[] SubmeshNames;
        
        public void CacheVertexAttributes(Mesh p_mesh)
        {
            Indices = p_mesh.triangles;
            Vertices = p_mesh.vertices;
            
            // If mesh is missing color data create it 
            if (p_mesh.colors == null || p_mesh.colors.Length < p_mesh.vertexCount)
            {
                Debug.Log("Paiting mesh without color data so we added it.");
                p_mesh.colors = Enumerable.Repeat(Color.white, p_mesh.vertexCount).ToArray();
            }
            
            Colors = p_mesh.colors;
            
            List<Vector4> uvs = new List<Vector4>();
            p_mesh.GetUVs(0, uvs);
            Uv0 = uvs.ToArray();
            uvs.Clear();
            p_mesh.GetUVs(1, uvs);
            Uv1 = uvs.ToArray();
            uvs.Clear();
            p_mesh.GetUVs(2, uvs);
            Uv2 = uvs.ToArray();
        }
        
        public Color GetColorAtIndex(int p_index, ChannelType p_channelType)
        {
            switch (p_channelType)
            {
                case ChannelType.COLOR:
                    return Colors[p_index];
                case ChannelType.UV0:
                    return Uv0[p_index];
                case ChannelType.UV1:
                    return Uv1[p_index];
                case ChannelType.UV2:
                    return Uv2[p_index];
            }

            return Color.black;
        }

        public void SetColorAtIndex(int p_index, Color p_color, ChannelType p_channelType)
        {
            switch (p_channelType)
            {
                case ChannelType.COLOR:
                    Colors[p_index] = p_color;
                    break;
                case ChannelType.UV0:
                    Uv0[p_index] = p_color;
                    break;
                case ChannelType.UV1:
                    Uv1[p_index] = p_color;
                    break;
                case ChannelType.UV2:
                    Uv2[p_index] = p_color;
                    break;
            }
        }

        public void SetAllColors(Color p_color, ChannelType p_channelType)
        {
            switch (p_channelType)
            {
                case ChannelType.COLOR:
                    Colors = Enumerable.Repeat(p_color, Colors.Length).ToArray();
                    break;
                case ChannelType.UV0:
                    Uv0 = Enumerable.Repeat((Vector4)p_color, Colors.Length).ToArray();
                    break;
                case ChannelType.UV1:
                    Uv0 = Enumerable.Repeat((Vector4)p_color, Colors.Length).ToArray();
                    break;
                case ChannelType.UV2:
                    Uv0 = Enumerable.Repeat((Vector4)p_color, Colors.Length).ToArray();
                    break;
            }
        }

        public Color[] GetAllColors(ChannelType p_channelType)
        {
            switch (p_channelType)
            {
                case ChannelType.COLOR:
                    return Colors;
                case ChannelType.UV0:
                    return Uv0.Select(v => (Color)v).ToArray();
                case ChannelType.UV1:
                    return Uv1.Select(v => (Color)v).ToArray();
                case ChannelType.UV2:
                    return Uv2.Select(v => (Color)v).ToArray();
            }

            return null;
        }

        public void InvalidateMeshColors(Mesh p_mesh, ChannelType p_channelType)
        {
            switch (p_channelType)
            {
                case ChannelType.COLOR:
                    p_mesh.colors = Colors;
                    break;
                case ChannelType.UV0:
                    p_mesh.SetUVs(0, Uv0);
                    break;
                case ChannelType.UV1:
                    p_mesh.SetUVs(1, Uv1);
                    break;
                case ChannelType.UV2:
                    p_mesh.SetUVs(2, Uv2);
                    break;
            }
        }
        
        public void EnumerateSubmeshColors(Mesh p_mesh)
        {
            SubmeshColors = new List<Color>();
            var names = new List<string>();

            for (int i = 0; i < p_mesh.subMeshCount; i++)
            {
                SubMeshDescriptor desc = p_mesh.GetSubMesh(i);
                SubmeshColors.Add(GetColorAtIndex(p_mesh.triangles[desc.indexStart], VCPEditorCore.Config.channelType));
                names.Add("Submesh " + i);
            }

            SubmeshNames = names.ToArray();
        }
    }
}