
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using VertexColorPainter.Runtime;

namespace VertexColorPainter.Editor
{
    [InitializeOnLoad]
    public class VCPEditorCore
    {
        const string VERSION = "0.6.2";
        
        public GUISkin Skin => (GUISkin)Resources.Load("Skins/VertexColorPainterSkin");
        
        private static VCPEditorCore _instance;
        public static VCPEditorCore Instance => _instance;
        
        private Material _vertexColorMaterial;

        public Material VertexColorMaterial
        {
            get
            {
                if (_vertexColorMaterial == null)
                {
                    _vertexColorMaterial = new Material(Shader.Find("Hidden/Vertex Color Painter/VertexColorPainterShader"));
                    _vertexColorMaterial.SetVector("_ChannelMask",
                        new Vector4(Config.channelType == ChannelType.UV0 ? 1 : 0,
                            Config.channelType == ChannelType.UV1 ? 1 : 0,
                            Config.channelType == ChannelType.UV2 ? 1 : 0, 0));
                }

                return _vertexColorMaterial;
            }
        }
        
        private Material _selectionMaterial; 
        public Material SelectionMaterial
        {
            get
            {
                if (_selectionMaterial == null)
                {
                    _selectionMaterial = new Material(Shader.Find("Hidden/Vertex Color Painter/SelectionShader"));
                }

                return _selectionMaterial;
            }
        }

        private GameObject _previousSelected;
        private Vector3 _previousSceneViewPivot;
        private Quaternion _previousSceneViewRotation;
        private float _previousSceneViewSize;

        public VCPEditorConfig Config { get; private set; }

        private PaintedMeshType _paintedType;
        
        private Mesh _paintedMesh;
        public Mesh PaintedMesh => _paintedMesh;

        private GameObject _paintedObject;
        public GameObject PaintedObject => _paintedObject;

        // Caches
        public int[] CachedIndices { get; private set; }
        public Vector3[] CachedVertices { get; private set; }
        public Color[] CachedColors;
        public Vector4[] CachedUv0;
        public Vector4[] CachedUv1;
        public Vector4[] CachedUv2;
        public List<Color> SubmeshColors { get; private set; }
        public List<string> SubmeshNames { get; private set; }
        
        private string[] _cachedToolTypeNames;
        private string[] _cachedChannelTypeNames;

        private ToolBase _currentTool;

        static VCPEditorCore()
        {
            _instance = new VCPEditorCore();
        }

        public VCPEditorCore()
        {
            Config = VCPEditorConfig.Create();
            
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            
            Undo.undoRedoPerformed -= UndoRedoCallback;
            Undo.undoRedoPerformed += UndoRedoCallback;
        }

        void CacheEnumLabels()
        {
            // This is hacky
            var names = ((ChannelType[])Enum.GetValues(typeof(ChannelType))).Select(t => t.ToString()).ToList();
            if (!Config.enableUv0Editing)
            {
                names.Remove("UV0");
            }
            _cachedChannelTypeNames = names.ToArray();

            if (_cachedToolTypeNames == null)
                _cachedToolTypeNames = ((ToolType[])Enum.GetValues(typeof(ToolType))).Select(t => t.ToString()).ToArray();
        }

        void UndoRedoCallback()
        {
            if (_paintedMesh == null || !Config.enabled)
                return;

            // Yes Unity hack
            _paintedMesh.colors = _paintedMesh.colors;
            CachedColors = _paintedMesh.colors;

            EditorUtility.SetDirty(_paintedMesh);
            SceneView.RepaintAll();
        }

