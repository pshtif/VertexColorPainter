/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using UnityEditor;
using UnityEngine;

namespace VertexColorPainter.Editor
{
    [Serializable]
    public class VCPEditorConfig : ScriptableObject
    {
        [HideInInspector]
        public bool enabled = false;

        [HideInInspector]
        public bool enableClosestPaint = false;

        [HideInInspector]
        public ChannelType channelType;
        
        [HideInInspector]
        public ToolType toolType;

        [HideInInspector]
        public float brushSize = 1;

        [HideInInspector]
        public Color brushColor = Color.white;

        [HideInInspector]
        public bool lockToSubmesh = false;
        
        [HideInInspector]
        public bool autoFill = false;

        [HideInInspector]
        public bool previousOutlineSetting = false;

        [HideInInspector] 
        public bool enableUv0Editing = false;

        [HideInInspector] 
        public bool overlayRender = true;

        [HideInInspector]
        public bool autoMeshFraming = false;

        [HideInInspector]
        public Color colorChangeCurrent = Color.white;
        
        [HideInInspector]
        public Color colorChangeNew = Color.white;

        public float brushOutlineSize = 0.004f;

        public float forcedMinBrushSize = 1f;

        public float forcedMaxBrushSize = 10f;

        public Material vertexColorMaterial;

        public static VCPEditorConfig Create()
        {
            var config = (VCPEditorConfig)AssetDatabase.LoadAssetAtPath(
                "Assets/Resources/Editor/VertexColorPainterEditorConfig.asset",
                typeof(VCPEditorConfig));

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<VCPEditorConfig>();
                if (config != null)
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Resources");
                    }

                    if (!AssetDatabase.IsValidFolder("Assets/Resources/Editor"))
                    {
                        AssetDatabase.CreateFolder("Assets/Resources", "Editor");
                    }

                    AssetDatabase.CreateAsset(config, "Assets/Resources/Editor/VertexColorPainterEditorConfig.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            return config;
        }
    }
}