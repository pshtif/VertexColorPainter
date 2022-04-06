/*
 *	Created by:  Peter @sHTiF Stefcek
 */

namespace VertexColorPainter.Editor
{
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    public class EditorRaycast
    {
        static private MethodInfo _internalRaycast;

        static void CacheUnityInternalCall()
        {
            var handleUtility = typeof(Editor).Assembly.GetTypes().FirstOrDefault(t => t.Name == "HandleUtility");
            _internalRaycast =
                handleUtility.GetMethod("IntersectRayMesh", (BindingFlags.Static | BindingFlags.NonPublic));
        }

        public static bool Raycast(Ray ray, MeshFilter meshFilter, out RaycastHit hit)
        {
            if (_internalRaycast == null) CacheUnityInternalCall();

            return Raycast(ray, meshFilter.mesh, meshFilter.transform.localToWorldMatrix, out hit);
        }

        private static bool Raycast(Ray ray, Mesh mesh, Matrix4x4 matrix, out RaycastHit hit)
        {
            if (_internalRaycast == null) CacheUnityInternalCall();

            var parameters = new object[] { ray, mesh, matrix, null };
            bool result = (bool)_internalRaycast.Invoke(null, parameters);
            hit = (RaycastHit)parameters[3];
            return result;
        }

        // Taken from Unity codebase for object raycasting in sceneview - sHTiF
        public static bool RaycastWorld_OBSOLETE(Vector2 p_position, out RaycastHit p_hit, out Transform p_transform,
            out Mesh p_mesh, GameObject[] p_ignore, GameObject[] p_filter)
        {
            p_hit = new RaycastHit();
            p_transform = null;
            p_mesh = null;

            GameObject picked = HandleUtility.PickGameObject(p_position, false, p_ignore, p_filter);
            if (!picked)
                return false;

            Ray mouseRay = HandleUtility.GUIPointToWorldRay(p_position);

            MeshFilter[] meshFil = picked.GetComponentsInChildren<MeshFilter>();
            float minT = Mathf.Infinity;
            foreach (MeshFilter mf in meshFil)
            {
                Mesh mesh = mf.sharedMesh;
                if (!mesh)
                    continue;
                RaycastHit localHit;

                if (Raycast(mouseRay, mesh, mf.transform.localToWorldMatrix, out localHit))
                {
                    if (localHit.distance < minT)
                    {
                        p_hit = localHit;
                        p_transform = mf.transform;
                        p_mesh = mesh;
                        minT = p_hit.distance;
                    }
                }
            }

            // Not needing colliders - sHTiF
            // if (minT == Mathf.Infinity)
            // {
            //     Collider[] colliders = picked.GetComponentsInChildren<Collider>();
            //     foreach (Collider col in colliders)
            //     {
            //         RaycastHit localHit;
            //         if (col.Raycast(mouseRay, out localHit, Mathf.Infinity))
            //         {
            //             if (localHit.distance < minT)
            //             {
            //                 p_hit = localHit;
            //                 p_transform = col.transform;
            //                 minT = p_hit.distance;
            //             }
            //         }
            //     }
            // }

            if (minT == Mathf.Infinity)
            {
                //p_hit.point = Vector3.Project(picked.transform.position - mouseRay.origin, mouseRay.direction) + mouseRay.origin;
                return false;
            }

            return true;
        }
        
        public static bool RaycastWorld(Vector2 p_position, out RaycastHit p_hit, out Transform p_transform,
            out Mesh p_mesh, GameObject[] p_ignore, GameObject[] p_filter)
        {
            p_hit = new RaycastHit();
            p_transform = null;
            p_mesh = null;

            MeshFilter[] meshes;
            if (p_filter != null)
            {
                meshes = new MeshFilter[0];
                foreach (var gameObject in p_filter)
                {
                    meshes = meshes.Concat(gameObject.GetComponentsInChildren<MeshFilter>()).ToArray();
                }

                meshes = p_filter[0].GetComponentsInChildren<MeshFilter>();
            }
            else
            {
                meshes = GameObject.FindObjectsOfType<MeshFilter>();
            }

            Ray mouseRay = HandleUtility.GUIPointToWorldRay(p_position);

            float minT = Mathf.Infinity;
            foreach (MeshFilter mf in meshes)
            {
                Mesh mesh = mf.sharedMesh;
                if (!mesh)
                    continue;
                RaycastHit localHit;

                if (Raycast(mouseRay, mesh, mf.transform.localToWorldMatrix, out localHit))
                {
                    if (localHit.distance < minT)
                    {
                        p_hit = localHit;
                        p_transform = mf.transform;
                        p_mesh = mesh;
                        minT = p_hit.distance;
                    }
                }
            }
            
            SkinnedMeshRenderer[] skinnedMeshes = GameObject.FindObjectsOfType<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer sm in skinnedMeshes)
            {
                Mesh mesh = sm.sharedMesh;
                if (!mesh)
                    continue;
                RaycastHit localHit;

                if (Raycast(mouseRay, mesh, sm.transform.localToWorldMatrix, out localHit))
                {
                    if (localHit.distance < minT)
                    {
                        p_hit = localHit;
                        p_transform = sm.transform;
                        p_mesh = mesh;
                        minT = p_hit.distance;
                    }
                }
            }
            
            if (minT == Mathf.Infinity)
            {
                Collider[] colliders = GameObject.FindObjectsOfType<Collider>();
                foreach (Collider col in colliders)
                {
                    RaycastHit localHit;
                    if (col.Raycast(mouseRay, out localHit, Mathf.Infinity))
                    {
                        if (localHit.distance < minT)
                        {
                            p_hit = localHit;
                            p_transform = col.transform;
                            minT = p_hit.distance;
                        }
                    }
                }
            }

            if (minT == Mathf.Infinity)
            {
                //p_hit.point = Vector3.Project(picked.transform.position - mouseRay.origin, mouseRay.direction) + mouseRay.origin;
                return false;
            }

            return true;
        }
    }
}