        private void OnSceneGUI(SceneView p_sceneView)
        {
            if (!Config.enabled)
                return;
            
            if (_paintedObject != null && _paintedMesh != null)
            {
                if (!IsValidChannel(Config.channelType, false))
                {
                    ChangeChannel(ChannelType.COLOR);
                }
                
                // if (Selection.activeGameObject != _paintedMesh.gameObject)
                //     Selection.activeGameObject = _paintedMesh.gameObject;
                
                InvalidateCurrentTool();

                Tools.current = Tool.None;
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                var rect = p_sceneView.camera.GetScaledPixelRect();
                rect = new Rect(0, rect.height - 55, rect.width, 55);
                
                // Don't draw tool under gui
                if (!rect.Contains(Event.current.mousePosition))
                {
                    _currentTool?.HandleMouseHit(p_sceneView);
                }
            }

            DrawGUI(p_sceneView);
        }

        private void CacheVertexAttributes()
        {
            CachedIndices = _paintedMesh.triangles;
            CachedVertices = _paintedMesh.vertices;
            
            // If mesh is missing color data create it 
            if (_paintedMesh.colors == null || _paintedMesh.colors.Length < _paintedMesh.vertexCount)
            {
                Debug.Log("Paiting mesh without color data so we added it.");
                _paintedMesh.colors = Enumerable.Repeat(Color.white, _paintedMesh.vertexCount).ToArray();
            }
            
            CachedColors = _paintedMesh.colors;
            
            List<Vector4> uvs = new List<Vector4>();
            _paintedMesh.GetUVs(0, uvs);
            CachedUv0 = uvs.ToArray();
            uvs.Clear();
            _paintedMesh.GetUVs(1, uvs);
            CachedUv1 = uvs.ToArray();
            uvs.Clear();
            _paintedMesh.GetUVs(2, uvs);
            CachedUv2 = uvs.ToArray();
        }

        private void DrawGUI(SceneView p_sceneView)
        {
            if (_paintedMesh == null)
            {
                if (Selection.activeGameObject?.GetComponent<MeshFilter>() != null ||
                    Selection.activeGameObject?.GetComponent<SkinnedMeshRenderer>() != null) 
                {
                    DrawDisabledGUI(p_sceneView);
                }
            }
            else
            {
                if (_paintedObject != null)
                {
                    DrawEnabledGUI(p_sceneView);
                }
                else
                {
                    DisablePainting();
                }
            }
        }

        private void DrawDisabledGUI(SceneView p_sceneView)
        {
            Handles.BeginGUI();

            var rect = p_sceneView.camera.GetScaledPixelRect();
            
            GUI.color = new Color(1, .5f, 0);
            if (GUI.Button(new Rect(5, rect.height - 25, 120, 20), "Enable Paiting"))
            {
                EnablePainting(Selection.activeGameObject);
            }
            GUI.color = Color.white;
            
            Handles.EndGUI();
        }

