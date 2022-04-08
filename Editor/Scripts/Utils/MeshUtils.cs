/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Collections.Generic;
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
            if (p_mesh == null)
            {
                return new Color[0];
            }

            var p_colors = new Color[p_mesh.subMeshCount];

            var cachedColors = p_mesh.colors;
            
            for (int i = 0; i < p_colors.Length; i++)
            {
                SubMeshDescriptor desc = p_mesh.GetSubMesh(i);
                p_colors[i] = cachedColors[p_mesh.triangles[desc.indexStart]];
                // Debug.Log(i+" : "+desc.firstVertex+" : "+desc.vertexCount+" : "+desc.baseVertex+" : "+desc.indexStart+" : "+desc.indexCount);
                // Debug.Log(p_mesh.triangles[desc.indexStart]);
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

        public static int GetClosesVertexIndex(Mesh p_mesh, Matrix4x4 p_worldToLocal, RaycastHit p_hit)
        {
            var triangleIndex = p_hit.triangleIndex;
            var indices = p_mesh.triangles;
            var vertices = p_mesh.vertices;
            int i1 = indices[triangleIndex * 3];
            int i2 = indices[triangleIndex * 3 + 1];
            int i3 = indices[triangleIndex * 3 + 2];

            var worldPoint = p_worldToLocal.MultiplyPoint(p_hit.point);

            var d = Vector3.Distance(worldPoint, vertices[i1]);
            var i = i1;
            var nd = Vector3.Distance(worldPoint, vertices[i2]);
            if (nd < d)
            {
                d = nd;
                i = i2;
            }
            
            nd = Vector3.Distance(worldPoint, vertices[i3]);
            if (nd < d)
            {
                i = i3;
            }

            return i;
        }

        public static bool CheckVertexUniqueness(Mesh p_mesh)
        {
            if (p_mesh.subMeshCount == 1)
                return true;
            
            List<int> checkedIndices = new List<int>();
            var triangles = p_mesh.triangles;

            for (int i = 0; i < p_mesh.subMeshCount; i++)
            {
                var desc = p_mesh.GetSubMesh(i);
                for (int j = 0; j < desc.indexCount; j++)
                {
                    var index = triangles[desc.indexStart + j];
                    if (checkedIndices.Contains(index))
                        return false;
                    checkedIndices.Add(index);
                }
            }

            return true;
        }
    }
}