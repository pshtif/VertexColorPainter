/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VertexColorPainter.Editor
{
    public class FBXUtils
    {
        public static void ExportToFBX(Mesh p_mesh)
        {
            Type[] types = AppDomain.CurrentDomain.GetAssemblies().First(x =>
                    x.FullName == "Unity.Formats.Fbx.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")
                .GetTypes();
            Type optionsInterfaceType = types.First(x => x.Name == "IExportOptions");
            Type optionsType = types.First(x => x.Name == "ExportOptionsSettingsSerializeBase");

            MethodInfo optionsProperty = typeof(ModelExporter)
                .GetProperty("DefaultOptions", BindingFlags.Static | BindingFlags.NonPublic).GetGetMethod(true);
            object optionsInstance = optionsProperty.Invoke(null, null);

            FieldInfo exportFormatField =
                optionsType.GetField("exportFormat", BindingFlags.Instance | BindingFlags.NonPublic);
            exportFormatField.SetValue(optionsInstance, 1);

            MethodInfo exportObjectMethod = typeof(ModelExporter).GetMethod("ExportObject",
                BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder,
                new Type[] { typeof(string), typeof(Object), optionsInterfaceType }, null);


            var filter = new GameObject().AddComponent<MeshFilter>();
            filter.sharedMesh = GameObject.Instantiate(p_mesh);
            var renderer = filter.gameObject.AddComponent<MeshRenderer>();
            var materials = new List<Material>();
            for (int i = 0; i < filter.sharedMesh.subMeshCount; i++)
            {
                materials.Add(new Material(Shader.Find("Standard")));
            }

            renderer.materials = materials.ToArray();
            
            var path = EditorUtility.SaveFilePanel("FBX", Application.dataPath, p_mesh.name, "fbx");
            if (path != null && path.Length > 0)
            {
                exportObjectMethod.Invoke(null, new object[] { path, filter.gameObject, optionsInstance });
            }
        }
    }
}