        private void DrawEnabledGUI(SceneView p_sceneView)
        {
            var rect = p_sceneView.camera.GetScaledPixelRect();

            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = Texture2D.whiteTexture; // must be white to tint properly
            GUI.color = new Color(0, 0, 0, .4f);
            
            // Handles.BeginGUI();
            //
            // GUI.Box(rect, "", style);
            //
            // Handles.EndGUI();

            // TODO move to a separate function
            if (Config.overlayRender)
            {
                //GL.Clear(true, false, Color.black);
                //VertexColorMaterial.SetPass(0);
                //Graphics.DrawMeshNow(_paintedMesh, _paintedObject.transform.localToWorldMatrix);
                for (int i = 0; i < _paintedMesh.subMeshCount; i++)
                {
                    Graphics.DrawMesh(_paintedMesh, _paintedObject.transform.localToWorldMatrix, VertexColorMaterial,
                        0, p_sceneView.camera, i);
                }
            }

            Handles.BeginGUI();
            
            GUI.Box(new Rect(0, rect.height - 30, rect.width , 30), "", style);
            
            GUI.color = Color.white;
            
            int space = 8;

            if (GUI.Button(new Rect(rect.width-125, rect.height - 55, 120, 20),
                    (Config.overlayRender ? "Disable" : "Enable") + " Overlay")) 
            {
                Config.overlayRender = !Config.overlayRender;
                return;
            }

            GUI.color = new Color(1, .5f, 0);
            if (GUI.Button(new Rect(5, rect.height - 55, 120, 20), "Disable Painting"))
            {
                DisablePainting();
                return;
            }
            GUI.color = Color.white;

            GUILayout.BeginArea(new Rect(5, rect.height - 22, rect.width - 5, 20));
            GUILayout.BeginHorizontal();
            
            style = new GUIStyle("label");
            style.fontStyle = FontStyle.Bold;
            
            CacheEnumLabels();

            GUILayout.Label("Channel: ", style, GUILayout.Width(60));

            HandleChannelSelection();

            GUILayout.Space(space);

            GUILayout.Label("Brush Type: ", style, GUILayout.Width(80));
            Config.toolType = (ToolType)EditorGUILayout.Popup((int)Config.toolType, _cachedToolTypeNames, GUILayout.Width(120));

            _currentTool.DrawGUI(p_sceneView);

            GUILayout.FlexibleSpace();

            //_meshIsolationEnabled = GUILayout.Toggle(_meshIsolationEnabled, "Isolate Mesh");
            
            GUILayout.Space(space);
            
            if (GUILayout.Button("Frame", GUILayout.Width(60)))
            {
                Frame();
            }
            
            style.fontStyle = FontStyle.Normal;
            style.normal.textColor = Color.gray;
            GUILayout.Label("VertexColorPainter v"+VERSION, style);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            
            _currentTool.DrawHelpGUI(p_sceneView);
            
            GUILayout.BeginArea(new Rect(rect.width / 2 - 500, rect.height-75, 1000, 30));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var s = new GUIStyle();
            s.normal.textColor = Color.white;
            GUILayout.Label("Painting object: ", s);
            s.normal.textColor = Color.green;
            s.fontStyle = FontStyle.Bold;
            GUILayout.Label(_paintedObject.name,s);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            
            Handles.EndGUI();
        }

        void HandleChannelSelection()
        {
            int channelType = (!Config.enableUv0Editing && (int)Config.channelType > 0)
                ? (int)Config.channelType - 1
                : (int)Config.channelType;
            
            EditorGUI.BeginChangeCheck();
            
            channelType = EditorGUILayout.Popup(channelType, _cachedChannelTypeNames, GUILayout.Width(80));
            
            if (EditorGUI.EndChangeCheck())
            {
                channelType = (!Config.enableUv0Editing && channelType > 0) ? channelType + 1 : channelType;
                ChangeChannel((ChannelType)channelType);
            }
        }
        
