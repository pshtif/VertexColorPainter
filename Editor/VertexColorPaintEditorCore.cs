
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VertexColorPainter
{
    [InitializeOnLoad]
    public class VertexColorPaintEditorCore
    {
        const string VERSION = "0.1.0";
        
        static private GameObject _previousSelected;

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

        static private float _minBrushSize = 0;
        static private float _maxBrushSize = 1;

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

            HandleBrush();
        }

        private static void DrawGUI(SceneView p_sceneView)
        {
            Handles.BeginGUI();

            if (_paintedMesh == null)
            {
                if (Selection.activeGameObject?.GetComponent<MeshFilter>() != null)
                {
                    DrawEnablePaitingGUI(p_sceneView);
                }
            }
            else
            {
                DrawPaitingGUI(p_sceneView);
            }

            Handles.EndGUI();
        }

        private static void DrawEnablePaitingGUI(SceneView p_sceneView)
        {
            var rect = p_sceneView.camera.pixelRect;
            
            if (GUI.Button(new Rect(5, rect.height - 25, 120, 20), "Enable Paiting"))
            {
                EnablePainting(Selection.activeGameObject.GetComponent<MeshFilter>());
            }
        }

        private static void DrawPaitingGUI(SceneView p_sceneView)
        {
            var rect = p_sceneView.camera.pixelRect;

            int space = 8;

            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = Texture2D.whiteTexture; // must be white to tint properly
            GUI.color = new Color(0, 0, 0, .4f);
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
            
            GUILayout.Label("Brush Color: ", style, GUILayout.Width(80));
            Config.brushColor = EditorGUILayout.ColorField(Config.brushColor, GUILayout.Width(60));

            GUILayout.Space(space);
            //Config.container = (GameObject)EditorGUILayout.ObjectField(Config.container, typeof(GameObject), true, GUILayout.Width(200));

            GUILayout.Label("Brush Size: ", style, GUILayout.Width(80));
            Config.brushSize = EditorGUILayout.Slider(Config.brushSize, _minBrushSize, _maxBrushSize, GUILayout.Width(200));

            GUILayout.Space(space);

            if (GUILayout.Button("Fill Color", GUILayout.Width(100)))
            {
                FillMeshColor(Config.brushColor, Config.lockToSubmesh ? _selectedSubmesh : -1);
            }

            if (_submeshColors.Count > 1)
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
            style.fontStyle = FontStyle.Normal;
            style.normal.textColor = Color.gray;
            GUILayout.Label("VertexColorPainter v"+VERSION, style);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
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
            
            var bounds = mesh.bounds;
            _maxBrushSize = Mathf.Min(bounds.size.x, bounds.size.y) / 4;
            _minBrushSize = _maxBrushSize / 10;

            Config.brushSize = Math.Min(_maxBrushSize, Math.Max(_minBrushSize, Config.brushSize));

            EnumerateSubmeshes();

            SceneView.duringSceneGui += OnSceneGUI;
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
                out _mouseHitMesh))
            {
                _mousePosition = hit.point;
                _mouseRaycastHit = hit;
            }
        }


        static void HandleBrush()
        {
            if (_mouseHitTransform == _paintedMesh.transform)
            {
                var rotation = Quaternion.LookRotation(_mouseRaycastHit.normal);
                Handles.ArrowHandleCap(3, _mouseRaycastHit.point, rotation, Config.brushSize, EventType.Repaint);
                Handles.CircleHandleCap(2, _mousePosition, rotation, Config.brushSize, EventType.Repaint);
                Handles.color = !Event.current.shift ? new Color(0, 1, 0, .2f) : new Color(1, 0, 0, .2f);
                Handles.DrawSolidDisc(_mousePosition, _mouseRaycastHit.normal, Config.brushSize);

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

        static void FillMeshColor(Color p_color, int p_submeshIndex)
        {
            Undo.RegisterCompleteObjectUndo(_paintedMesh.sharedMesh, "Fill Color");
            Mesh mesh = _paintedMesh.sharedMesh;

            if (p_submeshIndex > -1)
            {
                SubMeshDescriptor desc = mesh.GetSubMesh(p_submeshIndex);

                for (int j = 0; j < desc.indexCount; j++)
                {
                    _cachedColors[mesh.triangles[desc.indexStart + j]] = p_color;
                }
            }
            else
            {
                _cachedColors = Enumerable.Repeat(p_color, _cachedColors.Length).ToArray();
            }
            
            mesh.colors = _cachedColors;
            EditorUtility.SetDirty(_paintedMesh);
        }
    }
}