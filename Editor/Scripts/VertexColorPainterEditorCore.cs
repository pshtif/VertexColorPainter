
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
    public class VertexColorPainterEditorCore
    {
        const string VERSION = "0.1.5";

        static private Material _vertexColorMaterial;

        static public Material VertexColorMaterial
        {
            get
            {
                if (_vertexColorMaterial == null)
                {
                    _vertexColorMaterial = new Material(Shader.Find("Hidden/VertexColorPainter/VertexColorShader"));
                }

                return _vertexColorMaterial;
            }
        }
        
        static private Material _selectionMaterial;

        static public Material SelectionMaterial
        {
            get
            {
                if (_selectionMaterial == null)
                {
                    _selectionMaterial = new Material(Shader.Find("Hidden/VertexColorPainter/SelectionShader"));
                }

                return _selectionMaterial;
            }
        }

        static private GameObject _previousSelected;
        static private Vector3 _previousSceneViewPivot;
        static private Quaternion _previousSceneViewRotation;
        static private float _previousSceneViewSize;

        static public VertexColorPainterEditorConfig Config { get; private set; }

        static private MeshFilter _paintedMesh;

        static private RaycastHit _mouseRaycastHit;
        static private Vector3 _lastMousePosition;
        static private Transform _mouseHitTransform;
        static private Mesh _mouseHitMesh;

        static public int[] CachedIndices { get; private set; }
        static public Vector3[] CachedVertices { get; private set; }
        static public Color[] CachedColors;
        static public List<Color> SubmeshColors { get; private set; }
        static public List<string> SubmeshNames { get; private set; }
        static private int _selectedSubmesh = 0;
        static private string[] _cachedBrushTypeNames;
        
        static private bool _meshIsolationEnabled = false;

        static VertexColorPainterEditorCore()
        {
            CreateConfig();
            
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            
            Undo.undoRedoPerformed -= UndoRedoCallback;
            Undo.undoRedoPerformed += UndoRedoCallback;
        }

        static void UndoRedoCallback()
        {
            if (_paintedMesh == null || !Config.enabled)
                return;

            _paintedMesh.sharedMesh.colors = _paintedMesh.sharedMesh.colors;
            CachedColors = _paintedMesh.sharedMesh.colors;
            
            EditorUtility.SetDirty(_paintedMesh.sharedMesh);
            SceneView.RepaintAll();
        }

        static void CreateConfig()
        {
            Config = (VertexColorPainterEditorConfig)AssetDatabase.LoadAssetAtPath(
                "Assets/Resources/Editor/VertexColorPainterEditorConfig.asset",
                typeof(VertexColorPainterEditorConfig));

            if (Config == null)
            {
                Config = ScriptableObject.CreateInstance<VertexColorPainterEditorConfig>();
                if (Config != null)
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Resources");
                    }

                    if (!AssetDatabase.IsValidFolder("Assets/Resources/Editor"))
                    {
                        AssetDatabase.CreateFolder("Assets/Resources", "Editor");
                    }

                    AssetDatabase.CreateAsset(Config, "Assets/Resources/Editor/VertexColorPainterEditorConfig.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

        private static void OnSceneGUI(SceneView p_sceneView)
        {
            if (!Config.enabled)
                return;

            DrawGUI(p_sceneView);

            if (_paintedMesh == null)
                return;

            if (Selection.activeGameObject != _paintedMesh.gameObject)
                Selection.activeGameObject = _paintedMesh.gameObject;

            Tools.current = Tool.None;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (Event.current.isMouse)
            {
                HandleMouseHit(p_sceneView);
            }

            if (_mouseHitTransform != _paintedMesh.transform)
                return;
            
            switch (Config.brushType)
            {
                case BrushType.PAINT:
                    PaintTool.Handle(_mouseHitTransform, _mouseRaycastHit, _paintedMesh,
                        _selectedSubmesh);
                    break;
                case BrushType.FILL:
                    FillTool.Handle(_mouseRaycastHit, _paintedMesh);
                    break;
            }
        }

        private static void DrawGUI(SceneView p_sceneView)
        {
            if (_paintedMesh == null)
            {
                if (Selection.activeGameObject?.GetComponent<MeshFilter>() != null)
                {
                    DrawDisabledGUI(p_sceneView);
                }
            }
            else
            {
                DrawEnabledGUI(p_sceneView);
            }
        }

        private static void DrawDisabledGUI(SceneView p_sceneView)
        {
            Handles.BeginGUI();

            var rect = p_sceneView.camera.GetScaledPixelRect();
            
            
            if (GUI.Button(new Rect(5, rect.height - 25, 120, 20), "Enable Paiting"))
            {
                EnablePainting(Selection.activeGameObject.GetComponent<MeshFilter>());
            }
            
            Handles.EndGUI();
        }

        private static void DrawEnabledGUI(SceneView p_sceneView)
        {
            var rect = p_sceneView.camera.GetScaledPixelRect();

            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = Texture2D.whiteTexture; // must be white to tint properly
            GUI.color = new Color(0, 0, 0, .45f);
            
            // Handles.BeginGUI();
            //
            // GUI.Box(rect, "", style);
            //
            // Handles.EndGUI();

            // TODO move to a separate function
            if (_meshIsolationEnabled)
            {
                GL.Clear(true, true, Color.black);
                VertexColorMaterial.SetPass(0);
                Graphics.DrawMeshNow(_paintedMesh.sharedMesh, _paintedMesh.transform.localToWorldMatrix);
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
            }
            
            GUILayout.Space(space);

            style = new GUIStyle("label");
            style.fontStyle = FontStyle.Bold;

            if (_cachedBrushTypeNames == null)
                _cachedBrushTypeNames = ((BrushType[])Enum.GetValues(typeof(BrushType))).Select(t => t.ToString()).ToArray();

            GUILayout.Label("Brush Type: ", style, GUILayout.Width(80));
            Config.brushType = (BrushType)EditorGUILayout.Popup((int)Config.brushType, _cachedBrushTypeNames, GUILayout.Width(120));

            switch (Config.brushType)
            {
                case BrushType.PAINT:
                    PaintTool.DrawGUI(space);
                    break;
                case BrushType.FILL:
                    FillTool.DrawGUI(space, _paintedMesh);
                    break;
            }

            GUILayout.FlexibleSpace();

            _meshIsolationEnabled = GUILayout.Toggle(_meshIsolationEnabled, "Isolate Mesh");
            
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
            
            Handles.EndGUI();
        }

        public static void EnablePainting(MeshFilter p_meshFilter)
        {
            Config.previousOutlineSetting = AnnotationUtilityUtil.showSelectedOutline;
            AnnotationUtilityUtil.showSelectedOutline = false;
            
            _paintedMesh = p_meshFilter;
            var mesh = _paintedMesh.sharedMesh;
            CachedIndices = mesh.triangles;
            CachedVertices = mesh.vertices;
            CachedColors = mesh.colors;
            _selectedSubmesh = 0;

            Config.brushSize = Math.Min(Config.forcedMaxBrushSize, Math.Max(Config.forcedMinBrushSize, Config.brushSize));

            EnumerateSubmeshes();

            if (_paintedMesh.GetComponent<MeshRenderer>() && Config.autoMeshFraming)
            {
                //_usedFraming = true;
                Frame();
            }

            _meshIsolationEnabled = Config.autoMeshIsolation;
            SceneView.duringSceneGui += OnSceneGUI;

            if (_paintedMesh.gameObject.GetComponent<PaintedMeshFilter>() == null)
            {
                //if (AssetDatabase.Contains(_paintedMesh.sharedMesh))
                //{
                    Mesh tempMesh = (Mesh)UnityEngine.Object.Instantiate(_paintedMesh.sharedMesh);

                    var path = AssetDatabase.GetAssetPath(_paintedMesh.sharedMesh);
                    if (path.EndsWith(".fbx"))
                    {
                    //     if (EditorUtility.DisplayDialog("Mesh Changes", "Do you want to export modified mesh as asset?",
                    //         "Export", "No"))
                    //     {
                        var name = path.Substring(0, path.Length-4).Substring(path.LastIndexOf("/")+1);
                        path = path.Substring(0, path.LastIndexOf("/") + 1) + name + "_painted.asset";
                        MeshUtility.Optimize(tempMesh);
                        AssetDatabase.CreateAsset(tempMesh, path);
                        AssetDatabase.SaveAssets();
                    //     }
                    }

                    PaintedMeshFilter pmf = _paintedMesh.gameObject.AddComponent<PaintedMeshFilter>();
                    pmf.SetOriginalAsset(_paintedMesh.sharedMesh);
                    
                    _paintedMesh.sharedMesh = tempMesh;
                //}
            }
        }

        private static void Frame()
        {
            var view = SceneView.lastActiveSceneView;
            StoreSceneViewCamera(view);
            view.Frame(_paintedMesh.GetComponent<MeshRenderer>().bounds, false);
        }
        
        public static void DisablePainting()
        {
            AnnotationUtilityUtil.showSelectedOutline = Config.previousOutlineSetting;
            _paintedMesh = null;

            SceneView.duringSceneGui -= OnSceneGUI;
        }

        static void HandleMouseHit(SceneView p_sceneView)
        {
            RaycastHit hit;

            if (EditorRaycast.RaycastWorld(Event.current.mousePosition, out hit, out _mouseHitTransform,
                out _mouseHitMesh, null, new [] {_paintedMesh.gameObject}))
            {
                _mouseRaycastHit = hit;
            }
        }

        static void EnumerateSubmeshes()
        {
            SubmeshColors = new List<Color>();
            SubmeshNames = new List<string>();
            Mesh mesh = _paintedMesh.sharedMesh;

            if (CachedColors == null || CachedColors.Length < mesh.vertexCount)
            {
                CachedColors = Enumerable.Repeat(Color.white, mesh.vertexCount).ToArray();
            }

            mesh.colors = CachedColors;

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                SubMeshDescriptor desc = mesh.GetSubMesh(i);
                SubmeshColors.Add(CachedColors[mesh.triangles[desc.indexStart]]);
                SubmeshNames.Add("Submesh " + i);
            }
        }

        static void StoreSceneViewCamera(SceneView p_sceneView)
        {
            _previousSceneViewPivot = p_sceneView.pivot;
            _previousSceneViewRotation = p_sceneView.rotation;
            _previousSceneViewSize = p_sceneView.size;
        }
        
        static void RestoreSceneViewCamera(SceneView p_sceneView)
        {
            p_sceneView.pivot = _previousSceneViewPivot;
            p_sceneView.rotation = _previousSceneViewRotation;
            p_sceneView.size = _previousSceneViewSize;
            p_sceneView.Repaint();
        }
    }
}