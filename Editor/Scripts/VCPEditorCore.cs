
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
        const string VERSION = "0.5.3";
        
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
                    _vertexColorMaterial = new Material(Shader.Find("Hidden/Vertex Color Painter/VertexColorShader"));
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
        public List<Color> SubmeshColors { get; private set; }
        public List<string> SubmeshNames { get; private set; }
        private string[] _cachedBrushTypeNames;

        private bool _meshIsolationEnabled = false;

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

        void UndoRedoCallback()
        {
            if (_paintedMesh == null || !Config.enabled)
                return;

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
                // if (Selection.activeGameObject != _paintedMesh.gameObject)
                //     Selection.activeGameObject = _paintedMesh.gameObject;
                
                InvalidateCurrentTool();

                Tools.current = Tool.None;
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                var rect = p_sceneView.camera.GetScaledPixelRect();
                rect = new Rect(0, rect.height - 30, rect.width, 30);
                
                // Don't draw tool under gui
                //if (!rect.Contains(Event.current.mousePosition))
                {
                    _currentTool?.HandleMouseHit(p_sceneView);
                }
            }

            DrawGUI(p_sceneView);
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
            
            
            if (GUI.Button(new Rect(5, rect.height - 25, 120, 20), "Enable Paiting"))
            {
                EnablePainting(Selection.activeGameObject);
            }
            
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
            if (_meshIsolationEnabled)
            {
                //GL.Clear(true, true, Color.black);
                //VertexColorMaterial.SetPass(0);
                //Graphics.DrawMeshNow(_paintedMesh.sharedMesh, _paintedMesh.transform.localToWorldMatrix);
            }

            Handles.BeginGUI();

            int space = 8;
            
            GUI.Box(new Rect(0, rect.height - 30, rect.width , 30), "", style);

            GUILayout.BeginArea(new Rect(5, rect.height - 22, rect.width - 5, 20));
            GUILayout.BeginHorizontal();
            
            GUI.color = Color.white;

            if (GUILayout.Button("Disable Painting", GUILayout.Width(120)))
            {
                DisablePainting();
                return;
            }
            
            GUILayout.Space(space);

            style = new GUIStyle("label");
            style.fontStyle = FontStyle.Bold;

            if (_cachedBrushTypeNames == null)
                _cachedBrushTypeNames = ((ToolType[])Enum.GetValues(typeof(ToolType))).Select(t => t.ToString()).ToArray();

            GUILayout.Label("Brush Type: ", style, GUILayout.Width(80));
            Config.toolType = (ToolType)EditorGUILayout.Popup((int)Config.toolType, _cachedBrushTypeNames, GUILayout.Width(120));

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
            
            CachedIndices = _paintedMesh.triangles;
            CachedVertices = _paintedMesh.vertices;
            CachedColors = _paintedMesh.colors;

            Config.brushSize = Math.Min(Config.forcedMaxBrushSize, Math.Max(Config.forcedMinBrushSize, Config.brushSize));

            EnumerateSubmeshes();

            // if (_paintedMesh.GetComponent<MeshRenderer>() && Config.autoMeshFraming)
            // {
            //     //_usedFraming = true;
            //     Frame();
            // }

            _meshIsolationEnabled = Config.autoMeshIsolation;
            SceneView.duringSceneGui += OnSceneGUI;
            
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
        }

        public Mesh SaveToVCPAsset(Mesh p_mesh, string p_path = null)
        {
            VCPAsset asset = AssetDatabase.LoadAssetAtPath<VCPAsset>(p_path);

            // If it is already VCPAsset abort
            if (asset != null)
                return p_mesh;

            Mesh tempMesh = (Mesh)UnityEngine.Object.Instantiate(p_mesh);
            MeshUtility.Optimize(tempMesh);

            if (!String.IsNullOrEmpty(p_path))
            {
                p_path = p_path.ToLower();
                var name = p_path.Substring(0, p_path.Length - 4).Substring(p_path.LastIndexOf("/") + 1) + "_" +
                           p_mesh.name;
                p_path = p_path.Substring(0, p_path.LastIndexOf("/") + 1) + name + "_painted.asset";
                tempMesh.name = name + "_painted";
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
            
            asset = ScriptableObject.CreateInstance<VCPAsset>();
            asset.mesh = tempMesh;
            //asset.SetOriginalAsset(p_mesh);
            
            AssetDatabase.CreateAsset(asset, p_path);
            AssetDatabase.AddObjectToAsset(tempMesh, asset);
            AssetDatabase.SaveAssets();

            return tempMesh;
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

        void EnumerateSubmeshes()
        {
            SubmeshColors = new List<Color>();
            SubmeshNames = new List<string>();

            if (CachedColors == null || CachedColors.Length < _paintedMesh.vertexCount)
            {
                CachedColors = Enumerable.Repeat(Color.white, _paintedMesh.vertexCount).ToArray();
            }

            _paintedMesh.colors = CachedColors;

            for (int i = 0; i < _paintedMesh.subMeshCount; i++)
            {
                SubMeshDescriptor desc = _paintedMesh.GetSubMesh(i);
                SubmeshColors.Add(CachedColors[_paintedMesh.triangles[desc.indexStart]]);
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
    }
}