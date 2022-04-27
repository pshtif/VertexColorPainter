/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VertexColorPainter.Editor
{
    public class VCPAsset : ScriptableObject
    {
        public Mesh mesh;

        public Mesh GetMesh(int p_index)
        {
            Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this)).ToList()
                .FindAll(o => o is Mesh).ToArray();
            return objs.Length > p_index ? (Mesh)objs[p_index] : null;
        }

        static public VCPAsset CreateFromMesh(Mesh p_mesh, string p_path = null)
        {
            Mesh tempMesh = (Mesh)UnityEngine.Object.Instantiate(p_mesh);
            MeshUtility.Optimize(tempMesh);

            if (!String.IsNullOrEmpty(p_path))
            {
                //p_path = p_path.ToLower();
                //var name = p_path.Substring(0, p_path.Length - 4).Substring(p_path.LastIndexOf("/") + 1) + "_" + p_mesh.name;
                //p_path = p_path.Substring(0, p_path.LastIndexOf("/") + 1) + name + "_painted.asset";
                //tempMesh.name = name + "_painted";
                p_path = p_path.Substring(0, p_path.LastIndexOf("/") + 1) + p_mesh.name + ".asset";
                tempMesh.name = p_mesh.name;
            }
            else
            {
                if (!AssetDatabase.IsValidFolder("Assets/Painted"))
                {
                    AssetDatabase.CreateFolder("Assets", "Painted");
                }
                p_path = "Assets/Painted/"+p_mesh.name + ".asset";
                tempMesh.name = p_mesh.name;
            }
            
            VCPAsset asset = ScriptableObject.CreateInstance<VCPAsset>();
            asset.mesh = tempMesh;

            AssetDatabase.CreateAsset(asset, p_path);
            AssetDatabase.AddObjectToAsset(tempMesh, asset);
            AssetDatabase.SaveAssets();

            return asset;
        }
        
        static public VCPAsset CreateFromFBX(GameObject p_fbxAsset)
        {
            Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(p_fbxAsset));

            var path = AssetDatabase.GetAssetPath(p_fbxAsset);
            path = path.Substring(0, path.LastIndexOf(".")) + ".asset";
            VCPAsset asset = ScriptableObject.CreateInstance<VCPAsset>();
            AssetDatabase.CreateAsset(asset, path);

            foreach(Object o in objs){
                if (o is Mesh)
                {
                    // Clone mesh as asset cannot be added to two different assets at the same time
                    Mesh temp = (Mesh)UnityEngine.Object.Instantiate(o);
                    temp.name = o.name;
                    AssetDatabase.AddObjectToAsset(temp, asset);
                }
            }
            
            AssetDatabase.SaveAssets();

            return asset;
        }
    }
}