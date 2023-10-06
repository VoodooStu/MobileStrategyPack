using System;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Editor
{
    public class VoodooConfirmBuildWindow : EditorWindow
        {
            private String _text;
            private BuildPlayerOptions _buildPlayerOptions;

            private GUIStyle _labelStyle;

            private GUIStyle _titleStyle;

            private Action _buildAction;
            private Action _canceledBuildAction;

            public static void Show(String text, BuildPlayerOptions builderPlayerOptions, Action buildAction, Action canceledBuildAction)
            {
                var window = GetWindow<VoodooConfirmBuildWindow>();
                window._buildAction = buildAction;
                window._canceledBuildAction = canceledBuildAction;
                window.titleContent = new GUIContent("Confirm Build");
                //window.maxSize = window.minSize = new Vector2(450, 250);
                window.InitializeValues(text, builderPlayerOptions);
                window.Show();
            }

            private void InitializeValues(String text, BuildPlayerOptions builderPlayerOptions)
            {
                _text = text;
                _buildPlayerOptions = builderPlayerOptions;

                _titleStyle = new GUIStyle {
                    normal = {
                        textColor = Color.red
                    },
                    fontSize = 25,
                    padding = new RectOffset(10,10,10,10)
                };

                _labelStyle = new GUIStyle {
                    wordWrap = true, 
                    normal = {
                        textColor = Color.white
                    },
                    padding = new RectOffset(5,5,5,5)
                };

            }

            private void OnGUI()
            {
                GUILayout.BeginVertical();

                GUILayout.Label("Unity version issue", _titleStyle); 

                GUILayout.Label(_text, _labelStyle);

                if (GUILayout.Button("I confirm.  Continue the build")) {
                    _buildAction.Invoke();
                    Close();
                }

                if (GUILayout.Button("Cancel Build")) {
                    _canceledBuildAction.Invoke();
                    Close(); 
                } 

                GUILayout.EndVertical(); 

            }

        }
    }

