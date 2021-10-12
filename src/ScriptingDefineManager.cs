using System.Collections.Generic;
using System.Reflection;
using Appalachia.Utility.Reflection.Extensions;
using UnityEditor;
using UnityEngine;

namespace Appalachia.CI.Compilation
{
    [InitializeOnLoad]
    internal class ScriptingDefineManager : Editor
    {
        static ScriptingDefineManager()
        {
            if (!ScriptingDefineSettings.Enabled)
            {
                if (ScriptingDefineSettings.WarnIfDisabled)
                {
                    Debug.LogWarning("Scripting Define Manager is disabled.");
                }

                return;
            }

            Execute();
        }

        public static void Execute()
        {
            var defines = GetDefines();

            var changed = AddDefinesUsingFilter(defines);

            if (changed)
            {
                SaveNewDefinesInternal(defines);
            }
        }

        public static void ResetDefines()
        {
            var targetGroup = ScriptingDefineSettings.BuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            if (string.IsNullOrWhiteSpace(defines))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Empty);
            }
            else
            {
                ScriptingDefineSettings.BackupDefines = defines;

                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Empty);
            }
        }

        public static void RestoreDefines()
        {
            var backup = ScriptingDefineSettings.BackupDefines;

            ScriptingDefineSettings.BackupDefines = string.Empty;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                ScriptingDefineSettings.BuildTargetGroup,
                backup
            );
        }

        public static string GetDefinesUnformatted()
        {
            var defineString =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(
                    ScriptingDefineSettings.BuildTargetGroup
                );

            return defineString;
        }

        public static HashSet<string> GetDefines()
        {
            var defineString = GetDefinesUnformatted();
            var defines = new HashSet<string>();
            var splits = defineString.Split(';');
            
            for (var index = 0; index < splits.Length; index++)
            {
                var s = splits[index];
                var trimmed = s.Trim();

                defines.Add(trimmed);
            }

            return defines;
        }

        private static bool AddDefinesUsingFilter(HashSet<string> defines)
        {
            var doFilter = ScriptingDefineSettings.Filtered;
            var filter = ScriptingDefineSettings.FilterValue;
            var excludeTests = ScriptingDefineSettings.ExcludeTests;
            var filterStart = $"{filter.ToUpper()}_";
            var filterMid = $"_{filter.ToUpper()}_";
            var filterEnd = $"_{filter.ToUpper()}";

            var assemblies = ReflectionExtensions.GetAssemblies();

            var changed = false;
            
            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                var name = FormatAssemblyNameAsDefine(assembly);

                if (excludeTests &&
                    (name.Contains("TEST_") || name.Contains("TESTS_") || name.Contains("_TEST")))
                {
                    continue;
                }

                if (doFilter &&
                    !name.StartsWith(filterStart) &&
                    !name.Contains(filterMid) &&
                    !name.EndsWith(filterEnd))
                {
                    continue;
                }

                if (!defines.Contains(name))
                {
                    changed = true;
                }
                
                defines.Add(name);
            }

            return changed;
        }

        public static string FormatAssemblyNameAsDefine(Assembly assembly)
        {
            return assembly.FullName.Split(',')[0]
                           .ToLowerInvariant()
                           .Replace(".", "_")
                           .ToUpperInvariant();
        }

        public static void SaveNewDefines(IEnumerable<string> defines)
        {
            var hashed = new HashSet<string>(defines);
            SaveNewDefinesInternal(hashed);
        }

        private static void SaveNewDefinesInternal(HashSet<string> defines)
        {
            var newDefineString = string.Join(";", defines);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                ScriptingDefineSettings.BuildTargetGroup,
                newDefineString
            );
        }
    }
}
