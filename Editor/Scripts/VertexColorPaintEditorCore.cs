
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
    public class VertexColorPaintEditorCore
    {
        const string VERSION = "0.1.1";

        static private Material _vertexColorMaterial;
        
        static private GameObject _previousSelected;
        static private Vector3 _previousSceneViewPivot;
        static private Quaternion _previousSceneViewRotation;
        static private float _previousSceneViewSize;
        static private bool _usedFraming = false;

        static public VertexColorPainterEditorConfig Config { get; private set; }

        static private MeshFilter _paintedMesh;

        static private RaycastHit _mouseRaycastHit;
        static private Vector3 _mousePosition;
        static private Vector3 _lastMousePosition;
        static private Transform _mouseHitTransform;
        static private Mesh _mouseHitMesh;

        static private int[] _cachedIndices;
        static private Vector3[] _cachedVertices;
        static private Color[] _cachedColors;
        static private List<Color> _submeshColors;
        static private List<string> _submeshNames;
        static private int _selectedSubmesh = 0;
        static private string[] _cachedBrushTypeNames;

        static private float _minBrushSize = 1;
        static private float _maxBrushSize = 10;

        static private bool _meshIsolationEnabled = false;

        static VertexColorPaintEditorCore()
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
            _cachedColors = _paintedMesh.sharedMesh.colors;
            
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

            switch (Config.brushType)
            {
                case BrushType.PAINT:
                    HandlePaintBrush();
                    break;
                case BrushType.FILL:
                    HandleFillBrush();
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
            
            var rect = p_sceneView.camera.pixelRect;
            
            if (GUI.Button(new Rect(5, rect.height - 25, 120, 20), "Enable Paiting"))
            {
                EnablePainting(Selection.activeGameObject.GetComponent<MeshFilter>());
            }
            
            Handles.EndGUI();
        }

        private static void DrawEnabledGUI(SceneView p_sceneView)
        {
            var rect = p_sceneView.camera.pixelRect;

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
                if (_vertexColorMaterial == null)
                {
                    _vertexColorMaterial = new Material(Shader.Find("Hidden/VertexColorPainter/VertexColorShader"));
                }

                GL.Clear(true, true, Color.black);
                _vertexColorMaterial.SetPass(0);
                Graphics.DrawMeshNow(_paintedMesh.sharedMesh, _paintedMesh.transform.localToWorldMatrix);
            }

            Handles.BeginGUI();

            int space = 8;
            
            GUI.Box(new Rect(0, rect.height - 30, rect.width, 30), "", style);

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
            
            GUILayout.Space(space);
            
            GUILayout.Label("Brush Color: ", style, GUILayout.Width(80));
            Config.brushColor = EditorGUILayout.ColorField(Config.brushColor, GUILayout.Width(60));

            GUILayout.Space(space);

            if (Config.brushType == BrushType.PAINT)
            {
                GUILayout.Label("Brush Size: ", style, GUILayout.Width(80));
                Config.brushSize =
                    EditorGUILayout.Slider(Config.brushSize, _minBrushSize, _maxBrushSize, GUILayout.Width(200));

                GUILayout.Space(space);
            }

            if (_submeshColors.Count > 1 && Config.brushType != BrushType.FILL)
            {
                GUILayout.Space(space);

                GUILayout.Label("Lock to Submesh: ", style, GUILayout.Width(110));
                Config.lockToSubmesh = EditorGUILayout.Toggle(Config.lockToSubmesh, GUILayout.Width(20));

                if (Config.lockToSubmesh)
                {
                    GUILayout.Label("Submesh: ", style, GUILayout.Width(65));
                    _selectedSubmesh =
                        EditorGUILayout.Popup(_selectedSubmesh, _submeshNames.ToArray(), GUILayout.Width(120));
                }
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
            _cachedIndices = mesh.triangles;
            _cachedVertices = mesh.vertices;
            _cachedColors = mesh.colors;
            _selectedSubmesh = 0;
            
            if (Config.forceMinMaxBrushSize)
            {
                _minBrushSize = Config.forcedMinBrushSize;
                _maxBrushSize = Config.forcedMaxBrushSize;
            }

            Config.brushSize = Math.Min(_maxBrushSize, Math.Max(_minBrushSize, Config.brushSize));

            EnumerateSubmeshes();

            if (_paintedMesh.GetComponent<MeshRenderer>() && Config.autoMeshFraming)
            {
                _usedFraming = true;
                Frame();
            }

            _meshIsolationEnabled = Config.autoMeshIsolation;
            SceneView.duringSceneGui += OnSceneGUI;

            if (_paintedMesh.gameObject.GetComponent<PaintedMeshFilter>() == null)
            {
                //if (AssetDatabase.Contains(_paintedMesh.sharedMesh))
                {
                    Mesh tempMesh = (Mesh)UnityEngine.Object.Instantiate(_paintedMesh.sharedMesh);
                    tempMesh.name = _paintedMesh.sharedMesh.name;

                    // var path = AssetDatabase.GetAssetPath(_paintedMesh.sharedMesh);
                    // if (path.EndsWith(".fbx"))
                    // {
                    //     if (EditorUtility.DisplayDialog("Mesh Changes", "Do you want to export modified mesh as asset?",
                    //         "Export", "No"))
                    //     {
                    //         path = path.Substring(0, path.LastIndexOf("/") + 1) + mesh.name + "_painted.asset";
                    //         MeshUtility.Optimize(tempMesh);
                    //         AssetDatabase.CreateAsset(tempMesh, path);
                    //         AssetDatabase.SaveAssets();
                    //     }
                    // }

                    _paintedMesh.sharedMesh = tempMesh;
                    _paintedMesh.gameObject.AddComponent<PaintedMeshFilter>();
                }
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
            // if (_usedFraming)
            // {
            //     _usedFraming = false;
            //     RestoreSceneViewCamera(SceneView.lastActiveSceneView);
            // }

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
                _mousePosition = hit.point;
                _mouseRaycastHit = hit;
            }
        }


        static void HandlePaintBrush()
        {
            if (_mouseHitTransform == _paintedMesh.transform)
            {
                var rotation = Quaternion.LookRotation(_mouseRaycastHit.normal);
                Handles.color = Color.white;
                var gizmoSize = HandleUtility.GetHandleSize(_mouseRaycastHit.point) / 10f;
                //Handles.ArrowHandleCap(3, _mouseRaycastHit.point, rotation, Config.brushSize, EventType.Repaint);
                //Handles.CircleHandleCap(2, _mousePosition, rotation, Config.brushSize, EventType.Repaint);
                Handles.DrawSolidDisc(_mousePosition, _mouseRaycastHit.normal, gizmoSize*Config.brushSize+gizmoSize/5);
                Handles.color = Config.brushColor;
                Handles.DrawSolidDisc(_mousePosition, _mouseRaycastHit.normal, gizmoSize*Config.brushSize);

                if (Event.current.button == 0 && !Event.current.alt && Event.current.type == EventType.MouseDown)
                {
                    Undo.RegisterCompleteObjectUndo(_paintedMesh.sharedMesh, "Paint Color");
                }
                
                if (Event.current.button == 0 && !Event.current.alt && (Event.current.type == EventType.MouseDrag ||
                                                                        Event.current.type == EventType.MouseDown))
                {
                    PaintVertices(_mouseRaycastHit);
                }

                if (Event.current.button == 0 && !Event.current.alt && Event.current.type == EventType.MouseUp)
                {
                    EditorUtility.SetDirty(_paintedMesh);
                }
            }
        }
        
        static void HandleFillBrush()
        {
            if (_mouseHitTransform == _paintedMesh.transform)
            {
                var rotation = Quaternion.LookRotation(_mouseRaycastHit.normal);
                var gizmoSize = HandleUtility.GetHandleSize(_mouseRaycastHit.point) / 10f;
                //Handles.ArrowHandleCap(3, _mouseRaycastHit.point, rotation, _minBrushSize+(_maxBrushSize-_minBrushSize), EventType.Repaint);
                Handles.color = Color.white;
                //Handles.CircleHandleCap(2, _mousePosition, rotation, _minBrushSize+_minBrushSize/10f, EventType.Repaint);
                Handles.DrawSolidDisc(_mousePosition, _mouseRaycastHit.normal, gizmoSize+gizmoSize/5);
                Handles.color = Config.brushColor;
                Handles.DrawSolidDisc(_mousePosition, _mouseRaycastHit.normal, gizmoSize);

                if (Event.current.button == 0 && !Event.current.alt && (Event.current.type == EventType.MouseDrag ||
                                                                        Event.current.type == EventType.MouseDown))
                {
                    FillColor(_mouseRaycastHit);
                }
            }
        }

        static void PaintVertices(RaycastHit p_hit)
        {
            if (_cachedVertices == null)
                return;

            if (Config.lockToSubmesh)
            {
                Mesh mesh = _paintedMesh.sharedMesh;
                SubMeshDescriptor desc = _paintedMesh.sharedMesh.GetSubMesh(_selectedSubmesh);
                
                for (int i = 0; i < desc.indexCount; i++)
                {
                    int index = _cachedIndices[i + desc.indexStart];
                    if (Vector3.Distance(_mouseHitTransform.TransformPoint(_cachedVertices[index]), p_hit.point) <
                        Config.brushSize)
                    {
                        _cachedColors[index] = Config.brushColor;
                    }
                }
            }
            else
            {
                for (int i = 0; i < _cachedVertices.Length; i++)
                {
                    if (Vector3.Distance(_mouseHitTransform.TransformPoint(_cachedVertices[i]), p_hit.point) <
                        Config.brushSize)
                    {
                        _cachedColors[i] = Config.brushColor;
                    }
                }
            }

            _paintedMesh.sharedMesh.colors = _cachedColors;
        }

        static void EnumerateSubmeshes()
        {
            _submeshColors = new List<Color>();
            _submeshNames = new List<string>();
            Mesh mesh = _paintedMesh.sharedMesh;

            if (_cachedColors == null || _cachedColors.Length < mesh.vertexCount)
            {
                _cachedColors = Enumerable.Repeat(Color.white, mesh.vertexCount).ToArray();
            }

            mesh.colors = _cachedColors;

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                SubMeshDescriptor desc = mesh.GetSubMesh(i);
                _submeshColors.Add(_cachedColors[mesh.triangles[desc.indexStart]]);
                _submeshNames.Add("Submesh " + i);
            }
        }

        static void FillColor(RaycastHit p_hit)
        {
            Undo.RegisterCompleteObjectUndo(_paintedMesh.sharedMesh, "Fill Color");
            Mesh mesh = _paintedMesh.sharedMesh;

            if (mesh.subMeshCount > 1)
            {
                int firstVertexIndex = p_hit.triangleIndex * 3;
                int submeshIndex = GetSubMeshFromVertexIndex(mesh, firstVertexIndex);
                
                SubMeshDescriptor desc = mesh.GetSubMesh(submeshIndex);

                for (int j = 0; j < desc.indexCount; j++)
                {
                    _cachedColors[mesh.triangles[desc.indexStart + j]] = Config.brushColor;
                }
            }
            else
            {
                _cachedColors = Enumerable.Repeat(Config.brushColor, _cachedColors.Length).ToArray();
            }
            
            mesh.colors = _cachedColors;

            EditorUtility.SetDirty(_paintedMesh);
        }

        static int GetSubMeshFromVertexIndex(Mesh p_mesh, int p_vertexIndex)
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