        private bool IsValidChannel(ChannelType p_channel, bool p_showDialog)
        {
            List<Vector4> uvs = new List<Vector4>();
            if (p_channel == ChannelType.UV0)
            {
                if (!Config.enableUv0Editing)
                    return false;
                
                _paintedMesh.GetUVs(0, uvs);
                if (uvs.Count == 0)
                {
                    if (p_showDialog && EditorUtility.DisplayDialog("UV0 Channel Not Found",
                            "UV0 channel is missing in this mesh do you want to create it?", "YES", "NO"))
                    {
                        CachedUv0 = Enumerable.Repeat(Vector4.zero, _paintedMesh.vertexCount).ToArray();
                        _paintedMesh.SetUVs(0, CachedUv0);
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
                _paintedMesh.GetUVs(1, uvs);
                if (uvs.Count == 0)
                {
                    if (p_showDialog && EditorUtility.DisplayDialog("UV1 Channel Not Found",
                            "UV1 channel is missing in this mesh do you want to create it?", "YES", "NO"))
                    {
                        CachedUv1 = Enumerable.Repeat(Vector4.zero, _paintedMesh.vertexCount).ToArray();
                        _paintedMesh.SetUVs(1, CachedUv1);
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
                _paintedMesh.GetUVs(2, uvs);
                if (uvs.Count == 0)
                {
                    if (p_showDialog && EditorUtility.DisplayDialog("UV2 Channel Not Found",
                            "UV2 channel is missing in this mesh do you want to create it?", "YES", "NO"))
                    {
                        CachedUv2 = Enumerable.Repeat(Vector4.zero, _paintedMesh.vertexCount).ToArray();
                        _paintedMesh.SetUVs(2, CachedUv2);
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

        private void ChangeChannel(ChannelType p_channel)
        {
            if (p_channel == Config.channelType)
                return;
            
            if (!IsValidChannel(p_channel, true))
                return;
            
            // Do this even for same channel to fix serialization on recompile
            VertexColorMaterial.SetVector("_ChannelMask",
                new Vector4(p_channel == ChannelType.UV0 ? 1 : 0,
                    p_channel == ChannelType.UV1 ? 1 : 0,
                    p_channel == ChannelType.UV2 ? 1 : 0, 0));

            Config.channelType = p_channel;
            
            EnumerateSubmeshColors();
        }
        
        public void InvalidateCurrentTool()
        {
            switch (Config.toolType)
            {
                case ToolType.PAINT:
                    if (_currentTool?.GetType() != typeof(PaintTool)) _currentTool = new PaintTool();
                    break;
                case ToolType.FILL:
                    if (_currentTool?.GetType() != typeof(FillTool)) _currentTool = new FillTool();
                    break;
                case ToolType.COLOR:
                    if (_currentTool?.GetType() != typeof(ColorTool)) _currentTool = new ColorTool();
                    break;
            }
        }

        public void EnablePainting(GameObject p_object)
        {
            _paintedObject = p_object;
            
            if (_paintedObject.GetComponent<MeshFilter>() != null)
            {
                _paintedType = PaintedMeshType.STATIC;
                _paintedMesh = _paintedObject.GetComponent<MeshFilter>().sharedMesh;
            } else if (_paintedObject.GetComponent<SkinnedMeshRenderer>() != null)
            {
                _paintedType = PaintedMeshType.SKINNED;
                _paintedMesh = _paintedObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            }
            else
            {
                _paintedType = PaintedMeshType.NONE;
                return;
            }

            Config.previousOutlineSetting = AnnotationUtilityUtil.showSelectedOutline;
            AnnotationUtilityUtil.showSelectedOutline = false;

            CacheVertexAttributes();

            EnumerateSubmeshColors();

            if (_paintedObject.GetComponent<MeshRenderer>() && Config.autoMeshFraming)
            {
                Frame();
            }

            _paintedMesh = SaveToVCPAsset(_paintedMesh, AssetDatabase.GetAssetPath(_paintedMesh));

            switch (_paintedType)
            {
                case PaintedMeshType.STATIC:
                    _paintedObject.GetComponent<MeshFilter>().sharedMesh = _paintedMesh;
                    break;
                case PaintedMeshType.SKINNED:
                    _paintedObject.GetComponent<SkinnedMeshRenderer>().sharedMesh = _paintedMesh;
                    break;
            }
            
            Config.brushSize = Math.Min(Config.forcedMaxBrushSize, Math.Max(Config.forcedMinBrushSize, Config.brushSize));
            
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public Mesh SaveToVCPAsset(Mesh p_mesh, string p_path = null)
        {
            VCPAsset asset = AssetDatabase.LoadAssetAtPath<VCPAsset>(p_path);

            // If it is already VCPAsset abort
            if (asset != null)
                return p_mesh;

            asset = VCPAsset.CreateFromMesh(p_mesh, p_path);

            return asset.mesh;
        }

        private void Frame()
        {
            var view = SceneView.lastActiveSceneView;
            StoreSceneViewCamera(view);

            switch (_paintedType)
            {
                case PaintedMeshType.STATIC:
                    view.Frame(_paintedObject.GetComponent<MeshRenderer>().bounds, false);
                    break;
                case PaintedMeshType.SKINNED:
                    view.Frame(_paintedObject.GetComponent<SkinnedMeshRenderer>().bounds, false);
                    break;
            }
        }
        
        public void DisablePainting()
        {
            AnnotationUtilityUtil.showSelectedOutline = Config.previousOutlineSetting;
            _paintedMesh = null;
            _paintedObject = null;

            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void EnumerateSubmeshColors()
        {
            SubmeshColors = new List<Color>();
            SubmeshNames = new List<string>();

            for (int i = 0; i < _paintedMesh.subMeshCount; i++)
            {
                SubMeshDescriptor desc = _paintedMesh.GetSubMesh(i);
                SubmeshColors.Add(GetColorAtIndex(_paintedMesh.triangles[desc.indexStart]));
                SubmeshNames.Add("Submesh " + i);
            }
        }

        void StoreSceneViewCamera(SceneView p_sceneView)
        {
            _previousSceneViewPivot = p_sceneView.pivot;
            _previousSceneViewRotation = p_sceneView.rotation;
            _previousSceneViewSize = p_sceneView.size;
        }
        
        void RestoreSceneViewCamera(SceneView p_sceneView)
        {
            p_sceneView.pivot = _previousSceneViewPivot;
            p_sceneView.rotation = _previousSceneViewRotation;
            p_sceneView.size = _previousSceneViewSize;
            p_sceneView.Repaint();
        }

        public Color GetColorAtIndex(int p_index)
        {
            switch (Config.channelType)
            {
                case ChannelType.COLOR:
                    return CachedColors[p_index];
                case ChannelType.UV0:
                    return CachedUv0[p_index];
                case ChannelType.UV1:
                    return CachedUv1[p_index];
                case ChannelType.UV2:
                    return CachedUv2[p_index];
            }

            return Color.black;
        }

        public void SetColorAtIndex(int p_index, Color p_color)
        {
            switch (Config.channelType)
            {
                case ChannelType.COLOR:
                    CachedColors[p_index] = p_color;
                    break;
                case ChannelType.UV0:
                    CachedUv0[p_index] = p_color;
                    break;
                case ChannelType.UV1:
                    CachedUv1[p_index] = p_color;
                    break;
                case ChannelType.UV2:
                    CachedUv2[p_index] = p_color;
                    break;
            }
        }

        public void SetAllColors(Color p_color)
        {
            switch (Config.channelType)
            {
                case ChannelType.COLOR:
                    CachedColors = Enumerable.Repeat(Config.brushColor, CachedColors.Length).ToArray();
                    break;
                case ChannelType.UV0:
                    CachedUv0 = Enumerable.Repeat((Vector4)Config.brushColor, CachedColors.Length).ToArray();
                    break;
                case ChannelType.UV1:
                    CachedUv0 = Enumerable.Repeat((Vector4)Config.brushColor, CachedColors.Length).ToArray();
                    break;
                case ChannelType.UV2:
                    CachedUv0 = Enumerable.Repeat((Vector4)Config.brushColor, CachedColors.Length).ToArray();
                    break;
            }
        }

        public Color[] GetAllColors()
        {
            switch (Config.channelType)
            {
                case ChannelType.COLOR:
                    return CachedColors;
                case ChannelType.UV0:
                    return CachedUv0.Select(v => (Color)v).ToArray();
                case ChannelType.UV1:
                    return CachedUv1.Select(v => (Color)v).ToArray();
                case ChannelType.UV2:
                    return CachedUv2.Select(v => (Color)v).ToArray();
            }

            return null;
        }

        public void InvalidateMeshColors()
        {
            switch (Config.channelType)
            {
                case ChannelType.COLOR:
                    _paintedMesh.colors = CachedColors;
                    break;
                case ChannelType.UV0:
                    _paintedMesh.SetUVs(0, CachedUv0);
                    break;
                case ChannelType.UV1:
                    _paintedMesh.SetUVs(1, CachedUv1);
                    break;
                case ChannelType.UV2:
                    _paintedMesh.SetUVs(2, CachedUv2);
                    break;
            }
        }
    }
}