/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEditor;
using UnityEngine;

namespace VertexColorPainter.Editor
{
    public class VCPEditorWindow : EditorWindow
    {
        public VCPEditorCore Core => VCPEditorCore.Instance;
        
        public GUISkin Skin => (GUISkin)Resources.Load("Skins/VertexColorPainterSkin");

        private Vector2 _scrollPosition;

        public static VCPEditorWindow Instance { get; private set; } 
        
        public static VCPEditorWindow InitEditorWindow()
        {
            Instance = GetWindow<VCPEditorWindow>();
            Instance.titleContent = new GUIContent("Vertex Color Painter");
            Instance.minSize = new Vector2(200, 400);

            return Instance;
        }

        void OnEnable() {
            Instance = this;
        }

        public void OnGUI()
        {
            var style = new GUIStyle();
            style.normal.background = TextureUtils.GetColorTexture(new Color(.1f, .1f, .1f));
            style.normal.textColor = new Color(1, 0.5f, 0);
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 14;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Vertex Color Painter Editor", style, GUILayout.Height(28));
            GUILayout.Space(4);

            GUI.color = new Color(1, 0.5f, 0);
            if (GUILayout.Button(Core.Config.enabled ? "DISABLE" : "ENABLE", GUILayout.Height(32)))
            {
                Core.Config.enabled = !Core.Config.enabled;
            }
            GUILayout.Space(4);
            GUI.color = Color.white;

            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.LabelField("Settings", Skin.GetStyle("settingslabel"), GUILayout.Height(24));

            Core.Config.enableUv0Editing =
                GUILayout.Toggle(Core.Config.enableUv0Editing, new GUIContent("Enable Uv0 Editing", "Since Uv0 are used for main texture mapping you can disable its editation option here."));
            
            Core.Config.enableClosestPaint =
                GUILayout.Toggle(Core.Config.enableClosestPaint, new GUIContent("Enable Closest Vertex Painting", "If there are no vertices in the range of brush paint closest vertex outside of range."));
            
            Core.Config.autoMeshFraming =
                GUILayout.Toggle(Core.Config.autoMeshFraming, new GUIContent("Automatically Frame Painted Meshes", "Upon enabling painting automatically frame the painted mesh in view."));
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(Core.Config);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}