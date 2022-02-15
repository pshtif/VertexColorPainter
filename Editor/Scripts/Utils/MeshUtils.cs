/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;
using UnityEngine.Rendering;

namespace VertexColorPainter.Editor
{
    public class MeshUtils
    {
        public static int GetSubMeshFromVertexIndex(Mesh p_mesh, int p_vertexIndex)
        {
            if (p_mesh.subMeshCount == 1)
                return 0;

            for (int i = 0; i < p_mesh.subMeshCount; i++)
            {
                SubMeshDescriptor desc = p_mesh.GetSubMesh(i);

                if (p_vertexIndex >= desc.indexStart && p_vertexIndex < desc.indexStart + desc.indexCount)
                    return i;
            }

            return -1;
        }
        
        public static SubMeshDescriptor[] GetSubMeshDescriptors(Mesh p_mesh)
        {
            if (p_mesh == null)
                return new SubMeshDescriptor[0];
            
            SubMeshDescriptor[] descriptors = new SubMeshDescriptor[p_mesh.subMeshCount];

            for (int i = 0; i < descriptors.Length; i++)
            {
                descriptors[i] = p_mesh.GetSubMesh(i);
            }

            return descriptors;
        }
        
        public static Color[] GetSubMeshColors(Mesh p_mesh)
        {
            var p_colors = new Color[p_mesh.subMeshCount];

            var cachedColors = p_mesh.colors;
            
            for (int i = 0; i < p_colors.Length; i++)
            {
                SubMeshDescriptor desc = p_mesh.GetSubMesh(i);
                p_colors[i] = cachedColors[p_mesh.triangles[desc.indexStart]];
            }

            return p_colors;
        }

        public static int GetSubMeshIndexFromTriangle(Mesh p_mesh, int p_triangleIndex)
        {
            var indices = p_mesh.triangles;
            int i1 = indices[p_triangleIndex * 3];
            int i2 = indices[p_triangleIndex * 3 + 1];
            int i3 = indices[p_triangleIndex * 3 + 2];

            for (int i = 0; i < p_mesh.subMeshCount; i++)
            {
                var subMeshIndices = p_mesh.GetTriangles(i);
                for (int j = 0; j < subMeshIndices.Length; j+=3)
                {
                    if (subMeshIndices[j] == i1 && subMeshIndices[j + 1] == i2 && subMeshIndices[j + 2] == i3)
                        return i;
                }
            }

            Debug.LogWarning("Invalid triangle index.");
            return -1;
        }
    }
}