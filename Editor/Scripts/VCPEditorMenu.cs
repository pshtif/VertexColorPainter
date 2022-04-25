/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.IO;
using UnityEditor;
using UnityEngine;

namespace VertexColorPainter.Editor
{
    public class VCPEditorMenu
    {
        static VCPEditorCore Core => VCPEditorCore.Instance; 
        
        [MenuItem("Tools/Vertex Color Painter/Enabled")]
        private static void ToggleEnabled()
        {
            Core.Config.enabled = !Core.Config.enabled;
        }
        
        [MenuItem("Tools/Vertex Color Painter/Settings")]
        private static void ShowSettings()
        {
            VCPEditorWindow.InitEditorWindow();
        }

        [MenuItem("Tools/Vertex Color Painter/Enabled", true)]
        // [MenuItem("Tools/Vertex Color Painter/Closest Paint", true)]
        // [MenuItem("Tools/Vertex Color Painter/Auto Mesh Isolation (Experimental)", true)]
        // [MenuItem("Tools/Vertex Color Painter/Auto Mesh Framing", true)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked("Tools/Vertex Color Painter/Enabled", Core.Config.enabled);
            // Menu.SetChecked("Tools/Vertex Color Painter/Closest Paint", Core.Config.enableClosestPaint);
            // Menu.SetChecked("Tools/Vertex Color Painter/Auto Mesh Isolation (Experimental)", Core.Config.autoMeshIsolation);
            // Menu.SetChecked("Tools/Vertex Color Painter/Auto Mesh Framing", Core.Config.autoMeshFraming);
            return true;
        }
        
        /**
         *  ASSET MENU ITEMS
         */
        
        [MenuItem("Assets/Create VCPAsset")]
        private static void CreateVCPAsset()
        {
            VCPAsset.CreateFromFBX(Selection.activeObject as GameObject);
        }
        
        [MenuItem("Assets/Create VCPAsset", true)]
        private static bool CreateVCPAssetValidation()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return Selection.activeObject is GameObject && Path.GetExtension(path) == ".fbx";
        }
    }
}