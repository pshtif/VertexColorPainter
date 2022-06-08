
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VertexColorPainter.Editor
{
    [InitializeOnLoad]
    public static class VCPEditorCore
    {
        private static GameObject _previousSelected;
        
        public static string VERSION = "0.7.0";
        public static GUISkin Skin => (GUISkin)Resources.Load("Skins/VertexColorPainterSkin");

        public static VCPEditorConfig Config { get; }

        public static PaintedMeshType PaintedType { get; private set; }
        
        public static Mesh PaintedMesh { get; private set; }
        
        public static GameObject PaintedObject { get; private set; }

        public static VCPCache Cache { get; }

        public static ToolBase CurrentTool { get; private set; }

        static VCPEditorCore()
        {
            Config = VCPEditorConfig.Create();

            VCPSceneGUI.Initialize();
            Cache = new VCPCache();

            Undo.undoRedoPerformed -= UndoRedoCallback;
            Undo.undoRedoPerformed += UndoRedoCallback;
        }

        static void UndoRedoCallback()
        {
            if (PaintedMesh == null || !Config.enabled)
                return;

            // Yes Unity hack
            PaintedMesh.colors = PaintedMesh.colors;
            Cache.Colors = PaintedMesh.colors;

            EditorUtility.SetDirty(PaintedMesh);
            SceneView.RepaintAll();
        }

        public static void InvalidateCurrentTool()
        {
            switch (Config.toolType)
            {
                case ToolType.PAINT:
                    if (CurrentTool?.GetType() != typeof(PaintTool)) CurrentTool = new PaintTool();
                    break;
                case ToolType.FILL:
                    if (CurrentTool?.GetType() != typeof(FillTool)) CurrentTool = new FillTool();
                    break;
                case ToolType.COLOR:
                    if (CurrentTool?.GetType() != typeof(ColorTool)) CurrentTool = new ColorTool();
                    break;
            }
        }

        public static void EnablePainting(GameObject p_object)
        {
            PaintedObject = p_object;

            if (PaintedObject.GetComponent<MeshFilter>() != null)
            {
                PaintedType = PaintedMeshType.STATIC;
                PaintedMesh = PaintedObject.GetComponent<MeshFilter>().sharedMesh;
            } else if (PaintedObject.GetComponent<SkinnedMeshRenderer>() != null)
            {
                PaintedType = PaintedMeshType.SKINNED;
                PaintedMesh = PaintedObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            }
            else
            {
                PaintedType = PaintedMeshType.NONE;
                return;
            }

            Config.previousOutlineSetting = AnnotationUtilityUtil.showSelectedOutline;
            AnnotationUtilityUtil.showSelectedOutline = false;

            Cache.CacheVertexAttributes(PaintedMesh);

            if (!VCPChannel.IsValidChannel(Config.channelType, false))
            {
                VCPChannel.ChangeChannel(ChannelType.COLOR);
            } else {
                Cache.EnumerateSubmeshColors(PaintedMesh);
            }

            // if (PaintedObject.GetComponent<MeshRenderer>() && Config.autoMeshFraming)
            // {
            //     Frame();
            // }

            PaintedMesh = VCPAsset.SaveToVCPAsset(PaintedMesh, AssetDatabase.GetAssetPath(PaintedMesh));

            switch (PaintedType)
            {
                case PaintedMeshType.STATIC:
                    PaintedObject.GetComponent<MeshFilter>().sharedMesh = PaintedMesh;
                    break;
                case PaintedMeshType.SKINNED:
                    PaintedObject.GetComponent<SkinnedMeshRenderer>().sharedMesh = PaintedMesh;
                    break;
            }
            
            Config.brushSize = Math.Min(Config.forcedMaxBrushSize, Math.Max(Config.forcedMinBrushSize, Config.brushSize));
        }

        public static void DisablePainting()
        {
            AnnotationUtilityUtil.showSelectedOutline = Config.previousOutlineSetting;
            PaintedMesh = null;
            PaintedObject = null;
        }
    }
}