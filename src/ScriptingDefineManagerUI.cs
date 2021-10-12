using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Appalachia.CI.Compilation
{
    internal static class ScriptingDefineManagerUI
    {
        [NonSerialized] private static List<string> _defines;
        [NonSerialized] private static ReorderableList _reorderableDefines;
        [NonSerialized] private static bool _definesHaveChanged;

        [SettingsProvider]
        public static SettingsProvider CustomUserPreferences()
        {
            var provider = new SettingsProvider("Preferences/Appalachia/Scripting Defines", SettingsScope.User)
            {
                guiHandler = s => DrawUI(), label = "Scripting Defines"
            };
            return provider;
        }

        private static void DrawUI()
        {
            ScriptingDefineSettings.Initialize();

            EditorGUILayout.HelpBox(
                "When compilation finishes, all assemblies in the project will be scanned.  " +
                "If the filter is enabled, all assemblies not passing the filter will be removed.  " +
                "Those remaining will have an all-caps, underscore-separated version of their assembly " +
                "name added to the Player's scripting defines.",
                MessageType.Info
            );

            DrawWarnIfDisabled();
            DrawEnable();
            DrawFiltered();
            DrawFilterValue();
            DrawExcludeTests();
            EditorGUILayout.Space();
            DrawDefines();
        }

        private static void PostLabel(string message)
        {
            EditorGUILayout.LabelField(
                message,
                EditorStyles.wordWrappedMiniLabel,
                GUILayout.ExpandWidth(true)
            );
        }

        private static void DrawWarnIfDisabled()
        {
            var warn = ScriptingDefineSettings.WarnIfDisabled;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();

            warn = EditorGUILayout.Toggle("Warn If Disabled", warn);

            if (warn)
            {
                PostLabel(
                    "I'm warning you... whenever a compilation finishes, but this utility is disabled."
                );
            }
            else
            {
                PostLabel(
                    "I'm not warning you when a compilation finishes and this utility is disabled.  Except for this warning.  This warning doesn't count."
                );
            }

            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                ScriptingDefineSettings.WarnIfDisabled = warn;
            }
        }

        private static void DrawEnable()
        {
            var enabled = ScriptingDefineSettings.Enabled;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();

            enabled = EditorGUILayout.Toggle("Enabled", enabled);

            if (enabled)
            {
                PostLabel("Scripting defines are being updated with every compilation.");
            }
            else
            {
                PostLabel("Scripting defines are not being updated.");
            }

            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                ScriptingDefineSettings.Enabled = enabled;
            }
        }

        private static void DrawExcludeTests()
        {
            var excludeTests = ScriptingDefineSettings.ExcludeTests;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();

            excludeTests = EditorGUILayout.Toggle("Exclude Tests", excludeTests);

            if (excludeTests)
            {
                PostLabel("Assemblies that contain 'test' will not be giving a scripting define.");
            }
            else
            {
                PostLabel(
                    "Assemblies passing the filter, even those that contain 'test', will be giving a scripting define.  This is not inherently bad but this use case is not apparent.  Probably leave it enabled.  Or not.  Its your life."
                );
            }

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                ScriptingDefineSettings.ExcludeTests = excludeTests;
            }
        }

        private static void DrawFiltered()
        {
            var filtered = ScriptingDefineSettings.Filtered;
            var filterValue = ScriptingDefineSettings.FilterValue;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();

            filtered = EditorGUILayout.Toggle("Filtered", filtered);

            if (filtered)
            {
                PostLabel(
                    $"Assemblies that contain [{filterValue}] will be giving a scripting define."
                );
            }
            else
            {
                PostLabel(
                    "If enabled but not filtered, all assemblies will be given a scripting define.  This is not inherently bad, but there will be a lot of defines.."
                );
            }

            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                ScriptingDefineSettings.Filtered = filtered;
            }
        }

        private static void DrawFilterValue()
        {
            var filterValue = ScriptingDefineSettings.FilterValue;

            EditorGUI.BeginChangeCheck();
            filterValue = EditorGUILayout.DelayedTextField("Filter Value", filterValue);

            if (EditorGUI.EndChangeCheck())
            {
                ScriptingDefineSettings.FilterValue = filterValue;
            }
        }

        private static void DrawDefines()
        {
            RefreshDefineState();

            GUILayout.Label("Scripting Define Symbols", EditorStyles.boldLabel);

            using (new GUILayout.HorizontalScope())
            {
                DrawButtons();
            }

            _reorderableDefines.DoLayoutList();
        }

        private static void RefreshDefineState(bool force = false)
        {
            if (force || (_defines == null) || (_reorderableDefines == null))
            {
                _defines = new List<string>(ScriptingDefineManager.GetDefines());

                _reorderableDefines =
                    new ReorderableList(_defines, typeof(string), true, true, true, true)
                    {
                        headerHeight = 1,
                        drawElementCallback =
                            (rect, index, isActive, isFocused) => DrawTextField(rect, index),
                        onAddCallback = list =>
                        {
                            _defines.Add("");
                            _definesHaveChanged = true;
                        },
                        onRemoveCallback = list =>
                        {
                            _defines.RemoveAt(list.index);
                            _definesHaveChanged = true;
                        }
                    };

                _reorderableDefines.onChangedCallback += list => _definesHaveChanged = true;
            }
        }

        private static void DrawButtons()
        {
            var enabled = GUI.enabled;
            GUI.enabled = true;

            if (GUILayout.Button("Manual Update", EditorStyles.miniButton))
            {
                ScriptingDefineManager.Execute();
                RefreshDefineState();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Sort", EditorStyles.miniButton))
            {
                _defines.Sort();
                ScriptingDefineManager.SaveNewDefines(_defines);
                RefreshDefineState();
            }

            if (GUILayout.Button("Sort (Desc)", EditorStyles.miniButton))
            {
                _defines.Sort();
                _defines.Reverse();
                ScriptingDefineManager.SaveNewDefines(_defines);
                RefreshDefineState();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Clear", EditorStyles.miniButton))
            {
                ScriptingDefineManager.ResetDefines();
                RefreshDefineState();
            }

            GUI.enabled = !string.IsNullOrWhiteSpace(ScriptingDefineSettings.BackupDefines);

            if (GUILayout.Button("Restore", EditorStyles.miniButton))
            {
                ScriptingDefineManager.RestoreDefines();
                RefreshDefineState();
            }

            EditorGUILayout.Space();
            GUI.enabled = true;

            if (GUILayout.Button("Copy", EditorStyles.miniButton))
            {
                EditorGUIUtility.systemCopyBuffer = ScriptingDefineManager.GetDefinesUnformatted();
            }

            GUI.enabled = _definesHaveChanged;

            if (GUILayout.Button("Revert", EditorStyles.miniButton))
            {
                RefreshDefineState();
            }

            if (GUILayout.Button("Apply", EditorStyles.miniButton))
            {
                ScriptingDefineManager.SaveNewDefines(_defines);
                RefreshDefineState();
            }

            GUI.enabled = enabled;
        }

        private static void DrawTextField(Rect rect, int index)
        {
            // Handle list selection before the TextField grabs input
            var evt = Event.current;
            if ((evt.type == EventType.MouseDown) && rect.Contains(evt.mousePosition))
            {
                if (_reorderableDefines.index != index)
                {
                    _reorderableDefines.index = index;
                    _reorderableDefines.onSelectCallback?.Invoke(_reorderableDefines);
                }
            }

            var define = _defines[index];
            _defines[index] = GUI.TextField(rect, _defines[index]);
            if (!_defines[index].Equals(define))
            {
                _definesHaveChanged = true;
            }
        }
    }
}
