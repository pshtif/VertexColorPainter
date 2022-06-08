/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEditor;
using UnityEngine;

namespace VertexColorPainter.Editor
{
    public class VCPSettingsWindow : EditorWindow
    {
        public GUISkin Skin => (GUISkin)Resources.Load("Skins/VertexColorPainterSkin");

        private Vector2 _scrollPosition;

        public static VCPSettingsWindow Instance { get; private set; } 
        
        public static VCPSettingsWindow InitEditorWindow()
        {
            Instance = GetWindow<VCPSettingsWindow>();
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

            EditorGUILayout.LabelField("Vertex Color Painter Settings", style, GUILayout.Height(28));
            GUILayout.Space(4);

            GUI.color = new Color(1, 0.5f, 0);
            if (GUILayout.Button(VCPEditorCore.Config.enabled ? "DISABLE" : "ENABLE", GUILayout.Height(32)))
            {
                VCPEditorCore.Config.enabled = !VCPEditorCore.Config.enabled;
            }
            GUILayout.Space(4);
            GUI.color = Color.white;

            EditorGUI.BeginChangeCheck();
            
            if (VCPEditorCore.CurrentTool != null)
            {
                VCPEditorCore.CurrentTool.DrawSettingsGUI();
                
                GUILayout.Space(4);
            }

            EditorGUILayout.LabelField("Settings", Skin.GetStyle("settingslabel"), GUILayout.Height(24));

            VCPEditorCore.Config.autoMeshFraming =
                GUILayout.Toggle(VCPEditorCore.Config.autoMeshFraming, new GUIContent("Automatically Frame Painted Meshes", "Upon enabling painting automatically frame the painted mesh in view."));
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(VCPEditorCore.Config);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}